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
        public string ocrLanguage = "eng";

        double totalFrames;
        double fps;

        int frame_number;
        int prevFrame;
        int currentFrame;
        int minFrameForSub;


        public ExtractSubsWindow(List<Subtitle> globalListOfSubs)
        {
            InitializeComponent();
            this.globalListOfSubs = globalListOfSubs;
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
                /*subTempimg = subTempimg.SmoothMedian(3).Laplace(3);
                CvInvoke.ConvertScaleAbs(subTempimg, subTempimg, 1, 0);
                subTempimg = subTempimg.ThresholdBinary(new Gray(20), new Gray(255));
                CvInvoke.AbsDiff(tempimg.SmoothMedian(3).ThresholdBinary(new Gray(20), new Gray(255)), subTempimg, tempimg);*/

                //currentframe = cv2.absdiff(currentframe, previousframe)
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

                                break;
                            }
                    }
                    catch(Exception ex) { Debug.WriteLine(ex.Message); }

            }
        }

        private async void RunButton_Click(object sender, RoutedEventArgs e)
        {
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
                    listOfSubs.Add(new Subtitle() { text = GetRegionsText(tempimg)});
                }
            }
        }

        int imH;
        int imW;
        double minHeight;
        double minWidth;
        int k1;
        int k2;

        private List<System.Drawing.Rectangle> FindRegions(Mat img)
        {
            Image<Bgr, Byte> origImage = img.ToImage<Bgr, Byte>();
            Image<Gray, Byte> grayFrame = img.ToImage<Gray, Byte>();

            imH = grayFrame.Height;
            imW = grayFrame.Width;
            minHeight = 0.01 * imH;
            minWidth = 0.01 * imW;
            //var maxHeight = 100;

            grayFrame = grayFrame.SmoothMedian(3);
            grayFrame = grayFrame.SmoothGaussian(3);

            var laplacian = grayFrame.Laplace(3);
            CvInvoke.ConvertScaleAbs(laplacian, grayFrame, 1, 0);

            CvInvoke.Threshold(grayFrame, grayFrame, 170, 255, ThresholdType.Binary);

            Mat kernel1 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new System.Drawing.Size(5, 5), new System.Drawing.Point(1, 1));
            grayFrame = grayFrame.MorphologyEx(MorphOp.Gradient, kernel1, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());

            k1 = imH / 30;
            k2 = imW / 70;
            Mat element = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new System.Drawing.Size(k1, k2), new System.Drawing.Point(-1, -1));
            CvInvoke.Dilate(grayFrame, grayFrame, element, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());


            k1 = imH / 20;
            k2 = imW / 65;
            element = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new System.Drawing.Size(k1, k2), new System.Drawing.Point(-1, -1));
            CvInvoke.Erode(grayFrame, grayFrame, element, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());


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

                    int countWhitePixels = 0;
                    for (int i1 = 0; i1 < tempImg.Rows; i1++)
                    {
                        for (int j1 = 0; j1 < tempImg.Cols; j1++)
                        {
                            if (tempImg.Data[i1, j1, 0] == 255)
                                countWhitePixels++;
                        }
                    }

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
            origImage.Dispose();
            grayFrame.Dispose();
            laplacian.Dispose();
            kernel1.Dispose();
            element.Dispose();
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
            tempPartImage = tempPartImage.SmoothMedian(3);
            //tempPartImage = tempPartImage.SmoothGaussian(3);

            var laplacian = tempPartImage.Laplace(3);
            CvInvoke.ConvertScaleAbs(laplacian, tempPartImage, 1, 0);
            CvInvoke.Threshold(tempPartImage, tempPartImage, 170, 255, ThresholdType.Binary);
            int k1 = 1;
            int k2 = 1;
            Mat element = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new System.Drawing.Size(k1, k2), new System.Drawing.Point(-1, -1));
            CvInvoke.Dilate(tempPartImage, tempPartImage, element, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());

            //image.Source = ImageSourceFromBitmap(tempPartImage.ToBitmap<Gray, Byte>());
            var ocrengine = new TesseractEngine(@".\tessdata", ocrLanguage, EngineMode.Default);
            var imgPix = Pix.LoadFromMemory(ImageToByte(tempPartImage.ToBitmap<Gray, Byte>()));
            var res = ocrengine.Process(imgPix);
            var returnText = res.GetText();
            textBlock.Text = returnText;

            tempPartImage.Dispose();
            laplacian.Dispose();
            element.Dispose();
            ocrengine.Dispose();
            imgPix.Dispose();

            return returnText;
        }
    }
}
