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

namespace SubRed
{
    static class EastDetector
    {
        public static void EastDetect()
        {
            double confThreshold = 0.5;
            double nmsThreshold = 0.4;
            int inpWidth = 320;
            int inpHeight = 320;
            string model = "frozen_east_text_detector.pb";

            Net net = DnnInvoke.ReadNet(model);
            CvInvoke.NamedWindow("EAST");

            VectorOfMat outs = new VectorOfMat();
        }
        
    }
}
