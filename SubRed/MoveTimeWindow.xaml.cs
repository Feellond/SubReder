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
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace SubRed
{
    /// <summary>
    /// Логика взаимодействия для MoveTimeWindow.xaml
    /// </summary>
    public partial class MoveTimeWindow : Window
    {
        MainWindow mainWindow;
        public MoveTimeWindow()
        {
            InitializeComponent();
        }
        public void LoadWindow(MainWindow window)
        {
            mainWindow = window;
        }
        private static readonly Regex _regex = new Regex("[^0-9]+"); //regex that matches disallowed text
        private void IsTextAllowed(object sender, TextCompositionEventArgs e)
        {
            e.Handled = _regex.IsMatch(e.Text);
        }
        public TimeSpan GetTimeSpan()
        {
            int.TryParse(hoursTextBox.Text, out int hours);
            int.TryParse(minutesTextBox.Text, out int minutes);
            int.TryParse(secondsTextBox.Text, out int seconds);
            int.TryParse(milisecondsTextBox.Text, out int miliseconds);

            TimeSpan time = new TimeSpan(0, hours, minutes, seconds, miliseconds);
            return time;
        }
        private void allButton_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan time = GetTimeSpan();
            foreach (Subtitle item in mainWindow.currentSubRedProject.SubtitlesList)
            {
                item.Start += time;
                item.End += time;
            }
        }

        private void selectedButton_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan time = GetTimeSpan();

            if (mainWindow.SubtitleGridMenuItem.IsChecked == false || mainWindow.SubtitleGrid.Visibility == Visibility.Collapsed)
            {
                if (mainWindow.SelectedIndexOfSubtitle >= 0)
                {
                    mainWindow.currentSubRedProject.SubtitlesList[mainWindow.SelectedIndexOfSubtitle].Start += time;
                    mainWindow.currentSubRedProject.SubtitlesList[mainWindow.SelectedIndexOfSubtitle].End += time;
                }
            }
            else
            {
                foreach (Subtitle item in mainWindow.SubtitleGrid.SelectedItems)
                {
                    mainWindow.currentSubRedProject.SubtitlesList[item.Id].Start += time;
                    mainWindow.currentSubRedProject.SubtitlesList[item.Id].End += time;
                }
            }
        }
    }
}
