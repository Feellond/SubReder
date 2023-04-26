using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Automation.Text;

namespace SubRed
{
    public class SubProject
    {
        public string Title { get; set; }              //Название скрипта.
        public string OriginalScript { get; set; }     //Автор скрипта.
        public string OriginalTranslation { get; set; }//Переводчик.
        public string OriginalEditing { get; set; }    //Редактор.
        public string OriginalTiming { get; set; }     //Таймеровщик
        public string SyncPoint { get; set; }          //Описание кадра, позволяющее синхронизировать скрипт с видео.
        public string ScriptUpdatedBy { get; set; }    //Редактор, не связанный с автором скрипта.
        public string UpdateDetails { get; set; }      //Детальное описание внесенных сторонним редактором изменений.
        public string ScriptType { get; set; }         //Версия формата.
        public string Collisions { get; set; }         //Обработка коллизий (субтитров со взаимоперекрывающимся таймингом).
        public string PlayResY { get; set; }           //Разрешение по вертикали.
        public string PlayResX { get; set; }           //Разрешение по горизонтали.
        public string PlayDepth { get; set; }          //Глубина цвета (возможно, насыщенность).
        public string Timer { get; set; }              //Масштабирование тайминга в процентах. “100.0000” соответствует 100%. Должно содержать 4 знака после запятой.
        public string Wav { get; set; }
        public string LastWav { get; set; }
        public string WrapStyle { get; set; }          //Определение стиля переноса слов (когда строка не влезает в отведенное для нее место):
                                                       //0: автоматический перенос, возможен принудительный перенос с помощью тегов \n и \N,
                                                       //1: перенос по символу “конец строки”, работает только тег \N
                                                       //2: нет переноса, теги \n и \N игнорируются
                                                       //3: аналогично 0, но нижняя строка всегда будет длиннее верхней.
        public List<Subtitle> SubtitlesList { get; set; }
        public List<SubtitleStyle> SubtitleStyleList { get; set; }

        public SubProject() 
        {
            Title = "Default SubRed project";
            OriginalScript = "";
            OriginalTranslation = "";
            OriginalEditing = "";
            OriginalTiming = "";
            SyncPoint = "v4.00";
            ScriptUpdatedBy = "";
            UpdateDetails = "";
            ScriptType = "";
            Collisions = "";
            PlayResX = "720";
            PlayResY = "480";
            PlayDepth = "0";
            Timer = "";
            WrapStyle = "0";
            Wav = "";
            LastWav = "";

            SubtitleStyleList = new List<SubtitleStyle> { new SubtitleStyle() };

            SubtitlesList = new List<Subtitle>
            {
                new Subtitle() { Style = SubtitleStyleList[0]},
                new Subtitle() { Style = SubtitleStyleList[0]}
            };
            SubtitleRenum();
        }

        public void SubtitleRenum()
        {
            for (int index = 0; index < SubtitlesList.Count; index++)
                SubtitlesList[index].Id = index;
        }

        public void SubtitleSort()
        {
            SubtitlesList = SubtitlesList.OrderBy(x => x.Start).ToList();
        }
    }
}
