using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRed
{
    internal class Subtitle
    {
        public TimeSpan start { get; set; }
        public TimeSpan end { get; set; }
        public TimeSpan timeStamp { get; set; }
        public string text { get; set; }
        public int frameBeginNum { get; set; }
        public System.Drawing.Rectangle frameRegion { get; set; }
        public Bitmap frameImage { get; set; }

        public Subtitle()
        {
            start = TimeSpan.Zero;
            end = TimeSpan.Zero;
            timeStamp = TimeSpan.Zero;
            text = string.Empty;
        }
    }
}
