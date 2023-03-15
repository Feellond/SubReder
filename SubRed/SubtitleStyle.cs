﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRed
{
    public class SubtitleStyle
    {
        public string Name { get; set; }
        public string Fontname { get; set; }
        public int Fontsize { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set;}
        public string OutlineColor { get; set; }
        public string BackColor { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public bool StrikeOut { get; set; }
        public int ScaleX { get; set; }
        public int ScaleY { get; set; }
        public int Spacing { get; set; }
        public int Angle { get; set; }
        public int BorderStyle { get; set; }
        public int Outline { get; set; }
        public int Shadow { get; set; }
        public int Alignment { get; set; }
        public int MarginL { get; set; }
        public int MarginR { get; set;}
        public int MarginV { get; set; }
        public int AlphaLevel { get; set; }
        public string Encoding { get; set; }

        public SubtitleStyle() 
        {
            Name = "Default Style";
            Fontname = "Times New Roman";
            Fontsize = 12;
            PrimaryColor = "FFFFFF";
            SecondaryColor = "";
            OutlineColor = "000000";
            BackColor = "";
            Bold = false;
            Italic = false;
            Underline = false;
            StrikeOut = false;
            ScaleX = 100;
            ScaleY = 100;
            Spacing = 4; //TODO
            BorderStyle = 1;
            Outline = 1;
            Shadow = 1;
            Alignment = 2;
            MarginL = 0;
            MarginR = 0;
            MarginV = 0;
            AlphaLevel = 0;
        }

        /*
         * Стиль определяет внешний вид субтитра (цвет, шрифт, размер букв) и его положение на экране. Все стили используемые в скрипте, должны быть определены в этом разделе.
         * Большинство (в случае ASS – все) параметры установленные в стиле, могут быть переопределены в конкретном субтитре с помощью управляющих кодов – тегов.
         * Определению стилей всегда должна предшествовать строка начинающаяся словом  “Format:”. Эта строка определяет, в каком порядке будут следовать устанавливаемые параметры. 
         * Соблюдайте правильность написания названий параметров (с учетом регистров):
         * 
         * Name, Fontname, Fontsize, PrimaryColour, SecondaryColour, TertiaryColour, BackColour, Bold, Italic, Underline, StrikeOut, ScaleX, ScaleY, Spacing, Angle, 
         * BorderStyle, Outline, Shadow, Alignment, MarginL, MarginR, MarginV, AlphaLevel, Encoding
         * 
         * Строка определения параметров необходима для совместимости последующих версий формата со старым программным обеспечением.
         * 
         * Field 1:      Name. Имя стиля. Чувствительно к регистру. Не должно содержать “запятые”.
         * Field 2:      Fontname. Имя шрифта, так как оно определено в Windows. Чувствительно к регистру.
         * Field 3:      Fontsize.
         * Field 4:      PrimaryColour. Основной цвет субтитров в формате BGR (голубой-зеленый-красный). В шестнадцатеричном формате это будет выглядеть как BBGGRR.
         *               Белый – FFFFFF, черный – 000000, голубой – FF0000, зеленый – 00FF00, красный – 0000FF.
         *               Это основной цвет для отображения субтитров.
         * Field 5:      SecondaryColour. Дополнительный цвет субтитров в формате BGR (blue-green-red).
         *               Этот цвет используется при организации спецэффектов, таких как караоке.
         * Field 6:      OutlineColor (TertiaryColour). Цвет окаймления букв в субтитре в формате BGR (blue-green-red).
         *               При установке внешнего вида “Outline+Shadow” - цвет окаймления, при установке “Opaque Box” – цвет поля, но котором размещены субтитры.
         *               Для лучшего восприятия рекомендуется делать его контрастным по отношению к Основному цвету.
         * Field 7:      BackColour. Цвет тени под буквами в субтитре в формате BGR (blue-green-red).
         * Field 4-7:    Описание цвета также содержит число определяющее прозрачность букв. В шестнадцатеричном формате это будет выглядеть как AABBGGRR.
         *               При этом АА=00 – непрозрачные буквы, AA=FF – абсолютно прозрачные буквы.
         * Field 8:      Bold. Определяет, будет ли текст отображаться жирным шрифтом (-1) или обычным (0). Не исключает применение наклонного шрифта.
         * Field 9:      Italic. Определяет, будет ли текст отображаться наклонным шрифтом (-1) или обычным (0). Не исключает применение жирного шрифта.
         * Field 9.1:  Underline. Подчеркивание. [-1 (есть) или 0(нет)]
         * Field 9.2:  StrikeOut. Зачеркивание. [-1 (есть) или 0(нет)]
         * Field 9.3:  ScaleX. Изменение ширины шрифта [в процентах]. Если без изменений, то 100.
         * Field 9.4:  ScaleY. Изменение высоты шрифта [в процентах]. Если без изменений, то 100.
         *             Внимание: при установке BorderStyle=3 (фон под субтитрами), возможно не пропорциональное изменение ширины или высоты шрифта и фоновой полосы.
         * Field 9.5:  Spacing. Расстояние между буквами. [в пикселях]
         * Field 9.6:  Angle.  Угол поворота субтитра вокруг оси Z (ось, перпендикулярная плоскости экрана) против часовой стрелки. Может быть дробным и отрицательным. [в градусах]
         * Field 10:    BorderStyle. 1=Окаймление + тень, 3=фон под субтитрами
         * Field 11:    Outline. Если BorderStyle – 1, то определяет толщину окаймления в пикселях. Если BorderStyle – 2, то определяет размер фонового бокса.
         * Field 12:    Shadow. Определяет глубину размещения тени под субтитрами.
         * Field 13:    Alignment. Число, определяющее горизонтальное и вертикальное выравнивание субтитров. По умолчанию вертикальное выравнивание идет по нижнему краю кадра.
         *              Горизонтальное: 1=по левому краю, 2=по центру, 3=по правому краю.
         *              Для вертикального выравнивания по верхнему краю, добавьте к значению горизонтального выравнивания 4. 
         *              Для вертикального выравнивания по центру кадра, добавьте к значению горизонтального выравнивания 8.
         *              Например. 5 = выравнивание по верхнему левому углу.
         * Field 13:   В ASS выравнивание определяется так, как расположены кнопки на дополнительной числовой клавиатуре  (1-3 по нижнему краю, 4-6 по центру, 7-9 по верхнему краю).
         * Field 14:    MarginL. Отступ от левого края кадра в пикселях. Определяет расстояние от левого края кадра до левой границы области отображения субтитров.
         * Field 15:    MarginR. Отступ от правого края кадра в пикселях. Определяет расстояние от правого края кадра до правой границы области отображения субтитров.
         * Field 16:    MarginV. Вертикальный отступ в пикселях.
         *              Когда выравнивание по нижнему краю – отступ от нижнего края кадра до нижней границы обрасти отображения субтитров.
         *              Когда выравнивание по верхнему краю – отступ от верхнего края кадра до верхней границы.
         *              Когда выравнивание по вертикальному центру – игнорируется, так как субтитр должен располагаться по центру
         * Field 17:    AlphaLevel. Определяет прозрачность текста. В SSA ЕЩЕ не используется.
         * Field 17:   В ASS УЖЕ не используется. J
         * Field 18:    Encoding. Число определяющее кодовую страницу для шрифта. Для поддержки русского языка – 204.
         */
    }
}