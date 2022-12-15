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

namespace SubRed
{
    /// <summary>
    /// Логика взаимодействия для ExtractSubsWindow.xaml
    /// </summary>
    public partial class ExtractSubsWindow : Window
    {
        List<Subtitle> listOfSubs = new List<Subtitle>();
        public string filePath { get; set; }
        bool isVideoOpened = false;
        public ExtractSubsWindow()
        {
            InitializeComponent();
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

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            listOfSubs.Clear();
            if (isVideoOpened)
            {
                var video = new Emgu.CV.VideoCapture(filePath);
                double totalFrames = video.Get(CapProp.FrameCount);
                double fps = video.Get(CapProp.Fps);

                var frame_number = 150;
                var prevFrame = 0;
                var currentFrame = frame_number;
                var minFrameForSub = fps * 1;

                video.Set(CapProp.PosFrames, frame_number - 1);
                while(video.IsOpened && currentFrame < frame_number + 3)
                {
                    Mat img = video.QueryFrame();
                    var listOfRegions = FindRegions(img);

                    if(listOfSubs.Count == 0)
                    {
                        foreach (var region in listOfRegions)
                        {
                            var tempimg = img.ToImage<Gray, Byte>();
                            tempimg.ROI = region;
                            listOfSubs.Add(new Subtitle() {
                                text = GetRegionsText(tempimg),
                                frameBeginNum = currentFrame,
                                frameImage = tempimg.ToBitmap<Gray, Byte>(),
                                frameRegion = region
                            });
                        }
                    }
                    foreach (var region in listOfRegions)
                    {
                        bool IsFoundSub = false;
                        foreach(var sub in listOfSubs)
                        {
                            System.Drawing.Rectangle rectIntersect = sub.frameRegion;
                            rectIntersect.Intersect(region);
                            if (rectIntersect.Height * rectIntersect.Width > sub.frameRegion.Height * sub.frameRegion.Width * 0.2) //пересечение более 20%
                            {
                                //проверяем изменился ли текст
                                var tempimg = img.ToImage<Gray, Byte>();
                                tempimg.ROI = region;
                                var subTempimg = sub.frameImage.ToImage<Gray, Byte>().SmoothMedian(3).ThresholdBinary(new Gray(20), new Gray(255));
                                CvInvoke.AbsDiff(tempimg.SmoothMedian(3).ThresholdBinary(new Gray(20), new Gray(255)), subTempimg, tempimg);
                                
                                Emgu.CV.Util.VectorOfVectorOfPoint contours = new Emgu.CV.Util.VectorOfVectorOfPoint();
                                Mat hier = new Mat();
                                CvInvoke.FindContours(tempimg, contours, hier, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
                                for(int i = 0; i < contours.Length; i++)
                                    if (CvInvoke.ContourArea(contours[i]) > 250) //Если субтитр поменялся
                                    {
                                        var sumFrameNum = currentFrame - sub.frameBeginNum;
                                        if (fps < sumFrameNum && fps*60 > sumFrameNum) // Если субтитр дольше секунды и меньше минуты
                                        {
                                            //Запись в глобальную переменную субтитр
                                        }

                                        //Добавляем новый субтитр
                                        var newTempimg = img.ToImage<Gray, Byte>();
                                        newTempimg.ROI = region;
                                        listOfSubs.Add(new Subtitle()
                                        {
                                            text = GetRegionsText(newTempimg),
                                            frameBeginNum = currentFrame,
                                            frameImage = newTempimg.ToBitmap<Gray, Byte>(),
                                            frameRegion = region
                                        });

                                        IsFoundSub = true; // нашли субтитр
                                        break;
                                    }
                            }
                        }
                        if (!IsFoundSub) // если не нашли пересечение, то потенциально новый текст
                        {
                            var tempimg = img.ToImage<Gray, Byte>();
                            tempimg.ROI = region;
                            listOfSubs.Add(new Subtitle() { 
                                text = GetRegionsText(tempimg),
                                frameBeginNum = currentFrame,
                                frameImage = tempimg.ToBitmap<Gray, Byte>(),
                                frameRegion = region
                            });
                        }
                    }

                    currentFrame++;
                }
                if (listOfSubs.Count > 0)
                {
                    foreach (var sub in listOfSubs)
                    {
                        var sumFrameNum = currentFrame - sub.frameBeginNum;
                        if (fps < sumFrameNum && fps * 60 > sumFrameNum) // Если субтитр дольше секунды и меньше минуты
                        {
                            //Запись в глобальную переменную субтитр
                        }
                    }
                }
            }
            else
            {
                Mat img = CvInvoke.Imread(filePath);
                var listOfRegions = FindRegions(img);
                foreach(var region in listOfRegions )
                {
                    var tempimg = img.ToImage<Gray, Byte>();
                    tempimg.ROI = region;
                    listOfSubs.Add(new Subtitle() { text = GetRegionsText(tempimg)});
                }
            }
        }

        private List<System.Drawing.Rectangle> FindRegions(Mat img)
        {
            Image<Bgr, Byte> origImage = img.ToImage<Bgr, Byte>();
            Image<Gray, Byte> grayFrame = img.ToImage<Gray, Byte>();

            var imH = grayFrame.Height;
            var imW = grayFrame.Width;
            var minHeight = 0.01 * imH;
            var minWidth = 0.01 * imW;
            var maxHeight = 100;

            grayFrame = grayFrame.SmoothMedian(3);
            grayFrame = grayFrame.SmoothGaussian(3);

            var laplacian = grayFrame.Laplace(3);
            CvInvoke.ConvertScaleAbs(laplacian, grayFrame, 1, 0);

            CvInvoke.Threshold(grayFrame, grayFrame, 170, 255, ThresholdType.Binary);

            Mat kernel1 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new System.Drawing.Size(5, 5), new System.Drawing.Point(1, 1));
            grayFrame = grayFrame.MorphologyEx(MorphOp.Gradient, kernel1, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());

            int k1 = imH / 30;
            int k2 = imW / 70;
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
                }
            }
            image.Source = ImageSourceFromBitmap(origImage.ToBitmap<Bgr, Byte>());
            return rois;
        }
        public string GetRegionsText(Image<Gray, Byte> frameRegion)
        {
            Image<Gray, Byte> tempPartImage = frameRegion.Clone();
            tempPartImage = tempPartImage.SmoothMedian(3);
            tempPartImage = tempPartImage.SmoothGaussian(3);

            var laplacian = tempPartImage.Laplace(3);
            CvInvoke.ConvertScaleAbs(laplacian, tempPartImage, 1, 0);
            CvInvoke.Threshold(tempPartImage, tempPartImage, 170, 255, ThresholdType.Binary);
            int k1 = 1;
            int k2 = 1;
            Mat element = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new System.Drawing.Size(k1, k2), new System.Drawing.Point(-1, -1));
            CvInvoke.Dilate(tempPartImage, tempPartImage, element, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());

            var ocrengine = new TesseractEngine(@".\tessdata", "eng", EngineMode.TesseractAndLstm);
            var imgPix = Pix.LoadFromMemory(ImageToByte(tempPartImage.ToBitmap<Gray, Byte>()));
            var res = ocrengine.Process(imgPix);
            textBlock.Text = res.GetText();

            return res.GetText();
            /*
             var imageParts = new List<Image<Gray, byte>>(); // List of extracted image parts
            foreach (var roi in rois)
            {
                tempPartImage.ROI = roi;
                imageParts.Add(tempPartImage.Copy());
            }
             */
            /*foreach (var rectangle in rois)
            {
                await Task.Delay(500);
                origImage.Draw(rectangle, new Bgr(System.Drawing.Color.Red));

                //textBlock.Text = Result.Text;
            }*/
            //image.Source = ImageSourceFromBitmap(grayFrame.ToBitmap<Gray, Byte>());
        }
    }
}
