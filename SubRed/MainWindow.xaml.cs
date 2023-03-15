using Microsoft.Win32;
using SubRed.Sub_formats;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
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

        #region

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            currentSubRedProject = new SubProject();

            UIdispatcher = this.Dispatcher;

            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            // Default installation path of VideoLAN.LibVLC.Windows
            var libDirectory = new DirectoryInfo(System.IO.Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
            var options = new string[]
            {
                // VLC options can be given here. Please refer to the VLC command line documentation.
                "--no-sub-autodetect-file",
                "--sub-autodetect-fuzzy=1"
            };

            LastFilePlay = Properties.Settings.Default.LastFilePlay;
            LastTime = Properties.Settings.Default.LastTime;

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
        private void UpdateForm()
        {
            ViewGrid();
            ViewSubtitleTab();
        }

        public void ViewSubtitleTab()
        {
            subListGrid.Children.Add();
            for(int index = 0; index < currentSubRedProject.SubtitlesList.Count; index++)
            {
                StackPanel sp = new StackPanel { 
                    Orientation = Orientation.Horizontal,
                    Height = 100,
                    VerticalAlignment= VerticalAlignment.Top,
                    Margin = new Thickness(3)
                };
                sp.SetValue(Grid.RowProperty, index);

                Border border = new Border {
                    Background = new SolidColorBrush(Colors.GhostWhite),
                    BorderBrush = new SolidColorBrush(Colors.Silver),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8, 8, 8, 8)
                };

                Grid globalGrid = new Grid();
                Grid innerGrid = new Grid { Margin = new Thickness(3) };
                innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto});
                innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                Grid labelGrid = new Grid { Margin = new Thickness(3) };
                labelGrid.SetValue(Grid.ColumnProperty, 0);
                Label idLabel = new Label { Name = "idLabel" + index.ToString(), Content = index.ToString() };
                labelGrid.Children.Add(idLabel);
                innerGrid.Children.Add(labelGrid);
                
                Grid timeGrid = new Grid { Margin= new Thickness(3) };
                timeGrid.SetValue(Grid.ColumnProperty, 1);
                timeGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                timeGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                timeGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                TextBox beginTextBox = new TextBox { Name = "beginTextBox" + index.ToString(), Text = "00:00:00.0000" };
                beginTextBox.SetValue(Grid.RowProperty, 0);
                TextBox endTextBox = new TextBox { Name = "endTextBox" + index.ToString(), Text = "00:00:00.0000" };
                endTextBox.SetValue(Grid.RowProperty, 1);
                timeGrid.Children.Add(beginTextBox);
                timeGrid.Children.Add(endTextBox);
                innerGrid.Children.Add(timeGrid);

                Grid textBlockGrid = new Grid { Margin = new Thickness(3) };
                TextBlock subtitleTextBlock = new TextBlock { 
                    Name = "subtitleTextBlock",
                    Text = "Text", 
                    Background = new SolidColorBrush(Colors.White)
                };
                textBlockGrid.Children.Add(subtitleTextBlock);
                innerGrid.Children.Add(textBlockGrid);

                #region Правая колонка
                Grid selectionGrid = new Grid { Margin = new Thickness(3) };
                selectionGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                selectionGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                selectionGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                selectionGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                selectionGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                selectionGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                selectionGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                ComboBox styleSelectComboBox = new ComboBox { Name = "StyleSelectComboBox" + index.ToString() };
                styleSelectComboBox.SetValue(Grid.RowProperty, 0);
                styleSelectComboBox.SetValue(Grid.ColumnProperty, 0);
                styleSelectComboBox.SetValue(Grid.ColumnSpanProperty, 4);
                ComboBoxItem itemSelected = new ComboBoxItem { Name = "item1", IsSelected = true};
                StackPanel panelForItem = new StackPanel();
                TextBlock blockForPanel = new TextBlock {Text = "---"};
                panelForItem.Children.Add(blockForPanel);
                itemSelected.Content = panelForItem;
                styleSelectComboBox.Items.Add(itemSelected);
                selectionGrid.Children.Add(styleSelectComboBox);

                Label hLabel = new Label {Content = "H"};
                hLabel.SetValue(Grid.RowProperty, 1);
                hLabel.SetValue(Grid.ColumnProperty, 0);
                selectionGrid.Children.Add(hLabel);

                ComboBox hSelectComboBox = new ComboBox();
                hSelectComboBox.SetValue(Grid.RowProperty, 1);
                hSelectComboBox.SetValue(Grid.ColumnProperty, 1);
                itemSelected = new ComboBoxItem { Name = "hItem1", IsSelected = true};
                panelForItem = new StackPanel();
                blockForPanel = new TextBlock { Text = "Центр"};
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
                selectionGrid.Children.Add(xTextBox);

                Label yLabel = new Label { Content = "Y" };
                yLabel.SetValue(Grid.RowProperty, 2);
                yLabel.SetValue(Grid.ColumnProperty, 2);
                selectionGrid.Children.Add(yLabel);

                TextBox yTextBox = new TextBox();
                yTextBox.SetValue(Grid.RowProperty, 2);
                yTextBox.SetValue(Grid.ColumnProperty, 3);
                selectionGrid.Children.Add(yTextBox);
                #endregion
                innerGrid.Children.Add(selectionGrid);
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

        /// <summary>
        /// Окно для выгрузки обнаруженных субтитров из файла
        /// </summary>
        private void ExtractSubsFromFile_Click(object sender, RoutedEventArgs e)
        {
            ExtractSubsWindow ESWindow = new ExtractSubsWindow(currentSubRedProject.SubtitlesList);
            if (ESWindow.ShowDialog() == true)
            {
                currentSubRedProject.SubtitlesList = new List<Subtitle>();
                currentSubRedProject.SubtitlesList.AddRange(ESWindow.globalListOfSubs);
                ViewGrid();
            }
        }

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
        #region Методы загрузки и сохранения субтитров
        private void SrtSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFile(".srt");
        }
        private void AssSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFile(".ass");
        }
        private void SrtLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadFile(".srt");
        }
        private void AssLoad_Click(object sender, RoutedEventArgs e)
        {
            LoadFile(".ass");
        }
        private void SaveFile(string format = "")
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "All Files|*.*"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                SubFormats.SelectFormat(saveFileDialog.FileName, currentSubRedProject, false, format);
            }
        }

        private void LoadFile(string format = "")
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "All Files|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                SubFormats.SelectFormat(openFileDialog.FileName, currentSubRedProject, true, format);
            }
        }
        #endregion
        private void Row_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (player.SourceProvider.MediaPlayer.State == MediaStates.Playing)
                    ThreadPool.QueueUserWorkItem(_ => player.SourceProvider.MediaPlayer.Pause());

                Subtitle dataRow = (Subtitle)SubtitleGrid.SelectedItem;
                int index = 4;
                int frameNum = (int)(dataRow.FrameBeginNum * player.SourceProvider.MediaPlayer.FramesPerSecond);
                slider.Value = frameNum;

                player.SourceProvider.MediaPlayer.Time = (long)(frameNum);
                /*
                if (SubtitleGrid.SelectedItem != null)
                    if (player.SourceProvider.MediaPlayer != null)
                        if (!string.IsNullOrWhiteSpace(LastFilePlay) && File.Exists(LastFilePlay))
                            if (globalListOfSubs.Count > 0)
                            {
                                DataGridRow row = sender as DataGridRow;
                                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(rowContainer);

                                // try to get the cell but it may possibly be virtualized
                                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                                if (cell == null)
                                {
                                    // now try to bring into view and retreive the cell
                                    dataGrid.ScrollIntoView(rowContainer, dataGrid.Columns[column]);

                                    cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                                }


                                int ID = Convert.ToInt32(row);
                                player.SourceProvider.MediaPlayer.Time = (long)(globalListOfSubs[ID - 1].frameBeginNum * player.SourceProvider.MediaPlayer.FramesPerSecond * 1000);
                            }*/
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabControl.SelectedIndex == 0)
            {
                tabControl.SelectedIndex = 1;
            }
        }
    }
}
