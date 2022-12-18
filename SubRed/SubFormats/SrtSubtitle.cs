using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRed.Sub_formats
{
    class SrtSubtitle
    {
        private string[] separator = { "&&" };
        private string[] separatorForText = { "\\N", "\\n", "\n" };
        private string[] textFormatSplit = { "Start", "End", "Text" };

        public void Save(string filename, string sub)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(filename, false, System.Text.Encoding.Default))
                {
                    int num = 0;
                   
                    string[] subSplit = sub.Split(separator, StringSplitOptions.None);
                    for (int i = 0; i < subSplit.Length; i++)
                    {
                        if (subSplit[i].Contains("TextFormat:"))    // TextFormat: Start, End, Text, ...
                        {
                            textFormatSplit = subSplit[i].Replace("TextFormat:", "").Trim().Split(',');    // Start, End, Text, ...
                        }
                        else if (subSplit[i].Contains("Dialogue:"))
                        {
                            num++;
                            string[] dialogueSplit = subSplit[i].Replace("Dialogue:", "").Trim().Split(',');
                            sw.WriteLine(num);

                            sw.Write(dialogueSplit[Array.IndexOf(textFormatSplit, "Start")]);
                            sw.Write(" --> ");
                            sw.WriteLine(dialogueSplit[Array.IndexOf(textFormatSplit, "End")]);

                            string[] textSplit = dialogueSplit[Array.IndexOf(textFormatSplit, "Text")].Split(separatorForText, StringSplitOptions.None);
                            foreach (string str in textSplit)
                                sw.WriteLine(str);
                        }
                    }

                    
                }
            }
            catch
            {

            }
        }

        public string Load(string filename)
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
