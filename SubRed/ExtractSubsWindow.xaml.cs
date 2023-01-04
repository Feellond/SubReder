using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Ocl;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;
using System.Configuration;
using System.Web;
using Tesseract;
using System.Xml.Linq;
using System.IO;
using System.Security.Policy;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace SubRed
{
    /// <summary>
    /// Логика взаимодействия для ExtractSubsWindow.xaml
    /// </summary>
    public partial class ExtractSubsWindow : Window
    {
        public List<Subtitle> globalListOfSubs = new List<Subtitle>();
        public List<Subtitle> tempGlobalListOfSubs = new List<Subtitle>();
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
            for (int i = 0; i < tempGlobalListOfSubs.Count; i++)
                tempGlobalListOfSubs[i].id = i + 1;

            //https://social.msdn.microsoft.com/Forums/en-US/47ce71aa-5bde-482a-9574-764e45cb9031/bind-list-to-datagrid-in-wpf?forum=wpf
            this.SubtitleGrid.ItemsSource = null;
            this.SubtitleGrid.ItemsSource = tempGlobalListOfSubs;

            imageProgressBar.Visibility = Visibility.Hidden;
        }

        public ExtractSubsWindow(ref List<Subtitle> globalListOfSubs)
        {
            InitializeComponent();
            if (globalListOfSubs == null) globalListOfSubs = new List<Subtitle>();
            tempGlobalListOfSubs = new List<Subtitle>();
            this.globalListOfSubs.AddRange(globalListOfSubs);
            this.tempGlobalListOfSubs.AddRange(globalListOfSubs);

            ViewGrid();
        }

        private void SaveJson_Click(object sender, RoutedEventArgs e)
        {
            string json = JsonConvert.SerializeObject(tempGlobalListOfSubs);
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
                maxFrameNumber = (int)(RangeSlider.UpperValue * fps);

                RangeSlider.Minimum = 0;
                RangeSlider.Maximum = totalFrames / fps;

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
                tempimg.ROI = sub.frameRegion;

                var subTempimg = sub.frameImage.ToImage<Gray, Byte>();

                CvInvoke.DestroyAllWindows();

                CvInvoke.AbsDiff(subTempimg, tempimg, subTempimg);
                CvInvoke.Threshold(subTempimg, subTempimg, 20, 255, ThresholdType.Binary);
                var contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
                var hier = new Mat();
                CvInvoke.FindContours(subTempimg, contours, hier, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

                bool isChanged = false;
                var subChangedControlValue = sub.frameRegion.Height * sub.frameRegion.Width * 0.3;
                for (int j = 0; j < contours.Size; j++)
                {
                    using (VectorOfPoint contour = contours[j])
                        if (CvInvoke.ContourArea(contour) > subChangedControlValue) //Если субтитр поменялся
                        {
                            var sumFrameNum = currentFrame - sub.frameBeginNum;
                            if (fps / 2 < sumFrameNum && fps * 120 > sumFrameNum) // Если субтитр дольше секунды и меньше 2 минут
                            {
                                //Запись в глобальную переменную субтитр
                                tempGlobalListOfSubs.Add(new Subtitle()
                                {
                                    text = sub.text,
                                    start = new TimeSpan(0, 0, 0, (int)(fps * sub.frameBeginNum * 1000)),
                                    end = new TimeSpan(0, 0, 0, (int)(fps * sub.frameEndNum * 1000)),
                                    frameBeginNum = sub.frameBeginNum,
                                    frameEndNum = currentFrame,
                                    //frameImage = sub.frameImage,
                                    frameRegion = sub.frameRegion,
                                    xCoord = sub.frameRegion.X,
                                    yCoord = sub.frameRegion.Y
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
                    listOfSubs[index].frameImage = tempimg.ToBitmap<Gray, byte>();
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
            tempGlobalListOfSubs.Clear();
            listOfSubs.Clear();

            runButton.Visibility = Visibility.Hidden;
            pauseResumeButton.Visibility = Visibility.Visible;

            RangeSliderUpdate(ref minFrameNumber, ref maxFrameNumber);
            currentFrame = minFrameNumber;

            doOCR();
        }

        private async void doOCR()
        {
            CvInvoke.DestroyAllWindows();
            try
            {
                SetOCRValues();
            }
            catch (Exception ex)
            {
                MessageBox.Show("A handled exception just occurred: " + ex.Message, "Exception", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Mat previousFrame = new Mat();
            if (filePath != null)
            {
                if (isVideoOpened)
                {
                    imageProgressBar.Value = 0;
                    imageProgressBar.Visibility = Visibility.Visible;
                    imageProgressBar.Maximum = totalFrames;

                    video.Set(CapProp.PosFrames, minFrameNumber - 1);
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
                            foreach (var region in listOfRegions)
                            {
                                var tempimg = newFrame.ToImage<Gray, Byte>();
                                tempimg.ROI = region;
                                var text = SubtitleOCR.GetRegionsTextTesseract(tempimg);
                                if (text.Replace(Environment.NewLine, "").Replace("\n", "").Replace(" ", "") != "")
                                    listOfSubs.Add(new Subtitle()
                                    {
                                        text = text,
                                        frameBeginNum = currentFrame,
                                        frameImage = tempimg.ToBitmap<Gray, Byte>(),
                                        frameRegion = region
                                    });
                            }
                        }
                        else if (listOfRegions.Count > 0)
                        {
                            foreach (var region in listOfRegions)
                            {
                                bool IsFoundSub = false;
                                foreach (var sub in listOfSubs)
                                {
                                    System.Drawing.Rectangle rectIntersect = sub.frameRegion;
                                    rectIntersect.Intersect(region);
                                    if (rectIntersect.Height * rectIntersect.Width > sub.frameRegion.Height * sub.frameRegion.Width * 0.2) //Если пересекается более чем на 20%
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
                                    if (text.Replace(Environment.NewLine, "").Replace("\n", "").Replace(" ", "") != "")
                                        listOfSubs.Add(new Subtitle()
                                        {
                                            text = SubtitleOCR.GetRegionsTextTesseract(tempimg),
                                            frameBeginNum = currentFrame,
                                            frameImage = tempimg.ToBitmap<Gray, Byte>(),
                                            frameRegion = region
                                        });
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
                            var sumFrameNum = currentFrame - sub.frameBeginNum;
                            if (fps / 2 < sumFrameNum && fps * 60 > sumFrameNum) // Если субтитр дольше секунды и меньше минуты
                            {
                                //Запись в глобальную переменную субтитр
                                tempGlobalListOfSubs.Add(new Subtitle()
                                {
                                    text = sub.text,
                                    start = new TimeSpan(0, 0, 0, (int)(fps * sub.frameBeginNum * 1000)),
                                    end = new TimeSpan(0, 0, 0, (int)(fps * sub.frameEndNum * 1000)),
                                    frameBeginNum = sub.frameBeginNum,
                                    frameEndNum = currentFrame,
                                    //frameImage = sub.frameImage,
                                    frameRegion = sub.frameRegion,
                                    xCoord = sub.frameRegion.X,
                                    yCoord = sub.frameRegion.Y
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
                    var listOfRegions = SubtitleOCR.FindRegions(img);
                    var index = 0;
                    foreach (var region in listOfRegions)
                    {
                        index++;
                        var tempimg = img.ToImage<Gray, Byte>();
                        tempimg.ROI = region;
                        var text = SubtitleOCR.GetRegionsTextTesseract(tempimg, index.ToString());
                        if (text.Replace(Environment.NewLine, "").Replace("\n", "").Replace(" ", "") != "")
                            tempGlobalListOfSubs.Add(new Subtitle()
                            {
                                text = text,
                                xCoord = region.X,
                                yCoord = region.Y
                            });
                        break;
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

        private void SetOCRValues()
        {
            SubtitleOCR.gausSeed = int.Parse(textBoxGausSeed.Text);
            SubtitleOCR.meanSeed = int.Parse(textBoxMeanSeed.Text);
            SubtitleOCR.laplaceSeed = int.Parse(textBoxLaplaceSeed.Text);
            SubtitleOCR.morphSize = int.Parse(textBoxMorphSize.Text);
            SubtitleOCR.thresholdLaplace = int.Parse(textBoxThresholdLaplace.Text);
            SubtitleOCR.dilateHeight = int.Parse(textBoxDilateHeight.Text);
            SubtitleOCR.dilateWidth = int.Parse(textBoxDilateWidth.Text);
            SubtitleOCR.erodeHeight = int.Parse(textBoxErodeHeight.Text);
            SubtitleOCR.erodeWidth = int.Parse(textBoxErodeWidth.Text);

            ComboBoxItem ComboItem = (ComboBoxItem)comboBoxLanguage.SelectedItem;
            SubtitleOCR.OCRLanguageChange(ComboItem.Name);
        }

        private void defaultButton_Click(object sender, RoutedEventArgs e)
        {
            SubtitleOCR.ReturnDefaultValues();
            textBoxGausSeed.Text = SubtitleOCR.gausSeed.ToString();
            textBoxMeanSeed.Text = SubtitleOCR.meanSeed.ToString();
            textBoxLaplaceSeed.Text = SubtitleOCR.laplaceSeed.ToString();
            textBoxMorphSize.Text = SubtitleOCR.morphSize.ToString();
            textBoxThresholdLaplace.Text = SubtitleOCR.thresholdLaplace.ToString();
            textBoxDilateHeight.Text = SubtitleOCR.dilateHeight.ToString();
            textBoxDilateWidth.Text = SubtitleOCR.dilateWidth.ToString();
            textBoxErodeHeight.Text = SubtitleOCR.erodeHeight.ToString();
            textBoxErodeWidth.Text = SubtitleOCR.erodeWidth.ToString();

            var lang = SubtitleOCR.ocrLanguage;
            switch (lang)
            {
                case "eng":
                    comboBoxLanguage.SelectedItem = comboBoxLanguage.Items[comboBoxLanguage.Items.IndexOf("Английский")];
                    break;
                case "rus":
                    comboBoxLanguage.SelectedItem = comboBoxLanguage.Items[comboBoxLanguage.Items.IndexOf("Русский")];
                    break;
                default:
                    comboBoxLanguage.SelectedItem = comboBoxLanguage.Items[0];
                    break;
            }
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (tempGlobalListOfSubs.Count > 0)
            {
                globalListOfSubs.Clear();
                globalListOfSubs.AddRange(tempGlobalListOfSubs);
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
    }
}
