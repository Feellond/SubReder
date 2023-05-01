using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SubRed.Sub_formats
{
    static class SmiSubtitle
    {
        public static void Save(string filename, SubProject project)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filename, false, System.Text.Encoding.Default))
                {
                    sw.WriteLine("<!-- Converted at SubRed -->");
                    sw.WriteLine("<SAMI>");
                    sw.WriteLine("<HEAD>");
                    sw.WriteLine("<TITLE></TITLE>");
                    sw.WriteLine("<STYLE TYPE='text/css'");
                    sw.WriteLine("</STYLE>");
                    sw.WriteLine("</HEAD>");
                    sw.WriteLine("<BODY>");
                    foreach (var sub in project.SubtitlesList)
                    {
                        sw.Write("<SYNC Start=" + sub.Start.TotalMilliseconds.ToString() + ">");
                        sw.Write("<P Class=krcc>");
                        sw.Write(sub.Text.Replace("\\N", "<br>"));
                    }
                    sw.WriteLine("</BODY>");
                    sw.WriteLine("</SAMI>");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения .smi формата субтитров\n" + ex.Message, "Ошибка загрузки .smi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static SubProject Load(string filename, SubProject project)
        {
            project.SubtitlesList = new List<Subtitle>();
            string line = "";
            try
            {
                //sub = "TextFormat: Start, Text&&";
                // Read the file and display it line by line.  
                System.IO.StreamReader file = new System.IO.StreamReader(filename);
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains("<SYNC"))
                    {
                        string time = line.Replace("<SYNC Start=", "").Replace(">", "").Replace("-", "");
                        /*if (time.Length <= 5)
                            time.Insert(time.Length - 3, ".");
                        if (time.Length <= 7)
                            time.Insert(time.Length - 5, ".");
                        if (time.Length > 8)
                            time.Insert(time.Length - 7, ".");*/
                        TimeSpan start = new TimeSpan(0, 0, 0, 0, int.Parse(time));

                        string text = file.ReadLine();
                        line.Replace("<P Class=", "");
                        line.Substring(line.IndexOf(">"), line.Length - line.IndexOf(">") - 1);
                        line.Replace("<br>", "\\N");
                        line.Replace("</P>", "");
                        text += line;

                        project.SubtitlesList.Add(new Subtitle {Text = text, Start = start});
                    }
                }
                file.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки .smi формата субтитров\n" + ex.Message, "Ошибка загрузки .smi", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return project;
        }
    }
}
