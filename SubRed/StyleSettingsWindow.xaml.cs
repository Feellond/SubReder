using System;
using System.Collections.Generic;
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

        }
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            currentStyle.Name = StyleNameTextBox.Text;
            currentStyle.Fontname = FontNameTextBox.Text;
            currentStyle.Fontsize = (int)FontSizeNumericUpDown.Value;
            currentStyle.Bold = BoldCheckBox.IsChecked;
            currentStyle.Italic = CursiveCheckBox.IsChecked;
            currentStyle.Underline = UnderlineCheckBox.IsChecked;
            currentStyle.StrikeOut = CrossedCheckBox.IsChecked;
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

        }

        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color?> e)
        {

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
