using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SubRed
{
    public class Subtitle
    {
        public int id { get; set; }
        public TimeSpan start { get; set; }
        public TimeSpan end { get; set; }
        public TimeSpan duration { get; set; }
        public int frameBeginNum { get; set; }
        public int frameEndNum { get; set; }
        public System.Drawing.Rectangle frameRegion { get; set; }
        public int xCoord { get; set; }
        public int yCoord { get; set; }
        public Bitmap frameImage { get; set; }
        public string text { get; set; }
        public Subtitle()
        {
            start = TimeSpan.Zero;
            end = TimeSpan.Zero;
            duration = TimeSpan.Zero;
            text = string.Empty;
        }
    }
}
