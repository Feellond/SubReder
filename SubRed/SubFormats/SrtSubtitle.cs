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
        private static string[] separator = { "&&" };
        private static string[] separatorForText = { "\\N", "\\n", "\n" };
        private static string[] textFormatSplit = { "Start", "End", "Text" };

        public static void Save(string filename, List<Subtitle> subList)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filename, false, System.Text.Encoding.Default))
                {
                    int num = 0;
                    foreach (Subtitle sub in subList)
                    {
                        num++;
                        sw.WriteLine(num);

                        sw.Write(sub.Start.ToString());
                        sw.Write(" --> ");
                        sw.WriteLine(sub.End.ToString());

                        sw.WriteLine(sub.Text);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\n\n Error in SrtSubtitle Save method", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static string Load(string filename)
        {
            string sub = "";
            try
            {
                string line = "";
                string[] separator = { "-->" };
                int n;

                sub = "TextFormat: Start, End, Text&&";
                // Read the file and display it line by line.  
                System.IO.StreamReader file = new System.IO.StreamReader(filename);
                while ((line = file.ReadLine()) != null)
                {
                    sub = sub + "Dialogue: ";
                    if (int.TryParse(line, out n))
                    {
                        // запись времени начала и конца
                        string[] time;
                        line = file.ReadLine(); // чтение времени
                        time = line.Split(separator, StringSplitOptions.None);
                        sub = sub + time[0].Trim().Replace(",", ".") + ", ";  // начало
                        sub = sub + time[1].Trim().Replace(",", ".") + ", ";  // конец

                        line = file.ReadLine();
                        while (true)
                        {
                            sub = sub + line;   // запись первой строки
                            line = file.ReadLine(); // берем вторую строку
                            if (line != null && line != "" && line != Environment.NewLine) // если не конец строки
                                sub += "\\N";
                            else
                                break;  // иначе выходим из цикла
                        }
                    }
                    sub = sub + "&&";
                }

                file.Close();
            }
            catch
            {
                
            }

            return sub;
        }

    }
}
