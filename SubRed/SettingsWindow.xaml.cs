using Microsoft.Win32;
using System.Windows;

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
            this.currentSubRedProject               = project;
            nameTextBox.Text                        = currentSubRedProject.Title;
            originalScriptTextBox.Text              = currentSubRedProject.OriginalScript;
            originalTranslationTextBox.Text         = currentSubRedProject.OriginalTranslation;
            scriptTypeTextBox.Text                  = currentSubRedProject.ScriptType;
            collisionTextBox.Text                   = currentSubRedProject.Collisions;
            timerTextBox.Text                       = currentSubRedProject.Timer;
            syncTextBox.Text                        = currentSubRedProject.SyncPoint;

            x_resolutionTextBox.Text                = currentSubRedProject.PlayResX;
            y_resolutionTextBox.Text                = currentSubRedProject.PlayResY;

            this.mainWindow = mainWindow;
        }

        public SettingsWindow(SubProject project)
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
        }

        private void fromVideoButton_Click(object sender, RoutedEventArgs e)
        {
            if (mainWindow.player.SourceProvider.MediaPlayer != null)
            {
                var x = mainWindow.player.Height;
                var y = mainWindow.player.Width;

                x_resolutionTextBox.Text = ((int)x).ToString();
                y_resolutionTextBox.Text = ((int)y).ToString();
            }
        }

        private void changeButton_Click(object sender, RoutedEventArgs e)
        {
            int xResult = 1, yResult = 1;
            if (int.TryParse(x_resolutionTextBox.Text, out xResult) || int.TryParse(y_resolutionTextBox.Text, out yResult))
            {
                int xPrev = 1;
                int yPrev = 1;

                int.TryParse(mainWindow.currentSubRedProject.PlayResX, out xPrev);
                int.TryParse(mainWindow.currentSubRedProject.PlayResY, out yPrev);

                double xMult = xResult / xPrev;
                double yMult = yResult / yPrev;

                foreach (var style in mainWindow.currentSubRedProject.SubtitleStyleList)
                {
                    if (xMult != 1 && yMult != 1)
                        style.Fontsize = (int)(style.Fontsize * ((xPrev * yPrev) / (xResult * yResult)));
                    style.ScaleX = (int)(xMult * style.ScaleX);
                    style.ScaleY = (int)(yMult * style.ScaleY);
                }

                foreach (var sub in mainWindow.currentSubRedProject.SubtitlesList)
                {
                    sub.Style = mainWindow.currentSubRedProject.SubtitleStyleList.Find(x => x.Name == sub.Style.Name);
                }

                mainWindow.currentSubRedProject.PlayResX = x_resolutionTextBox.Text;
                mainWindow.currentSubRedProject.PlayResY = y_resolutionTextBox.Text;
            }
        }

        private void filesButton_Click(object sender, RoutedEventArgs e)
        {
            ResizeGridWindow resizeGridWindow = new ResizeGridWindow();
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "All Files|*.*",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                resizeGridWindow.LoadWindow(openFileDialog.FileNames);
                resizeGridWindow.ShowDialog();
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            currentSubRedProject.Title = nameTextBox.Text;
            currentSubRedProject.OriginalScript = originalScriptTextBox.Text;
            currentSubRedProject.OriginalTranslation = originalTranslationTextBox.Text;
            currentSubRedProject.ScriptType = scriptTypeTextBox.Text;
            currentSubRedProject.Collisions = collisionTextBox.Text;
            currentSubRedProject.Timer = timerTextBox.Text;
            currentSubRedProject.SyncPoint = syncTextBox.Text;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
