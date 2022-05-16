using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using UI.ViewModel;

namespace UI {
    public partial class MainWindow : Window {
        public MainWindow () {
            InitializeComponent();
            DataContext = vm;
            mediaList.ItemsSource = vm.MediaItems;
            Closing += MainWindow_Closing;
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

        void addPictureUsingFileDialog () {
            Microsoft.Win32.OpenFileDialog dialog = new() {
                FileName = "",
                Filter = "Pictures and Videos |*.jpg;*.jpeg;*.png;*.gif;*.mp4;*.wav",
                Multiselect = true,
            };
            dialog.ShowDialog();

            if (Enumerable.SequenceEqual(dialog.SafeFileNames, NoResults))
                return;

            processMediaItems(dialog.FileNames);

            if (0 < mediaList.Items.Count) {
                mediaList.SelectedIndex = 0;
                mediaList.Focus();
            }
        }

        void movePictureDown () {
            if (mediaList.SelectedItem == null) return;
            if (vm.MediaItems.Count == 0) return;
            if (mediaList.Items.Count - 1 <= mediaList.SelectedIndex) return;
            var i = mediaList.SelectedIndex;

            // Necessary to cause the picture thumbnail to update
            mediaList.ItemsSource = null;

            (vm.MediaItems[i], vm.MediaItems[i + 1]) = (vm.MediaItems[i + 1], vm.MediaItems[i]);
            mediaList.ItemsSource = vm.MediaItems;

            mediaList.SelectedIndex = i + 1;
            mediaList.Focus();
        }

        void movePictureUp () {
            if (mediaList.SelectedItem == null) return;
            if (vm.MediaItems.Count == 0) return;
            if (mediaList.SelectedIndex == 0) return;
            var i = mediaList.SelectedIndex;

            // Necessary to cause the picture thumbnail to update
            mediaList.ItemsSource = null;

            (vm.MediaItems[i], vm.MediaItems[i - 1]) = (vm.MediaItems[i - 1], vm.MediaItems[i]);
            mediaList.ItemsSource = vm.MediaItems;

            mediaList.SelectedIndex = i - 1;
            mediaList.Focus();
        }

        void playSlideshow () {
            if (vm.MediaItems.Count == 0 || mediaList.Items.Count == 0) {
                vm.MediaListHasContents = false;
                return;
            }
            if (mediaList.SelectedValue == null)
                mediaList.SelectedIndex = 0;
            vm.CurrentMediaItemIndex = mediaList.SelectedIndex;
            media = new();
            SecondMonitor.ShowMediaWindow(media, vm, (s, e) => { vm.MediaDisplayMode = false; });
            vm.MediaDisplayMode = true;
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
        }

        void stopSlideshow () {
            media?.Close();
            vm.MediaDisplayMode = false;
        }

        // Keyboard access key events

        void KeyboardLeft_Executed (object sender, ExecutedRoutedEventArgs e) { media?.SkipBackwardVideo(); }
        void KeyboardRight_Executed (object sender, ExecutedRoutedEventArgs e) { media?.SkipForwardVideo(); }
        void KeyboardSpace_Executed (object sender, ExecutedRoutedEventArgs e) { media?.PlayPauseVideo(); }
        void KeyboardEscape_Executed (object sender, ExecutedRoutedEventArgs e) { media?.Close(); }

        // Manage picture list

        void AddPicture_Click (object sender, RoutedEventArgs e) { addPictureUsingFileDialog(); }
        void AddPicture_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { addPictureUsingFileDialog(); }
        void PictureList_SelectionChanged (object sender, SelectionChangedEventArgs e) { vm.MediaItemSelected = 0 < e.AddedItems.Count; }

        void PictureList_Drop (object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var paths = (string[]) e.Data.GetData(DataFormats.FileDrop);
            processMediaItems(paths);
            if (0 < mediaList.Items.Count) {
                mediaList.SelectedIndex = 0;
                mediaList.Focus();
            }
        }

        void PictureList_MouseDown (object sender, MouseButtonEventArgs e) {
            vm.MediaItemSelected = false;
            mediaList.SelectedIndex = -1;
        }

        // Navigate picture list

        void MovePictureDown_Click (object sender, RoutedEventArgs e) { movePictureDown(); }
        void MovePictureDown_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { movePictureDown(); }
        void MovePictureLeft_Click (object sender, RoutedEventArgs e) { vm.MoveToPreviousMediaItem(); }
        void MovePictureLeft_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { vm.MoveToPreviousMediaItem(); }
        void MovePictureRight_Click (object sender, RoutedEventArgs e) { vm.MoveToNextMediaItem(); }
        void MovePictureRight_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { vm.MoveToNextMediaItem(); }
        void MovePictureUp_Click (object sender, RoutedEventArgs e) { movePictureUp(); }
        void MovePictureUp_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { movePictureUp(); }

        // Navigate slideshow

        void PlaySlideshow_Click (object sender, RoutedEventArgs e) { playSlideshow(); }
        void PlaySlideshow_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { playSlideshow(); }
        void StopSlideshow_Click (object sender, RoutedEventArgs e) { stopSlideshow(); }
        void StopSlideshow_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { stopSlideshow(); }

        // Manage window

        void Close_Click (object sender, RoutedEventArgs e) { Close(); }
        void MainWindow_Closing (object? sender, System.ComponentModel.CancelEventArgs e) { media?.Close(); }

        void Picture_Click (object sender, RoutedEventArgs e) {
            if (!vm.MediaListMode)
                vm.MediaListMode = true;
        }

        void Video_Click (object sender, RoutedEventArgs e) {
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
    }
}
