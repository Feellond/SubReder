using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRed.Sub_formats
{
    public static class SubFormats
    {

        public static string SelectFormat(string filename, SubProject project, bool isLoadFunction, string format = "")
        {
            string[] formatSplit = filename.Split('.');
            switch (formatSplit[formatSplit.Length - 1])
            {
                case "ass":
                    if (isLoadFunction)
                        AssSubtitle.Load(filename, project);
                    else
                        AssSubtitle.Save(filename, project);
                    break;
                case "ssa":
                    if (isLoadFunction)
                        SsaSubtitle.Load(filename, project);
                    else
                        SsaSubtitle.Save(filename, project);
                    break;
                case "srt":
                    if (isLoadFunction)
                        SrtSubtitle.Load(filename, project);
                    else
                        SrtSubtitle.Save(filename, project);
                    break;
                case "smi":
                    if (isLoadFunction)
                        SmiSubtitle.Load(filename, project);
                    else
                        SmiSubtitle.Save(filename, project);
                    break;
                default:
                    return "";
            }
            return "";
        }
    }
}
