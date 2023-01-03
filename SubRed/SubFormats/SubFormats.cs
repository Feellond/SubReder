using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRed.Sub_formats
{
    public static class SubFormats
    {

        public static string SelectFormat(string filename, List<Subtitle> subList, bool isLoadFunction)
        {
            string[] format = filename.Split('.');
            switch (format[format.Length - 1])
            {
                case "ass":
                    if (isLoadFunction)
                        return AssSubtitle.Load(filename);
                    else
                        //AssSubtitle.Save(filename, sub);
                    break;
                case "ssa":
                    if (isLoadFunction)
                        return SsaSubtitle.Load(filename);
                    else
                        //SsaSubtitle.Save(filename, sub);
                    break;
                case "srt":
                    if (isLoadFunction)
                        return SrtSubtitle.Load(filename);
                    else
                        SrtSubtitle.Save(filename, subList);
                    break;
                case "smi":
                    if (isLoadFunction)
                        return SmiSubtitle.Load(filename);
                    else
                        //SmiSubtitle.Save(filename, sub);
                    break;
                default:
                    return "";
            }
            return "";
        }
    }
}
