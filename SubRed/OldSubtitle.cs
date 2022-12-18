using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubRed
{
    public class OldSubtitle
    {
        //[Script Info]
        public string title { get; set; }              //Название скрипта.
        public string originalScript { get; set; }     //Автор скрипта.
        public string originalTranslation { get; set; }//Переводчик.
        public string originalEditing { get; set; }    //Редактор.
        public string originalTiming { get; set; }     //Таймеровщик
        public string syncPoint { get; set; }         //Описание кадра, позволяющее синхронизировать скрипт с видео.
        public string scriptUpdatedBy { get; set; }    //Редактор, не связанный с автором скрипта.
        public string updateDetails { get; set; }      //Детальное описание внесенных сторонним редактором изменений.
        public string scriptType { get; set; }         //Версия формата.
        public string collisions { get; set; }         //Обработка коллизий (субтитров со взаимоперекрывающимся таймингом).
        public string playResY { get; set; }           //Разрешение по вертикали.
        public string playResX { get; set; }           //Разрешение по горизонтали.
        public string playDepth { get; set; }          //Глубина цвета (возможно, насыщенность).
        public string timer { get; set; }              //Масштабирование тайминга в процентах. “100.0000” соответствует 100%. Должно содержать 4 знака после запятой.
        public string wav { get; set; }
        public string lastWav { get; set; }
        public string wrapStyle { get; set; }          //Определение стиля переноса слов (когда строка не влезает в отведенное для нее место):
                                                       //0: автоматический перенос, возможен принудительный перенос с помощью тегов \n и \N,
                                                       //1: перенос по символу “конец строки”, работает только тег \N
                                                       //2: нет переноса, теги \n и \N игнорируются
                                                       //3: аналогично 0, но нижняя строка всегда будет длиннее верхней.

        //[V4 Styles]
        public string stylesFormat { get; set; }             //Используемые форматы субтитров
        public List<string> style { get; set; }        //Описание стиля, определяющего внешний вид субтитров.

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


        //[Events]
        public string TextFormat { get; set; }       //Эта строка определяет, в каком порядке будут следовать устанавливаемые параметры.
        public List<string> text { get; set; }     //Событие “диалог”, то есть текст субтитра.

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

        public OldSubtitle()
        {
            title = "Default SubRed subtitle";
            originalScript = "";
            originalTranslation = "";
            originalEditing = "";
            originalTiming = "";
            syncPoint = "v4.00";
            scriptUpdatedBy = "";
            updateDetails = "";
            scriptType = "";
            collisions = "";
            playResX = "720";
            playResY = "480";
            playDepth = "0";
            timer = "";
            wrapStyle = "0";
            wav = "";
            lastWav = "";

            stylesFormat = "";
            style = new List<string>();

            TextFormat = "";
            text = new List<string>();
        }

        public string getSubtitlesString()
        {
            string str = "";
            str += "Title: " + title + "&&";
            str += " OriginalScript: " + originalScript + "&&";
            str += " OriginalTranslation: " + originalTranslation + "&&";
            str += " OriginalEditing: " + originalEditing + "&&";
            str += " OriginalTiming: " + originalTiming + "&&";
            str += " SyncPoint: " + syncPoint + "&&";
            str += " ScriptUpdatedBy: " + scriptUpdatedBy + "&&";
            str += " UpdateDetails: " + updateDetails + "&&";
            str += " ScriptType: " + scriptType + "&&";
            str += " Collisions: " + collisions + "&&";
            str += " PlayResY: " + playResY + "&&";
            str += " PlayResX: " + playResX + "&&";
            str += " PlayDepth: " + playDepth + "&&";
            str += " Wav: " + wav + "&&";
            str += " LastWav: " + lastWav + "&&";
            str += " Timer: " + timer + "&&";
            str += " WrapStyle: " + wrapStyle + "&&";

            str += " StyleFormat: " + stylesFormat + "&&";
            for (int i = 0; i < style.Count(); i++)
                str += " Style: " + style[i] + "&&";

            str += " TextFormat: " + TextFormat + "&&";
            for (int i = 0; i < text.Count(); i++)
                str += " Text: " + text[i].Trim(' ') + "&&";

            return str;

            /*
             * styleFormat = "Name,Fontname,Fontsize,PrimaryColour,SecondaryColour," +
                    "OutlineColour,BackColour,Bold,Italic,Underline,StrikeOut,ScaleX," +
                    "ScaleY,Spacing,Angle,BorderStyle,Outline,Shadow,Alignment," +
                    "MarginL,MarginR,MarginV,AlphaLevel,Encoding";
             */
        }

        public void setSubtitleString(string str)
        {
            title = "";
            originalScript = "";
            originalTranslation = "";
            originalEditing = "";
            originalTiming = "";
            syncPoint = "";
            scriptUpdatedBy = "";
            updateDetails = "";
            scriptType = "";
            collisions = "";
            playResY = "480";
            playResX = "720";
            playDepth = "0";
            timer = "";
            wrapStyle = "";
            wav = "";
            lastWav = "";
            stylesFormat = "";
            style.Clear();
            TextFormat = "";
            text.Clear();

            string[] strSplit = str.Split(new string[] { "&&" }, StringSplitOptions.RemoveEmptyEntries);
            int x = 2;
            char[] separator = { ':' };
            for (int i = 0; i < strSplit.Count(); i++)
            {
                string[] s = strSplit[i].Split(separator, x);
                switch (s[0].Trim(' '))
                {
                    case "Title":
                        //i++;
                        title = s[1].Trim(' ');
                        strSplit[i] = "";
                        break;
                    case "OriginalScript":
                        //i++;
                        originalScript = s[1].Trim(' ');
                        strSplit[i] = "";
                        break;
                    case "OriginalTranslation":
                        //i++;
                        originalTranslation = s[1].Trim(' ');
                        strSplit[i] = "";
                        break;
                    case "OriginalEditing":
                        //i++;
                        originalEditing = s[1].Trim(' ');
                        strSplit[i] = "";
                        break;
                    case "OriginalTiming":
                        //i++;
                        originalTiming = s[1].Trim(' ');
                        strSplit[i] = "";
                        break;
                    case "SyncPoint":
                        //i++;
                        syncPoint = s[1].Trim(' ');
                        strSplit[i] = "";
                        break;
                    case "ScriptUpdatedBy":
                        //i++;
                        scriptUpdatedBy = s[1].Trim(' ');
                        strSplit[i] = "";
                        break;
                    case "UpdateDetails":
                        //i++;
                        updateDetails = s[1].Trim(' ');
                        strSplit[i] = "";
                        break;
                    case "ScriptType":
                        //i++;
                        scriptType = s[1].Trim(' ');
                        strSplit[i] = "";
                        break;
                    case "Collisions":
                        //i++;
                        collisions = s[1].Trim(' ');
                        strSplit[i] = "";
                        break;
                    case "PlayResY":
                        //i++;
                        playResY = s[1].Trim(' ');
                        strSplit[i] = "";
                        break;
                    case "PlayResX":
                        //i++;
                        playResX = s[1].Trim(' ');
                        strSplit[i] = "";
                        break;
                    case "PlayDepth":
                        //i++;
                        playDepth = s[1].Trim(' ');
                        strSplit[i] = "";
                        break;
                    case "Timer":
                        //i++;
                        timer = s[1].Trim(' ');
                        strSplit[i] = "";
                        break;
                    case "WrapStyle":
                        //i++;
                        wrapStyle = s[1].Trim(' ');
                        strSplit[i] = "";
                        break;
                    case "Wav":
                        //i++;
                        wav = s[1].Trim(' ');
                        strSplit[i] = "";
                        break;
                    case "LastWav":
                        //i++;
                        lastWav = s[1].Trim(' ');
                        strSplit[i] = "";
                        break;
                    case "StyleFormat":
                        //i++;
                        string[] sF = s[1].Trim(' ').Split(',');
                        for (int k = 0; k < sF.Length; k++)
                            sF[k] = sF[k].Trim(' ');
                        stylesFormat = String.Join(",", sF);
                        strSplit[i] = "";
                        break;
                    case "Style":
                        //i++;
                        style.Add(s[1].Trim(' '));
                        strSplit[i] = "";
                        break;
                    case "TextFormat":
                        //i++;
                        string[] tF = s[1].Trim(' ').Split(',');
                        for (int k = 0; k < tF.Length; k++)
                            tF[k] = tF[k].Trim(' ');
                        TextFormat = String.Join(",", tF);
                        strSplit[i] = "";
                        break;
                    case "Text":
                    case "Dialogue":
                        //i++;
                        text.Add(s[1].Trim(' '));
                        strSplit[i] = "";
                        break;
                }
            }

            if (playResX == "0" && playResY != "0")
            {
                playResX = playResY;
            }
            else if (playResX != "0" && playResY == "0")
            {
                playResY = playResX;
            }


            string[] styleFormatSplit = stylesFormat.Split(',');
            List<string> styleFormatSplitList = new List<string>();
            for (int i = 0; i < styleFormatSplit.Length; i++)
                styleFormatSplitList.Add(styleFormatSplit[i]);

            string[] formatsStyle = { "Name", "Fontname", "Fontsize", "PrimaryColour", "SecondaryColour", "OutlineColour", "BackColour", "Bold",
                                    "Italic", "Underline", "StrikeOut", "ScaleX", "ScaleY", "Spacing", "Angle", "BorderStyle", "Outline",
                                    "Shadow", "Alignment", "MarginL", "MarginR", "MarginV", "AlphaLevel", "Encoding"};

            for (int i = 0; i < formatsStyle.Length; i++)
            {
                if (!styleFormatSplitList.Contains(formatsStyle[i]))
                {
                    styleFormatSplitList.Insert(i, formatsStyle[i]);
                    int index = 0;
                    for (int j = 0; j < style.Count; j++)
                    {
                        string st = style[j];
                        string[] stylesSplit = st.Split(',');
                        List<string> stylesSplitList = stylesSplit.OfType<string>().ToList();

                        switch (i)
                        {
                            case 1:
                            case 2:
                            case 3:
                                stylesSplitList.Insert(i, "");
                                break;
                            default:
                                stylesSplitList.Insert(i, "0");
                                break;
                        }

                        string[] s = stylesSplitList.ToArray();

                        style.RemoveAt(index);
                        style.Insert(index, string.Join(",", s));
                        index++;
                    }
                }
            }

            stylesFormat = String.Join(",", styleFormatSplitList.ToArray());


            string[] textFormatSplit = TextFormat.Split(',');
            string[] formatsText = { "Layer", "Start", "End", "Style", "Name", "Effect", "MarginL", "MarginR", "MarginV", "Text" };


        }
    }
}
