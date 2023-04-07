using Emgu.CV.Flann;
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
    /// Логика взаимодействия для FindChangeWindow.xaml
    /// </summary>
    public partial class FindChangeWindow : Window
    {
        MainWindow mainWindow;
        List<int> foundedIdList;
        int currentIndex;
        public FindChangeWindow()
        {
            InitializeComponent();
        }

        public void LoadWindow(MainWindow window)
        {
            mainWindow = window;
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void changeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void changeAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(var id in foundedIdList)
            {
                var sub = mainWindow.currentSubRedProject.SubtitlesList.Find(x => x.Id == id);
                sub.Text = sub.Text.Replace(findTextBox.Text, changeToTextBox.Text);
            }
        }

        private void findTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var subList = mainWindow.currentSubRedProject.SubtitlesList.ToList();
            foundedIdList.Clear();
            currentIndex = 0;
            foreach (var sub in subList)
            {
                if (sub.Text.IndexOf(findTextBox.Text) != -1)
                    foundedIdList.Add(sub.Id);
            }

            if (foundedIdList.Count == 0)
                foundedLabel.Content = "0 из 0";
            else
            {
                currentIndex++;
                foundedLabel.Content = currentIndex + " из " + foundedIdList.Count;
            }
        }
    }
}
