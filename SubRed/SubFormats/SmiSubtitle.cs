using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRed.Sub_formats
{
    class SmiSubtitle
    {
        private string[] separator = { "&&" };
        private string[] separatorForText = { "\\N", "\\n", "\n" };
        private string[] textFormatSplit = { "Start", "Text" };

        public void Save(string filename, string sub)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filename, false, System.Text.Encoding.Default))
                {
                    
                    string[] subSplit = sub.Split(separator, StringSplitOptions.None);

                    sw.WriteLine("<!-- Converted at SubRed -->");
                    sw.WriteLine("<SAMI>");
                    sw.WriteLine("<HEAD>");
                    sw.WriteLine("<TITLE></TITLE>");
                    sw.WriteLine("<STYLE TYPE='text/css'");
                    sw.WriteLine("</STYLE>");
                    sw.WriteLine("</HEAD>");
                    sw.WriteLine("<BODY>");
                    for (int i = 0; i < subSplit.Length; i++)
                    {
                        if (subSplit[i].Contains("TextFormat:"))    // TextFormat: Start, End, Text, ...
                        {
                            textFormatSplit = subSplit[i].Replace("TextFormat:", "").Trim().Split(',');    // Start, End, Text, ...
                        }
                        else if (subSplit[i].Contains("Dialogue:"))
                        {
                            string[] dialogueSplit = subSplit[i].Replace("Dialogue:", "").Trim().Split(',');

                            sw.Write("<SYNC Start=" + dialogueSplit[Array.IndexOf(textFormatSplit, "Start")].Replace(":", "").Replace(",", "") + ">");
                            sw.Write("<P Class=krcc>");

                            string[] textSplit = dialogueSplit[Array.IndexOf(textFormatSplit, "Text")].Split(separatorForText, StringSplitOptions.None);
                            for (int j = 0; j < textSplit.Length; j++)
                            {
                                sw.Write(textSplit[j]);
                                if (j + 1 < textSplit.Length)
                                {
                                    sw.WriteLine("<br>");
                                    sw.WriteLine();
                                }
                            }
                        }
                    }

                    sw.WriteLine("</BODY>");
                    sw.WriteLine("</SAMI>");
                }
            }
            catch { }
        }

        public string Load(string filename)
        {
            string sub = "";
            string line = "";
            try
            {
                sub = "TextFormat: Start, Text&&";
                // Read the file and display it line by line.  
                System.IO.StreamReader file = new System.IO.StreamReader(filename);
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("<TITLE>"))
                    {
                        sub += "Title: " + line.Replace("<TITLE>", "").Replace("</TITLE>", "") + "&&";
                    }
                    else if (line.Contains("<SYNC"))
                    {
                        string time = line.Remove(0, 12).Replace(">", "").Replace("-", "");
                        if (time.Length <= 5)
                            time.Insert(time.Length - 3, ".");
                        if (time.Length <= 7)
                            time.Insert(time.Length - 5, ".");
                        if (time.Length > 8)
                            time.Insert(time.Length - 7, ".");
                        sub += "Dialogue: " + time + ",";

                        line = file.ReadLine();
                        line.Replace("<P Class=krcc>", "");
                        while (!(line.Contains("</P>")))
                        {
                            line.Replace("<br>", "\\N");
                            sub += line;
                            line = file.ReadLine();
                        }
                        line.Replace("</P>", "");
                        sub += line;
                    }
                }
                file.Close();
            }
            catch { }

            return sub;
        }
    }
}
