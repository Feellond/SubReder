using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu;
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.Dnn;
using Emgu.CV.Util;
using System.Drawing;
using ControlzEx.Standard;

namespace SubRed
{
    class EastDetector
    {
        public Net net;

        public EastDetector()
        {
            string model = "frozen_east_text_detection.pb";
            net = DnnInvoke.ReadNet(model);
        }
        public void EastDetect(Mat inputFrame)
        {
            double confThreshold = 0.5;
            double nmsThreshold = 0.4;
            int inpWidth = 320;
            int inpHeight = 320;
            
            VectorOfMat outs = new VectorOfMat();
            string[] outNames = new string[2];
            outNames[0] = "feature_fusion/Conv_7/Sigmoid";
            outNames[1] = "feature_fusion/concat_3";

            Mat frame, blob;
            frame = inputFrame;

            blob = DnnInvoke.BlobFromImage(frame, 1, new System.Drawing.Size(inpWidth, inpHeight), new MCvScalar(123.68, 116.78, 103.94), true, false);
            net.SetInput(blob);
            net.Forward(outs, outNames);
            Mat scores = outs[0];
            Mat geometry = outs[1];

            VectorOfRect boxes = new VectorOfRect();
            VectorOfFloat confidences = new VectorOfFloat();

            DecodeBox(scores, geometry, confThreshold, boxes, confidences);

            VectorOfInt indices = new VectorOfInt();
            DnnInvoke.NMSBoxes(boxes, confidences, (float)confThreshold, (float)nmsThreshold, indices);

            PointF ratio = new PointF((float)((float)frame.Cols / (float)inpWidth), (float)((float)frame.Rows / (float)inpHeight));
            for (int i = 0; i < indices.Size; i++)
            {
                Rectangle box = boxes[indices[i]];

                var vertices_x = box.X * ratio.X;
                var vertices_y = box.Y * ratio.Y;
                var vertices_width = box.Width * ratio.X;
                var vertices_height = box.Height * ratio.Y;

                var p_x = vertices_x - 0.5 * vertices_width;
                var p_y = vertices_y - 0.5 * vertices_height;

                Rectangle box_in = new Rectangle(new Point((int)(p_x), (int)(p_y)), new Size((int)vertices_width, (int)vertices_height));
                CvInvoke.Rectangle(frame, box_in, new MCvScalar(255, 255, 0), 4);
            }

            CvInvoke.Resize(frame, frame, new Size(1024, 720));
            CvInvoke.Imshow("result", frame);
        }

        public void DecodeBox(Mat scores, Mat geometry, double scoreThresh, VectorOfRect detections, VectorOfFloat confidences)
        {
            if (detections.Size > 0)
                detections.Clear();

            CvException.Equals(scores.Dims, 4);
            CvException.Equals(geometry.Dims, 4);

            var s = scores.SizeOfDimension;
            var sData = scores.GetData();
            var gData = geometry.GetData();

            List<RotatedRect> t_detections = new List<RotatedRect>();
            List<float> t_confidences = new List<float>();
            List<Rectangle> r_detections = new List<Rectangle>();

            for (int y = 0; y < s[2]; ++y)
            {
                for (int x = 0; x < s[3]; x++) 
                {
                    float score = (float)sData.GetValue(0, 0, y, x);
                    if (score < scoreThresh)
                    {
                        continue;
                    }

                    float offsetX = x * 4.0f, offsetY = y * 4.0f;
                    float angle = (float)gData.GetValue(0, 4, y, x);
                    float cosA = (float)Math.Cos(angle);
                    float sinA = (float)Math.Sin(angle);
                    float h = (float)gData.GetValue(0, 0, y, x) + (float)gData.GetValue(0, 2, y, x);
                    float w = (float)gData.GetValue(0, 1, y, x) + (float)(gData.GetValue(0, 3, y, x));

                    PointF offset = new PointF(offsetX + cosA * (float)gData.GetValue(0, 1, y, x) + sinA * (float)gData.GetValue(0, 2, y, x),
                        offsetY - sinA * (float)gData.GetValue(0, 1, y, x) + cosA * (float)gData.GetValue(0, 2, y, x));

                    PointF p1 = new PointF();
                    p1.X = -sinA * h + offset.X;
                    p1.Y = -cosA * h + offset.Y;

                    PointF p3 = new PointF();
                    p3.X = -cosA * w + offset.X;
                    p3.Y = sinA * w + offset.Y;

                    PointF center = new PointF();
                    center.X = (float)0.5 * (p1.X + p3.X);
                    center.Y = (float)0.5 * (p1.Y + p3.Y);

                    SizeF size = new SizeF(w, h);
                    float box_angle = -angle * 180.0f / (float)Math.PI;


                    RotatedRect r = new RotatedRect(center, size, box_angle);
                    Point i_center = new Point((int)(center.X), (int)(center.Y));
                    Size i_size = new Size((int)w, (int)h);

                    Rectangle r_d = new Rectangle(i_center, i_size);

                    r_detections.Add(r_d);
                    t_detections.Add(r);
                    t_confidences.Add(score);
                }
            }

            Rectangle[] k_detections = r_detections.ToArray();
            detections.Push(k_detections);

            float[] k_confidences = t_confidences.ToArray();
            confidences.Push(k_confidences);
        }
    }
}
