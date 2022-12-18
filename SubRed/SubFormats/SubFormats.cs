using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRed.Sub_formats
{
    public class SubFormats
    {
        AssSubtitle assSubtitle = new AssSubtitle();
        SsaSubtitle ssaSubtitle = new SsaSubtitle();
        SrtSubtitle srtSubtitle = new SrtSubtitle();
        SmiSubtitle smiSubtitle = new SmiSubtitle();

        public SubFormats()
        {
            assSubtitle = new AssSubtitle();
            ssaSubtitle = new SsaSubtitle();
            srtSubtitle = new SrtSubtitle();
            smiSubtitle = new SmiSubtitle();
        }

        public string SelectFormat(string filename, string sub, bool isLoadFunction)
        {
            string[] format = filename.Split('.');
            switch (format[format.Length - 1])
            {
                case "ass":
                    if (isLoadFunction)
                        return assSubtitle.Load(filename);
                    else
                        assSubtitle.Save(filename, sub);
                    break;
                case "ssa":
                    if (isLoadFunction)
                        return ssaSubtitle.Load(filename);
                    else
                        ssaSubtitle.Save(filename, sub);
                    break;
                case "srt":
                    if (isLoadFunction)
                        return srtSubtitle.Load(filename);
                    else
                        srtSubtitle.Save(filename, sub);
                    break;
                case "smi":
                    if (isLoadFunction)
                        return smiSubtitle.Load(filename);
                    else
                        smiSubtitle.Save(filename, sub);
                    break;
                default:
                    return "";
            }
            return "";
        }
    }
}
