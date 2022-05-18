using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        }

        public MainViewModel vm => App.ViewModel;
        MediaWindow? media;

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

        void addMediaUsingFileDialog () {
            if (vm.MediaListMode) {
                Microsoft.Win32.OpenFileDialog many = new() {
                    FileName = "",
                    Filter = "Pictures and Videos |*.jpg;*.jpeg;*.png;*.gif;*.mp4;*.wav",
                    Multiselect = true,
                };
                many.ShowDialog();

                if (Enumerable.SequenceEqual(many.SafeFileNames, NoResults))
                    return;

                processMediaItems(many.FileNames);
                return;
            }

            Microsoft.Win32.OpenFileDialog one = new() {
                FileName = "",
                Filter = "Video |*.mp4;*.wav",
                Multiselect = false,
            };
            one.ShowDialog();
            if (string.IsNullOrEmpty(one.FileName)) return;
            if (!VideoExtensions.Contains(Path.GetExtension(one.FileName))) return;
            vm.SingleVideo = new();

            singleVideoText.Text = "Loading...";

            // Force the text to update before the thumbnail is generated
            singleVideoText.Dispatcher.Invoke(delegate { }, DispatcherPriority.ApplicationIdle);

            var uri = new Uri(one.FileName);
            vm.SingleVideo = new VideoItem {
                Name = Path.GetFileName(one.FileName),
                Path = one.FileName,
                Media = uri,
                IsPicture = false,
                Preview = generateVideoThumbnail(uri, 2),
            };

            singleVideoText.Text = "Drop video file here";
        }

        // The logic in this function must execute on the UI thread.
        // Force the UI of any frozen element to update before it is called.
        RenderTargetBitmap generateVideoThumbnail (Uri a, double seconds) {
            MediaPlayer player = new() {
                ScrubbingEnabled = true,
                Volume = 0,
            };
            player.Open(a);
            player.Position = TimeSpan.FromSeconds(seconds);
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

        void processMediaItems (string[] paths) {
            if (paths == null) return;
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

        void closeMedia () {
            media?.Close();
            vm.MediaDisplayed = false;
        }

        void removeMediaItem () {
            var i = mediaList.SelectedIndex;
            if (i < 0) return;
            if (vm.MediaItems.Count <= i) return;
            vm.RemoveMediaItem(i);
            if (vm.MediaItems.Count == 0) return;
            if (vm.MediaItems.Count <= i) i = vm.MediaItems.Count - 1;
            mediaList.SelectedIndex = i;
        }

        // Manage media list

        void AddMedia_Click (object sender, RoutedEventArgs e) { addMediaUsingFileDialog(); }
        void AddMedia_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { addMediaUsingFileDialog(); }
        void RemoveMedia_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { removeMediaItem(); }
        void RemoveMedia_Click (object sender, RoutedEventArgs e) { removeMediaItem(); }

        void MediaList_SelectionChanged (object sender, SelectionChangedEventArgs e) {
            if (mediaList.SelectedIndex != -1)
                vm.MediaItemsCurrentIndex = mediaList.SelectedIndex;
        }

        void MediaList_Drop (object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var paths = (string[]) e.Data.GetData(DataFormats.FileDrop);
            processMediaItems(paths);
        }

        // Navigate media list

        void down () {
            if (vm.MediaDisplayed) moveNext();
            else shiftDown();
        }

        void up () {
            if (vm.MediaDisplayed) movePrevious();
            else shiftUp();
        }

        void MoveDown_Click (object sender, RoutedEventArgs e) { down(); }
        void MoveDown_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { down(); }
        void MoveUp_Click (object sender, RoutedEventArgs e) { up(); }
        void MoveUp_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { up(); }

        void mediaList_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
            if (!vm.MediaDisplayed) showMedia();
            else {
                if (vm.MediaItems[vm.MediaItemsCurrentIndex] is VideoItem)
                    media?.PlayPauseVideo();
            }
        }

        // Manage media window

        void PlayMedia_Click (object sender, RoutedEventArgs e) { showMedia(); }
        void PlayMedia_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { showMedia(); }
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
            e.Handled = true;
        }
    }
}
