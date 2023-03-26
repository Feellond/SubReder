using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для MoreSettingsWindow.xaml
    /// </summary>
    public partial class MoreSettingsWindow : Window
    {
        public MoreSettingsWindow()
        {
            InitializeComponent();
        }
        private static readonly Regex _regex = new Regex("[^0-9.-]+"); //regex that matches disallowed text
        private void PreviewTextInputNumbers(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
        public static bool IsValid(string name, string str)
        {
            int i;
            switch (name)
            {
                case "textBoxGausSeed":
                case "textBoxMeanSeed":
                case "textBoxLaplaceSeed":
                case "textBoxMorphSize":
                case "textBoxDilateHeight":
                case "textBoxDilateWidth":
                case "textBoxErodeHeight":
                case "textBoxErodeWidth":
                    return int.TryParse(str, out i) && i >= 1 && i <= 9999 && i % 2 == 1;
                case "textBoxThresholdLaplace":
                    return int.TryParse(str, out i) && i >= 0 && i <= 255;
                default:
                    return false;
            }

        }
        private void SetOCRValues()
        {
            SubtitleOCR.gausSeed = int.Parse(textBoxGausSeed.Text);
            SubtitleOCR.meanSeed = int.Parse(textBoxMeanSeed.Text);
            SubtitleOCR.laplaceSeed = int.Parse(textBoxLaplaceSeed.Text);
            SubtitleOCR.morphSize = int.Parse(textBoxMorphSize.Text);
            SubtitleOCR.thresholdLaplace = int.Parse(textBoxThresholdLaplace.Text);
            /*SubtitleOCR.dilateHeight = int.Parse(textBoxDilateHeight.Text);
            SubtitleOCR.dilateWidth = int.Parse(textBoxDilateWidth.Text);
            SubtitleOCR.erodeHeight = int.Parse(textBoxErodeHeight.Text);
            SubtitleOCR.erodeWidth = int.Parse(textBoxErodeWidth.Text);*/

            ComboBoxItem ComboItem = (ComboBoxItem)comboBoxLanguage.SelectedItem;
            SubtitleOCR.OCRLanguageChange(ComboItem.Name);
        }
        private void defaultButton_Click(object sender, RoutedEventArgs e)
        {
            SubtitleOCR.ReturnDefaultValues();
            textBoxGausSeed.Text = SubtitleOCR.gausSeed.ToString();
            textBoxMeanSeed.Text = SubtitleOCR.meanSeed.ToString();
            textBoxLaplaceSeed.Text = SubtitleOCR.laplaceSeed.ToString();
            textBoxMorphSize.Text = SubtitleOCR.morphSize.ToString();
            textBoxThresholdLaplace.Text = SubtitleOCR.thresholdLaplace.ToString();
            /*textBoxDilateHeight.Text = SubtitleOCR.dilateHeight.ToString();
            textBoxDilateWidth.Text = SubtitleOCR.dilateWidth.ToString();
            textBoxErodeHeight.Text = SubtitleOCR.erodeHeight.ToString();
            textBoxErodeWidth.Text = SubtitleOCR.erodeWidth.ToString();*/

            var lang = SubtitleOCR.ocrLanguage;
            switch (lang)
            {
                case "eng":
                    comboBoxLanguage.SelectedItem = comboBoxLanguage.Items[comboBoxLanguage.Items.IndexOf("Английский")];
                    break;
                case "rus":
                    comboBoxLanguage.SelectedItem = comboBoxLanguage.Items[comboBoxLanguage.Items.IndexOf("Русский")];
                    break;
                default:
                    comboBoxLanguage.SelectedItem = comboBoxLanguage.Items[0];
                    break;
            }
        }
    }
}
