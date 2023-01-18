using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using UI.ViewModel;

namespace UI {
    public partial class MainWindow : Window {
        public MainWindow () {
            InitializeComponent();
            DataContext = vm;
            mediaList.ItemsSource = vm.MediaItems;
            Closing += Window_Closing;
            Drop += Media_Drop;
            Loaded += Window_Loaded;
            LocationChanged += Window_LocationChanged;
            MouseDown += Window_MouseDown;
            PreviewKeyDown += Window_PreviewKeyDown;
            vm.MoveDown += (_, _) => moveNext();
            vm.MoveUp += (_, _) => movePrevious();
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.PreviewMouseDownEvent, new MouseButtonEventHandler(TextBox_PreviewMouseDown));
        }

        public MainViewModel vm => App.ViewModel;
        MediaWindow? media;

        const double minimalHeight = 130;
        const double maximalHeight = 710;

        readonly string[] NoResults = new string[] { "" };

        static bool isDigitKeyPress (KeyEventArgs a) =>
            a.KeyboardDevice.Modifiers == 0 &&
            Key.D0 <= a.Key &&
            a.Key <= Key.D9;

        void addMediaUsingFileDialog () {
            Microsoft.Win32.OpenFileDialog many = new() {
                FileName = "",
                Filter = "Pictures and Videos |*.bmp;*.gif;*.jpg;*.jpeg;*.ico;*.png;*.tif;*.tiff;*.avi;*.mov;*.mp4;*.mpe;*.mpeg;*.mpg;*.wmv;*.pdf",
                Multiselect = true,
            };
            many.ShowDialog();
            if (Enumerable.SequenceEqual(many.SafeFileNames, NoResults)) return;
            processMediaItems(many.FileNames);
        }

        void addSingleVideoUsingFileDialog () {
            Microsoft.Win32.OpenFileDialog one = new() {
                FileName = "",
                Filter = "Video |*.avi;*.mov;*.mp4;*.mpe;*.mpeg;*.mpg;*.wmv",
                Multiselect = false,
            };
            one.ShowDialog();
            if (string.IsNullOrEmpty(one.FileName)) return;
            if (MainViewModel.GetMediaType(one.FileName) != MediaType.Video) return;
            setSingleVideo(one.FileName);
        }

        void closeMedia () {
            media?.Close();
            vm.MediaDisplayed = false;
        }

        void down () {
            if (vm.MediaDisplayed) moveNext();
            else shiftDown();
        }

        void initializeSliderVideoPreview () {
            try { sliderMinifiedPositionChangePreview.Stop(); } catch { }
            try { sliderMaxifiedPositionChangePreview.Stop(); } catch { }
            if (vm.InternetMode) return;
            if (vm.MediaListMode && (!vm.MediaListHasContents || vm.CurrentMediaItem is not VideoItem)) return;
            var source = vm.SingleVideoMode ? vm.SingleVideo.Source : ((VideoItem) vm.CurrentMediaItem).Source;
            if (vm.Minified) {
                sliderMinifiedPositionChangePreview.Source = source;
                sliderMinifiedPositionChangePreview.Pause();
            }
            else {
                sliderMaxifiedPositionChangePreview.Source = source;
                sliderMaxifiedPositionChangePreview.Pause();
            }
        }

        void maxify () {
            vm.Minified = false;
            Height = maximalHeight;
            WindowManager.SetWindowPosition(this, vm);
            initializeSliderVideoPreview();
        }

        void minify () {
            vm.Minified = true;
            Height = minimalHeight;
            WindowManager.SetWindowPosition(this, vm);
            initializeSliderVideoPreview();
        }

        void moveNext () {
            if (mediaList.Items.Count == 0) return;
            if (mediaList.Items.Count - 1 <= mediaList.SelectedIndex) return;
            ++mediaList.SelectedIndex;
            initializeSliderVideoPreview();
        }

        void movePrevious () {
            if (mediaList.Items.Count == 0) return;
            if (mediaList.SelectedIndex == 0) return;
            --mediaList.SelectedIndex;
            initializeSliderVideoPreview();
        }

        void navigateUrl () {
            if (web == null || web.CoreWebView2 == null) return;
            var a = url.Text ?? "";
            var b = new string[] {
                a,
                "https://" + a,
                "https://www." + a,
                "http://" + a,
                "http://www." + a,
            };
            try { web.CoreWebView2.Navigate(b[0]); vm.WebpageUrl = new(b[0]); goto done; } catch { }
            try { web.CoreWebView2.Navigate(b[1]); vm.WebpageUrl = new(b[1]); goto done; } catch { }
            try { web.CoreWebView2.Navigate(b[2]); vm.WebpageUrl = new(b[2]); goto done; } catch { }
            try { web.CoreWebView2.Navigate(b[3]); vm.WebpageUrl = new(b[3]); goto done; } catch { }
            try { web.CoreWebView2.Navigate(b[4]); vm.WebpageUrl = new(b[4]); goto done; } catch { }
            vm.WebpageUrl = new("about:blank");
            vm.InternetNavigationFailed = true;
            return;
        done: vm.InternetNavigationFailed = false;
        }

        void playPauseVideo () {
            if (vm.InternetMode) return;
            if (vm.MediaDisplayed) media?.PlayPauseVideo();
            else if (vm.MediaListMode) showMediaListMedia();
            else showSingleVideo();
        }

        void processMediaItems (string[] paths) {
            if (vm.MediaDisplayed) return;
            if (paths == null || paths.Length == 0) return;
            if (vm.InternetMode) return;
            if (vm.SingleVideoMode) {
                if (paths[0] == vm.SingleVideo.FilePath) return;
                setSingleVideo(paths[0]);
                return;
            }
            uint count = 0;
            foreach (var path in paths) {
                var name = Path.GetFileName(path);
                var uri = new Uri(path);
                switch(MainViewModel.GetMediaType(path)) {
                    case MediaType.Picture:
                        ++count;
                        var bmp = new BitmapImage(uri);
                        vm.AddMediaItem(new PictureItem {
                            FileName = name,
                            FilePath = path,
                            Source = bmp,
                        });
                        break;
                    case MediaType.Video:
                        ++count;
                        vm.AddMediaItem(new VideoItem {
                            FileName = name,
                            FilePath = path,
                            Source = uri,
                        });
                        break;
                    default: break;
                }
            }
            if (0 < paths.Length) {
                mediaList.SelectedIndex = 0;
                mediaList.Focus();
                initializeSliderVideoPreview();
            }
            if (count == 0)
                showNoMediaFilesFoundMessage();
        }

        void removeMediaItem () {
            var i = mediaList.SelectedIndex;
            if (i < 0) return;
            if (vm.MediaItems.Count <= i) return;
            vm.RemoveMediaItem(i);
            if (vm.MediaItems.Count == 0) return;
            if (vm.MediaItems.Count <= i)
                i = vm.MediaItems.Count - 1;
            mediaList.SelectedIndex = i;
            initializeSliderVideoPreview();
        }

        void setSingleVideo (string path) {
            if (MainViewModel.GetMediaType(path) != MediaType.Video) return;

            vm.SingleVideoPreviewIsLoading = true;
            vm.SingleVideo = new();

            try { singleVideoPreview.Stop(); } catch { }

            Dispatcher.Invoke(delegate { }, DispatcherPriority.Render);
            WindowManager.LetUIUpdate();

            Uri uri = new(path);
            vm.SingleVideo = new VideoItem {
                FileName = Path.GetFileName(path),
                FilePath = path,
                Source = uri,
            };
            singleVideoPreview.Source = uri;
            singleVideoPreview.Position = TimeSpan.Zero;

            singleVideoPreview.MediaOpened += (_, _) => vm.SingleVideoPreviewTotalLength = singleVideoPreview.NaturalDuration.TimeSpan;

            System.Threading.Thread.Sleep(1000);

            vm.SingleVideoPreviewIsLoading = false;
            Dispatcher.Invoke(delegate { }, DispatcherPriority.Render);
            WindowManager.LetUIUpdate();

            initializeSliderVideoPreview();
        }

        void shiftDown () {
            if (vm.MediaItems.Count == 0) return;
            if (mediaList.Items.Count - 1 <= mediaList.SelectedIndex) return;
            if (mediaList.SelectedItem == null) mediaList.SelectedIndex = 0;
            var i = mediaList.SelectedIndex;

            // Necessary to cause the thumbnail to update
            mediaList.ItemsSource = null;

            (vm.MediaItems[i], vm.MediaItems[i + 1]) = (vm.MediaItems[i + 1], vm.MediaItems[i]);
            mediaList.ItemsSource = vm.MediaItems;

            mediaList.SelectedIndex = i + 1;
        }

        void shiftUp () {
            if (vm.MediaItems.Count == 0) return;
            if (mediaList.SelectedIndex == 0) return;
            if (mediaList.SelectedItem == null) mediaList.SelectedIndex = 0;
            var i = mediaList.SelectedIndex;

            // Necessary to cause the thumbnail to update
            mediaList.ItemsSource = null;

            (vm.MediaItems[i], vm.MediaItems[i - 1]) = (vm.MediaItems[i - 1], vm.MediaItems[i]);
            mediaList.ItemsSource = vm.MediaItems;

            mediaList.SelectedIndex = i - 1;
        }

        void showMediaListMedia () {
            if (vm.MediaItems.Count == 0 || mediaList.Items.Count == 0) return;
            if (mediaList.SelectedValue == null) mediaList.SelectedIndex = 0;
            vm.VideoPosition = TimeSpan.Zero;
            media = new();
            WindowManager.ShowMediaWindow(media, vm, (s, e) => {
                vm.MediaDisplayed = false;
                vm.VideoPaused = true;
            });
            vm.MediaDisplayed = true;
            initializeSliderVideoPreview();
        }

        void showNoMediaFilesFoundMessage () {
            Storyboard showNoMediaFilesFoundMessage = new();

            DoubleAnimation opacityIn = new() {
                From = 0.0,
                To = 0.8,
                Duration = new(new TimeSpan(0, 0, 0, 0, 200)),
                EasingFunction = new ExponentialEase(),
            };
            Storyboard.SetTargetProperty(opacityIn, new PropertyPath(OpacityProperty));
            showNoMediaFilesFoundMessage.Children.Add(opacityIn);

            DoubleAnimation growWidth = new() {
                From = 0,
                To = 490,
                Duration = new(new TimeSpan(0, 0, 0, 0, 200)),
                EasingFunction = new ExponentialEase(),
            };
            Storyboard.SetTargetProperty(growWidth, new PropertyPath(WidthProperty));
            showNoMediaFilesFoundMessage.Children.Add(growWidth);

            DoubleAnimation growHeight = new() {
                From = 0,
                To = 70,
                Duration = new(new TimeSpan(0, 0, 0, 0, 200)),
                EasingFunction = new ExponentialEase(),
            };
            Storyboard.SetTargetProperty(growHeight, new PropertyPath(HeightProperty));
            showNoMediaFilesFoundMessage.Children.Add(growHeight);

            showNoMediaFilesFoundMessage.Completed += (_, _) => {
                Thread.Sleep(2000);
                noMediaFilesFoundMessage.Dispatcher.BeginInvoke(new Action(() => {
                    Storyboard hide = new();
                    hide.Completed += (_, _) => {
                        noMediaFilesFoundMessage.Visibility = Visibility.Collapsed;
                    };

                    DoubleAnimation fadeOut = new() {
                        From = 0.8,
                        To = 0.0,
                        Duration = new(new TimeSpan(0, 0, 0, 0, 500)),
                        EasingFunction = new ExponentialEase(),
                    };
                    Storyboard.SetTargetProperty(fadeOut, new PropertyPath(OpacityProperty));
                    hide.Children.Add(fadeOut);

                    hide.Begin(noMediaFilesFoundMessage);
                }));
            };

            noMediaFilesFoundMessage.Visibility = Visibility.Visible;
            showNoMediaFilesFoundMessage.Begin(noMediaFilesFoundMessage);
        }

        void showSingleVideo () {
            if (string.IsNullOrEmpty(vm.SingleVideo.FileName)) return;
            vm.VideoPosition = vm.SingleVideoPreviewPosition;
            vm.VideoDisplayedOnMediaWindow = true;
            media = new();
            WindowManager.ShowMediaWindow(media, vm, (s, e) => {
                vm.MediaDisplayed = false;
                vm.VideoPaused = true;
            });
            vm.MediaDisplayed = true;
            vm.VideoPaused = false;
            initializeSliderVideoPreview();
        }

        void showWebpage () {
            media = new();
            web.Source = new("about:blank");
            WindowManager.ShowMediaWindow(media, vm, (s, e) => {
                vm.MediaDisplayed = false;
                web.Source = vm.WebpageUrl;
            });
            vm.MediaDisplayed = true;
            initializeSliderVideoPreview();
        }

        void skipBackwardSingleVideoPreview (int seconds) {
            var a = singleVideoPreview.Position.Subtract(new TimeSpan(0, 0, seconds));
            if (a < TimeSpan.Zero) a = TimeSpan.Zero;
            vm.SingleVideoPreviewPosition = a;
            singleVideoPreview.Position = a;
        }

        void skipForwardSingleVideoPreview (int seconds) {
            var a = singleVideoPreview.Position.Add(new TimeSpan(0, 0, seconds));
            if (vm.SingleVideoPreviewTotalLength <= a) a = vm.SingleVideoPreviewTotalLength;
            vm.SingleVideoPreviewPosition = a;
            singleVideoPreview.Position = a;
        }

        void up () {
            if (vm.MediaDisplayed) movePrevious();
            else shiftUp();
        }

        // Internet

        void NavigateUrl_Click (object sender, RoutedEventArgs e) { navigateUrl(); }
        void NavigateUrl_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { navigateUrl(); }

        // Media list setup

        void AddMediaToMediaList_Click (object sender, RoutedEventArgs e) { addMediaUsingFileDialog(); }
        void AddMediaToMediaList_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { addMediaUsingFileDialog(); }
        void AddSingleVideo_Click (object sender, RoutedEventArgs e) { addSingleVideoUsingFileDialog(); }
        void AddSingleVideo_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { addSingleVideoUsingFileDialog(); }
        void MoveDown_Click (object sender, RoutedEventArgs e) { down(); }
        void MoveDown_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { down(); }
        void MoveUp_Click (object sender, RoutedEventArgs e) { up(); }
        void MoveUp_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { up(); }
        void RemoveMediaFromMediaList_Click (object sender, RoutedEventArgs e) { removeMediaItem(); }
        void RemoveMediaFromMediaList_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { removeMediaItem(); }

        void Media_Drop (object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var paths = (string[]) e.Data.GetData(DataFormats.FileDrop);
            processMediaItems(paths);
        }

        void MediaList_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
            if (!vm.MediaDisplayed) showMediaListMedia();
            else {
                if (vm.CurrentMediaItem is VideoItem)
                    media?.PlayPauseVideo();
            }
        }

        void MediaList_SelectionChanged (object sender, SelectionChangedEventArgs e) {
            if (mediaList.SelectedIndex != -1)
                vm.MediaItemsCurrentIndex = mediaList.SelectedIndex;
        }

        void Minutes_KeyDown (object sender, KeyEventArgs e) { e.Handled = !isDigitKeyPress(e); }
        void Seconds_KeyDown (object sender, KeyEventArgs e) { e.Handled = !isDigitKeyPress(e); }

        // Single video setup

        void SkipBackwardLongSingleVideoPreview_Click (object sender, RoutedEventArgs e) { skipBackwardSingleVideoPreview(20); }
        void SkipBackwardLongSingleVideoPreview_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { skipBackwardSingleVideoPreview(20); }
        void SkipBackwardShortSingleVideoPreview_Click (object sender, RoutedEventArgs e) { skipBackwardSingleVideoPreview(2); }
        void SkipBackwardShortSingleVideoPreview_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { skipBackwardSingleVideoPreview(2); }

        void SkipForwardLongSingleVideoPreview_Click (object sender, RoutedEventArgs e) { skipForwardSingleVideoPreview(20); }
        void SkipForwardLongSingleVideoPreview_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { skipForwardSingleVideoPreview(20); }
        void SkipForwardShortSingleVideoPreview_Click (object sender, RoutedEventArgs e) { skipForwardSingleVideoPreview(2); }
        void SkipForwardShortSingleVideoPreview_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { skipForwardSingleVideoPreview(2); }

        // Manage media window

        void Pause_Click (object sender, RoutedEventArgs e) { playPauseVideo(); }
        void Pause_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { playPauseVideo(); }
        void Play_Click (object sender, RoutedEventArgs e) { playPauseVideo(); }
        void Play_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { playPauseVideo(); }
        void SkipBackward_Click (object sender, RoutedEventArgs e) { media?.SkipBackwardVideo(); }
        void SkipBackward_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { media?.SkipBackwardVideo(); }
        void SkipForward_Click (object sender, RoutedEventArgs e) { media?.SkipForwardVideo(); }
        void SkipForward_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { media?.SkipForwardVideo(); }
        void ShowWebpage_Click (object sender, RoutedEventArgs e) { showWebpage(); }
        void ShowWebpage_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { showWebpage(); }
        void Stop_Click (object sender, RoutedEventArgs e) { closeMedia(); }
        void Stop_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { closeMedia(); }

        bool videoPlayingBeforeSliderDragEvent = false;

        void Slider_DragStarted (object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e) {
            vm.SetVideoPositionSliderActive = true;
            videoPlayingBeforeSliderDragEvent = !vm.VideoPaused;
            vm.StopTimer();
            vm.PauseVideo();
            if (vm.Minified) sliderMinifiedPositionChangePreview.Position = vm.VideoPosition;
            else sliderMaxifiedPositionChangePreview.Position = vm.VideoPosition;
        }

        void Slider_DragCompleted (object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e) {
            vm.SetVideoPositionSliderActive = false;
            TimeSpan a = new(0, 0, (int) ((Slider) e.Source).Value);
            vm.UpdateVideoPosition(a);
            if (vm.Minified) sliderMinifiedPositionChangePreview.Position = a;
            else sliderMaxifiedPositionChangePreview.Position = a;
            if (videoPlayingBeforeSliderDragEvent) {
                vm.PlayVideo();
                vm.StartTimer();
            }
        }

        void Slider_ValueChanged (object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (!vm.SetVideoPositionSliderActive) return;
            TimeSpan a = new(0, 0, (int) ((Slider) e.Source).Value);
            vm.SetVideoPositionSliderPreviewPositionText = a.ToString(@"hh\:mm\:ss");
            if (vm.Minified) sliderMinifiedPositionChangePreview.Position = a;
            else sliderMaxifiedPositionChangePreview.Position = a;
        }

        // Manage window

        void Close_Click (object sender, RoutedEventArgs e) { Close(); }
        void Maxify_Click (object sender, RoutedEventArgs e) { maxify(); }
        void Maxify_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { maxify(); }
        void Minify_Click (object sender, RoutedEventArgs e) { minify(); }
        void Minify_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { minify(); }

        void Window_LocationChanged (object? sender, EventArgs e) {
            SettingsStorage.StartLocationLeft = Left;
            SettingsStorage.StartLocationTop = Top;
        }

        void Minimize_Click (object sender, RoutedEventArgs e) {
            SystemCommands.MinimizeWindow(this);
            if (media != null) SystemCommands.MinimizeWindow(media);
        }

        void InternetMode_Click (object sender, RoutedEventArgs e) {
            if (vm.MediaDisplayed) return;
            vm.MainWindowMode = MainWindowMode.Internet;
        }

        void MediaListMode_Click (object sender, RoutedEventArgs e) {
            if (vm.MediaDisplayed) return;
            vm.MainWindowMode = MainWindowMode.MediaList;
        }

        void SingleVideoMode_Click (object sender, RoutedEventArgs e) {
            if (vm.MediaDisplayed) return;
            vm.SingleVideoPreviewIsLoading = false;
            vm.MainWindowMode = MainWindowMode.SingleVideo;
        }

        void Menu_Click (object sender, RoutedEventArgs e) {
            IsEnabled = false;
            var menu = new MenuWindow {
                Owner = this,
                Top = Top + 15,
                Left = Left + 15,
            };
            menu.Closed += (_, _) => IsEnabled = true;
            menu.Show();
        }

        void TextBox_PreviewMouseDown (object sender, MouseButtonEventArgs e) {
            var a = (TextBox) sender;
            if (!a.IsKeyboardFocusWithin) a.Focus();
            a?.SelectAll();
            e.Handled = true;
        }

        void Window_Closing (object? sender, System.ComponentModel.CancelEventArgs e) {
            media?.Close();
            vm.BackupMediaItems();
        }

        void Window_Loaded (object sender, RoutedEventArgs e) {
            WindowManager.SetWindowPosition(this, vm);
            if (0 < mediaList.Items.Count) {
                mediaList.SelectedIndex = 0;
                mediaList.Focus();
            }
            var a = SettingsStorage.SingleVideoPath;
            if (a != "") {
                // Hide the drag-and-drop hint text
                vm.SingleVideo = new() { FileName = " " };
                setSingleVideo(a);
            }
        }

        void Window_MouseDown (object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
                try { DragMove(); } catch { }
            mediaList.Focus();
        }

        void Window_PreviewKeyDown (object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Down:
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift) shiftDown();
                    else moveNext();
                    break;
                case Key.Escape: media?.Close(); break;
                case Key.Left: media?.SkipBackwardVideo(); break;
                case Key.Right: media?.SkipForwardVideo(); break;
                case Key.Space:
                    if (mediaList.SelectedValue is PictureItem) return;
                    media?.PlayPauseVideo();
                    break;
                case Key.Up:
                    if (e.KeyboardDevice.Modifiers == ModifierKeys.Shift) shiftUp();
                    else movePrevious();
                    break;
            }
            if (!url.IsFocused)
                e.Handled = true;
        }

        void Url_KeyDown (object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                navigateUrl();
                e.Handled = true;
            }
        }
    }
}
