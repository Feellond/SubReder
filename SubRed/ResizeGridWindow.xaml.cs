using Emgu.CV;
using Microsoft.Win32;
using SubRed.Sub_formats;
using System;
using System.Collections.Generic;
using System.DirectoryServices.ActiveDirectory;
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
    /// Логика взаимодействия для ResizeGridWindow.xaml
    /// </summary>
    public partial class ResizeGridWindow : Window
    {
        List<SubProject> projectList = new List<SubProject>();
        public ResizeGridWindow()
        {
            InitializeComponent();
        }

        public void LoadWindow(string[] fileNames)
        {
            projectList = new List<SubProject>();
            foreach (string fileName in fileNames)
            {
                SubProject project = new SubProject();
                project = SubFormats.SelectFormat(fileName, project, true).Result;
                project.Filename = fileName;
                projectList.Add(project);
            }

            FilesDataGrid.ItemsSource = projectList;
        }

        private void changeAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(SubProject project in projectList)
                ChangeResolution(project);
        }

        public void ChangeResolution(SubProject currentProject)
        {
            int xResult = 1, yResult = 1;
            if (int.TryParse(xNameTextBox.Text, out xResult) || int.TryParse(yNameTextBox.Text, out yResult))
            {
                int xPrev = 1;
                int yPrev = 1;

                int.TryParse(currentProject.PlayResX, out xPrev);
                int.TryParse(currentProject.PlayResY, out yPrev);

                double xMult = xResult / xPrev;
                double yMult = yResult / yPrev;

                foreach (var style in currentProject.SubtitleStyleList)
                {
                    if (xMult != 1 && yMult != 1)
                        style.Fontsize = (int)(style.Fontsize * ((xPrev * yPrev) / (xResult * yResult)));
                    style.ScaleX = (int)(xMult * style.ScaleX);
                    style.ScaleY = (int)(yMult * style.ScaleY);
                }

                foreach (var sub in currentProject.SubtitlesList)
                {
                    sub.Style = currentProject.SubtitleStyleList.Find(x => x.Name == sub.Style.Name);
                }

                currentProject.PlayResX = xNameTextBox.Text;
                currentProject.PlayResY = yNameTextBox.Text;
            }
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilesDataGrid.SelectedItem != null)
            {
                SubProject subProject = FilesDataGrid.SelectedItem as SubProject;
                SettingsWindow settingsWindow = new SettingsWindow(subProject);
                settingsWindow.filesButton.IsEnabled = false;
                settingsWindow.filesButton.Visibility = Visibility.Hidden;
                settingsWindow.ShowDialog();
            }
            else
                MessageBox.Show("Не выбран файл в таблице");
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            foreach(SubProject subProject in projectList)
            {
                SubFormats.SelectFormat(subProject.Filename, subProject, false);
            }
            this.Close();
        }
    }
}
