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
        public string ocrLanguage = "rus";

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
                image.Source = ImageSourceFromBitmap(img.ToImage<Bgr, Byte>().ToBitmap<Bgr, Byte>());

                totalFrames = video.Get(CapProp.FrameCount);
                fps = video.Get(CapProp.Fps);

                frame_number = 150;
                prevFrame = 0;
                currentFrame = frame_number;
                minFrameForSub = (int)(fps * 1);

                return;
            }
        }

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }
        public static byte[] ImageToByte(System.Drawing.Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public void SubRegionChanged(Mat img)
        {
            for (int index = 0; index < listOfSubs.Count; index++)
            {
                var sub = listOfSubs[index];
                //проверяем изменился ли текст
                var tempimg = img.ToImage<Gray, Byte>();
                tempimg.ROI = sub.frameRegion;

                var subTempimg = sub.frameImage.ToImage<Gray, Byte>();

                CvInvoke.DestroyAllWindows();
                CvInvoke.Imshow("output" + currentFrame.ToString(), subTempimg);

                CvInvoke.AbsDiff(subTempimg, tempimg, subTempimg);
                Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
                Mat hier = new Mat();
                CvInvoke.FindContours(subTempimg, contours, hier, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                for (int i = 0; i < contours.Length; i++)
                    try
                    {
                        using (VectorOfPoint contour = contours[i])
                            if (CvInvoke.ContourArea(contour) > 250) //Если субтитр поменялся
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
                                        frameRegion = sub.frameRegion
                                    });

                                    listOfSubs.Remove(sub);
                                    index--;
                                }

                                break;
                            }
                    }
                    catch(Exception ex) { Debug.WriteLine(ex.Message); }

            }
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
            globalListOfSubs.Clear();
            listOfSubs.Clear();
            if (isVideoOpened)
            {
                video.Set(CapProp.PosFrames, frame_number - 1);
                while(video.IsOpened && currentFrame < frame_number + 100)
                {
                    Mat img = video.QueryFrame();
                    List <System.Drawing.Rectangle> listOfRegions = new List<System.Drawing.Rectangle>();
                    await Task.Run(() =>
                    {
                        SubRegionChanged(img);
                        listOfRegions = FindRegions(img);
                    });

                    if (listOfSubs.Count == 0)
                    {
                        foreach (var region in listOfRegions)
                        {
                            var tempimg = img.ToImage<Gray, Byte>();
                            tempimg.ROI = region;
                            var text = GetRegionsText(tempimg);
                            if (text.Replace(Environment.NewLine, "").Replace("\n", "").Replace(" ", "") != "")
                                listOfSubs.Add(new Subtitle() {
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
                                var tempimg = img.ToImage<Gray, Byte>();
                                tempimg.ROI = region;
                                var text = GetRegionsText(tempimg);
                                if (text.Replace(Environment.NewLine, "").Replace("\n", "").Replace(" ", "") != "")
                                    listOfSubs.Add(new Subtitle()
                                    {
                                        text = GetRegionsText(tempimg),
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
                        if (fps < sumFrameNum && fps * 60 > sumFrameNum) // Если субтитр дольше секунды и меньше минуты
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
                var listOfRegions = FindRegions(img);
                foreach(var region in listOfRegions)
                {
                    var tempimg = img.ToImage<Gray, Byte>();
                    tempimg.ROI = region;
                    var text = GetRegionsText(tempimg);
                    if (text.Replace(Environment.NewLine, "").Replace("\n", "").Replace(" ", "") != "")
                        globalListOfSubs.Add(new Subtitle()
                        {
                            text = text
                        });
                }
            }

            ViewGrid();
        }

        int imH;
        int imW;
        double minHeight;
        double minWidth;
        int k1;
        int k2;

        private List<System.Drawing.Rectangle> FindRegions(Mat img)
        {
            Image<Gray, Byte> grayFrame = img.ToImage<Gray, Byte>();

            imH = grayFrame.Height;
            imW = grayFrame.Width;
            minHeight = 0.01 * imH;
            minWidth = 0.01 * imW;
            //var maxHeight = 100;

            grayFrame = grayFrame.SmoothGaussian(3);
            grayFrame = grayFrame.SmoothMedian(3);

            var laplacian = grayFrame.Laplace(3);
            CvInvoke.ConvertScaleAbs(laplacian, grayFrame, 1, 0);

            CvInvoke.Threshold(grayFrame, grayFrame, 170, 255, ThresholdType.Binary);

            Mat kernel1 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new System.Drawing.Size(5, 5), new System.Drawing.Point(1, 1));
            grayFrame = grayFrame.MorphologyEx(MorphOp.Gradient, kernel1, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());

            k1 = imH / 30;
            k2 = imW / 70;
            kernel1 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new System.Drawing.Size(k1, k2), new System.Drawing.Point(-1, -1));
            CvInvoke.Dilate(grayFrame, grayFrame, kernel1, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());


            k1 = imH / 20;
            k2 = imW / 65;
            kernel1 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new System.Drawing.Size(k1, k2), new System.Drawing.Point(-1, -1));
            CvInvoke.Erode(grayFrame, grayFrame, kernel1, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());


            Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
            Mat hier = new Mat();
            CvInvoke.FindContours(grayFrame, contours, hier, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            var rois = new List<System.Drawing.Rectangle>(); // List of rois
            if (contours.Size > 0)
            {
                for (int i = 0; i < contours.Size; i++)
                {
                    System.Drawing.Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);

                    Image<Gray, Byte> tempImg = img.ToImage<Gray, Byte>();
                    grayFrame.CopyTo(tempImg);

                    int countWhitePixels = CvInvoke.CountNonZero(tempImg);
                    

                    if (!(rect.Height < minHeight ||
                        rect.Width < minWidth ||
                        rect.Width / rect.Height < 1.3 ||
                        countWhitePixels < (rect.Width * rect.Height * 0.1)))
                    {
                        if (rect.X > 10 && (rect.X + rect.Width + 20) < imW)
                        {
                            rect.X -= 10;
                            rect.Width += 20;
                        }
                        if (rect.Y > 10 && (rect.Y + rect.Height + 20) < imW)
                        {
                            rect.Y -= 10;
                            rect.Height += 20;
                        }
                        rois.Add(rect);
                    }

                    tempImg.Dispose();
                }
            }
            //image.Source = ImageSourceFromBitmap(origImage.ToBitmap<Bgr, Byte>());
            grayFrame.Dispose();
            laplacian.Dispose();
            kernel1.Dispose();
            hier.Dispose();
            contours.Dispose();
            //Force garbage collection.
            GC.Collect();

            // Wait for all finalizers to complete before continuing.
            GC.WaitForPendingFinalizers();


            return rois;
        }
        public string GetRegionsText(Image<Gray, Byte> frameRegion)
        {
            Image<Gray, Byte> tempPartImage = frameRegion.Clone();
            Image<Gray, Byte> tempLaplace = frameRegion.Clone();
            tempPartImage = tempPartImage.SmoothMedian(3);
            //tempPartImage = tempPartImage.SmoothGaussian(3);

            float[,] matrix = new float[3, 3] {
                { 0, -1, 0},
                { -1, 4, -1},
                { 0, -1, 0}
            };
            ConvolutionKernelF matrixKernel = new ConvolutionKernelF(matrix);

            //Mat kernel = new Mat(*size, DepthType.Default, *data);
            //CvInvoke.Laplacian(tempPartImage, tempPartImage, DepthType.Default, 3);
            CvInvoke.DestroyAllWindows();
            CvInvoke.Imshow("outputGray", tempPartImage);
            //var laplace = tempPartImage.Laplace(3);
            //CvInvoke.ConvertScaleAbs(laplace, tempLaplace, 1, 0);

            CvInvoke.Filter2D(tempPartImage, tempLaplace, matrixKernel, new System.Drawing.Point(-1, -1));

            CvInvoke.Imshow("outputLaplace", tempLaplace);

            //CvInvoke.Threshold(tempPartImage, tempPartImage, 255, 255, ThresholdType.Binary);
            CvInvoke.AdaptiveThreshold(tempLaplace, tempPartImage, 255, AdaptiveThresholdType.MeanC, ThresholdType.Binary, 7, 20);
            CvInvoke.Imshow("outputThresh", tempPartImage);

            //Mat white = Mat.Ones(tempLaplace.Rows, tempLaplace.Cols, DepthType.Default, 1);
            //Mat dst = new Mat();
            //CvInvoke.AbsDiff(white, tempLaplace.Mat, dst);

            tempPartImage = tempPartImage.Not();
            CvInvoke.Imshow("outputNOT", tempPartImage);
            //CvInvoke.AbsDiff(tempPartImage, tempLaplace, tempPartImage);
            //CvInvoke.Imshow("outputABSDiff", tempPartImage);

            /*int k1 = 1;
            int k2 = 1;
            Mat kernel = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new System.Drawing.Size(k1, k2), new System.Drawing.Point(-1, -1));
            CvInvoke.Dilate(tempPartImage, tempPartImage, kernel, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            CvInvoke.Imshow("outputDilate", tempPartImage);*/

            /*tempPartImage = tempPartImage.SmoothGaussian(3);
            CvInvoke.Threshold(tempPartImage, tempPartImage, 10, 1, ThresholdType.Binary);
            //CvInvoke.Imshow("output??", tempPartImage);
            CvInvoke.Multiply(tempPartImage, tempLaplace, tempPartImage, 100);
            CvInvoke.Imshow("output??Mul", tempPartImage);*/

            //CvInvoke.Threshold(tempPartImage, tempPartImage, 0, 255, ThresholdType.Binary);
            //CvInvoke.Imshow("output??THRESH", tempPartImage);

            //image.Source = ImageSourceFromBitmap(tempPartImage.ToBitmap<Gray, Byte>());
            var ocrengine = new TesseractEngine(@".\tessdata", ocrLanguage, EngineMode.Default);
            var imgPix = Pix.LoadFromMemory(ImageToByte(tempPartImage.ToBitmap<Gray, Byte>()));
            var res = ocrengine.Process(imgPix);
            var returnText = res.GetText();

            tempPartImage.Dispose();
            ocrengine.Dispose();
            imgPix.Dispose();

            return returnText;
        }
    }
}
