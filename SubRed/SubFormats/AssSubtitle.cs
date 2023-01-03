using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRed.Sub_formats
{
    static class AssSubtitle
    {
        private static string[] separator = { "&&" };
        private static string[] separatorForText = { "\\N", "\\n", "\n" };
        private static string[] textFormatSplit = { "Name", "Fontname", "Fontsize", "PrimaryColour", "SecondaryColour", "OutlineColour", "BackColour", "" };

        public static void Save(string filename, string sub)
        {
            sub = sub.Replace("OriginalScript:", "Original Script:");
            sub = sub.Replace("OriginalTranslation:", "Original Translation:");
            try
            {
                using (StreamWriter sw = new StreamWriter(filename, false, System.Text.Encoding.Default))
                {
                    //Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColor, 
                    //BackColour, Bold, Italic, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding
                    //Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, OutlineColour,
                    //BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding

                    string[] subSplit = sub.Split(separator, StringSplitOptions.None);

                    for (int i = 0; i < subSplit.Length; i++)
                    {
                        subSplit[i] = subSplit[i].Trim();
                        if (subSplit[i].Contains("StyleFormat:"))
                        {
                            textFormatSplit = subSplit[i].Replace("StyleFormat:", "").Trim().Split(',');
                            subSplit[i] = subSplit[i].Replace("StyleFormat:", "Format:");

                            try
                            {
                                string[] styleSplit = subSplit[i].Replace("Format:", "").Trim().Split(',');
                                styleSplit[Array.IndexOf(textFormatSplit, "AlphaLevel")] = "";

                                subSplit[i] = "Format: " + String.Join(",", styleSplit.Where(s => !string.IsNullOrEmpty(s)));
                            }
                            catch { }
                        }
                        else if (subSplit[i].Contains("TextFormat:"))
                        {
                            subSplit[i] = subSplit[i].Replace("TextFormat:", "Format:");
                        }
                        else if (subSplit[i].Contains("Style:") && !subSplit[i].Contains("WrapStyle:"))
                        {
                            try
                            {
                                string[] styleSplit = subSplit[i].Replace("Style:", "").Trim().Split(',');
                                styleSplit[Array.IndexOf(textFormatSplit, "AlphaLevel")] = "";

                                subSplit[i] = "Style: " + String.Join(",", styleSplit.Where(s => !string.IsNullOrEmpty(s)));
                            }
                            catch { }
                        }
                        else if (subSplit[i].Contains("Text:"))
                        {
                            subSplit[i] = subSplit[i].Replace("Text:", "Dialogue:");
                        }
                    }

                    sw.WriteLine("[Script Info]");
                    int n = 0;
                    for (int i = 0; i < subSplit.Length; i++)
                    {
                        if (subSplit[i].Contains("Format:"))
                        {
                            sw.WriteLine();
                            if (n == 0)
                                sw.WriteLine("[V4+ Styles]");
                            else
                                sw.WriteLine("[Events]");

                            sw.WriteLine(subSplit[i]);
                            n++;
                        }
                        else
                            sw.WriteLine(subSplit[i]);
                    }
                }
            }
            catch
            {

            }
        }

        public static string Load(string filename)
        {
            string sub = "";
            try
            {
                string line = "";
                System.IO.StreamReader file = new System.IO.StreamReader(filename);
                while ((line = file.ReadLine()) != null)
                {
                    if (line != "" && line != Environment.NewLine && line != "\n")
                        if (line != "[Script Info]" && line != "[V4+ Styles]" && line != "[Events]")
                            if (line[0] != ';')
                            {
                                string[] lineSplit = line.Split(new char[] { ':' }, 2);
                                lineSplit[0] = lineSplit[0].Trim();

                                sub = sub + lineSplit[0] + ":" + lineSplit[1];
                                sub += "&&";
                            }
                }

                string format = "Format";
                int i = sub.IndexOf(format);
                sub = sub.Remove(i, format.Length).Insert(i, "StyleFormat");
                i = sub.IndexOf(format, i + "StyleFormat".Length);
                sub = sub.Remove(i, format.Length).Insert(i, "TextFormat");

                file.Close();
            }
            catch
            {

            }

            return sub;
        }
    }
}
