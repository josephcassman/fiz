using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            Loaded += Window_Loaded;
            vm.MoveDown += (_, _) => moveNext();
            vm.MoveUp += (_, _) => movePrevious();
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.PreviewMouseDownEvent, new MouseButtonEventHandler(TextBox_PreviewMouseDown));
        }

        public MainViewModel vm => App.ViewModel;
        MediaWindow? media;
        string skipMinutes = "";
        string skipSeconds = "";

        static readonly Regex DigitPattern = new(@"\d");

        readonly string[] NoResults = new string[] { "" };

        void addMediaUsingFileDialog () {
            Microsoft.Win32.OpenFileDialog many = new() {
                FileName = "",
                Filter = "Pictures and Videos |*.jpg;*.jpeg;*.png;*.gif;*.mp4;*.wav",
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

        void moveNext () {
            if (mediaList.Items.Count == 0) return;
            if (mediaList.Items.Count - 1 <= mediaList.SelectedIndex) return;
            vm.MediaItemsCurrentIndex = ++mediaList.SelectedIndex;
        }

        void movePrevious () {
            if (mediaList.Items.Count == 0) return;
            if (mediaList.SelectedIndex == 0) return;
            vm.MediaItemsCurrentIndex = --mediaList.SelectedIndex;
        }

        void playPauseVideo () {
            if (vm.MediaDisplayed) media?.PlayPauseVideo();
            else if (vm.MediaListMode) showMediaListMedia();
            else showSingleVideo();
        }

        void processMediaItems (string[] paths) {
            if (paths == null || paths.Length == 0) return;
            if (!vm.MediaListMode) { setSingleVideo(paths[0]); return; }
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
                        IsPicture = false,
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

            singleVideoTextOne.Text = "Loading...";
            singleVideoTextTwo.Text = "";

            // Force the text to update before the thumbnail is generated
            singleVideoGrid.Dispatcher.Invoke(delegate { }, DispatcherPriority.ApplicationIdle);

            var uri = new Uri(path);
            vm.SingleVideo = new VideoItem {
                Name = Path.GetFileName(path),
                Path = path,
                Media = uri,
                IsPicture = false,
                Preview = MainViewModel.GenerateSingleVideoThumbnail(uri, TimeSpan.FromSeconds(2)),
            };

            singleVideoTextOne.Text = "Drag and drop";
            singleVideoTextTwo.Text = "video here";
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
            SecondMonitor.ShowMediaWindow(media, vm, (s, e) => {
                vm.MediaDisplayed = false;
                vm.VideoPaused = true;
            });
            vm.MediaDisplayed = true;
        }

        void showSingleVideo () {
            if (string.IsNullOrEmpty(vm.SingleVideo.Name)) return;
            media = new();
            SecondMonitor.ShowMediaWindow(media, vm, (s, e) => {
                vm.MediaDisplayed = false;
                vm.VideoPaused = true;
            });
            vm.MediaDisplayed = true;
        }

        void up () {
            if (vm.MediaDisplayed) movePrevious();
            else shiftUp();
        }

        void updateSingleVideoThumbnail () {
            vm.SingleVideoSkipUpdated = false;

            var a = vm.SingleVideo;

            singleVideoTextOne.Text = "Loading...";
            singleVideoTextTwo.Text = "";

            vm.SingleVideo = new();

            // Force the text to update before the thumbnail is generated
            singleVideoGrid.Dispatcher.Invoke(delegate { }, DispatcherPriority.ApplicationIdle);

            _ = int.TryParse(minutes.Text, out int min);
            _ = int.TryParse(seconds.Text, out int sec);
            TimeSpan skip = new(0, min, sec);

            vm.SingleVideo = new VideoItem {
                Name = a.Name,
                Path = a.Path,
                Media = a.Media,
                IsPicture = false,
                Preview = MainViewModel.GenerateSingleVideoThumbnail(a.Media, skip),
                Skip = skip,
            };

            singleVideoTextOne.Text = "Drag and drop";
            singleVideoTextTwo.Text = "video here";
        }

        // Media setup

        void AddMediaToMediaList_Click (object sender, RoutedEventArgs e) { addMediaUsingFileDialog(); }
        void AddMediaToMediaList_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { addMediaUsingFileDialog(); }
        void AddSingleVideo_Click (object sender, RoutedEventArgs e) { addSingleVideoUsingFileDialog(); }
        void AddSingleVideo_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { addSingleVideoUsingFileDialog(); }
        void MoveDown_Click (object sender, RoutedEventArgs e) { down(); }
        void MoveDown_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { down(); }
        void MoveUp_Click (object sender, RoutedEventArgs e) { up(); }
        void MoveUp_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { up(); }
        void RefreshSingleVideoThumbnail_Click (object sender, RoutedEventArgs e) { updateSingleVideoThumbnail(); }
        void RefreshSingleVideoThumbnail_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { updateSingleVideoThumbnail(); }
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
                if (vm.MediaItems[vm.MediaItemsCurrentIndex] is VideoItem)
                    media?.PlayPauseVideo();
            }
        }

        void MediaList_SelectionChanged (object sender, SelectionChangedEventArgs e) {
            if (mediaList.SelectedIndex != -1)
                vm.MediaItemsCurrentIndex = mediaList.SelectedIndex;
        }

        void Minutes_KeyDown (object sender, KeyEventArgs e) {
            e.Handled = !DigitPattern.IsMatch(e.Key.ToString());
        }

        void Minutes_TextChanged (object sender, TextChangedEventArgs e) {
            if (minutes.Text != skipMinutes) {
                skipMinutes = minutes.Text;
                vm.SingleVideoSkipUpdated = true;
            }
            minutes.CaretIndex = minutes.Text.Length;
        }

        void Seconds_KeyDown (object sender, KeyEventArgs e) {
            e.Handled = !DigitPattern.IsMatch(e.Key.ToString());
        }

        void Seconds_TextChanged (object sender, TextChangedEventArgs e) {
            _ = int.TryParse(seconds.Text, out int a);
            if (59 < a) seconds.Text = "59";
            if (seconds.Text != skipSeconds) {
                skipSeconds = seconds.Text;
                vm.SingleVideoSkipUpdated = true;
            }
            seconds.CaretIndex = seconds.Text.Length;
        }

        // Manage media window

        void Pause_Click (object sender, RoutedEventArgs e) { playPauseVideo(); }
        void Pause_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { playPauseVideo(); }
        void Play_Click (object sender, RoutedEventArgs e) { playPauseVideo(); }
        void Play_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { playPauseVideo(); }
        void SkipBackward_Click (object sender, RoutedEventArgs e) { media?.SkipBackwardVideo(); }
        void SkipBackward_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { media?.SkipBackwardVideo(); }
        void SkipForward_Click (object sender, RoutedEventArgs e) { media?.SkipForwardVideo(); }
        void SkipForward_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { media?.SkipForwardVideo(); }
        void Stop_Click (object sender, RoutedEventArgs e) { closeMedia(); }
        void Stop_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { closeMedia(); }

        // Manage window

        void Close_Click (object sender, RoutedEventArgs e) { Close(); }

        void Minimize_Click (object sender, RoutedEventArgs e) {
            SystemCommands.MinimizeWindow(this);
            if (media != null) SystemCommands.MinimizeWindow(media);
        }

        void MediaList_Click (object sender, RoutedEventArgs e) {
            if (vm.MediaDisplayed) return;
            if (!vm.MediaListMode)
                vm.MediaListMode = true;
        }

        void SingleVideo_Click (object sender, RoutedEventArgs e) {
            if (vm.MediaDisplayed) return;
            if (vm.MediaListMode)
                vm.MediaListMode = false;
        }

        void Menu_Click (object sender, RoutedEventArgs e) {
            var menu = new MenuWindow {
                Owner = this
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
            if (0 < mediaList.Items.Count) {
                mediaList.SelectedIndex = 0;
                mediaList.Focus();
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
                    if (vm.MediaDisplayed) moveNext();
                    else shiftDown();
                    break;
                case Key.Escape: media?.Close(); break;
                case Key.Left: media?.SkipBackwardVideo(); break;
                case Key.Right: media?.SkipForwardVideo(); break;
                case Key.Space:
                    if (mediaList.SelectedValue is PictureItem) return;
                    media?.PlayPauseVideo();
                    break;
                case Key.Up:
                    if (vm.MediaDisplayed) movePrevious();
                    else shiftUp();
                    break;
            }
            if (!minutes.IsFocused && !seconds.IsFocused)
                e.Handled = true;
        }
    }
}
