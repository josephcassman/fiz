using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using UI.ViewModel;

namespace UI {
    public partial class MainWindow : Window {
        public MainWindow () {
            InitializeComponent();
            DataContext = vm;
            mediaList.ItemsSource = vm.MediaItems;
            Closing += MainWindow_Closing;
            vm.MoveDown += (_, _) => moveNext();
            vm.MoveUp += (_, _) => movePrevious();
            EventManager.RegisterClassHandler(typeof(TextBox), TextBox.PreviewMouseDownEvent, new MouseButtonEventHandler(TextBox_PreviewMouseDown));
        }

        public MainViewModel vm => App.ViewModel;
        MediaWindow? media;
        string skipMinutes = "";
        string skipSeconds = "";

        static readonly Regex DigitPattern = new(@"\d");

        public static readonly HashSet<string> PictureExtensions = new() {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
        };

        public static readonly HashSet<string> VideoExtensions = new() {
            ".mp4",
            ".wav",
        };

        readonly string[] NoResults = new string[] { "" };

        // The logic in this function must execute on the UI thread.
        // Force the UI of any frozen element to update before it is called.
        static RenderTargetBitmap generateSingleVideoThumbnail (Uri a, TimeSpan skip) {
            MediaPlayer player = new() {
                ScrubbingEnabled = true,
                Volume = 0,
            };
            player.Open(a);
            player.Position = skip;
            System.Threading.Thread.Sleep(2000);

            DrawingVisual dv = new();
            DrawingContext dc = dv.RenderOpen();
            dc.DrawVideo(player, new Rect(0, 0, 330, 330));
            dc.Close();

            RenderTargetBitmap bmp = new(330, 330, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            player.Close();
            return bmp;
        }

        void addMediaUsingFileDialog () {
            Microsoft.Win32.OpenFileDialog many = new() {
                FileName = "",
                Filter = "Pictures and Videos |*.jpg;*.jpeg;*.png;*.gif;*.mp4;*.wav",
                Multiselect = true,
            };
            many.ShowDialog();

            if (Enumerable.SequenceEqual(many.SafeFileNames, NoResults))
                return;

            processMediaItems(many.FileNames);
        }

        void addSingleVideoUsingFileDialog () {
            Microsoft.Win32.OpenFileDialog one = new() {
                FileName = "",
                Filter = "Video |*.mp4;*.wav",
                Multiselect = false,
            };
            one.ShowDialog();
            if (string.IsNullOrEmpty(one.FileName))
                return;
            if (!VideoExtensions.Contains(Path.GetExtension(one.FileName)))
                return;
            vm.SingleVideo = new();

            singleVideoText.Text = "Loading...";

            // Force the text to update before the thumbnail is generated
            singleVideoGrid.Dispatcher.Invoke(delegate { }, DispatcherPriority.ApplicationIdle);

            var uri = new Uri(one.FileName);
            vm.SingleVideo = new VideoItem {
                Name = Path.GetFileName(one.FileName),
                Path = one.FileName,
                Media = uri,
                IsPicture = false,
                Preview = generateSingleVideoThumbnail(uri, TimeSpan.FromSeconds(2)),
            };

            singleVideoText.Text = "Drop video file here";
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

        void processMediaItems (string[] paths) {
            if (paths == null)
                return;
            foreach (var path in paths) {
                var name = Path.GetFileName(path);
                var extension = Path.GetExtension(path);
                var uri = new Uri(path);
                if (PictureExtensions.Contains(extension)) {
                    var bmp = new BitmapImage(uri);
                    vm.AddMediaItem(new PictureItem {
                        Name = name,
                        Path = path,
                        Preview = bmp,
                        Media = bmp,
                    });
                }
                else if (VideoExtensions.Contains(extension)) {
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
            if (i < 0)
                return;
            if (vm.MediaItems.Count <= i)
                return;
            vm.RemoveMediaItem(i);
            if (vm.MediaItems.Count == 0)
                return;
            if (vm.MediaItems.Count <= i)
                i = vm.MediaItems.Count - 1;
            mediaList.SelectedIndex = i;
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

        void showMedia () {
            if (vm.MediaItems.Count == 0 || mediaList.Items.Count == 0) return;
            if (mediaList.SelectedValue == null) mediaList.SelectedIndex = 0;
            media = new();
            SecondMonitor.ShowMediaWindow(media, vm, (s, e) => { vm.MediaDisplayed = false; });
            vm.MediaDisplayed = true;
        }

        void showSingleVideo () {
            if (string.IsNullOrEmpty(vm.SingleVideo.Name)) return;
            media = new();
            SecondMonitor.ShowMediaWindow(media, vm, (s, e) => { vm.MediaDisplayed = false; });
            vm.MediaDisplayed = true;
        }

        void up () {
            if (vm.MediaDisplayed) movePrevious();
            else shiftUp();
        }

        void updateSingleVideoThumbnail () {
            vm.SingleVideoSkipUpdated = false;

            var a = vm.SingleVideo;

            singleVideoText.Text = "Loading...";
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
                Preview = generateSingleVideoThumbnail(a.Media, skip),
                Skip = skip,
            };

            singleVideoText.Text = "Drop video file here";
        }

        // Media setup

        void AddMedia_Click (object sender, RoutedEventArgs e) { addMediaUsingFileDialog(); }
        void AddMedia_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { addMediaUsingFileDialog(); }
        void AddSingleVideo_Click (object sender, RoutedEventArgs e) { addSingleVideoUsingFileDialog(); }
        void AddSingleVideo_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { addSingleVideoUsingFileDialog(); }
        void MoveDown_Click (object sender, RoutedEventArgs e) { down(); }
        void MoveDown_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { down(); }
        void MoveUp_Click (object sender, RoutedEventArgs e) { up(); }
        void MoveUp_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { up(); }
        void RefreshSingleVideoThumbnail_Click (object sender, RoutedEventArgs e) { updateSingleVideoThumbnail(); }
        void RefreshSingleVideoThumbnail_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { updateSingleVideoThumbnail(); }
        void RemoveMedia_Click (object sender, RoutedEventArgs e) { removeMediaItem(); }
        void RemoveMedia_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { removeMediaItem(); }

        void MediaList_Drop (object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var paths = (string[]) e.Data.GetData(DataFormats.FileDrop);
            processMediaItems(paths);
        }

        void MediaList_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
            if (!vm.MediaDisplayed) showMedia();
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

        void PlayMedia_Click (object sender, RoutedEventArgs e) { showMedia(); }
        void PlayMedia_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { showMedia(); }
        void PlaySingleVideo_Click (object sender, RoutedEventArgs e) { showSingleVideo(); }
        void PlaySingleVideo_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { showSingleVideo(); }
        void StopMedia_Click (object sender, RoutedEventArgs e) { closeMedia(); }
        void StopMedia_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { closeMedia(); }

        // Manage window

        void Close_Click (object sender, RoutedEventArgs e) { Close(); }
        void MainWindow_Closing (object? sender, System.ComponentModel.CancelEventArgs e) { media?.Close(); }

        void Minimize_Click (object sender, RoutedEventArgs e) {
            SystemCommands.MinimizeWindow(this);
            if (media != null) SystemCommands.MinimizeWindow(media);
        }

        void MediaList_Click (object sender, RoutedEventArgs e) {
            if (!vm.MediaListMode)
                vm.MediaListMode = true;
        }

        void SingleVideo_Click (object sender, RoutedEventArgs e) {
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

        void Window_MouseDown (object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
                try { DragMove(); } catch { }
            mediaList.Focus();
        }

        private void Window_PreviewKeyDown (object sender, KeyEventArgs e) {
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
            if (!minutes.IsFocused && !seconds.IsFocused)
                e.Handled = true;
        }
    }
}
