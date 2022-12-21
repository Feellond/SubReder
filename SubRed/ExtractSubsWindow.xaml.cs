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

namespace SubRed
{
    /// <summary>
    /// Логика взаимодействия для ExtractSubsWindow.xaml
    /// </summary>
    public partial class ExtractSubsWindow : Window
    {
        public List<Subtitle> globalListOfSubs;
        public List<Subtitle> listOfSubs = new List<Subtitle>();
        public string filePath { get; set; }
        public bool isVideoOpened = false;
        public VideoCapture video;

        double totalFrames;
        double fps;

        int frame_number;
        int prevFrame;
        int currentFrame;
        int minFrameForSub;

        public void ViewGrid()
        {
            //https://social.msdn.microsoft.com/Forums/en-US/47ce71aa-5bde-482a-9574-764e45cb9031/bind-list-to-datagrid-in-wpf?forum=wpf
            this.SubtitleGrid.ItemsSource = null;
            this.SubtitleGrid.ItemsSource = globalListOfSubs;
            
        }

        public ExtractSubsWindow(List<Subtitle> globalListOfSubs)
        {
            InitializeComponent();
            this.globalListOfSubs = globalListOfSubs;

            ViewGrid();
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

                frame_number = 150;
                prevFrame = 0;
                currentFrame = frame_number;
                minFrameForSub = (int)(fps * 1);

                return;
            }
        }

        public void FrameChanged(Mat newFrame, Mat previousFrame)
        {
            try
            {
                if (!previousFrame.IsEmpty)
                {
                    var tempimg = newFrame.ToImage<Gray, Byte>();
                    CvInvoke.AbsDiff(previousFrame, newFrame, tempimg);
                    Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
                    Mat hier = new Mat();
                    CvInvoke.FindContours(tempimg, contours, hier, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                    for (int i = 0; i < contours.Length; i++)
                    {
                        var subChangedControlValue = tempimg.Height * tempimg.Width * 0.5;
                        using (VectorOfPoint contour = contours[i])
                            if (CvInvoke.ContourArea(contour) > subChangedControlValue) //Если кадр поменялся
                            {
                                SubRegionChanged(newFrame);
                                break;
                            }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("A handled exception just occurred: " + ex.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning); }

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
                CvInvoke.Imshow("output" + currentFrame.ToString(), subTempimg);

                CvInvoke.AbsDiff(subTempimg, tempimg, subTempimg);
                var contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
                var hier = new Mat();
                CvInvoke.FindContours(subTempimg, contours, hier, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                for (int j = 0; j < contours.Length; j++)
                {
                    var subChangedControlValue = sub.frameRegion.Height * sub.frameRegion.Width * 0.3;
                    using (VectorOfPoint contour = contours[j])
                        if (CvInvoke.ContourArea(contour) > subChangedControlValue) //Если субтитр поменялся
                        {
                            var sumFrameNum = currentFrame - sub.frameBeginNum;
                            if (fps / 2 < sumFrameNum && fps * 240 > sumFrameNum) // Если субтитр дольше секунды и меньше 3 минут
                            {
                                //Запись в глобальную переменную субтитр
                                globalListOfSubs.Add(new Subtitle()
                                {
                                    text = sub.text,
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

                            break;
                        }
                }
            }
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            globalListOfSubs.Clear();
            listOfSubs.Clear();
            Mat previousFrame = new Mat();
            if (filePath != null)
            {
                if (isVideoOpened)
                {
                    video.Set(CapProp.PosFrames, frame_number - 1);
                    while (video.IsOpened && currentFrame < frame_number + 100)
                    {
                        Mat newFrame = video.QueryFrame();
                        List<System.Drawing.Rectangle> listOfRegions = new List<System.Drawing.Rectangle>();
                        await Task.Run(() =>
                        {
                            FrameChanged(newFrame, previousFrame);
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

                        currentFrame++;
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
                                globalListOfSubs.Add(new Subtitle()
                                {
                                    text = sub.text,
                                    frameBeginNum = sub.frameBeginNum,
                                    frameEndNum = currentFrame,
                                    //frameImage = sub.frameImage,
                                    frameRegion = sub.frameRegion
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
                    foreach (var region in listOfRegions)
                    {
                        var tempimg = img.ToImage<Gray, Byte>();
                        tempimg.ROI = region;
                        var text = SubtitleOCR.GetRegionsTextTesseract(tempimg);
                        if (text.Replace(Environment.NewLine, "").Replace("\n", "").Replace(" ", "") != "")
                            globalListOfSubs.Add(new Subtitle()
                            {
                                text = text,
                                xCoord = region.X,
                                yCoord = region.Y
                            });
                    }
                }

                ViewGrid();
            }
        }
    }
}
