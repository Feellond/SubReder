using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRed
{
    class SubtitleStyle
    {
        public string Name { get; set; }
        public string Fontname { get; set; }
        public int Fontsize { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set;}
        public string OutlineColor { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public bool StrikeOut { get; set; }
        public int ScaleX { get; set; }
        public int ScaleY { get; set; }
        public int SpacingX { get; set; }

    }
}
