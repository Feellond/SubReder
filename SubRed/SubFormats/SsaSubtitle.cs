using ControlzEx.Standard;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SubRed.Sub_formats
{
    static class SsaSubtitle
    {
        public static void Save(string filename, SubProject project)
        {
            try
            {
                using StreamWriter sw = new StreamWriter(filename, false, System.Text.Encoding.Default);
                sw.WriteLine("[Script Info]");
                sw.WriteLine("Title: " + project.Title);
                sw.WriteLine("OriginalScript: " + project.OriginalScript);
                sw.WriteLine("OriginalTranslation: " + project.OriginalTranslation);
                sw.WriteLine("OriginalEditing: " + project.OriginalEditing);
                sw.WriteLine("OriginalTiming: " + project.OriginalTiming);
                sw.WriteLine("SyncPoint: " + project.SyncPoint);
                sw.WriteLine("ScriptUpdatedBy: " + project.ScriptUpdatedBy);
                sw.WriteLine("UpdateDetails: " + project.UpdateDetails);
                sw.WriteLine("ScriptType: " + project.ScriptType);
                sw.WriteLine("Collisions: " + project.Collisions);
                sw.WriteLine("PlayResX: " + project.PlayResX);
                sw.WriteLine("PlayResY: " + project.PlayResY);
                sw.WriteLine("PlayDepth: " + project.PlayDepth);
                sw.WriteLine("Timer: " + project.Timer);
                sw.WriteLine("Wav: " + project.Wav);
                sw.WriteLine("LastWav: " + project.LastWav);
                sw.WriteLine("WrapStyle: " + project.WrapStyle);

                sw.WriteLine("[V4 Styles]");
                sw.WriteLine("Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour," +
                    "BackColour, Bold, Italic, BorderStyle, Outline, Shadow, Alignment," +
                    " MarginL, MarginR, MarginV, AlphaLevel, Encoding");
                foreach (var style in project.SubtitleStyleList)
                {
                    sw.Write("Style: ");
                    sw.Write(style.Name + ",");
                    sw.Write(style.Fontname + ",");
                    sw.Write(style.Fontsize.ToString() + ",");
                    sw.Write(style.PrimaryColor + ",");
                    sw.Write(style.SecondaryColor + ",");
                    sw.Write(style.OutlineColor + ",");
                    sw.Write(style.BackColor + ",");
                    sw.Write(style.Bold ? -1 : 0 + ",");
                    sw.Write(style.Italic ? -1 : 0 + ",");
                    sw.Write(style.BorderStyle.ToString() + ",");
                    sw.Write(style.Outline.ToString().Replace(',', '.') + ",");
                    sw.Write(style.Shadow.ToString().Replace(',', '.') + ",");

                    int? alignmentNumber = style.HorizontalAlignment;
                    if (style.VerticalAlignment == 1)
                        alignmentNumber += 4;
                    else if (style.VerticalAlignment == 2)
                        alignmentNumber += 8;

                    sw.Write(alignmentNumber.ToString() + ",");
                    sw.Write(style.MarginL.ToString() + ",");
                    sw.Write(style.MarginR.ToString() + ",");
                    sw.Write(style.MarginV.ToString() + ",");
                    sw.Write(style.AlphaLevel.ToString() + ",");
                    sw.Write(style.Encoding.ToString() + ",");
                    sw.WriteLine();
                }

                sw.WriteLine("[Events]");
                sw.WriteLine("Format: Marked, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");
                foreach (var dialogue in project.SubtitlesList)
                {
                    sw.Write("Dialogue: ");
                    sw.Write(dialogue.Marked + ",");
                    sw.Write(dialogue.Start.ToString("hh\\:mm\\:ss\\.ff") + ",");
                    sw.Write(dialogue.End.ToString("hh\\:mm\\:ss\\.ff") + ",");
                    sw.Write(dialogue.Style.Name + ",");
                    sw.Write(dialogue.Name + ",");
                    sw.Write(dialogue.Style.MarginL.ToString() + ",");
                    sw.Write(dialogue.Style.MarginR.ToString() + ",");
                    sw.Write(dialogue.Style.MarginV.ToString() + ",");
                    sw.Write(dialogue.Effect.ToString() + ",");
                    sw.Write(dialogue.Text + ",");
                    sw.WriteLine();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения .ass формата субтитров\n" + ex.Message, "Ошибка сохранения .ass", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public static SubProject Load(string filename, SubProject project)
        {
            project = new SubProject();
            project.SubtitleStyleList = new List<SubtitleStyle>();
            project.SubtitlesList = new List<Subtitle>();
            int numSubs = 1;
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

                                switch (lineSplit[0])
                                {
                                    #region Данные проекта [Script info]
                                    case "Title":
                                        project.Title = lineSplit[1];
                                        break;
                                    case "OriginalScript":
                                        project.OriginalScript = lineSplit[1];
                                        break;
                                    case "OriginalTranslation":
                                        project.OriginalTranslation = lineSplit[1];
                                        break;
                                    case "OriginalEditing":
                                        project.OriginalEditing = lineSplit[1];
                                        break;
                                    case "OriginalTiming":
                                        project.OriginalTiming = lineSplit[1];
                                        break;
                                    case "SyncPoint":
                                        project.SyncPoint = lineSplit[1];
                                        break;
                                    case "ScriptUpdatedBy":
                                        project.ScriptUpdatedBy = lineSplit[1];
                                        break;
                                    case "UpdateDetails":
                                        project.UpdateDetails = lineSplit[1];
                                        break;
                                    case "ScriptType":
                                        project.ScriptType = lineSplit[1];
                                        break;
                                    case "Collisions":
                                        project.Collisions = lineSplit[1];
                                        break;
                                    case "PlayResX":
                                        project.PlayResX = lineSplit[1];
                                        break;
                                    case "PlayResY":
                                        project.PlayResY = lineSplit[1];
                                        break;
                                    case "PlayDepth":
                                        project.PlayDepth = lineSplit[1];
                                        break;
                                    case "Timer":
                                        project.Timer = lineSplit[1];
                                        break;
                                    case "WrapStyle":
                                        project.WrapStyle = lineSplit[1];
                                        break;
                                    case "Wav":
                                        project.Wav = lineSplit[1];
                                        break;
                                    case "LastWav":
                                        project.LastWav = lineSplit[1];
                                        break;
                                    #endregion
                                    #region Стили [V4+ Styles]
                                    case "Style":
                                        var splitStyle = lineSplit[1].Split(',');

                                        int alignmentNumber = Convert.ToInt32(splitStyle[11]);
                                        int horizontalNumber = 0, verticalNumber = 0;
                                        if (alignmentNumber > 8)
                                        {
                                            alignmentNumber -= 8;
                                            verticalNumber = 2;
                                        }
                                        else if (alignmentNumber > 5)
                                        {
                                            alignmentNumber -= 5;
                                            verticalNumber = 1;

                                        }
                                        else verticalNumber = 3;
                                        horizontalNumber = alignmentNumber;

                                        var style = new SubtitleStyle();
                                        style.Name = String.Join(" ", splitStyle[0].Split(" ", StringSplitOptions.RemoveEmptyEntries));
                                        style.Fontname = String.Join(" ", splitStyle[1].Split(" ", StringSplitOptions.RemoveEmptyEntries));
                                        style.Fontsize = Convert.ToInt32(String.Join(" ", splitStyle[2].Split(" ", StringSplitOptions.RemoveEmptyEntries)));
                                        style.PrimaryColor = String.Join(" ", splitStyle[3].Split(" ", StringSplitOptions.RemoveEmptyEntries));
                                        style.SecondaryColor = String.Join(" ", splitStyle[4].Split(" ", StringSplitOptions.RemoveEmptyEntries));
                                        style.BackColor = String.Join(" ", splitStyle[5].Split(" ", StringSplitOptions.RemoveEmptyEntries));
                                        style.Bold = String.Join(" ", splitStyle[6].Split(" ", StringSplitOptions.RemoveEmptyEntries)) == "0" ? false : true;
                                        style.Italic = String.Join(" ", splitStyle[7].Split(" ", StringSplitOptions.RemoveEmptyEntries)) == "0" ? false : true;
                                        style.BorderStyle = Convert.ToInt32(String.Join(" ", splitStyle[8].Split(" ", StringSplitOptions.RemoveEmptyEntries)));
                                        style.OutlineColor = String.Join(" ", splitStyle[9].Split(" ", StringSplitOptions.RemoveEmptyEntries));
                                        style.Shadow = Convert.ToDouble(String.Join(" ", splitStyle[10].Split(" ", StringSplitOptions.RemoveEmptyEntries)));
                                        style.HorizontalAlignment = horizontalNumber;
                                        style.VerticalAlignment = verticalNumber;
                                        style.MarginL = Convert.ToInt32(String.Join(" ", splitStyle[12].Split(" ", StringSplitOptions.RemoveEmptyEntries)));
                                        style.MarginR = Convert.ToInt32(String.Join(" ", splitStyle[13].Split(" ", StringSplitOptions.RemoveEmptyEntries)));
                                        style.MarginV = Convert.ToInt32(String.Join(" ", splitStyle[14].Split(" ", StringSplitOptions.RemoveEmptyEntries)));
                                        style.AlphaLevel = Convert.ToInt32(String.Join(" ", splitStyle[15].Split(" ", StringSplitOptions.RemoveEmptyEntries)));
                                        style.Encoding = String.Join(" ", splitStyle[16].Split(" ", StringSplitOptions.RemoveEmptyEntries));

                                        project.SubtitleStyleList.Add(style);
                                        break;
                                    #endregion
                                    #region Субтитры [Events]
                                    case "Dialogue":
                                        var splitDialogue = lineSplit[1].Split(',');
                                        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                                        project.SubtitlesList.Add(new Subtitle
                                        {
                                            Id = numSubs,
                                            Marked = Convert.ToInt32(splitDialogue[0]),
                                            Start = TimeSpan.Parse(splitDialogue[1]),   //00:00:00.00
                                            End = TimeSpan.Parse(splitDialogue[2]),
                                            Style = project.SubtitleStyleList.Find(x => x.Name == splitDialogue[3]) ?? new SubtitleStyle(),
                                            Name = splitDialogue[4],
                                            Effect = splitDialogue[8],
                                            Text = splitDialogue[9]
                                        });
                                        break;
                                        #endregion
                                }
                            }
                }
                file.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки .ass формата субтитров\n" + ex.Message, "Ошибка загрузки .ass", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return project;
        }
    }
}
