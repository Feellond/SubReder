using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Microsoft.Win32;
using System.IO;
using Newtonsoft.Json;
using SubRed.Sub_formats;

namespace SubRed
{
    /// <summary>
    /// Логика взаимодействия для ExtractSubsWindow.xaml
    /// </summary>
    public partial class ExtractSubsWindow : Window
    {
        public SubProject globalProject = new ();
        public SubProject tempProject = new();
        public List<Subtitle> listOfSubs = new List<Subtitle>();
        public string filePath { get; set; }
        public bool isVideoOpened = false;
        public VideoCapture video;

        double totalFrames;
        double fps;

        int minFrameNumber;
        int maxFrameNumber;
        int prevFrame;
        int currentFrame;

        bool isPausedOCR = false;

        public void ViewGrid()
        {
            for (int i = 0; i < tempProject.SubtitlesList.Count; i++)
                tempProject.SubtitlesList[i].Id = i + 1;

            //https://social.msdn.microsoft.com/Forums/en-US/47ce71aa-5bde-482a-9574-764e45cb9031/bind-list-to-datagrid-in-wpf?forum=wpf
            this.SubtitleGrid.ItemsSource = null;
            GC.Collect();
            this.SubtitleGrid.ItemsSource = tempProject.SubtitlesList;

            imageProgressBar.Visibility = Visibility.Hidden;
        }

        public ExtractSubsWindow(List<Subtitle> globalListOfSubs)
        {
            InitializeComponent();
            if (globalListOfSubs == null) globalListOfSubs = new List<Subtitle>();
            tempProject.SubtitlesList = new List<Subtitle>();
            this.globalProject.SubtitlesList.AddRange(globalListOfSubs);
            this.tempProject.SubtitlesList.AddRange(globalListOfSubs);

            ViewGrid();
        }

        private void SaveJson_Click(object sender, RoutedEventArgs e)
        {
            string json = JsonConvert.SerializeObject(tempProject.SubtitlesList);
            try
            {
                System.IO.File.WriteAllText(@"subtitleJson.txt", json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
        }

        private void OpenImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "Select a picture";
            op.Filter = "All supported graphics|*.jpg;*.jpeg;*.png|" +
              "JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
              "Portable Network Graphic (*.png)|*.png";
            if (op.ShowDialog() == true)
            {
                image.Source = new BitmapImage(new Uri(op.FileName));
                filePath = op.FileName;
                isVideoOpened = false;
            }
        }
        private void OpenVideo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "All Media Files|*.wav;*.aac;*.wma;*.wmv;*.avi;*.mpg;*.mpeg;*.m1v;*.mp2;*.mp3;*.mpa;*.mpe;*.m3u;*.mp4;*.mov;*.3g2;*.3gp2;*.3gp;*.3gpp;*.m4a;*.cda;*.aif;*.aifc;*.aiff;*.mid;*.midi;*.rmi;*.mkv;*.WAV;*.AAC;*.WMA;*.WMV;*.AVI;*.MPG;*.MPEG;*.M1V;*.MP2;*.MP3;*.MPA;*.MPE;*.M3U;*.MP4;*.MOV;*.3G2;*.3GP2;*.3GP;*.3GPP;*.M4A;*.CDA;*.AIF;*.AIFC;*.AIFF;*.MID;*.MIDI;*.RMI;*.MKV";
            if (openFileDialog.ShowDialog() == true)
            {
                FileInfo fileInfo = new FileInfo(openFileDialog.FileName);

                //MediaElement1.Source = new Uri(fileInfo.FullName);
                filePath = fileInfo.FullName;
                isVideoOpened = true;
                video = new Emgu.CV.VideoCapture(filePath);
                Mat img = video.QueryFrame();
                image.Source = SubtitleOCR.ImageSourceFromBitmap(img.ToImage<Bgr, Byte>().ToBitmap<Bgr, Byte>());

                totalFrames = video.Get(CapProp.FrameCount);
                fps = video.Get(CapProp.Fps);

                minFrameNumber = 0;
                prevFrame = 0;
                currentFrame = minFrameNumber;

                RangeSlider.Minimum = 0;
                RangeSlider.Maximum = totalFrames / fps;

                RangeSlider.LowerValue = 0;
                RangeSlider.UpperValue = totalFrames / fps;

                maxFrameNumber = (int)(RangeSlider.UpperValue * fps);

                imageProgressBar.Maximum = maxFrameNumber;

                return;
            }
        }

        public void FrameChanged(Mat newFrame, Mat previousFrame)
        {
            if (!previousFrame.IsEmpty)
            {
                var tempNewFrame = newFrame.ToImage<Gray, Byte>();
                var tempPrevFrame = previousFrame.ToImage<Gray, Byte>();
                CvInvoke.AbsDiff(tempPrevFrame, tempNewFrame, tempPrevFrame);
                CvInvoke.Threshold(tempPrevFrame, tempPrevFrame, 20, 255, ThresholdType.Binary);
                Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
                Mat hier = new Mat();
                CvInvoke.FindContours(tempPrevFrame, contours, hier, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                var subChangedControlValue = tempPrevFrame.Height * tempPrevFrame.Width * 0.5;
                for (int i = 0; i < contours.Size; i++)
                {
                    VectorOfPoint contour = contours[i];
                    if (CvInvoke.ContourArea(contour) > subChangedControlValue) //Если кадр поменялся
                    {
                        SubRegionChanged(newFrame);
                        break;
                    }
                }
            }

        }

        public void SubRegionChanged(Mat newFrame)
        {
            for (int index = 0; index < listOfSubs.Count; index++)
            {
                var sub = listOfSubs[index];
                //проверяем изменился ли текст
                var tempimg = newFrame.ToImage<Gray, Byte>();
                tempimg.ROI = sub.FrameRegion;

                var subTempimg = sub.FrameImage.ToImage<Gray, Byte>();

                //CvInvoke.DestroyAllWindows();

                CvInvoke.AbsDiff(subTempimg, tempimg, subTempimg);
                CvInvoke.Threshold(subTempimg, subTempimg, 20, 255, ThresholdType.Binary);
                var contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
                var hier = new Mat();
                CvInvoke.FindContours(subTempimg, contours, hier, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

                bool isChanged = false;
                var subChangedControlValue = sub.FrameRegion.Height * sub.FrameRegion.Width * 0.3;
                for (int j = 0; j < contours.Size; j++)
                {
                    using (VectorOfPoint contour = contours[j])
                        if (CvInvoke.ContourArea(contour) > subChangedControlValue) //Если субтитр поменялся
                        {
                            var sumFrameNum = currentFrame - sub.FrameBeginNum;
                            if (fps / 2 < sumFrameNum && fps * 120 > sumFrameNum) // Если субтитр дольше секунды и меньше 2 минут
                            {
                                //Запись в глобальную переменную субтитр
                                tempProject.SubtitlesList.Add(new Subtitle()
                                {
                                    Text = sub.Text,
                                    Start = new TimeSpan(0, 0, 0, 0, (int)(sub.FrameBeginNum / fps * 1000)),
                                    End = new TimeSpan(0, 0, 0, 0, (int)(currentFrame / fps * 1000)),
                                    FrameBeginNum = sub.FrameBeginNum,
                                    FrameEndNum = currentFrame,
                                    //frameImage = sub.frameImage,
                                    FrameRegion = sub.FrameRegion,
                                    XCoord = sub.FrameRegion.X,
                                    YCoord = sub.FrameRegion.Y
                                });

                                listOfSubs.Remove(sub);
                                index--;
                                isChanged = true;
                            }

                            break;
                        }
                }

                if (!isChanged)
                {
                    listOfSubs[index].FrameImage = tempimg.ToBitmap<Gray, byte>();
                }
            }
        }
        private void RangeSliderUpdate(ref int minFrameNumber, ref int maxFrameNumber)
        {
            minFrameNumber = (int)(RangeSlider.LowerValue * fps);
            maxFrameNumber = (int)(RangeSlider.UpperValue * fps);
        }

        private void RunPauseButtonChange(object sender, RoutedEventArgs e)
        {
            if (pauseResumeButton.Content.ToString() == "Пауза")
            {
                pauseResumeButton.Content = "Продолжить";
                isPausedOCR = true;
            }
            else if (pauseResumeButton.Content.ToString() == "Продолжить")
            {
                pauseResumeButton.Content = "Пауза";
                isPausedOCR = false;
                doOCR();
            }
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            if (filePath != null)
            {
                if (filePath != "")
                {
                    this.ResizeMode = ResizeMode.NoResize;

                    tempProject.SubtitlesList.Clear();
                    listOfSubs.Clear();

                    runButton.Visibility = Visibility.Hidden;
                    pauseResumeButton.Visibility = Visibility.Visible;

                    RangeSliderUpdate(ref minFrameNumber, ref maxFrameNumber);
                    currentFrame = minFrameNumber;

                    doOCR();
                }
            }
            this.ResizeMode = ResizeMode.CanResize;
        }

        private async void doOCR()
        {
            CvInvoke.DestroyAllWindows();
            Mat previousFrame = new Mat();
            if (filePath != null)
            {
                if (isVideoOpened)
                {
                    imageProgressBar.Value = currentFrame;
                    imageProgressBar.Visibility = Visibility.Visible;

                    video.Set(CapProp.PosFrames, currentFrame);
                    while (video.IsOpened && currentFrame <= maxFrameNumber && !isPausedOCR)
                    {
                        Mat newFrame = video.QueryFrame();
                        if (newFrame == null)
                            break;

                        image.Source = SubtitleOCR.ImageSourceFromBitmap(newFrame.ToBitmap());

                        List<System.Drawing.Rectangle> listOfRegions = new List<System.Drawing.Rectangle>();
                        await Task.Run(() =>
                        {
                            //FrameChanged(newFrame, previousFrame);
                            SubRegionChanged(newFrame);
                            listOfRegions = SubtitleOCR.FindRegions(newFrame);
                        });

                        if (listOfSubs.Count == 0)
                        {
                            EastDetector eastDetector = new EastDetector();
                            foreach (var region in listOfRegions)
                            {
                                var tempimg = newFrame.ToImage<Gray, Byte>();
                                tempimg.ROI = region;
                                var text = SubtitleOCR.GetRegionsTextTesseract(tempimg);

                                bool foundedInEast = true;
                                if (SubtitleOCR.useEast)
                                {
                                    foundedInEast = false;
                                    if (eastDetector.EastDetect(tempimg.Mat).Count > 0)
                                    {
                                        foundedInEast = true;
                                    }
                                }

                                if (foundedInEast)
                                {
                                    if (text.Replace(Environment.NewLine, "").Replace("\n", "").Replace(" ", "") != "")
                                        listOfSubs.Add(new Subtitle()
                                        {
                                            Text = text,
                                            FrameBeginNum = currentFrame,
                                            FrameImage = tempimg.ToBitmap<Gray, Byte>(),
                                            FrameRegion = region
                                        });
                                }
                            }
                        }
                        else if (listOfRegions.Count > 0)
                        {
                            EastDetector eastDetector = new EastDetector();
                            foreach (var region in listOfRegions)
                            {
                                bool IsFoundSub = false;
                                foreach (var sub in listOfSubs)
                                {
                                    System.Drawing.Rectangle rectIntersect = sub.FrameRegion;
                                    rectIntersect.Intersect(region);
                                    if (rectIntersect.Height * rectIntersect.Width > sub.FrameRegion.Height * sub.FrameRegion.Width * 0.2) //Если пересекается более чем на 20%
                                    {
                                        IsFoundSub = true; // нашли субтитр
                                        break;
                                    }
                                }
                                if (!IsFoundSub) // если не нашли пересечение, то потенциально новый текст
                                {
                                    var tempimg = newFrame.ToImage<Gray, Byte>();
                                    tempimg.ROI = region;
                                    var text = SubtitleOCR.GetRegionsTextTesseract(tempimg);

                                    bool foundedInEast = true;
                                    if (SubtitleOCR.useEast)
                                    {
                                        foundedInEast = false;
                                        if (eastDetector.EastDetect(tempimg.Mat).Count > 0)
                                        {
                                            foundedInEast = true;
                                        }
                                    }

                                    if (foundedInEast)
                                    {
                                        if (text.Replace(Environment.NewLine, "").Replace("\n", "").Replace(" ", "") != "")
                                            listOfSubs.Add(new Subtitle()
                                            {
                                                Text = SubtitleOCR.GetRegionsTextTesseract(tempimg),
                                                FrameBeginNum = currentFrame,
                                                FrameImage = tempimg.ToBitmap<Gray, Byte>(),
                                                FrameRegion = region
                                            });
                                    }
                                }
                            }
                        }

                        previousFrame = newFrame.Clone();
                        currentFrame++;
                        imageProgressBar.Value = currentFrame;
                    }
                    if (listOfSubs.Count > 0)
                    {
                        for (int index = 0; index < listOfSubs.Count; index++)
                        {
                            var sub = listOfSubs[index];
                            var sumFrameNum = currentFrame - sub.FrameBeginNum;
                            if (fps / 2 < sumFrameNum && fps * 60 > sumFrameNum) // Если субтитр дольше секунды и меньше минуты
                            {
                                //Запись в глобальную переменную субтитр
                                tempProject.SubtitlesList.Add(new Subtitle()
                                {
                                    Text = sub.Text,
                                    Start = new TimeSpan(0, 0, 0, 0, (int)(sub.FrameBeginNum / fps * 1000)),
                                    End = new TimeSpan(0, 0, 0, 0, (int)(sub.FrameEndNum / fps * 1000)),
                                    FrameBeginNum = sub.FrameBeginNum,
                                    FrameEndNum = currentFrame,
                                    //frameImage = sub.frameImage,
                                    FrameRegion = sub.FrameRegion,
                                    XCoord = sub.FrameRegion.X,
                                    YCoord = sub.FrameRegion.Y
                                });

                                listOfSubs.Remove(sub);
                                index--;
                            }
                        }
                        listOfSubs.Clear();
                    }
                }
                else
                {
                    Mat img = CvInvoke.Imread(filePath);

                    EastDetector eastDetector = new EastDetector();
                    //var listOfEastRegions = eastDetector.EastDetect(img);

                    var listOfRegions = SubtitleOCR.FindRegions(img);
                    var index = 0;
                    foreach (var region in listOfRegions)
                    {
                        index++;
                        var tempimg = img.ToImage<Gray, Byte>();
                        tempimg.ROI = region;
                        bool foundedInEast = true;
                        if (SubtitleOCR.useEast)
                        {
                            foundedInEast = false;
                            if (eastDetector.EastDetect(tempimg.Mat).Count > 0)
                            {
                                foundedInEast = true;
                            }
                        }

                        if (foundedInEast)
                        {
                            var text = SubtitleOCR.GetRegionsTextTesseract(tempimg, index.ToString());
                            if (text.Replace(Environment.NewLine, "").Replace("\n", "").Replace(" ", "") != "")
                                tempProject.SubtitlesList.Add(new Subtitle()
                                {
                                    Text = text,
                                    XCoord = region.X,
                                    YCoord = region.Y
                                });
                        }
                    }
                }

                if (!isPausedOCR)
                {
                    runButton.Visibility = Visibility.Visible;
                    pauseResumeButton.Visibility = Visibility.Hidden;
                }

                ViewGrid();
            }
        }

        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private void PreviewTextInputNumbers(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        public static bool IsValid(string name, string str)
        {
            int i;
            switch (name)
            {
                case "textBoxGausSeed":
                case "textBoxMeanSeed":
                case "textBoxLaplaceSeed":
                case "textBoxMorphSize":
                case "textBoxDilateHeight":
                case "textBoxDilateWidth":
                case "textBoxErodeHeight":
                case "textBoxErodeWidth":
                    return int.TryParse(str, out i) && i >= 1 && i <= 9999 && i % 2 == 1;
                case "textBoxThresholdLaplace":
                    return int.TryParse(str, out i) && i >= 0 && i <= 255;
                default:
                    return false;
            }
            
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (tempProject.SubtitlesList.Count > 0)
            {
                globalProject.SubtitlesList.Clear();
                globalProject.SubtitlesList.AddRange(tempProject.SubtitlesList);
                this.DialogResult = true;
                CloseWindow();
            }
        }

        private void CloseWindow()
        {
            GC.Collect(); // find finalizable objects
            GC.WaitForPendingFinalizers(); // wait until finalizers executed
            GC.Collect(); // collect finalized objects
            this.Close();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            CloseWindow();
        }

        private void SrtSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFile("srt");
        }

        private void SaveFile(string format = "")
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "All Files|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                if (format == "")
                    SubFormats.SelectFormat(saveFileDialog.FileName, tempProject, false);
                else
                    SubFormats.SelectFormat(saveFileDialog.FileName, tempProject, false, format);
            }
        }

        private void MoreSettings_Click(object sender, RoutedEventArgs e)
        {
            MoreSettingsWindow moreSettingsWindow = new MoreSettingsWindow();
            moreSettingsWindow.Show();
        }
    }
}
