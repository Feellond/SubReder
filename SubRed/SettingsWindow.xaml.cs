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
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public MainWindow mainWindow;
        public SubProject currentSubRedProject;
        public SettingsWindow()
        {
            InitializeComponent();
        }
        public SettingsWindow(SubProject project, MainWindow mainWindow)
        {
            InitializeComponent();
            this.currentSubRedProject = project;
            nameTextBox.Text = currentSubRedProject.Title;
            originalScriptTextBox.Text = currentSubRedProject.OriginalScript;
            originalTranslationTextBox.Text = currentSubRedProject.OriginalTranslation;
            scriptTypeTextBox.Text = currentSubRedProject.ScriptType;
            collisionTextBox.Text = currentSubRedProject.Collisions;
            timerTextBox.Text = currentSubRedProject.Timer;
            syncTextBox.Text = currentSubRedProject.SyncPoint;

            x_resolutionTextBox.Text = currentSubRedProject.PlayResX;
            y_resolutionTextBox.Text = currentSubRedProject.PlayResY;

            this.mainWindow = mainWindow;
        }

        private void nameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentSubRedProject.Title = nameTextBox.Text;
        }

        private void originalScriptTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentSubRedProject.OriginalScript = originalScriptTextBox.Text;
        }

        private void originalTranslationTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentSubRedProject.OriginalTranslation = originalTranslationTextBox.Text;
        }

        private void scriptTypeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentSubRedProject.ScriptType = scriptTypeTextBox.Text;
        }

        private void collisionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentSubRedProject.Collisions = collisionTextBox.Text;
        }

        private void timerTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentSubRedProject.Timer = timerTextBox.Text;
        }

        private void syncTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentSubRedProject.SyncPoint = syncTextBox.Text;
        }

        private void fromVideoButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void changeButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void filesButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
