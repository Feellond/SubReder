using Aspose.Words;
using Emgu.CV.CvEnum;
using Emgu.CV.Stitching;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Text;
using System.Windows.Documents;
using System.Xml.Linq;

namespace SubRed.Sub_formats
{
    static class AssSubtitle
    {
        public static void Save(string filename, SubProject project)
        {
            try
            {
                using StreamWriter sw = new StreamWriter(filename, false, System.Text.Encoding.Default);
                sw.WriteLine("[Script Info]");
                sw.WriteLine("Title:" + project.Title);
                sw.WriteLine("OriginalScript:" + project.OriginalScript);
                sw.WriteLine("OriginalTranslation:" + project.OriginalTranslation);
                sw.WriteLine("OriginalEditing:" + project.OriginalEditing);
                sw.WriteLine("OriginalTiming:" + project.OriginalTiming);
                sw.WriteLine("SyncPoint:" + project.SyncPoint);
                sw.WriteLine("ScriptUpdatedBy:" + project.ScriptUpdatedBy);
                sw.WriteLine("UpdateDetails:" + project.UpdateDetails);
                sw.WriteLine("ScriptType:" + project.ScriptType);
                sw.WriteLine("Collisions:" + project.Collisions);
                sw.WriteLine("PlayResX:" + project.PlayResX);
                sw.WriteLine("PlayResY:" + project.PlayResY);
                sw.WriteLine("PlayDepth:" + project.PlayDepth);
                sw.WriteLine("Timer:" + project.Timer);
                sw.WriteLine("Wav:" + project.Wav);
                sw.WriteLine("LastWav:" + project.LastWav);
                sw.WriteLine("WrapStyle:" + project.WrapStyle);

                sw.WriteLine("[V4+ Styles]");
                sw.WriteLine("Format: Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, " +
                    "OutlineColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, " +
                    "Spacing, Angle, BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, Encoding");
                foreach (var style in project.SubtitleStyleList)
                {
                    sw.Write("Style:");
                    sw.Write(style.Name + ",");
                    sw.Write(style.Fontname + ",");
                    sw.Write(style.Fontsize.ToString() + ",");
                    sw.Write(style.PrimaryColor + ",");
                    sw.Write(style.SecondaryColor + ",");
                    sw.Write(style.OutlineColor + ",");
                    sw.Write(style.BackColor + ",");
                    sw.Write(style.Bold ? -1 : 0 + ",");
                    sw.Write(style.Italic ? -1 : 0 + ",");
                    sw.Write(style.Underline ? -1 : 0 + ",");
                    sw.Write(style.StrikeOut ? -1 : 0 + ",");
                    sw.Write(style.ScaleX.ToString() + ",");
                    sw.Write(style.ScaleY.ToString() + ",");
                    sw.Write(style.Spacing.ToString() + ",");
                    sw.Write(style.Angle.ToString() + ",");
                    sw.Write(style.BorderStyle.ToString() + ",");
                    sw.Write(style.Outline.ToString() + ",");
                    sw.Write(style.Shadow.ToString() + ",");

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
                    sw.Write(style.Encoding.ToString());
                    sw.WriteLine();
                }

                sw.WriteLine("[Events]");
                sw.WriteLine("Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text");
                foreach (var dialogue in project.SubtitlesList)
                {
                    sw.Write("Dialogue: ");
                    sw.Write(dialogue.Layer + ",");
                    sw.Write(dialogue.Start.ToString("hh\\:mm\\:ss\\.ffff") + ",");
                    sw.Write(dialogue.End.ToString("hh\\:mm\\:ss\\.ffff") + ",");
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

                                        int alignmentNumber = Convert.ToInt32(splitStyle[18]);
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
                                        style.Name = splitStyle[0];
                                        style.Fontname = splitStyle[1];
                                        style.Fontsize = Convert.ToInt32(splitStyle[2]);
                                        style.PrimaryColor = splitStyle[3];
                                        style.SecondaryColor = splitStyle[4];
                                        style.OutlineColor = splitStyle[5];
                                        style.BackColor = splitStyle[6];
                                        style.Bold = splitStyle[7] == "0" ? false : true;
                                        style.Italic = splitStyle[8] == "0" ? false : true;
                                        style.Underline = splitStyle[9] == "0" ? false : true;
                                        style.StrikeOut = splitStyle[10] == "0" ? false : true;
                                        style.ScaleX = Convert.ToInt32(splitStyle[11]);
                                        style.ScaleY = Convert.ToInt32(splitStyle[12]);
                                        style.Spacing = Convert.ToInt32(splitStyle[13]);
                                        style.Angle = Convert.ToDouble(splitStyle[14]);
                                        style.BorderStyle = Convert.ToInt32(splitStyle[15]);
                                        style.Outline = Convert.ToDouble(splitStyle[16].Replace('.', ','));
                                        style.Shadow = Convert.ToDouble(splitStyle[17]);
                                        style.HorizontalAlignment = horizontalNumber;
                                        style.VerticalAlignment = verticalNumber;
                                        style.MarginL = Convert.ToInt32(splitStyle[19]);
                                        style.MarginR = Convert.ToInt32(splitStyle[20]);
                                        style.MarginV = Convert.ToInt32(splitStyle[21]);
                                        style.Encoding = splitStyle[22];

                                        project.SubtitleStyleList.Add(style);
                                        break;
                                    #endregion
                                    #region Субтитры [Events]
                                    case "Dialogue":
                                        var splitDialogue = lineSplit[1].Split(',');
                                        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                                        project.SubtitlesList.Add(new Subtitle
                                        {
                                            Layer = Convert.ToInt32(splitDialogue[0]),
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
