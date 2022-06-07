using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        double mainGridHeight = 0;
        GridLength mainGridRow0Height = new(0);

        readonly string[] NoResults = new string[] { "" };

        static bool isDigitKeyPress (KeyEventArgs a) =>
            a.KeyboardDevice.Modifiers == 0 &&
            Key.D0 <= a.Key &&
            a.Key <= Key.D9;

        void addMediaUsingFileDialog () {
            Microsoft.Win32.OpenFileDialog many = new() {
                FileName = "",
                Filter = "Pictures and Videos |*.jpg;*.jpeg;*.png;*.gif;*.mp4;*.wav;*.pdf",
                Multiselect = true,
            };
            many.ShowDialog();
            if (Enumerable.SequenceEqual(many.SafeFileNames, NoResults)) return;
            processMediaItems(many.FileNames);
        }

        void addSingleVideoUsingFileDialog () {
            Microsoft.Win32.OpenFileDialog one = new() {
                FileName = "",
                Filter = "Video |*.mp4;*.wav",
                Multiselect = false,
            };
            one.ShowDialog();
            if (string.IsNullOrEmpty(one.FileName)) return;
            if (!MainViewModel.VideoExtensions.Contains(Path.GetExtension(one.FileName))) return;
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

        void maxify () {
            vm.Minified = false;

            mainGrid.Height = mainGridHeight;
            mainGrid.RowDefinitions[0].Height = mainGridRow0Height;
            mainGrid.RowDefinitions[1].Height = GridLength.Auto;

            maxifyBorder.Visibility = Visibility.Collapsed;
            minifyBorder.Visibility = Visibility.Visible;

            Height = maximalHeight;
            WindowManager.SetWindowPosition(this, vm, maximalHeight);
        }

        void minify () {
            vm.Minified = true;

            mainGridHeight = mainGrid.Height;
            mainGridRow0Height = mainGrid.RowDefinitions[0].Height;
            mainGrid.RowDefinitions[0].Height = new(0);
            mainGrid.RowDefinitions[1].Height = new(0);

            minifyBorder.Visibility = Visibility.Collapsed;
            maxifyBorder.Visibility = Visibility.Visible;

            Height = minimalHeight;
            WindowManager.SetWindowPosition(this, vm, minimalHeight);
        }

        void moveNext () {
            if (mediaList.Items.Count == 0) return;
            if (mediaList.Items.Count - 1 <= mediaList.SelectedIndex) return;
            ++mediaList.SelectedIndex;
        }

        void movePrevious () {
            if (mediaList.Items.Count == 0) return;
            if (mediaList.SelectedIndex == 0) return;
            --mediaList.SelectedIndex;
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
            if (paths == null || paths.Length == 0) return;
            if (vm.InternetMode) return;
            if (vm.SingleVideoMode) {
                if (paths[0] == vm.SingleVideo.Path) return;
                setSingleVideo(paths[0]);
                return;
            }
            foreach (var path in paths) {
                var name = Path.GetFileName(path);
                var extension = Path.GetExtension(path);
                var uri = new Uri(path);
                if (MainViewModel.PictureExtensions.Contains(extension)) {
                    var bmp = new BitmapImage(uri);
                    vm.AddMediaItem(new PictureItem {
                        Name = name,
                        Path = path,
                        Preview = bmp,
                        Media = bmp,
                    });
                }
                else if (MainViewModel.VideoExtensions.Contains(extension)) {
                    vm.AddMediaItem(new VideoItem {
                        Name = name,
                        Path = path,
                        Media = uri,
                    });
                }
                else if (extension == ".pdf") {
                    vm.AddMediaItem(new PdfItem {
                        Name = name,
                        Path = path,
                        Media = uri,
                    });
                }
            }
            if (0 < paths.Length) {
                mediaList.SelectedIndex = 0;
                mediaList.Focus();
            }
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
        }

        void setSingleVideo (string path) {
            if (!MainViewModel.VideoExtensions.Contains(Path.GetExtension(path))) return;

            vm.SingleVideo = new();
            singleVideoTotalLength.Text = "00:00:00";
            singleVideoTotalLength.Foreground = new SolidColorBrush(Colors.Transparent);

            Uri uri = new(path);
            vm.SingleVideo = new VideoItem {
                Name = Path.GetFileName(path),
                Path = path,
                Media = uri,
            };

            try { singleVideoPreview.Stop(); } catch { }
            singleVideoPreviewPosition.Text = "00:00";
            singleVideoPreview.Source = vm.SingleVideo.Media;
            singleVideoPreview.Position = TimeSpan.Zero;
            singleVideoPreview.MediaOpened += (_, _) => singleVideoPreview.Pause();
            singleVideoPreview.Play();
            singleVideoPreview.Loaded += (_, _) => singleVideoPreview.Pause();
            singleVideoPreview.MediaOpened += (_, _) => {
                var a = singleVideoPreview.NaturalDuration.TimeSpan;
                singleVideoTotalLength.Text = a.ToString(@"hh\:mm\:ss");
                vm.SingleVideo.TotalLength = a;
                singleVideoTotalLength.Foreground = new SolidColorBrush(Colors.Gray);
            };
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
            media = new();
            WindowManager.ShowMediaWindow(media, vm, (s, e) => {
                vm.MediaDisplayed = false;
                vm.VideoPaused = true;
            });
            vm.MediaDisplayed = true;
        }

        void showSingleVideo () {
            if (string.IsNullOrEmpty(vm.SingleVideo.Name)) return;
            media = new();
            WindowManager.ShowMediaWindow(media, vm, (s, e) => {
                vm.MediaDisplayed = false;
                vm.VideoPaused = true;
            });
            vm.MediaDisplayed = true;
            vm.VideoDisplayedOnMediaWindow = true;
        }

        void showWebpage () {
            media = new();
            WindowManager.ShowMediaWindow(media, vm, (s, e) => {
                vm.MediaDisplayed = false;
            });
            vm.MediaDisplayed = true;
        }

        void skipBackwardSingleVideoPreview (int seconds) {
            var a = singleVideoPreview.Position.Subtract(new TimeSpan(0, 0, seconds));
            if (a < TimeSpan.Zero) a = TimeSpan.Zero;
            vm.SingleVideo.Skip = a;
            singleVideoPreview.Position = a;
            singleVideoPreviewPosition.Text = a.ToString(@"mm\:ss");
            singleVideoPreview.Play();
            System.Threading.Thread.Sleep(120);
            singleVideoPreview.Pause();
        }

        void skipForwardSingleVideoPreview (int seconds) {
            var a = singleVideoPreview.Position.Add(new TimeSpan(0, 0, seconds));
            if (vm.SingleVideo.TotalLength <= a) a = vm.SingleVideo.TotalLength;
            vm.SingleVideo.Skip = a;
            singleVideoPreview.Position = a;
            singleVideoPreviewPosition.Text = a.ToString(@"mm\:ss");
            singleVideoPreview.Play();
            System.Threading.Thread.Sleep(120);
            singleVideoPreview.Pause();
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

        // Manage window

        void Close_Click (object sender, RoutedEventArgs e) { Close(); }
        void Maxify_Click (object sender, RoutedEventArgs e) { maxify(); }
        void Maxify_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { maxify(); }
        void Minify_Click (object sender, RoutedEventArgs e) { minify(); }
        void Minify_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { minify(); }

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
            vm.MainWindowMode = MainWindowMode.SingleVideo;
        }

        void Menu_Click (object sender, RoutedEventArgs e) {
            IsEnabled = false;
            mainGrid.Opacity = 0.15;
            var menu = new MenuWindow {
                Owner = this,
                Top = Top + 15,
                Left = Left + 15,
            };
            menu.Closed += (_, _) => {
                mainGrid.Opacity = 1.0;
                IsEnabled = true;
            };
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
            if (!string.IsNullOrEmpty(vm.SingleVideo.Name))
                setSingleVideo(vm.SingleVideo.Media.AbsolutePath);
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
