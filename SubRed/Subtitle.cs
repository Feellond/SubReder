using System;
using System.Drawing;


namespace SubRed
{
    public class Subtitle : ICloneable
    {
        public int Id { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
        public TimeSpan Duration { get; set; }
        public int FrameBeginNum { get; set; }
        public int FrameEndNum { get; set; }
        public System.Drawing.Rectangle FrameRegion { get; set; }
        public int XCoord { get; set; }
        public int YCoord { get; set; }
        public Bitmap FrameImage { get; set; }
        public string Text { get; set; }
        public SubtitleStyle Style { get; set; }

        #region Дополнительные настройки субтитров
        public int Marked { get; set; }
        public int Layer { get; set; }
        public string Name { get; set; }
        public string Effect { get; set; }
        #endregion

        public Subtitle()
        {
            Id = 0;
            Start = TimeSpan.Zero;
            End = new TimeSpan(0, 0, 10);
            Duration = TimeSpan.Zero;
            FrameBeginNum = 0;
            FrameEndNum = 0;
            XCoord = 0;
            YCoord = 0;
            Text = "Как оно работает? Тест переводчика. Что он выдаст в этот раз?";

            Style = new SubtitleStyle();

            Marked = 0;
            Layer = 0;
            Name = string.Empty;
            Effect = string.Empty;
        }

        public object Clone()
        {
            Subtitle subtitle = MemberwiseClone() as Subtitle;
            subtitle.Style = subtitle.Style.Clone() as SubtitleStyle;

            return subtitle;
        }

        /// <summary>
        /// Применяет тег к тексту субтитра
        /// </summary>
        /// <param name="beginIndex">Позиция вставки начала</param>
        /// <param name="endIndex">Позиция вставки окончания</param>
        /// <param name="action">Применяемое действие: bold, cursive, underline, strikethrough</param>
        public void ChangeInTextAction(int beginIndex, int endIndex, string action)
        {
            string beginTag = "";
            string endTag = "";
            switch (action)
            {
                case "bold":
                    beginTag = "<b>";
                    endTag = "</b>";
                    break;
                case "cursive":
                    beginTag = "<i>";
                    endTag = "</i>";
                    break;
                case "underline":
                    beginTag = "<u>";
                    endTag = "</u>";
                    break;
                case "strikethrough":
                    beginTag = "<s>";
                    endTag = "</s>";
                    break;
                case "break":
                    beginTag = "</br>";
                    break;
            }

            this.Text = this.Text.Insert(beginIndex, beginTag);
            if (endIndex > 0 && endTag != "")
                this.Text = this.Text.Insert(endIndex, endTag);
        }

        /*
        * Field 1:      Marked.    Marked=1 означает, что в Sub Station Alpha строка будет показана как "отмеченная"
        *                          Marked=0 означает, что в Sub Station Alpha строка не будет показана как "отмеченная"
        * Field 1:      Layer
        *                          Любое целое число. В случае коллизий, субтитр с большим значением данного параметра будет располагаться  поверх субтитра с меньшим значением параметра.
        * Field 2:      Start. 
        *                          Время начала показа субтитра в формате Час:Минуты:Секунды:Сотые_доли_секунды. (Ч:ММ:СС:ДД). Час должен содержать не более одной цифры.
        * Field 3:      End
        *                          Время окончания показа субтитра в формате Час:Минуты:Секунды:Сотые_доли_секунды. (Ч:ММ:СС:ДД). Час должен содержать не более одной цифры.
        * Field 4:      Style
        *                          Имя стиля определенного в разделе описания стилей.
        * Field 5:      Name
        *                          Имя героя. Дополнительная информация не имеющая значащего значения для показа субтитра.
        * Field 6:      MarginL
        *                          Отступ от левого края кадра в пикселях для текущего субтитра. Должен содержать четыре цифры. Если имеет значение 0000 – то используется отступ, определенный в стиле.
        * Field 7:      MarginR
        *                          Отступ от правого края кадра в пикселях для текущего субтитра. Должен содержать четыре цифры. Если имеет значение 0000 – то используется отступ, определенный в стиле.
        * Field 8:      MarginV
        *                          Вертикальный отступ в пикселях для текущего субтитра. Должен содержать четыре цифры. Если имеет значение 0000 – то используется отступ, определенный в стиле.
        * Field 9:      Effect
        *                          Эффекты движения. Может содержать информацию для реализации одного из трех встроенных эффектов движения доступных в SSA v4.x
        *                          Имена эффектов чувствительны к регистру и должны быть записаны в точности так, как это показано ниже.
        *                          Karaoke – означает, что текст субтитра будет высвечиваться последовательно, слово за словом.
        *                          Эффект Karaoke устарел и не применяется.
        *                          
        * Scroll up;y1;y2;delay[;fadeawayheight] – эффект прокрутки текста вверх по экрану. Параметры отделяются от имени эффекта и друг от друга “точкой с запятой”.
        *                          Величины y1 и y2 определяют вертикальные границы области прокрутки. Задаются в пикселях от верхней границы кадра. 
        *                          Последовательность значения не имеет, прокрутка будет осуществляться от большей границы к меньшей.
        *                          Параметр delay определяет скорость прокрутки и может находиться в пределах от 0 до 100. Чем больше значение, тем медленнее скорость.
        *                          Banner;delay – эффект горизонтального перемещения текста вдоль экрана справа на лево. Текст будет показан в одну строку независимо от длины и кодов переноса строки.
        *                          Параметр delay определяет скорость прокрутки и может находиться в пределах от 0 до 100. Чем больше значение, тем медленнее скорость.
        *                          
        * Scroll down;y1;y2;delay[;fadeawayheight] - эффект прокрутки текста вниз по экрану. Аналогичен Scroll up.
        * 
        * Banner;delay[;lefttoright;fadeawaywidth] – улучшеный эффект горизонтального перемещения текста вдоль экрана.
        *                          Параметр lefttoright может иметь значения 0 или 1. Значение 0 означает перемещение справа на лево и может отсутствовать. Значение 1 означает перемещение слева на право.
        *                          Когда параметр delay больше 0, перемещение буде идти со скоростью 1 пиксель за (1000/delay) секунд.
        *                          (ВНИМАНИЕ: Avery Lee’s фильтр субтитров воспринимает параметры эффекта “Scroll up” как delay;y1;y2)
        *                          Параметы fadeawayheight и fadeawaywidth определяют длину (в пикселях) промежутка, на котором субтитры постепенно появляются и постепенно исчезают.
        * Field 10:    Text
        *                          Текст субтитра. Может содержать “запятые”, так как все, что расположено после 9-ой запятой считается текстом субтитра.
        */
    }
}
