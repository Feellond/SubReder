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
    /// Логика взаимодействия для StyleListWindow.xaml
    /// </summary>
    public partial class StyleListWindow : Window
    {
        public SubProject currentProject;
        public List<SubtitleStyle> stylesList;
        public StyleListWindow()
        {
            stylesList = new()
            {
                new SubtitleStyle()
            };
            InitializeComponent();
        }

        public void LoadWindow(SubProject project, List<SubtitleStyle> newStylesList)
        {
            currentProject = project;
            stylesList = new List<SubtitleStyle>(newStylesList);
            foreach (var style in stylesList)
                stylesListBox.Items.Add(style.Name);
        }
        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            SubtitleStyle newStyle = new SubtitleStyle();
            stylesListBox.Items.Add(newStyle);
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            int index = stylesListBox.Items.IndexOf(stylesListBox.SelectedItem);
            if (index != -1)
            {
                StyleSettingsWindow ssWin = new StyleSettingsWindow();
                ssWin.LoadWindow(index, currentProject, stylesList[index]);
                ssWin.ShowDialog();
            }
        }

        private void copyButton_Click(object sender, RoutedEventArgs e)
        {
            int index = stylesListBox.Items.IndexOf(stylesListBox.SelectedItem);
            if (index != -1)
                stylesListBox.Items.Add(stylesList[index]);
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            int index = stylesListBox.Items.IndexOf(stylesListBox.SelectedItem);
            if (index != -1)
                if (MessageBox.Show("Удалить стиль под именем: " + stylesList[index].Name, "Удалить стиль?", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    stylesList.RemoveAt(index);
                    stylesListBox.Items.RemoveAt(index);
                }
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            currentProject.SubtitleStyleList = new List<SubtitleStyle>(stylesList);
            this.Close();
        }
    }
}
