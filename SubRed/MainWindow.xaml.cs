using Aspose.Words;
using ControlzEx.Standard;
using Emgu.CV.Flann;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using SubRed.Sub_formats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Vlc.DotNet.Core.Interops.Signatures;
using Border = System.Windows.Controls.Border;

namespace SubRed
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public SubProject currentSubRedProject;
        public Subtitle copiedSubtitle;
        public List<SubProject> subProjectHistory;

        #region Переменные видеофайла
        public readonly Dispatcher UIdispatcher;
        private void ActionDispatcher(Action action)
            => UIdispatcher.BeginInvoke(action, null);
        private string LastFilePlay;
        private long LastTime;
        #endregion

        #region Оконные переменные
        public int SelectedIndexOfSubtitle { get; set; }
        int copiedIndexSubtitle;
        string currentSubFileName;
        #endregion

        private Border? prevBorder;
        private System.Windows.Media.Color defaultBackgroundColor = Colors.LightCyan;
        private System.Windows.Media.Color selectedBackgroundColor = Colors.LightSkyBlue;

        public MainWindow()
        {
            currentSubRedProject = new SubProject();
            subProjectHistory = new List<SubProject>();
            SelectedIndexOfSubtitle = -1;
            InitializeComponent();
            UIdispatcher = this.Dispatcher;

            WindowFirstLoad();
        }

        public async void WindowFirstLoad()
        {
            SelectedIndexOfSubtitle = -1;
            await SubFormats.SelectFormat("currentProject.ass", currentSubRedProject, false, ".ass");

            RestartVideoPlayer("currentProject.ass");
            UpdateWindow();
        }

        private void RestartVideoPlayer(string SubFileName = "")
        {
            if (SubFileName != "")
                currentSubFileName = SubFileName;

            var currentAssembly = Assembly.GetEntryAssembly();
            var currentDirectory = new FileInfo(currentAssembly.Location).DirectoryName;
            // Default installation path of VideoLAN.LibVLC.Windows
            var libDirectory = new DirectoryInfo(System.IO.Path.Combine(currentDirectory, "libvlc", IntPtr.Size == 4 ? "win-x86" : "win-x64"));
            var options = new string[]
            {
                // VLC options can be given here. Please refer to the VLC command line documentation.
                "--no-sub-autodetect-file",
                "--sub-autodetect-fuzzy=1",
                "--sub-file=" + currentSubFileName
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
        /// Полное обновление всех данных в окне
        /// </summary>
        public async void UpdateWindow()
        {
            currentSubRedProject.SubtitleRenum();
            _ = SubFormats.SelectFormat(currentSubFileName, currentSubRedProject, false, ".ass");
            ViewGrid();
            await ViewSubtitleTab(subListGrid);
            
            /*Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                new Action(() => ViewSubtitleTab(subListGrid)
                ));*/

            RestartVideoPlayer();
            HistorySave(currentSubRedProject.Clone() as SubProject);
        }

        public async Task ViewSubtitleTab(Grid subGrid, bool useTranslator = false, string language = "")
        {
            subGrid.Children.Clear();
            subGrid.RowDefinitions.Clear();

            for (int index = 0; index < currentSubRedProject.SubtitlesList.Count; index++)
            {
                subListGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                StackPanel sp = new StackPanel
                {
                    Orientation = System.Windows.Controls.Orientation.Horizontal,
                    Height = 120,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(3),
                    Name = "SubtitleStackPanel_" + currentSubRedProject.SubtitlesList[index].Id.ToString()
                };
                sp.SetValue(Grid.RowProperty, index);

                Border border = new Border
                {
                    Background = new SolidColorBrush(defaultBackgroundColor),
                    BorderBrush = new SolidColorBrush(Colors.Silver),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8, 8, 8, 8),
                    Name = "SubtitleBorder_" + currentSubRedProject.SubtitlesList[index].Id.ToString(),
                };
                if (!useTranslator) border.MouseLeftButtonUp += Border_MouseLeftButtonUp;

                object o = FindName(border.Name);
                if (o != null) this.UnregisterName(border.Name);
                this.RegisterName(border.Name, border);

                Grid globalGrid = new Grid();
                await Task.Run(async () =>
                {
                    await CreateBorder(index.ToString() + (useTranslator ? "_" + language : ""), useTranslator, language, globalGrid);
                }).ContinueWith(antecendent =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        border.Child = globalGrid;
                        sp.Children.Add(border);
                        subGrid.Children.Add(sp);
                    });
                }, TaskScheduler.FromCurrentSynchronizationContext());
                
                this.UpdateLayout();
                /*border.Child = globalGrid;
                sp.Children.Add(border);
                subGrid.Children.Add(sp);*/
            }
            
            if (SelectedIndexOfSubtitle >= 0)
            {
                SelectSubtitle(null, SelectedIndexOfSubtitle);
            }
        }

        public async Task CreateBorder(string index, bool useTranslator, string languageTo, Grid globalGrid)
        {
            _ = Application.Current.Dispatcher.InvokeAsync(() =>
            {
                object o;
                Grid innerGrid = new Grid { Margin = new Thickness(3) };
                innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                innerGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                innerGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                innerGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

                Grid labelGrid = new Grid { Margin = new Thickness(3), VerticalAlignment = VerticalAlignment.Top };
                labelGrid.SetValue(Grid.ColumnProperty, 0);
                labelGrid.SetValue(Grid.RowProperty, 0);
                Label idLabel = new Label
                {
                    Name = "idLabel_" + index,
                    Content = index,
                    MinWidth = 42,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };
                labelGrid.Children.Add(idLabel);
                innerGrid.Children.Add(labelGrid);

                Grid timeGrid = new Grid { Margin = new Thickness(3), VerticalAlignment = VerticalAlignment.Top };
                timeGrid.SetValue(Grid.ColumnProperty, 1);
                timeGrid.SetValue(Grid.RowProperty, 0);
                timeGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                timeGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                timeGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                TextBox beginTextBox = new TextBox
                {
                    Name = "beginTextBox_" + index,
                    MinWidth = 87,
                    Text = ""
                };
                if (int.TryParse(index, out int beginValue))
                    beginTextBox = new TextBox
                    {
                        Name = "startSubtitleTextBox_" + index,
                        MinWidth = 87,
                        Text = currentSubRedProject.SubtitlesList[beginValue].Start.ToString("hh\\:mm\\:ss\\.ffff")
                    };

                beginTextBox.SetValue(Grid.RowProperty, 0);
                if (!useTranslator) beginTextBox.TextChanged += startSubtitleTextBox_TextChanged;
                TextBox endTextBox = new TextBox
                {
                    Name = "endSubtitleTextBox_" + index.ToString(),
                    MinWidth = 87,
                    Text = ""
                };
                if (int.TryParse(index, out int endValue))
                    endTextBox = new TextBox
                    {
                        Name = "endSubtitleTextBox_" + index.ToString(),
                        MinWidth = 87,
                        Text = currentSubRedProject.SubtitlesList[endValue].End.ToString("hh\\:mm\\:ss\\.ffff")
                    };

                endTextBox.SetValue(Grid.RowProperty, 1);
                if (!useTranslator) endTextBox.TextChanged += endSubtitleTextBox_TextChanged;
                timeGrid.Children.Add(beginTextBox);
                timeGrid.Children.Add(endTextBox);
                innerGrid.Children.Add(timeGrid);

                Grid textBlockGrid = new Grid { Margin = new Thickness(3) };
                textBlockGrid.SetValue(Grid.ColumnProperty, 2);
                textBlockGrid.SetValue(Grid.RowProperty, 0);
                textBlockGrid.SetValue(Grid.RowSpanProperty, 2);
                ScrollViewer scrollViewer = new ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
                };
                RichTextBox richTextBox = new RichTextBox { Width = 250, Name = "subtitleTextRichTextBox_" + index.ToString() };
                if (int.TryParse(index, out int textValue))
                {
                    if (!useTranslator)
                        richTextBox.AppendText(currentSubRedProject.SubtitlesList[textValue].Text);
                    else
                    {
                        var translatedText = TranslateText(textValue, languageTo).Result;
                        richTextBox.AppendText(translatedText);
                    }
                }

                scrollViewer.Content = richTextBox;
                if (!useTranslator) richTextBox.TextChanged += subtitleTextRichTextBox_TextChanged;

                switch (currentSubRedProject.SubtitlesList[textValue].Style.HorizontalAlignment)
                {
                    case 1:
                        richTextBox.HorizontalContentAlignment = HorizontalAlignment.Left;
                        EditingCommands.AlignLeft.Execute(null, richTextBox);
                        break;
                    case 2:
                        richTextBox.HorizontalContentAlignment = HorizontalAlignment.Center;
                        EditingCommands.AlignCenter.Execute(null, richTextBox);
                        break;
                    case 3:
                        richTextBox.HorizontalContentAlignment = HorizontalAlignment.Right;
                        EditingCommands.AlignRight.Execute(null, richTextBox);
                        break;
                }

                textBlockGrid.Children.Add(scrollViewer);
                innerGrid.Children.Add(textBlockGrid);

                #region Правая колонка
                if (!useTranslator)
                {
                    Grid selectionGrid = new Grid { Margin = new Thickness(3) };
                    selectionGrid.SetValue(Grid.ColumnProperty, 3);
                    selectionGrid.SetValue(Grid.RowProperty, 0);
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

                    int iter = 0;
                    foreach (var style in currentSubRedProject.SubtitleStyleList)
                    {
                        ComboBoxItem item = new ComboBoxItem { Name = style.Name.Replace(" ", "_"), Content = style.Name };
                        if (iter == 0)
                            item = new ComboBoxItem { Name = style.Name.Replace(" ", "_"), IsSelected = true, Content = style.Name};

                        //StackPanel panel = new StackPanel();
                        //TextBlock block = new TextBlock { Text = style.Name.Replace(" ", "_") };
                        //panel.Children.Add(block);
                        //item.Content = panel;
                        styleSelectComboBox.Items.Add(item);
                        iter++;
                    }

                    selectionGrid.Children.Add(styleSelectComboBox);

                    Label hLabel = new Label { Content = "H" };
                    hLabel.SetValue(Grid.RowProperty, 1);
                    hLabel.SetValue(Grid.ColumnProperty, 0);
                    selectionGrid.Children.Add(hLabel);

                    ComboBox hSelectComboBox = new ComboBox { Name = "hSelectComboBox_" + index.ToString() };
                    hSelectComboBox.SetValue(Grid.RowProperty, 1);
                    hSelectComboBox.SetValue(Grid.ColumnProperty, 1);

                    ComboBoxItem itemSelected1 = new ComboBoxItem { Name = "Left" };
                    ComboBoxItem itemSelected2 = new ComboBoxItem { Name = "Center", IsSelected = true };
                    ComboBoxItem itemSelected3 = new ComboBoxItem { Name = "Right" };

                    StackPanel panelForItem = new StackPanel { Name = "panelUpForItem_" + index.ToString() };
                    TextBlock blockForPanel = new TextBlock { Text = "Вверх" };
                    panelForItem.Children.Add(blockForPanel);
                    itemSelected1.Content = panelForItem;

                    panelForItem = new StackPanel { Name = "panelCenterForItem_" + index.ToString() };
                    blockForPanel = new TextBlock { Text = "Центр" };
                    panelForItem.Children.Add(blockForPanel);
                    itemSelected2.Content = panelForItem;

                    panelForItem = new StackPanel { Name = "panelDownForItem_" + index.ToString() };
                    blockForPanel = new TextBlock { Text = "Низ" };
                    panelForItem.Children.Add(blockForPanel);
                    itemSelected3.Content = panelForItem;

                    hSelectComboBox.Items.Add(itemSelected1);
                    hSelectComboBox.Items.Add(itemSelected2);
                    hSelectComboBox.Items.Add(itemSelected3);

                    hSelectComboBox.SelectionChanged += horizontalSelectComboBox_SelectionChanged;
                    selectionGrid.Children.Add(hSelectComboBox);

                    Label vLabel = new Label { Content = "V" };
                    vLabel.SetValue(Grid.RowProperty, 1);
                    vLabel.SetValue(Grid.ColumnProperty, 2);
                    selectionGrid.Children.Add(vLabel);

                    ComboBox vSelectComboBox = new ComboBox { Name = "vSelectComboBox_" + index.ToString() };
                    vSelectComboBox.SetValue(Grid.RowProperty, 1);
                    vSelectComboBox.SetValue(Grid.ColumnProperty, 3);

                    itemSelected1 = new ComboBoxItem { Name = "Up" };
                    itemSelected2 = new ComboBoxItem { Name = "Center" };
                    itemSelected3 = new ComboBoxItem { Name = "Down", IsSelected = true };

                    panelForItem = new StackPanel();
                    blockForPanel = new TextBlock { Text = "Слева" };
                    panelForItem.Children.Add(blockForPanel);
                    itemSelected1.Content = panelForItem;

                    panelForItem = new StackPanel();
                    blockForPanel = new TextBlock { Text = "Центр" };
                    panelForItem.Children.Add(blockForPanel);
                    itemSelected2.Content = panelForItem;

                    panelForItem = new StackPanel();
                    blockForPanel = new TextBlock { Text = "Справа" };
                    panelForItem.Children.Add(blockForPanel);
                    itemSelected3.Content = panelForItem;

                    vSelectComboBox.Items.Add(itemSelected1);
                    vSelectComboBox.Items.Add(itemSelected2);
                    vSelectComboBox.Items.Add(itemSelected3);
                    vSelectComboBox.SelectionChanged += verticalSelectComboBox_SelectionChanged;

                    selectionGrid.Children.Add(vSelectComboBox);

                    /*Label xLabel = new Label { Content = "X" };
                    xLabel.SetValue(Grid.RowProperty, 2);
                    xLabel.SetValue(Grid.ColumnProperty, 0);
                    selectionGrid.Children.Add(xLabel);

                    TextBox xTextBox = new TextBox { Name = "xTextBox_" + index.ToString() };
                    xTextBox.SetValue(Grid.RowProperty, 2);
                    xTextBox.SetValue(Grid.ColumnProperty, 1);
                    xTextBox.TextChanged += xCoordTextBox_TextChanged;
                    selectionGrid.Children.Add(xTextBox);

                    Label yLabel = new Label { Content = "Y" };
                    yLabel.SetValue(Grid.RowProperty, 2);
                    yLabel.SetValue(Grid.ColumnProperty, 2);
                    selectionGrid.Children.Add(yLabel);

                    TextBox yTextBox = new TextBox { Name = "yTextBox_" + index.ToString() };
                    yTextBox.SetValue(Grid.RowProperty, 2);
                    yTextBox.SetValue(Grid.ColumnProperty, 3);
                    yTextBox.TextChanged += yCoordTextBox_TextChanged;
                    selectionGrid.Children.Add(yTextBox);*/

                    if (!useTranslator) styleSelectComboBox.SelectionChanged += styleSelectComboBox_SelectionChanged;
                    innerGrid.Children.Add(selectionGrid);

                    o = this.FindName(styleSelectComboBox.Name);
                    if (o != null) this.UnregisterName(styleSelectComboBox.Name);
                    o = this.FindName(hSelectComboBox.Name);
                    if (o != null) this.UnregisterName(hSelectComboBox.Name);
                    o = this.FindName(vSelectComboBox.Name);
                    if (o != null) this.UnregisterName(vSelectComboBox.Name);
                    //o = this.FindName(xTextBox.Name);
                    //if (o != null) this.UnregisterName(xTextBox.Name);
                    //o = this.FindName(yTextBox.Name);
                    //if (o != null) this.UnregisterName(yTextBox.Name);

                    this.RegisterName(styleSelectComboBox.Name, styleSelectComboBox);
                    this.RegisterName(hSelectComboBox.Name, hSelectComboBox);
                    this.RegisterName(vSelectComboBox.Name, vSelectComboBox);
                    //this.RegisterName(xTextBox.Name, xTextBox);
                    //this.RegisterName(yTextBox.Name, yTextBox);
                }
                #endregion

                //Две кнопки принять и отмена
                Grid buttonGrid = new Grid
                {
                    HorizontalAlignment = HorizontalAlignment.Right,
                };
                buttonGrid.SetValue(Grid.ColumnProperty, 3);
                buttonGrid.SetValue(Grid.RowProperty, 1);
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });

                Button acceptButton = new Button
                {
                    Name = "acceptButton_" + index.ToString(),
                    Content = "Принять",
                    Width = 60,
                    Margin = new Thickness(0, 0, 10, 0),
                    Background = new SolidColorBrush(Colors.White),
                    Foreground = new SolidColorBrush(Colors.Black)
                };
                acceptButton.SetValue(Grid.ColumnProperty, 0);
                acceptButton.Visibility = Visibility.Collapsed;
                acceptButton.Click += Accept_Click;

                Button cancelButton = new Button
                {
                    Name = "cancelButton_" + index.ToString(),
                    Content = "Отмена",
                    Width = 60,
                    Background = new SolidColorBrush(Colors.White),
                    Foreground = new SolidColorBrush(Colors.Black)
                };
                cancelButton.SetValue(Grid.ColumnProperty, 1);
                cancelButton.Visibility = Visibility.Collapsed;
                cancelButton.Click += Cancel_Click;

                buttonGrid.Children.Add(acceptButton);
                buttonGrid.Children.Add(cancelButton);
                innerGrid.Children.Add(buttonGrid);

                globalGrid.Children.Add(innerGrid);

                o = this.FindName(idLabel.Name);
                if (o != null) this.UnregisterName(idLabel.Name);
                o = this.FindName(beginTextBox.Name);
                if (o != null) this.UnregisterName(beginTextBox.Name);
                o = this.FindName(endTextBox.Name);
                if (o != null) this.UnregisterName(endTextBox.Name);
                o = this.FindName(richTextBox.Name);
                if (o != null) this.UnregisterName(richTextBox.Name);
                o = this.FindName(acceptButton.Name);
                if (o != null) this.UnregisterName(acceptButton.Name);
                o = this.FindName(cancelButton.Name);
                if (o != null) this.UnregisterName(cancelButton.Name);

                this.RegisterName(idLabel.Name, idLabel);
                this.RegisterName(beginTextBox.Name, beginTextBox);
                this.RegisterName(endTextBox.Name, endTextBox);
                this.RegisterName(richTextBox.Name, richTextBox);
                this.RegisterName(acceptButton.Name, acceptButton);
                this.RegisterName(cancelButton.Name, cancelButton);
            });
        }

        public async Task<string> TranslateText(int textValue, string languageTo)
        {
            return TranslatorAPI.Translate(currentSubRedProject.SubtitlesList[textValue].Text, languageTo).Result;
        }

        /// <summary>
        /// Отображение субтитров в таблице
        /// </summary>
        public void ViewGrid()
        {
            //https://social.msdn.microsoft.com/Forums/en-US/47ce71aa-5bde-482a-9574-764e45cb9031/bind-list-to-datagrid-in-wpf?forum=wpf
            this.SubtitleGrid.ItemsSource = null;
            GC.Collect();
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
                CurrentTimeVideoTextBox.Text = new TimeSpan(0, 0, (int)(player.SourceProvider.MediaPlayer.Time / 1000)).ToString(@"hh\:mm\:ss");
                currentTimeTextBlock.Text = new TimeSpan(0, 0, (int)(player.SourceProvider.MediaPlayer.Time / 1000)).ToString(@"hh\:mm\:ss");
                if (LastTime + 20_000 < player.SourceProvider.MediaPlayer.Time)
                    Properties.Settings.Default.LastTime = LastTime = player.SourceProvider.MediaPlayer.Time + 10_000;
            });

        }

        private void MediaPlayer_LengthChanged(object sender, Vlc.DotNet.Core.VlcMediaPlayerLengthChangedEventArgs e)
        {
            TimeSpan maxTime = new TimeSpan(0,0, (int)(player.SourceProvider.MediaPlayer.Length / 1000));
            _ = Application.Current.Dispatcher.InvokeAsync(() =>
            {
                allTimeTextBlock.Text = maxTime.ToString(@"hh\:mm\:ss");
            });
            ActionDispatcher(() => slider.Maximum = player.SourceProvider.MediaPlayer.Length);
            StateCheck();
        }

        protected bool isValueChanged = false;

        private void LoadVideo_Click(object sender, RoutedEventArgs e)
        {
            RestartVideoPlayer();
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Media Files|*.mpg;*.avi;*.wma;*.mov;*.wav;*.mp2;*.mp3|All Files|*.*"
            };

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
        #endregion

        #region Методы загрузки и сохранения субтитров
        private void Save_Click(object sender, RoutedEventArgs e) => SaveSubFile(".srt");
        private void Load_Click(object sender, RoutedEventArgs e) => LoadSubFile(".srt");
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
            UpdateWindow();
        }

        private async void LoadSubFile(string format = "")
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "All Files|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                currentSubRedProject = await SubFormats.SelectFormat(openFileDialog.FileName, currentSubRedProject, true, format);
                currentSubFileName = openFileDialog.FileName;
            }

            if (player.SourceProvider.MediaPlayer.Length >= 0)
                RestartVideoPlayer(openFileDialog.FileName);

            currentSubRedProject.SubtitleRenum();
            UpdateWindow();
        }
        #endregion

        #region Методы выбора субтитров в таблице и в списке
        private void Row_Click(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try
                {
                    var row = GetVisualParentByType((FrameworkElement)e.OriginalSource, typeof(DataGridRow)) as DataGridRow;
                    if (row != null)
                    {
                        row.IsSelected = !row.IsSelected;
                        e.Handled = true;
                    }

                    Subtitle? dataRow = null;
                    Border? newFoundedBorder = null;
                    if (SubtitleGrid.SelectedItems.Count > 0)
                    {
                        dataRow = (Subtitle)SubtitleGrid.SelectedItems[SubtitleGrid.SelectedItems.Count - 1];
                    }

                    if (dataRow != null)
                    {
                        newFoundedBorder = this.FindName("SubtitleBorder_" + dataRow.Id) as Border;
                        SelectSubtitle(newFoundedBorder, dataRow.Id);
                    }
                    else
                        SelectSubtitle(newFoundedBorder, -1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error in Row_Click method", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public static DependencyObject GetVisualParentByType(DependencyObject startObject, Type type)
        {
            DependencyObject parent = startObject;
            while (parent != null)
            {
                if (type.IsInstanceOfType(parent))
                    break;
                else
                    parent = VisualTreeHelper.GetParent(parent);
            }

            return parent;
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

                if (SelectedIndexOfSubtitle >= 0)
                {
                    DataGridRow row = (DataGridRow)SubtitleGrid.ItemContainerGenerator.ContainerFromIndex(currIndex);
                    row.IsSelected = true;
                    //SubtitleGrid.SelectedIndex = SelectedIndexOfSubtitle;
                    SubtitleGrid.UpdateLayout();
                    if (SubtitleGrid.SelectedItem != null) SubtitleGrid.ScrollIntoView(SubtitleGrid.SelectedItem);
                    scrollViewerSubListGrid.ScrollToVerticalOffset(100 * SelectedIndexOfSubtitle);
                    //------------------------//

                    if (player.SourceProvider.MediaPlayer.State == MediaStates.Playing)
                        ThreadPool.QueueUserWorkItem(_ => player.SourceProvider.MediaPlayer.Pause());

                    int frameNum = (int)(currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].Start.TotalMilliseconds);
                    slider.Value = frameNum;
                    player.SourceProvider.MediaPlayer.Time = (long)(frameNum) * 1000;
                }
            }
            else
            {
                SelectedIndexOfSubtitle = -1;
                if (SubtitleGrid != null)
                {
                    DataGridRow row = (DataGridRow)SubtitleGrid.ItemContainerGenerator.ContainerFromIndex(currIndex);
                    row.IsSelected = false;
                    //SubtitleGrid.SelectedIndex = -1;
                }
            }

            IdSubtitleTextBox.TextChanged -= IdSubtitleTextBox_TextChanged;
            IdSubtitleTextBox.Text = SelectedIndexOfSubtitle.ToString();
            IdSubtitleTextBox.TextChanged += IdSubtitleTextBox_TextChanged;

            if (player.SourceProvider.MediaPlayer != null)
                if (player.SourceProvider.MediaPlayer.Length > 0)
                    CurrentTimeVideoTextBox.Text = player.SourceProvider.MediaPlayer.Time.ToString();
        }

        private static readonly Regex _regex = new Regex("[^0-9]+"); //regex that matches disallowed text
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) 
            => e.Handled = _regex.IsMatch(e.Text);

        private void IdSubtitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(IdSubtitleTextBox.Text, out int currIndex))
            {
                var foundedBorder = this.FindName("SubtitleBorder_" + currIndex) as Border;
                SelectSubtitle(foundedBorder, currIndex);
            }
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

        #region Редактирование субтитров MenuItem
        private Grid? FindClickedItem(object sender)
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
                    currentSubRedProject.SubtitlesList.Insert(SelectedIndexOfSubtitle, new Subtitle());
                    UpdateWindow();
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
                        currentSubRedProject.SubtitlesList.RemoveAt(SelectedIndexOfSubtitle);
                        UpdateWindow();
                    }
                }
                else MessageBox.Show("Не выбран элемент для удаления", "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Question);
            }
        }

        private void TranslateRus_OnClick(object sender, RoutedEventArgs e)
        {
            var clickedItem = FindClickedItem(sender);
            if (clickedItem != null)
            {
                if (SelectedIndexOfSubtitle >= 0)
                {
                    TranslateFrase("ru");
                }
                else MessageBox.Show("Не выбран элемент, который переводить", "Ошибка перевода фразы", MessageBoxButton.OK, MessageBoxImage.Question);
            }
        }

        private void TranslateEng_OnClick(object sender, RoutedEventArgs e)
        {
            var clickedItem = FindClickedItem(sender);
            if (clickedItem != null)
            {
                if (SelectedIndexOfSubtitle >= 0)
                {
                    TranslateFrase("en");
                }
                else MessageBox.Show("Не выбран элемент, который переводить", "Ошибка перевода фразы", MessageBoxButton.OK, MessageBoxImage.Question);
            }
        }

        private void TranslateChi_OnClick(object sender, RoutedEventArgs e)
        {
            var clickedItem = FindClickedItem(sender);
            if (clickedItem != null)
            {
                if (SelectedIndexOfSubtitle >= 0)
                {
                    TranslateFrase("zh");
                }
                else MessageBox.Show("Не выбран элемент, который переводить", "Ошибка перевода фразы", MessageBoxButton.OK, MessageBoxImage.Question);
            }
        }

        private void TranslateJap_OnClick(object sender, RoutedEventArgs e)
        {
            var clickedItem = FindClickedItem(sender);
            if (clickedItem != null)
            {
                if (SelectedIndexOfSubtitle >= 0)
                {
                    TranslateFrase("ja");
                }
                else MessageBox.Show("Не выбран элемент, который переводить", "Ошибка перевода фразы", MessageBoxButton.OK, MessageBoxImage.Question);
            }
        }

        private async void TranslateFrase(string language)
        {
            var richTextBox = ((RichTextBox)this.FindName("subtitleTextRichTextBox_" + SelectedIndexOfSubtitle.ToString()));
            string richText = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text;

            var newText = await TranslatorAPI.Translate(richText, language);
            richTextBox.AppendText("\n"+newText);
        }
        #endregion

        #region Элементы редактирования субтитра в Panel
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string index = button.Name.Split('_').Last();
            if (int.TryParse(index, out int result))
            {
                var foundedSubtitle = currentSubRedProject.SubtitlesList.Find(x => x.Id == result);
                var indexOfSubtitle = currentSubRedProject.SubtitlesList.IndexOf(foundedSubtitle);

                foundedSubtitle.Start = TimeSpan.Parse(((TextBox)this.FindName("startSubtitleTextBox_" + result.ToString())).Text);
                foundedSubtitle.End = TimeSpan.Parse(((TextBox)this.FindName("endSubtitleTextBox_" + result.ToString())).Text);

                var richTextBox = ((RichTextBox)this.FindName("subtitleTextRichTextBox_" + result.ToString()));
                string richText = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text;
                var splittedText = richText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                richText = String.Join(Environment.NewLine, splittedText.ToArray());
                foundedSubtitle.Text = richText;

                var comboBox = (ComboBox)this.FindName("StyleSelectComboBox_" + result.ToString());
                var item = ((ComboBoxItem)comboBox.Items[comboBox.SelectedIndex]).Content as string;
                foundedSubtitle.Style = currentSubRedProject.SubtitleStyleList.Find(x => x.Name == item);

                comboBox = (ComboBox)this.FindName("hSelectComboBox_" + result.ToString());
                switch (((ComboBoxItem)comboBox.Items[comboBox.SelectedIndex]).Name)
                {
                    case "Left":
                        foundedSubtitle.Style.HorizontalAlignment = 1;
                        break;
                    case "Center":
                        foundedSubtitle.Style.HorizontalAlignment = 2;
                        break;
                    case "Right":
                        foundedSubtitle.Style.HorizontalAlignment = 3;
                        break;
                }

                comboBox = (ComboBox)this.FindName("vSelectComboBox_" + result.ToString());
                switch (((ComboBoxItem)comboBox.Items[comboBox.SelectedIndex]).Name)
                {
                    case "Up":
                        foundedSubtitle.Style.VerticalAlignment = 4;
                        break;
                    case "Center":
                        foundedSubtitle.Style.VerticalAlignment = 8;
                        break;
                    case "Down":
                        foundedSubtitle.Style.VerticalAlignment = 0;
                        break;
                }
                var style = currentSubRedProject.SubtitleStyleList.Find(x => x.Name == foundedSubtitle.Style.Name);
                var indexOfStyle = currentSubRedProject.SubtitleStyleList.IndexOf(style);
                currentSubRedProject.SubtitleStyleList[indexOfStyle] = foundedSubtitle.Style;

                currentSubRedProject.SubtitlesList[indexOfSubtitle] = foundedSubtitle;

                UpdateWindow();
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string index = button.Name.Split('_').Last();
            if (int.TryParse(index, out int result))
            {
                var foundedSubtitle = currentSubRedProject.SubtitlesList.Find(x => x.Id == result);

                ((TextBox)this.FindName("startSubtitleTextBox_" + result.ToString())).Text = foundedSubtitle.Start.ToString("hh\\:mm\\:ss\\.ffff");
                ((TextBox)this.FindName("endSubtitleTextBox_" + result.ToString())).Text = foundedSubtitle.End.ToString("hh\\:mm\\:ss\\.ffff");

                var richTextBox = ((RichTextBox)this.FindName("subtitleTextRichTextBox_" + result.ToString()));
                richTextBox.Document.Blocks.Clear();
                richTextBox.AppendText(foundedSubtitle.Text);

                var comboBox = (ComboBox)this.FindName("StyleSelectComboBox_" + result.ToString());
                ComboBoxItem item = (ComboBoxItem)comboBox.FindName(foundedSubtitle.Style.Name.Replace(" ", "_"));
                comboBox.SelectedItem = item;

                comboBox = (ComboBox)this.FindName("hSelectComboBox_" + result.ToString());
                switch (foundedSubtitle.Style.HorizontalAlignment)
                {
                    case 0:
                        comboBox.SelectedItem = "Left";
                        break;
                    case 1:
                        comboBox.SelectedItem = "Center";
                        break;
                    case 2:
                        comboBox.SelectedItem = "Right";
                        break;
                }

                comboBox = (ComboBox)this.FindName("vSelectComboBox_" + result.ToString());
                switch (foundedSubtitle.Style.VerticalAlignment)
                {
                    case 4:
                        comboBox.SelectedItem = "Up";
                        break;
                    case 8:
                        comboBox.SelectedItem = "Center";
                        break;
                    case 0:
                        comboBox.SelectedItem = "Down";
                        break;
                }

                //((TextBox)this.FindName("xTextBox_" + result.ToString())).Text = foundedSubtitle.XCoord.ToString();
                //((TextBox)this.FindName("yTextBox_" + result.ToString())).Text = foundedSubtitle.YCoord.ToString();

                button = (Button)this.FindName("acceptButton_" + index.ToString());
                if (button != null) button.Visibility = Visibility.Hidden;

                button = (Button)this.FindName("cancelButton_" + index.ToString());
                if (button != null) button.Visibility = Visibility.Hidden;
            }
        }

        private async Task ShowAcceptCancelButtons(string index)
        {
            if (int.TryParse(index, out int result))
            {
                var button = (Button)this.FindName("acceptButton_" + index.ToString());
                if (button != null) button.Visibility = Visibility.Visible;

                button = (Button)this.FindName("cancelButton_" + index.ToString());
                if (button != null) button.Visibility = Visibility.Visible;
            }
        }

        private async void startSubtitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            string index = textBox.Name.Split('_').Last();
            await ShowAcceptCancelButtons(index);
        }

        private void endSubtitleTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            string index = textBox.Name.Split('_').Last();
            ShowAcceptCancelButtons(index);
        }

        private async void subtitleTextRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var richTextBox = sender as RichTextBox;
            string index = richTextBox.Name.Split('_').Last();
            await ShowAcceptCancelButtons(index);
        }

        private void styleSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            string index = comboBox.Name.Split('_').Last();
            ShowAcceptCancelButtons(index);
        }

        private void horizontalSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            string index = comboBox.Name.Split('_').Last();
            ShowAcceptCancelButtons(index);
        }

        private void verticalSelectComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            string index = comboBox.Name.Split('_').Last();
            ShowAcceptCancelButtons(index);
        }

        private void xCoordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            string index = textBox.Name.Split('_').Last();
            ShowAcceptCancelButtons(index);
        }

        private void yCoordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            string index = textBox.Name.Split('_').Last();
            ShowAcceptCancelButtons(index);
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
                RichTextBox richTextBox = (RichTextBox)this.FindName("subtitleTextRichTextBox_" + SelectedIndexOfSubtitle.ToString());

                if (richTextBox != null)
                {
                    switch (alignment)
                    {
                        case 1:
                            richTextBox.HorizontalContentAlignment = HorizontalAlignment.Left;
                            EditingCommands.AlignLeft.Execute(null, richTextBox);
                            currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].Style.HorizontalAlignment = 0;
                            break;
                        case 2:
                            richTextBox.HorizontalContentAlignment = HorizontalAlignment.Center;
                            EditingCommands.AlignCenter.Execute(null, richTextBox);
                            currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].Style.HorizontalAlignment = 1;
                            break;
                        case 3:
                            richTextBox.HorizontalContentAlignment = HorizontalAlignment.Right;
                            EditingCommands.AlignRight.Execute(null, richTextBox);
                            currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].Style.HorizontalAlignment = 2;
                            break;
                    }
                }
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
                RichTextBox richTextBox = (RichTextBox)this.FindName("subtitleTextRichTextBox_" + SelectedIndexOfSubtitle.ToString());
                TextSelection text = richTextBox.Selection;

                if (text != null)
                {
                    if (text.Text != "")
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

                        System.Drawing.Font font = new System.Drawing.Font(currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].Style.Fontname,
                            (float)currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].Style.Fontsize, fontStyle);
                        text.ApplyPropertyValue(RichTextBox.FontStyleProperty, font);
                        currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].ChangeInTextAction(indexStart, indexEnd, inTextString);

                        return;
                    }
                }

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
                        richTextBox.Selection.ApplyPropertyValue(System.Windows.Documents.Inline.TextDecorationsProperty, TextDecorations.Underline);
                        richTextBox.Selection.Select(richTextBox.Document.ContentStart, richTextBox.Document.ContentStart);
                        currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].Style.Underline = true;
                        break;
                    case "strikethrough":
                        richTextBox.SelectAll();
                        richTextBox.Selection.ApplyPropertyValue(System.Windows.Documents.Inline.TextDecorationsProperty, TextDecorations.Strikethrough);
                        richTextBox.Selection.Select(richTextBox.Document.ContentStart, richTextBox.Document.ContentStart);
                        currentSubRedProject.SubtitlesList[SelectedIndexOfSubtitle].Style.StrikeOut = true;
                        break;
                }

            }

            UpdateWindow();
        }
        #endregion

        #region Создание, удаление, обновление вкладки с переводом субтитров
        public async void TranslateTabAdd(string Header, string Name, string language)
        {
            MenuItem newQuery = new MenuItem();
            newQuery.Header = "Удалить вкладку";
            newQuery.Click += deleteMenuItem_Click;

            TabItem newTabItem = new TabItem
            {
                Header = Header,
                Name = Name
            };

            newTabItem.ContextMenu = new();
            newTabItem.ContextMenu.Items.Add(newQuery);

            ScrollViewer scrollViewer = new ScrollViewer();

            Grid translatedGrid = new Grid { 
                Name = Name + "SubGrid"
            };

            ViewSubtitleTab(translatedGrid, true, language);
            scrollViewer.Content = translatedGrid;
            newTabItem.Content = scrollViewer;
            tabControl.Items.Add(newTabItem);
        }

        private void engTabAddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TranslateTabAdd("Eng", "EngTest", "en");
        }

        private void chiTabAddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TranslateTabAdd("Chi", "EngTest", "chi");
        }

        private void jpnTabAddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            TranslateTabAdd("Jpn", "EngTest", "jpn");
        }
        private void deleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (tabControl.SelectedIndex > 1)
                if (MessageBox.Show("Удалить вкладку?", "Удаление вкладки", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    tabControl.Items.RemoveAt(tabControl.SelectedIndex);                
        }
        #endregion

        private void ProjectSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow(currentSubRedProject, this);
            settingsWindow.Show();
        }

        private void AddTabItemButton_Click(object sender, RoutedEventArgs e)
        {
            Button b = sender as Button;
            ContextMenu contextMenu = b.ContextMenu;
            contextMenu.PlacementTarget = b;
            contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.MousePoint;
            contextMenu.IsOpen = true;
        }

        private void NewProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            WindowFirstLoad();
        }

        private async void SaveProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await SubFormats.SelectFormat("currentProject.ass", currentSubRedProject, false, ".ass");
        }

        private void CloseProjectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void FindAndChangeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FindChangeWindow fcWindow = new FindChangeWindow();
            fcWindow.LoadWindow(this);
            fcWindow.Show();
        }

        private void StyleMenuItem_Click(object sender, RoutedEventArgs e)
        {
            StyleListWindow styleListWindow = new StyleListWindow();
            styleListWindow.LoadWindow(currentSubRedProject);
            styleListWindow.ShowDialog();
        }

        private void PutInMSMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Aspose.Words.Document doc = new Aspose.Words.Document();
            DocumentBuilder builder = new DocumentBuilder(doc);

            // Specify font formatting
            Aspose.Words.Font font = builder.Font;
            font.Size = 32;
            font.Bold = true;
            font.Color = System.Drawing.Color.Black;
            font.Name = "Arial";
            font.Underline = Aspose.Words.Underline.Single;

            // Insert text
            for (int index = 0; index < currentSubRedProject.SubtitlesList.Count; index++)
            {
                builder.Writeln(index.ToString());
                builder.Writeln(currentSubRedProject.SubtitlesList[index].Text);
                builder.Writeln();
            }

            // Change formatting for next elements.
            font.Underline = Aspose.Words.Underline.None;
            font.Size = 10;
            font.Color = System.Drawing.Color.Blue;

            builder.Writeln("This following is a table");

            // Save the document
            doc.Save("DocumentSave.docx");
        }

        private void RenumMenuItem_Click(object sender, RoutedEventArgs e)
        {
            currentSubRedProject.SubtitleRenum();
        }

        private void RenumAndSortMenuItem_Click(object sender, RoutedEventArgs e)
        {
            currentSubRedProject.SubtitleSort();
            currentSubRedProject.SubtitleRenum();
        }

        private void MoveTimeMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MoveTimeWindow moveTimeWindow = new MoveTimeWindow();
            moveTimeWindow.LoadWindow(this);
            moveTimeWindow.Show();
        }

        #region Редактирование таблицы
        private void DuplicateRowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Subtitle? dataRow = (Subtitle)SubtitleGrid.Items[SelectedIndexOfSubtitle];
            if (dataRow != null)
            {
                //HistorySave(currentSubRedProject.Clone() as SubProject);
                currentSubRedProject.SubtitlesList.Insert(dataRow.Id, dataRow.Clone() as Subtitle);
                UpdateWindow();
            }
        }
        private void CopyRowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Subtitle dataRow = (Subtitle)SubtitleGrid.SelectedItem;
            if (dataRow != null)
                copiedSubtitle = dataRow;
            else if (SelectedIndexOfSubtitle >= 0)
                copiedSubtitle = (Subtitle)SubtitleGrid.Items[SelectedIndexOfSubtitle];
        }
        private void CutRowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Subtitle dataRow = (Subtitle)SubtitleGrid.Items[SelectedIndexOfSubtitle];
            if (dataRow != null)
            {
                //HistorySave(currentSubRedProject);
                copiedSubtitle = dataRow;
                currentSubRedProject.SubtitlesList.RemoveAt(dataRow.Id);
                UpdateWindow();
            }
        }
        private void PasteRowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (copiedSubtitle != null)
            {
                //HistorySave(currentSubRedProject.Clone() as SubProject);
                if (SelectedIndexOfSubtitle < 0)
                {
                    currentSubRedProject.SubtitlesList.Insert(0, copiedSubtitle.Clone() as Subtitle);
                }
                else
                {
                    currentSubRedProject.SubtitlesList.Insert(copiedSubtitle.Id, copiedSubtitle.Clone() as Subtitle);
                }
                UpdateWindow();
            }
        }
        private void DeleteRowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Subtitle dataRow = (Subtitle)SubtitleGrid.Items[SelectedIndexOfSubtitle];
            if (dataRow != null)
            {
                //HistorySave(currentSubRedProject.Clone() as SubProject);
                currentSubRedProject.SubtitlesList.RemoveAt(dataRow.Id);
                UpdateWindow();
            }
        }
        #endregion
        public void HistorySave(SubProject subProject)
        {
            if (subProjectHistory.Count >= 20)
                subProjectHistory.RemoveAt(0);

            subProjectHistory.Add(subProject);
            copiedIndexSubtitle = subProjectHistory.Count - 1;
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Сделал студент группы МИИ21-01: Харсекин Иван Романович, СибГУ им. М.Ф. Решетнева", "О программе");
        }

        private void BackMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (copiedIndexSubtitle < subProjectHistory.Count && copiedIndexSubtitle >= 0)
            {
                currentSubRedProject = subProjectHistory[copiedIndexSubtitle].Clone() as SubProject;
                UpdateWindow();
                copiedIndexSubtitle--;
            }
        }
        private void ForwardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            copiedIndexSubtitle++;
            if (copiedIndexSubtitle < subProjectHistory.Count && copiedIndexSubtitle >= 0)
            {
                currentSubRedProject = subProjectHistory[copiedIndexSubtitle].Clone() as SubProject;
                UpdateWindow();
            }
        }

        private void TimeCrossedSelectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            currentSubRedProject.SubtitleSort();
            for (int i = 0; i < currentSubRedProject.SubtitlesList.Count; i++)
            {
                for (int j = i; j < currentSubRedProject.SubtitlesList.Count; j++)
                {
                    if (currentSubRedProject.SubtitlesList[i].Start >= currentSubRedProject.SubtitlesList[j].Start ||
                        currentSubRedProject.SubtitlesList[i].End <= currentSubRedProject.SubtitlesList[j].End)
                    {
                        DataGridRow row1 = (DataGridRow)SubtitleGrid.ItemContainerGenerator.ContainerFromIndex(i);
                        row1.Foreground = new SolidColorBrush(Colors.Red);

                        DataGridRow row2 = (DataGridRow)SubtitleGrid.ItemContainerGenerator.ContainerFromIndex(j);
                        row2.Foreground = new SolidColorBrush(Colors.Red);
                    }
                }
            }
        }

        private void SubtitleGridMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SubtitleGridMenuItem.IsChecked = !SubtitleGridMenuItem.IsChecked;
            if (SubtitleGridMenuItem.IsChecked)
            {
                SubtitleListGrid.SetValue(Grid.RowSpanProperty, 1);
                VideoGrid.SetValue(Grid.RowSpanProperty, 1);
                SubtitleDataGridGrid.Visibility = Visibility.Visible;
            }
            else
            {
                SubtitleListGrid.SetValue(Grid.RowSpanProperty, 3);
                VideoGrid.SetValue(Grid.RowSpanProperty, 3);
                SubtitleDataGridGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void VideoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            VideoMenuItem.IsChecked = !VideoMenuItem.IsChecked;
            if (VideoMenuItem.IsChecked)
            {
                SubtitleListGrid.SetValue(Grid.ColumnProperty, 2);
                SubtitleListGrid.SetValue(Grid.ColumnSpanProperty, 1);
                VideoGrid.Visibility = Visibility.Visible;
            }
            else
            {
                //timeGrid.SetValue(Grid.ColumnProperty, 1);
                //timeGrid.SetValue(Grid.RowProperty, 0);
                SubtitleListGrid.SetValue(Grid.ColumnProperty, 0);
                SubtitleListGrid.SetValue(Grid.ColumnSpanProperty, 3);
                VideoGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void MainWindowSettingsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MainSettingsWindow mainSettingsWindow = new MainSettingsWindow();
            mainSettingsWindow.ShowDialog();
        }

        private void CurrentTimeVideoTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
