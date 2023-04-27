using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using SkiaSharp;

namespace SubRed
{
    /// <summary>
    /// Логика взаимодействия для StyleSettingsWindow.xaml
    /// </summary>
    public partial class StyleSettingsWindow : Window
    {
        public int currentIndex;
        public SubProject currentProject;

        public SubtitleStyle currentStyle;
        public int? horizontalAlignment, verticalAlignment;
        public StyleSettingsWindow()
        {
            InitializeComponent();
        }

        public void LoadWindow(int index, SubProject project, SubtitleStyle style)
        {
            currentIndex = index;
            currentProject = project;
            currentStyle = style;

            foreach (var fFamily in FontFamily.FamilyNames)
            {
                FontNameComboBox.Items.Add(fFamily.Value);
            }

            StyleNameTextBox.Text = currentStyle.Name;
            FontNameComboBox.Text = currentStyle.Fontname;
            FontSizeNumericUpDown.Value = currentStyle.Fontsize;
            BoldCheckBox.IsChecked = currentStyle.Bold;
            CursiveCheckBox.IsChecked = currentStyle.Italic;
            UnderlineCheckBox.IsChecked = currentStyle.Underline;
            CrossedCheckBox.IsChecked = currentStyle.StrikeOut;
            FirstColorPicker.ColorName = currentStyle.PrimaryColor;
            SecondColorPicker.ColorName = currentStyle.SecondaryColor;
            ContourColorPicker.ColorName = currentStyle.OutlineColor;
            ShadowColorPicker.ColorName = currentStyle.BackColor;
            horizontalAlignment = currentStyle.HorizontalAlignment;
            verticalAlignment = currentStyle.VerticalAlignment;
            ContourNumericUpDown.Value = currentStyle.Outline;
            ShadowNumericUpDown.Value = currentStyle.Shadow;
            x_NumericUpDown.Value = currentStyle.ScaleX;
            y_NumericUpDown.Value = currentStyle.ScaleY;
            RotationNumericUpDown.Value = currentStyle.Angle;
            IntervalNumericUpDown.Value = currentStyle.Spacing;
        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            currentStyle.Name = StyleNameTextBox.Text;
            currentStyle.Fontname = FontNameComboBox.Text;
            currentStyle.Fontsize = (int)FontSizeNumericUpDown.Value;
            currentStyle.Bold = BoldCheckBox.IsChecked ?? false;
            currentStyle.Italic = CursiveCheckBox.IsChecked ?? false;
            currentStyle.Underline = UnderlineCheckBox.IsChecked ?? false;
            currentStyle.StrikeOut = CrossedCheckBox.IsChecked ?? false;
            currentStyle.PrimaryColor = FirstColorPicker.ColorName; //Проверить цвета
            currentStyle.SecondaryColor = SecondColorPicker.ColorName; //Проверить цвета
            currentStyle.OutlineColor = ContourColorPicker.ColorName; //Проверить цвета
            currentStyle.BackColor = ShadowColorPicker.ColorName; //Проверить цвета
            currentStyle.HorizontalAlignment = horizontalAlignment;
            currentStyle.VerticalAlignment = verticalAlignment;
            currentStyle.Outline = (int)ContourNumericUpDown.Value;
            currentStyle.Shadow = (int)ShadowNumericUpDown.Value;
            currentStyle.ScaleX = (int)x_NumericUpDown.Value;
            currentStyle.ScaleY = (int)y_NumericUpDown.Value;
            currentStyle.Angle = (int)RotationNumericUpDown.Value;
            currentStyle.Spacing = (int)IntervalNumericUpDown.Value;

            currentProject.SubtitleStyleList[currentIndex] = currentStyle;
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SampleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            PreviewLoad();
        }

        private void SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Drawing.Color?> e)
        {
            PreviewLoad();
        }

        //If you get 'dllimport unknown'-, then add 'using System.Runtime.InteropServices;'
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);
        public ImageSource ImageSourceFromBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        public void PreviewLoad()
        {
            Bitmap bmp = new Bitmap((int)ImageExample.ActualWidth, (int)ImageExample.ActualHeight);
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(70, 90, 90, 50);

            Graphics g = Graphics.FromImage(bmp);

            var backgroundColor = System.Drawing.Color.FromArgb(
                BackgroundColor.SelectedColor.Value.A,
                BackgroundColor.SelectedColor.Value.R,
                BackgroundColor.SelectedColor.Value.G,
                BackgroundColor.SelectedColor.Value.B);

            var firstColor = System.Drawing.Color.FromArgb(
                FirstColorPicker.SelectedColor.Value.A,
                FirstColorPicker.SelectedColor.Value.R,
                FirstColorPicker.SelectedColor.Value.G,
                FirstColorPicker.SelectedColor.Value.B);

            var secondColor = System.Drawing.Color.FromArgb(
                SecondColorPicker.SelectedColor.Value.A,
                SecondColorPicker.SelectedColor.Value.R,
                SecondColorPicker.SelectedColor.Value.G,
                SecondColorPicker.SelectedColor.Value.B);

            var contourColor = System.Drawing.Color.FromArgb(
                ContourColorPicker.SelectedColor.Value.A,
                ContourColorPicker.SelectedColor.Value.R,
                ContourColorPicker.SelectedColor.Value.G,
                ContourColorPicker.SelectedColor.Value.B);

            var shadowColor = System.Drawing.Color.FromArgb(
                ShadowColorPicker.SelectedColor.Value.A,
                ShadowColorPicker.SelectedColor.Value.R,
                ShadowColorPicker.SelectedColor.Value.G,
                ShadowColorPicker.SelectedColor.Value.B);

            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.FillRectangle(new SolidBrush(backgroundColor), rect);

            Font font = new Font(
                   new System.Drawing.FontFamily(FontNameComboBox.Text),
                   Convert.ToInt32(FontSizeNumericUpDown.Value),
                   (BoldCheckBox.IsChecked ?? false) ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular
            );

            font = new Font(font, (CursiveCheckBox.IsChecked ?? false) ? font.Style ^ System.Drawing.FontStyle.Italic : font.Style);
            font = new Font(font, (UnderlineCheckBox.IsChecked ?? false) ? font.Style ^ System.Drawing.FontStyle.Underline : font.Style);
            font = new Font(font, (CrossedCheckBox.IsChecked ?? false) ? font.Style ^ System.Drawing.FontStyle.Strikeout : font.Style);

            Font fontContour = new Font(
                new System.Drawing.FontFamily(FontNameComboBox.Text),
                   Convert.ToInt32(FontSizeNumericUpDown.Value + ContourNumericUpDown.Value),
                   font.Style
                ); ;

            StringFormat format = new StringFormat(StringFormatFlags.NoClip);
            format.Alignment = StringAlignment.Center;

            var textSplit = SampleTextBox.Text.Remove(SampleTextBox.Text.IndexOf("\\n") + 1, 1).Split('\\');
            for (int i = 0; i < textSplit.Length; i++)
            {
                PointF point = new PointF();
                point.X = (float)(ImageExample.ActualWidth / 2);
                point.Y = Convert.ToInt32((i * FontSizeNumericUpDown.Value + 10) + ImageExample.ActualHeight / 4);
                string spacedText = spaced(textSplit[i], (int)IntervalNumericUpDown.Value);

                PointF pointShadow = new PointF(point.X + (int)ShadowNumericUpDown.Value, point.Y + (int)ShadowNumericUpDown.Value);
                g.DrawString(spacedText, font, new SolidBrush(shadowColor), pointShadow, format);


                g.DrawString(spacedText, fontContour, new SolidBrush(contourColor), point);


                g.DrawString(spacedText, font, new SolidBrush(firstColor), point);
            }
            g.Flush();
            ImageExample.Source = ImageSourceFromBitmap(bmp);
        }

        public string spaced(string text, int spacing)
        {
            char space = (char)0x200a;
            string spaces = "".PadLeft(spacing, space);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < text.Length; i++) sb.Append(text[i] + spaces);
            return sb.ToString().Trim(space);
        }

        private void NumericUpDown_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            PreviewLoad();
        }

        private void FontNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PreviewLoad();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            PreviewLoad();
        }

        private void AlignmentRadioButton_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            switch(rb.Name)
            {
                case "TopLeftRadioButton":
                    horizontalAlignment = 1;
                    verticalAlignment = 0;
                    break;
                case "TopRadioButton":
                    horizontalAlignment = 2;
                    verticalAlignment = 0;
                    break;
                case "TopRightRadioButton":
                    horizontalAlignment = 3;
                    verticalAlignment = 0;
                    break;
                case "CenterLeftRadioButton":
                    horizontalAlignment = 1;
                    verticalAlignment = 8;
                    break;
                case "CenterRadioButton":
                    horizontalAlignment = 2;
                    verticalAlignment = 8;
                    break;
                case "CenterRightRadioButton":
                    horizontalAlignment = 3;
                    verticalAlignment = 8;
                    break;
                case "DownLeftRadioButton":
                    horizontalAlignment = 1;
                    verticalAlignment = 4;
                    break;
                case "DownRadioButton":
                    horizontalAlignment = 2;
                    verticalAlignment = 4;
                    break;
                case "DownRightRadioButton":
                    horizontalAlignment = 3;
                    verticalAlignment = 4;
                    break;
            }

            TopLeftRadioButton.IsChecked = rb.Name == TopRightRadioButton.Name;
            TopRadioButton.IsChecked = rb.Name == TopRadioButton.Name;
            TopRightRadioButton.IsChecked = rb.Name == TopLeftRadioButton.Name;

            CenterLeftRadioButton.IsChecked = rb.Name == CenterLeftRadioButton.Name;
            CenterRadioButton.IsChecked = rb.Name == CenterRadioButton.Name;
            CenterRightRadioButton.IsChecked = rb.Name == CenterRightRadioButton.Name;

            DownLeftRadioButton.IsChecked = rb.Name == DownLeftRadioButton.Name;
            DownRadioButton.IsChecked = rb.Name == DownRadioButton.Name;
            DownRightRadioButton.IsChecked = rb.Name == DownRightRadioButton.Name;
        }
    }
}
