using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tesseract;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

namespace SubRed
{
    static class SubtitleOCR
    {
        public static string ocrLanguage = "eng";

        public static int imH;
        public static int imW;
        public static double minHeight;
        public static double minWidth;
        public static int k1;
        public static int k2;

        public static int gausSeed = 3;
        public static int meanSeed = 3;
        public static int laplaceSeed = 3;
        public static int thresholdLaplace = 170;
        public static int morphSize = 5;

        public static int dilateHeight = 30;
        public static int dilateWidth = 70;
        public static int erodeHeight = 20;
        public static int erodeWidth = 65;

        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
        public static ImageSource ImageSourceFromBitmap(Bitmap bmp)
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

        public static void OCRLanguageChange(string language)
        {
            switch (language)
            {
                case "Английский":
                case "English":
                    ocrLanguage = "eng";
                    break;
                case "Русский":
                case "Russian":
                    ocrLanguage = "rus";
                    break;
                default:
                    ocrLanguage = "eng";
                    break;
            }
        }

        public static void ReturnDefaultValues()
        {
            ocrLanguage = "eng";

            gausSeed = 3;
            meanSeed = 3;
            laplaceSeed = 3;
            thresholdLaplace = 170;
            morphSize = 5;

            dilateHeight = 30;
            dilateWidth = 70;
            erodeHeight = 20;
            erodeWidth = 65;
    }

        public static List<System.Drawing.Rectangle> FindRegions(Mat img)
        {
            Image<Gray, Byte> grayFrame = img.ToImage<Gray, Byte>();

            imH = grayFrame.Height;
            imW = grayFrame.Width;
            minHeight = 0.01 * imH;
            minWidth = 0.01 * imW;
            //var maxHeight = 100;

            grayFrame = grayFrame.SmoothGaussian(gausSeed);
            grayFrame = grayFrame.SmoothMedian(meanSeed);

            var laplacian = grayFrame.Laplace(laplaceSeed);
            CvInvoke.ConvertScaleAbs(laplacian, grayFrame, 1, 0);

            CvInvoke.Threshold(grayFrame, grayFrame, thresholdLaplace, 255, ThresholdType.Binary);

            Mat kernel1 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new System.Drawing.Size(morphSize, morphSize), new System.Drawing.Point(1, 1));
            grayFrame = grayFrame.MorphologyEx(MorphOp.Gradient, kernel1, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());

            k1 = imH / dilateHeight;
            k2 = imW / dilateWidth;
            kernel1 = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Cross, new System.Drawing.Size(k1, k2), new System.Drawing.Point(-1, -1));
            CvInvoke.Dilate(grayFrame, grayFrame, kernel1, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());


            k1 = imH / erodeHeight;
            k2 = imW / erodeWidth;
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
        public static string GetRegionsTextTesseract(Image<Gray, Byte> frameRegion, string n = "")
        {
            Image<Gray, Byte> tempPartImage = frameRegion.Clone();
            tempPartImage = tempPartImage.SmoothMedian(meanSeed);
            tempPartImage = tempPartImage.SmoothGaussian(gausSeed);

            int k1 = 2;
            int k2 = 2;
            Mat kernel = CvInvoke.GetStructuringElement(Emgu.CV.CvEnum.ElementShape.Rectangle, new System.Drawing.Size(k1, k2), new System.Drawing.Point(-1, -1));

            float[,] matrix = new float[3, 3] {
                { 0, -1, 0},
                { -1, 4, -1},
                { 0, -1, 0}
            };
            ConvolutionKernelF matrixKernel = new ConvolutionKernelF(matrix);

            //CvInvoke.DestroyAllWindows();

            //Mat kernel = new Mat(*size, DepthType.Default, *data);
            //CvInvoke.Laplacian(tempPartImage, tempPartImage, DepthType.Default, 3);

            //CvInvoke.Imshow("outputGray", tempPartImage);
            //var laplace = tempPartImage.Laplace(3);
            //CvInvoke.ConvertScaleAbs(laplace, tempLaplace, 1, 0);

            CvInvoke.Filter2D(tempPartImage, tempPartImage, matrixKernel, new System.Drawing.Point(-1, -1));
            //CvInvoke.Imshow("filter2d" + n, tempPartImage);
            CvInvoke.Dilate(tempPartImage, tempPartImage, kernel, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            //CvInvoke.Imshow("dilate" + n, tempPartImage);
            CvInvoke.Threshold(tempPartImage, tempPartImage, 20, 255, ThresholdType.Binary);
            //CvInvoke.AdaptiveThreshold(tempPartImage, tempPartImage, 255, AdaptiveThresholdType.MeanC, ThresholdType.Binary, 7, 10);
            //CvInvoke.Imshow("thresh" + n, tempPartImage);
            //CvInvoke.Dilate(tempPartImage, tempPartImage, kernel, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            tempPartImage = tempPartImage.Not();
            //CvInvoke.Imshow("not" + n, tempPartImage);
            //CvInvoke.AdaptiveThreshold(tempLaplace, tempPartImage, 255, AdaptiveThresholdType.MeanC, ThresholdType.Binary, 7, 20);
            //

            //Mat white = Mat.Ones(tempLaplace.Rows, tempLaplace.Cols, DepthType.Default, 1);
            //Mat dst = new Mat();
            //CvInvoke.AbsDiff(white, tempLaplace.Mat, dst);

            //tempPartImage = tempPartImage.Not();
            //CvInvoke.Imshow("outputNOT", tempPartImage);
            //CvInvoke.AbsDiff(tempPartImage, tempLaplace, tempPartImage);
            //CvInvoke.Imshow("outputABSDiff", tempPartImage);

            //CvInvoke.Dilate(tempPartImage, tempPartImage, kernel, new System.Drawing.Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            //CvInvoke.Imshow("outputDilate", tempPartImage);

            /*tempPartImage = tempPartImage.SmoothGaussian(3);
            CvInvoke.Threshold(tempPartImage, tempPartImage, 10, 1, ThresholdType.Binary);
            //CvInvoke.Imshow("output??", tempPartImage);
            CvInvoke.Multiply(tempPartImage, tempLaplace, tempPartImage, 100);
            CvInvoke.Imshow("output??Mul", tempPartImage);*/

            //CvInvoke.Threshold(tempPartImage, tempPartImage, 0, 255, ThresholdType.Binary);
            //CvInvoke.Imshow("output??THRESH", tempPartImage);

            //image.Source = ImageSourceFromBitmap(tempPartImage.ToBitmap<Gray, Byte>());
            //CvInvoke.Imshow("outputThresh", tempPartImage);

            /*string path = @".\tessdata\" + ocrLanguage;
            var _ocr = new Emgu.CV.OCR.Tesseract(path, ocrLanguage, OcrEngineMode.Default);
            _ocr.SetImage(tempPartImage);
            _ocr.Recognize();
            var returnText = _ocr.GetUTF8Text();*/

            var ocrengine = new TesseractEngine(@".\tessdata\" + ocrLanguage, ocrLanguage, EngineMode.TesseractAndLstm);
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
