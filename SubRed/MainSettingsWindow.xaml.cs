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
    /// Логика взаимодействия для MainSettingsWindow.xaml
    /// </summary>
    public partial class MainSettingsWindow : Window
    {
        public MainSettingsWindow()
        {
            InitializeComponent();
            proxiTextBox.Text = TranslatorAPI.userProxy;
        }

        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            TranslatorAPI.userProxy = proxiTextBox.Text;
            this.Close();
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
