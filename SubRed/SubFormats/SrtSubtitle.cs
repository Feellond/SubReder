using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SubRed.Sub_formats
{
    static class SrtSubtitle
    {
        public static void Save(string filename, SubProject project)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filename, false, System.Text.Encoding.Default))
                {
                    int num = 0;
                    foreach (Subtitle sub in project.SubtitlesList)
                    {
                        num++;
                        sw.WriteLine(num);

                        sw.Write(sub.Start.ToString());
                        sw.Write(" --> ");
                        sw.WriteLine(sub.End.ToString());

                        sw.WriteLine(sub.Text);
                        sw.WriteLine();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n Error in SrtSubtitle Save method", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static void Load(string filename, SubProject project)
        {
            try
            {
                string line = "";
                string[] separator = { "-->" };

                // Read the file and display it line by line.  
                System.IO.StreamReader file = new System.IO.StreamReader(filename);
                project.SubtitlesList = new List<Subtitle>();
                while ((line = file.ReadLine()) != null)
                {
                    Subtitle sub = new Subtitle();
                    if (int.TryParse(line, out int n))
                    {
                        // запись времени начала и конца
                        string[] time;
                        line = file.ReadLine(); // чтение времени
                        time = line.Split(separator, StringSplitOptions.None);

                        try { sub.Start = TimeSpan.ParseExact(time[0].Trim().Replace(",", "."), @"hh\:mm\:ss\.ffff", System.Globalization.CultureInfo.InvariantCulture); }
                        catch { sub.Start = TimeSpan.Parse(time[0].Trim().Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture); }
                        try { sub.End = TimeSpan.ParseExact(time[1].Trim().Replace(",", "."), @"hh\:mm\:ss\.ffff", System.Globalization.CultureInfo.InvariantCulture); }
                        catch { sub.End = TimeSpan.Parse(time[1].Trim().Replace(",", "."), System.Globalization.CultureInfo.InvariantCulture); }

                        line = file.ReadLine();
                        sub.Text = "";
                        while (true)
                        {
                            sub.Text += line;   // запись первой строки
                            line = file.ReadLine(); // берем вторую строку
                            if (line != null && line != "" && line != Environment.NewLine) // если не конец строки
                                sub.Text += "\\N";
                            else
                                break;  // иначе выходим из цикла
                        }
                        project.SubtitlesList.Add(sub);
                    }

                }

                file.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка чтения .srt формата\n" + ex.Message, "Ошибка чтения", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}
