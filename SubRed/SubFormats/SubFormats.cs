using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRed.Sub_formats
{
    public static class SubFormats
    {
        public static async Task<SubProject> SelectFormat(string filename, SubProject project, bool isLoadFunction, string format = "")
        {
            string[] formatSplit = filename.Split('.');
            switch (formatSplit[formatSplit.Length - 1])
            {
                case "ass":
                    if (isLoadFunction)
                        project = AssSubtitle.Load(filename, project);
                    else
                        AssSubtitle.Save(filename, project);
                    break;
                case "ssa":
                    if (isLoadFunction)
                        project = SsaSubtitle.Load(filename, project);
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
                        project = SmiSubtitle.Load(filename, project);
                    else
                        SmiSubtitle.Save(filename, project);
                    break;
            }
            return project;
        }
    }
}
