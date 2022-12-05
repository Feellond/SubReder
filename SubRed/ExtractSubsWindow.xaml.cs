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
using Microsoft.Win32;
using static System.Net.Mime.MediaTypeNames;

namespace SubRed
{
    /// <summary>
    /// Логика взаимодействия для ExtractSubsWindow.xaml
    /// </summary>
    public partial class ExtractSubsWindow : Window
    {
        public string filePath { get; set; }
        public ExtractSubsWindow()
        {
            InitializeComponent();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
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
            }
            /*
             OpenFileDialog openFileDialog1 = new OpenFileDialog();
           
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                { 
                    _capture = null;
                    _capture = new Capture(openFileDialog1.FileName);

                    FrameRate = _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FPS);
                    TotalFrames = _capture.GetCaptureProperty(Emgu.CV.CvEnum.CAP_PROP.CV_CAP_PROP_FRAME_COUNT);
                    Application.Idle += ProcessFrame ;
                    IsPlaying = true;
                }
                catch (NullReferenceException excpt)
                {
                    MessageBox.Show(excpt.Message);
                }
            }
             
             */
        }

        /*private List<Image<Bgr, Byte>> GetVideoFrames(int Time_millisecounds)
        {
            List<Image<Bgr, Byte>> image_array = new List<Image<Bgr, Byte>>();
            System.Diagnostics.Stopwatch SW = new System.Diagnostics.Stopwatch();

            bool Reading = true;
            VideoCapture _capture = new VideoCapture(fileName);
            SW.Start();

            while (Reading)
            {
                Image<Bgr, Byte> frame = _capture.QueryFrame();
                if (frame != null)
                {
                    image_array.Add(frame.Copy());
                    if (SW.ElapsedMilliseconds >= Time_millisecounds) Reading = false;
                }
                else
                {
                    Reading = false;
                }
            }

            return image_array;
        }*/

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

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            Mat img = CvInvoke.Imread(filePath);
            Image<Gray, Byte> grayFrame = img.ToImage<Gray, Byte>();

            //Emgu.CV.Util.VectorOfVectorOfPoint countouts = new Emgu.CV.Util.VectorOfVectorOfPoint();
            //Mat heir = new Mat();
            //CvInvoke.FindContours(grayFrame, countouts, heir, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

            //CvInvoke.GaussianBlur(grayFrame, grayFrame, new System.Drawing.Size(5, 5), 1.5);
            grayFrame = grayFrame.SmoothMedian(3);
            grayFrame = grayFrame.SmoothGaussian(3);

            CvInvoke.Threshold(grayFrame, grayFrame, 150, 255, ThresholdType.ToZero);

            grayFrame = grayFrame.InRange(new Gray(1), new Gray(255));

            Mat kernel1 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new System.Drawing.Size(3, 3), new System.Drawing.Point(1, 1));
            grayFrame = grayFrame.MorphologyEx(MorphOp.Gradient, kernel1, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());

            grayFrame = grayFrame.Dilate(1);
            grayFrame = grayFrame.Erode(1);

            image.Source = ImageSourceFromBitmap(grayFrame.ToBitmap<Gray, Byte>());
        }
    }
}
