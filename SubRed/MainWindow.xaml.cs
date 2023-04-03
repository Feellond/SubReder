using Microsoft.Win32;
using SubRed.Sub_formats;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Vlc.DotNet.Core.Interops.Signatures;

namespace SubRed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SubProject currentSubRedProject;

        #region Работа с видеофайлом
        public readonly Dispatcher UIdispatcher;
        private void ActionDispatcher(Action action)
            => UIdispatcher.BeginInvoke(action, null);
        private string LastFilePlay;
        private long LastTime;
        #endregion

        #region Оконные переменные
        int SelectedIndexOfSubtitle;
        #endregion

        private Border? prevBorder;
        private System.Windows.Media.Color defaultBackgroundColor = Colors.Gainsboro;
        private System.Windows.Media.Color selectedBackgroundColor = Colors.SteelBlue;

        public MainWindow()
        {
            InitializeComponent();

            currentSubRedProject = new SubProject();

            UIdispatcher = this.Dispatcher;

            SelectedIndexOfSubtitle = -1;
            RestartVideoPlayer();
            UpdateWindow();
        }

        private void RestartVideoPlayer(string SubFileName = "")
        {
            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            // Default installation path of VideoLAN.LibVLC.Windows
            var libDirectory = new DirectoryInfo(System.IO.Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
            var options = new string[]
            {
                // VLC options can be given here. Please refer to the VLC command line documentation.
                "--no-sub-autodetect-file",
                "--sub-autodetect-fuzzy=1",
                "--sub-file=" + SubFileName
            };

            LastFilePlay = Properties.Settings.Default.LastFilePlay;
            LastTime = Properties.Settings.Default.LastTime;

            if (player.SourceProvider.MediaPlayer != null) player.SourceProvider.MediaPlayer.ResetMedia();
            //player.SourceProvider.MediaPlayer.ResetPlayer();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                player.SourceProvider.CreatePlayer(libDirectory/* pass your player parameters here */, options);
                player.SourceProvider.MediaPlayer.LengthChanged += MediaPlayer_LengthChanged;
                player.SourceProvider.MediaPlayer.TimeChanged += MediaPlayer_TimeChanged;
                player.SourceProvider.MediaPlayer.Paused += (s, e) => StateCheck();
                player.SourceProvider.MediaPlayer.Playing += (s, e) => StateCheck();
                player.SourceProvider.MediaPlayer.Stopped += (s, e) => StateCheck();
                player.SourceProvider.MediaPlayer.Opening += (s, e) => StateCheck();

                if (!string.IsNullOrWhiteSpace(LastFilePlay) && File.Exists(LastFilePlay))
                {
                    //player.SourceProvider.MediaPlayer.Play(new Uri(LastFilePlay));
                    player.SourceProvider.MediaPlayer.Pause();
                    player.SourceProvider.MediaPlayer.Time = LastTime;
                }
                StateCheck();
            });
        }

        /// <summary>
        /// Создание нового проекта по загрузке
        /// </summary>
        private void NewLoad()
        {

        }

        /// <summary>
        /// Полное обновление всех данных в окне
        /// </summary>
        private async void UpdateWindow()
        {
            ViewGrid();
            await ViewSubtitleTab(this.subListGrid);
        }

        public async Task ViewSubtitleTab(Grid subListGrid)
        {
            subListGrid.Children.Clear();
            subListGrid.RowDefinitions.Clear();

            for (int index = 0; index < currentSubRedProject.SubtitlesList.Count; index++)
            {
                subListGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                StackPanel sp = new StackPanel {
                    Orientation = Orientation.Horizontal,
                    Height = 120,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(3),
                    Name = "SubtitleStackPanel_" + index.ToString()
                };
                sp.SetValue(Grid.RowProperty, index);

                Border border = new Border {
                    Background = new SolidColorBrush(defaultBackgroundColor),
                    BorderBrush = new SolidColorBrush(Colors.Silver),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8, 8, 8, 8),
                    Name = "SubtitleBorder_" + index.ToString(),
                };
                border.MouseLeftButtonUp += Border_MouseLeftButtonUp;
                try
                {
                    this.RegisterName(border.Name, border);
                }
                catch { }

                Grid globalGrid = new Grid();
                Grid innerGrid = new Grid { Margin = new Thickness(3) };
                innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                Grid labelGrid = new Grid { Margin = new Thickness(3), VerticalAlignment = VerticalAlignment.Top };
                labelGrid.SetValue(Grid.ColumnProperty, 0);
                Label idLabel = new Label {
                    Name = "idLabel_" + index.ToString(),
                    Content = index.ToString(),
                    MinWidth = 42,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };
                labelGrid.Children.Add(idLabel);
                innerGrid.Children.Add(labelGrid);

                Grid timeGrid = new Grid { Margin = new Thickness(3), VerticalAlignment = VerticalAlignment.Top };
                timeGrid.SetValue(Grid.ColumnProperty, 1);
                timeGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                timeGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                timeGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                TextBox beginTextBox = new TextBox { Name = "beginTextBox_" + index.ToString(), MinWidth = 87,
                    Text = currentSubRedProject.SubtitlesList[index].Start.ToString("hh\\:mm\\:ss\\.FFFF"),};
                beginTextBox.SetValue(Grid.RowProperty, 0);
                beginTextBox.TextChanged += startSubtitleTextBox_TextChanged;
                TextBox endTextBox = new TextBox { Name = "endTextBox_" + index.ToString(), MinWidth = 87,
                    Text = currentSubRedProject.SubtitlesList[index].End.ToString("hh\\:mm\\:ss\\.FFFF")
                };
                endTextBox.SetValue(Grid.RowProperty, 1);
                endTextBox.TextChanged += endSubtitleTextBox_TextChanged;
                timeGrid.Children.Add(beginTextBox);
                timeGrid.Children.Add(endTextBox);
                innerGrid.Children.Add(timeGrid);

                Grid textBlockGrid = new Grid { Margin = new Thickness(3) };
                textBlockGrid.SetValue(Grid.ColumnProperty, 2);
                ScrollViewer scrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                };
                RichTextBox richTextBox = new RichTextBox { Width = 250, Name = "subtitleRichTextBox_" + index.ToString() };
                richTextBox.AppendText(currentSubRedProject.SubtitlesList[index].Text);
                richTextBox.TextChanged += subtitleTextRichTextBox_TextChanged;
                scrollViewer.Content = richTextBox;
                textBlockGrid.Children.Add(scrollViewer);
                innerGrid.Children.Add(textBlockGrid);

                #region Правая колонка
                Grid selectionGrid = new Grid { Margin = new Thickness(3) };
                selectionGrid.SetValue(Grid.ColumnProperty, 3);
                selectionGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                selectionGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                selectionGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                selectionGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                selectionGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                selectionGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                selectionGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                ComboBox styleSelectComboBox = new ComboBox { Name = "StyleSelectComboBox_" + index.ToString() };
                styleSelectComboBox.SetValue(Grid.RowProperty, 0);
                styleSelectComboBox.SetValue(Grid.ColumnProperty, 0);
                styleSelectComboBox.SetValue(Grid.ColumnSpanProperty, 4);
                styleSelectComboBox.SelectionChanged += styleSelectComboBox_SelectionChanged;
                ComboBoxItem itemSelected = new ComboBoxItem { Name = "item1", IsSelected = true };
                StackPanel panelForItem = new StackPanel();
                TextBlock blockForPanel = new TextBlock { Text = "---" };
                panelForItem.Children.Add(blockForPanel);
                itemSelected.Content = panelForItem;
                styleSelectComboBox.Items.Add(itemSelected);
                selectionGrid.Children.Add(styleSelectComboBox);

                Label hLabel = new Label { Content = "H" };
                hLabel.SetValue(Grid.RowProperty, 1);
                hLabel.SetValue(Grid.ColumnProperty, 0);
                selectionGrid.Children.Add(hLabel);

                ComboBox hSelectComboBox = new ComboBox();
                hSelectComboBox.SetValue(Grid.RowProperty, 1);
                hSelectComboBox.SetValue(Grid.ColumnProperty, 1);
                hSelectComboBox.SelectionChanged += heightSelectComboBox_SelectionChanged;
                itemSelected = new ComboBoxItem { Name = "hItem1", IsSelected = true };
                panelForItem = new StackPanel();
                blockForPanel = new TextBlock { Text = "Центр" };
                panelForItem.Children.Add(blockForPanel);
                itemSelected.Content = panelForItem;
                hSelectComboBox.Items.Add(itemSelected);
                selectionGrid.Children.Add(hSelectComboBox);

                Label vLabel = new Label { Content = "V" };
                vLabel.SetValue(Grid.RowProperty, 1);
                vLabel.SetValue(Grid.ColumnProperty, 2);
                selectionGrid.Children.Add(vLabel);

                ComboBox vSelectComboBox = new ComboBox();
                vSelectComboBox.SetValue(Grid.RowProperty, 1);
                vSelectComboBox.SetValue(Grid.ColumnProperty, 3);
                vSelectComboBox.SelectionChanged += verticalSelectComboBox_SelectionChanged;
                itemSelected = new ComboBoxItem { Name = "vItem1", IsSelected = true };
                panelForItem = new StackPanel();
                blockForPanel = new TextBlock { Text = "Низ" };
                panelForItem.Children.Add(blockForPanel);
                itemSelected.Content = panelForItem;
                vSelectComboBox.Items.Add(itemSelected);
                selectionGrid.Children.Add(vSelectComboBox);

                Label xLabel = new Label { Content = "X" };
                xLabel.SetValue(Grid.RowProperty, 2);
                xLabel.SetValue(Grid.ColumnProperty, 0);
                selectionGrid.Children.Add(xLabel);

                TextBox xTextBox = new TextBox();
                xTextBox.SetValue(Grid.RowProperty, 2);
                xTextBox.SetValue(Grid.ColumnProperty, 1);
                xTextBox.TextChanged += xCoordTextBox_TextChanged;
                selectionGrid.Children.Add(xTextBox);

                Label yLabel = new Label { Content = "Y" };
                yLabel.SetValue(Grid.RowProperty, 2);
                yLabel.SetValue(Grid.ColumnProperty, 2);
                selectionGrid.Children.Add(yLabel);

                TextBox yTextBox = new TextBox();
                yTextBox.SetValue(Grid.RowProperty, 2);
                yTextBox.SetValue(Grid.ColumnProperty, 3);
                yTextBox.TextChanged += yCoordTextBox_TextChanged;
                selectionGrid.Children.Add(yTextBox);
                #endregion
                innerGrid.Children.Add(selectionGrid);

                globalGrid.Children.Add(innerGrid);
                border.Child = globalGrid;
                sp.Children.Add(border);
                subListGrid.Children.Add(sp);
            }
        }

        /// <summary>
        /// Отображение субтитров в таблице
        /// </summary>
        public void ViewGrid()
        {
            //https://social.msdn.microsoft.com/Forums/en-US/47ce71aa-5bde-482a-9574-764e45cb9031/bind-list-to-datagrid-in-wpf?forum=wpf
            this.SubtitleGrid.ItemsSource = null;
            this.SubtitleGrid.ItemsSource = currentSubRedProject.SubtitlesList;

            //imageProgressBar.Visibility = Visibility.Hidden;
        }

        public void ViewSubtitleOnVideo()
        {

        }

        /// <summary>
        /// Окно для выгрузки обнаруженных субтитров из файла
        /// </summary>
        private void ExtractSubsFromFile_Click(object sender, RoutedEventArgs e)
        {
            ExtractSubsWindow ESWindow = new ExtractSubsWindow(currentSubRedProject.SubtitlesList);
            if (ESWindow.ShowDialog() == true)
            {
                currentSubRedProject.SubtitlesList = new List<Subtitle>();
                currentSubRedProject.SubtitlesList.AddRange(ESWindow.globalProject.SubtitlesList);
                UpdateWindow();
            }
        }

        #region Работа с видеофайлом VLC
        private void MediaPlayer_TimeChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerTimeChangedEventArgs e)
        {
            if (isSliderDragStarted)
                return;
            StateCheck();

            ActionDispatcher(() =>
            {
                if (isSliderDragStarted)
                    return;
                slider.Value = player.SourceProvider.MediaPlayer.Time;
                CurrentTimeVideoTextBox.Text = player.SourceProvider.MediaPlayer.Time.ToString();
                if (LastTime + 20_000 < player.SourceProvider.MediaPlayer.Time)
                    Properties.Settings.Default.LastTime = LastTime = player.SourceProvider.MediaPlayer.Time + 10_000;
            });

        }

        private void MediaPlayer_LengthChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerLengthChangedEventArgs e)
        {
            ActionDispatcher(() => slider.Maximum = player.SourceProvider.MediaPlayer.Length);
            StateCheck();
        }

        protected bool isValueChanged = false;

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    player.SourceProvider.MediaPlayer.Play(new Uri(openFileDialog.FileName));
                    Properties.Settings.Default.LastFilePlay = LastFilePlay = openFileDialog.FileName;
                });
        }

        private void StateCheck()
        {
            string content = "Старт";
            switch (player.SourceProvider.MediaPlayer.State)
            {
                case MediaStates.Playing: content = "Пауза"; break;
                case MediaStates.Paused: content = "Продолжить"; break;
            }
            ActionDispatcher(() => StartPausePlayButton.Content = content);

        }

        private void StartPausePlay_Click(object sender, RoutedEventArgs e)
        {
            if (player.SourceProvider.MediaPlayer.State == MediaStates.Playing)
                ThreadPool.QueueUserWorkItem(_ => player.SourceProvider.MediaPlayer.Pause());
            else if (player.SourceProvider.MediaPlayer.State == MediaStates.Paused)
                ThreadPool.QueueUserWorkItem(_ => player.SourceProvider.MediaPlayer.Play());
            else if (!string.IsNullOrWhiteSpace(LastFilePlay) && File.Exists(LastFilePlay))
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    player.SourceProvider.MediaPlayer.Play(new Uri(LastFilePlay));
                    player.SourceProvider.MediaPlayer.Time = 0;
                });
        }

        private void Stop_Click(object sender, RoutedEventArgs e)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                ActionDispatcher(() => slider.Value = 0);
                player.SourceProvider.MediaPlayer.Stop();
                player.SourceProvider.MediaPlayer.Time = 0;
            });
        }

        private bool isSliderDragStarted = false;

        private void SliderDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
            => isSliderDragStarted = true;

        private void slider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
            => isSliderDragStarted = false;

        private void slider_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            long time = (long)slider.Value;
            ThreadPool.QueueUserWorkItem(_ => player.SourceProvider.MediaPlayer.Time = time);
        }
        private void slider_MouseUp(object sender, MouseButtonEventArgs e)
        {
            isSliderDragStarted = false;
            if (player.SourceProvider.MediaPlayer.State == MediaStates.Playing)
                ThreadPool.QueueUserWorkItem(_ => player.SourceProvider.MediaPlayer.Pause());

            int frameNum = (int)slider.Value;
            player.SourceProvider.MediaPlayer.Time = (long)(frameNum);
            
            ThreadPool.QueueUserWorkItem(_ => player.SourceProvider.MediaPlayer.Play());
        }
        #endregion

        #region Методы загрузки и сохранения субтитров
        private void SrtSave_Click(object sender, RoutedEventArgs e) => SaveSubFile(".srt");
        private void AssSave_Click(object sender, RoutedEventArgs e) => SaveSubFile(".ass");
        private void SsaSave_Click(object sender, RoutedEventArgs e) => SaveSubFile(".ssa");
        private void SmiSave_Click(object sender, RoutedEventArgs e) => SaveSubFile(".smi");
        private void SrtLoad_Click(object sender, RoutedEventArgs e) => LoadSubFile(".srt");
        private void AssLoad_Click(object sender, RoutedEventArgs e) => LoadSubFile(".ass");
        private void SsaLoad_Click(object sender, RoutedEventArgs e) => LoadSubFile(".ssa");
        private void SmiLoad_Click(object sender, RoutedEventArgs e) => LoadSubFile(".smi");
        private async void SaveSubFile(string format = "")
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "All Files|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                await SubFormats.SelectFormat(saveFileDialog.FileName, currentSubRedProject, false, format);
            }
        }

        private async void LoadSubFile(string format = "")
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "All Files|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                await SubFormats.SelectFormat(openFileDialog.FileName, currentSubRedProject, true, format);
            }

            if (player.SourceProvider.MediaPlayer.Length >= 0)
                RestartVideoPlayer(openFileDialog.FileName);

            UpdateWindow();
        }
        #endregion

        #region Методы выбора субтитров в таблице и в списке
        private void Row_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Subtitle dataRow = (Subtitle)SubtitleGrid.SelectedItem;
                var newFoundedBorder = this.FindName("SubtitleBorder_" + dataRow.Id) as Border;
                SelectSubtitle(newFoundedBorder, dataRow.Id);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error in Row_Click method", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // TODO необходимо проверить, работает ли
        private void Border_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Border border = e.Source as Border;
            int currIndex = Convert.ToInt32(border.Name.Split('_')[1]);   // SubtitleBorder_1 -> (string)1 -> (int)1
            SelectSubtitle(border, currIndex);
        }

        private void SelectSubtitle(Border? border, int currIndex)
        {
            if (prevBorder != null) prevBorder.Background = new SolidColorBrush(defaultBackgroundColor);

            if (SelectedIndexOfSubtitle != currIndex)
            {
                if (border != null) border.Background = new SolidColorBrush(selectedBackgroundColor);
                SelectedIndexOfSubtitle = currIndex;
                prevBorder = border;

                SubtitleGrid.SelectedIndex = SelectedIndexOfSubtitle;
                SubtitleGrid.UpdateLayout();
                SubtitleGrid.ScrollIntoView(SubtitleGrid.SelectedItem);

                scrollViewerSubListGrid.ScrollToVerticalOffset(100 * SelectedIndexOfSubtitle);
                //------------------------//

                if (player.SourceProvider.MediaPlayer.State == MediaStates.Playing)
                    ThreadPool.QueueUserWorkItem(_ => player.SourceProvider.MediaPlayer.Pause());

                int frameNum = (int)(currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].Start.TotalMilliseconds);
                slider.Value = frameNum;
                player.SourceProvider.MediaPlayer.Time = (long)(frameNum) * 1000;
            }
            else SelectedIndexOfSubtitle = -1;

            IdSubtitleTextBox.Text = SelectedIndexOfSubtitle.ToString();
            CurrentTimeVideoTextBox.Text = player.SourceProvider.MediaPlayer.Time.ToString();
        }

        private static readonly Regex _regex = new Regex("[^0-9]+"); //regex that matches disallowed text
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) 
            => e.Handled = _regex.IsMatch(e.Text);

        private void IdSubtitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            int.TryParse(IdSubtitleTextBox.Text, out int currIndex);
            var foundedBorder = this.FindName("SubtitleBorder_" + currIndex) as Border;
            SelectSubtitle(foundedBorder, currIndex);
        }

        private void PrevSubtitleButton_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(IdSubtitleTextBox.Text, out int currIndex);

            if (currIndex > 0)
            {
                currIndex--;
                IdSubtitleTextBox.Text = currIndex.ToString();

                var foundedBorder = this.FindName("SubtitleBorder_" + currIndex) as Border;
                SelectSubtitle(foundedBorder, currIndex);
            }
        }

        private void NextSubtitleButton_Click(object sender, RoutedEventArgs e)
        {
            int.TryParse(IdSubtitleTextBox.Text, out int currIndex);

            if (currIndex < currentSubRedProject.SubtitlesList.Count - 1)
            {
                currIndex++;
                IdSubtitleTextBox.Text = currIndex.ToString();

                var foundedBorder = this.FindName("SubtitleBorder_" + currIndex) as Border;
                SelectSubtitle(foundedBorder, currIndex);
            }
        }
        #endregion

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl.SelectedIndex == 0) tabControl.SelectedIndex = 1;
        }

        #region Редактирование субтитров
        private Grid FindClickedItem(object sender)
        {
            MenuItem mi = sender as MenuItem;
            if (mi != null)
            {
                ContextMenu cm = mi.CommandParameter as ContextMenu;
                if (cm != null)
                {
                    Grid g = cm.PlacementTarget as Grid;
                    if (g != null)
                    {
                        return g;
                    }
                }
            }
            return null;
        }

        private void Add_OnClick(object sender, RoutedEventArgs e)
        {
            var clickedItem = FindClickedItem(sender);
            if (clickedItem != null)
            {
                if (SelectedIndexOfSubtitle >= 0)
                {
                    // Do this
                }
                else MessageBox.Show("Не выбран элемент, перед которым добавлять", "Ошибка добавления", MessageBoxButton.OK, MessageBoxImage.Question);
            }
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            var clickedItem = FindClickedItem(sender);
            if (clickedItem != null)
            {
                if (SelectedIndexOfSubtitle >= 0)
                {
                    MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить элемент id = " + SelectedIndexOfSubtitle
                        , "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        // Do this
                    }
                }
                else MessageBox.Show("Не выбран элемент для удаления", "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Question);
            }
        }

        #region Элементы редактирования субтитра в Panel
        private void startSubtitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
        }

        private void endSubtitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
        }

        private void subtitleTextRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var richTextBox = sender as RichTextBox;
        }

        private void styleSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
        }

        private void heightSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
        }

        private void verticalSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
        }

        private void xCoordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
        }

        private void yCoordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
        }
        #endregion

        #region Кнопки форматирования субтитров
        private void leftAlignButton_Click(object sender, RoutedEventArgs e)
        {
            SubtitleAlignmentChange(1);
        }

        private void centerAlignButton_Click(object sender, RoutedEventArgs e)
        {
            SubtitleAlignmentChange(2);
        }

        private void rightAlignButton_Click(object sender, RoutedEventArgs e)
        {
            SubtitleAlignmentChange(3);
        }

        public void SubtitleAlignmentChange(int alignment)
        {
            if (SelectedIndexOfSubtitle >= 0)
            {
                RichTextBox richTextBox = (RichTextBox)this.FindName("subtitleRichTextBox_" + SelectedIndexOfSubtitle.ToString());
                richTextBox.HorizontalAlignment = HorizontalAlignment.Left;

                currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].Style.HorizontalAlignment = 3;
            }
        }

        private void copyButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cutButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void pasteButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void boldButton_Click(object sender, RoutedEventArgs e)
        {
            SubtitleInTextChange("bold");
        }

        private void italicButton_Click(object sender, RoutedEventArgs e)
        {
            SubtitleInTextChange("italic");
        }

        private void underlineButton_Click(object sender, RoutedEventArgs e)
        {
            SubtitleInTextChange("underline");
        }

        private void strikethroughButton_Click(object sender, RoutedEventArgs e)
        {
            SubtitleInTextChange("strikethrough");
        }

        public void SubtitleInTextChange(string inTextString)
        {
            if (SelectedIndexOfSubtitle >= 0)
            {
                RichTextBox richTextBox = (RichTextBox)this.FindName("subtitleRichTextBox_" + SelectedIndexOfSubtitle.ToString());
                TextSelection text = richTextBox.Selection;

                if (text != null)
                {
                    var docStart = richTextBox.Document.ContentStart;

                    var selectionStart = richTextBox.Selection.Start;
                    var selectionEnd = richTextBox.Selection.End;

                    //these will give you the positions needed to apply highlighting
                    var indexStart = docStart.GetOffsetToPosition(selectionStart);
                    var indexEnd = docStart.GetOffsetToPosition(selectionEnd);

                    //these values will give you the absolute character positions relative to the very beginning of the text.
                    TextRange start = new TextRange(docStart, selectionStart);
                    TextRange end = new TextRange(docStart, selectionEnd);


                    System.Drawing.FontStyle fontStyle = System.Drawing.FontStyle.Regular;
                    switch (inTextString)
                    {
                        case "bold":
                            fontStyle = System.Drawing.FontStyle.Bold;
                            break;
                        case "italic":
                            fontStyle = System.Drawing.FontStyle.Italic;
                            break;
                        case "underline":
                            fontStyle = System.Drawing.FontStyle.Underline;
                            break;
                        case "strikethrough":
                            fontStyle = System.Drawing.FontStyle.Strikeout;
                            break;
                    }

                    Font font = new Font(currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].Style.Fontname,
                        currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].Style.Fontsize, fontStyle);
                    text.ApplyPropertyValue(RichTextBox.FontStyleProperty, font);
                    currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].ChangeInTextAction(indexStart, indexEnd, inTextString);
                }
                else
                {
                    switch (inTextString)
                    {
                        case "bold":
                            richTextBox.FontWeight = FontWeights.Bold;
                            currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].Style.Bold = true;
                            break;
                        case "italic":
                            richTextBox.FontStyle = FontStyles.Italic;
                            currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].Style.Italic = true;
                            break;
                        case "underline":
                            richTextBox.SelectAll();
                            richTextBox.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
                            richTextBox.Selection.Select(richTextBox.Document.ContentStart, richTextBox.Document.ContentStart);
                            currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].Style.Underline = true;
                            break;
                        case "strikethrough":
                            richTextBox.SelectAll();
                            richTextBox.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
                            richTextBox.Selection.Select(richTextBox.Document.ContentStart, richTextBox.Document.ContentStart);
                            currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].Style.StrikeOut = true;
                            break;
                    }
                }
            }

            UpdateWindow();
        }





        #endregion

        #endregion

        private void BackVideoButton_Click(object sender, RoutedEventArgs e)
        {
            player.SourceProvider.MediaPlayer.Time -= 10 * 1000;
        }

        private void ForwardVideoButton_Click(object sender, RoutedEventArgs e)
        {
            player.SourceProvider.MediaPlayer.Time += 10 * 1000;
        }

        private void EndVideoButton_Click(object sender, RoutedEventArgs e)
        {
            player.SourceProvider.MediaPlayer.Time = player.SourceProvider.MediaPlayer.Length;
        }

        #region Создание вкладки с переводом субтитров
        private void engTabAddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TabItem newTabItem = new TabItem
            {
                Header = "Eng",
                Name = "Test"
            };
            tabControl.Items.Add(newTabItem);
        }

        private void chiTabAddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TabItem newTabItem = new TabItem
            {
                Header = "Chi",
                Name = "Test"
            };
            tabControl.Items.Add(newTabItem);
        }

        private void jpnTabAddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TabItem newTabItem = new TabItem
            {
                Header = "Jpn",
                Name = "Test"
            };
            tabControl.Items.Add(newTabItem);
        }
        #endregion
    }
}
