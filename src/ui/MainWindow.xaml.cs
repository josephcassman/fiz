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
            pictureList.ItemsSource = vm.MediaItems;
            Closing += MainWindow_Closing;
        }

        public MainViewModel vm => App.ViewModel;
        PictureSlideshowWindow? slideshow;

        static readonly HashSet<string> PictureExtensions = new() {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
        };

        readonly string[] NoResults = new string[] { "Document" };

        void addPictureUsingFileDialog () {
            Microsoft.Win32.OpenFileDialog dialog = new() {
                FileName = "Document",
                Filter = "Pictures |*.jpg;*.jpeg;*.png;*.gif",
                Multiselect = true,
            };
            var result = dialog.ShowDialog();

            // SafeFileNames = ["Document"] when the dialog is cancelled or closed without
            // selecting a file
            if (Enumerable.SequenceEqual(dialog.SafeFileNames, NoResults))
                return;

            foreach (var path in dialog.SafeFileNames.Zip(dialog.FileNames, (a, b) => (a, b))) {
                vm.AddPicture(new PictureItem {
                    Name = path.a,
                    Path = path.b,
                    Bitmap = new BitmapImage(new Uri(path.b)) { DecodePixelHeight = 450 },
                });
            }
            if (0 < pictureList.Items.Count) {
                pictureList.SelectedIndex = 0;
                pictureList.Focus();
            }
        }

        void movePictureDown () {
            if (pictureList.SelectedItem == null) return;
            if (vm.MediaItems.Count == 0) return;
            if (pictureList.Items.Count - 1 <= pictureList.SelectedIndex) return;
            var i = pictureList.SelectedIndex;

            // Necessary to cause the picture thumbnail to update
            pictureList.ItemsSource = null;

            (vm.MediaItems[i], vm.MediaItems[i + 1]) = (vm.MediaItems[i + 1], vm.MediaItems[i]);
            pictureList.ItemsSource = vm.MediaItems;

            pictureList.SelectedIndex = i + 1;
            pictureList.Focus();
        }

        void movePictureUp () {
            if (pictureList.SelectedItem == null) return;
            if (vm.MediaItems.Count == 0) return;
            if (pictureList.SelectedIndex == 0) return;
            var i = pictureList.SelectedIndex;

            // Necessary to cause the picture thumbnail to update
            pictureList.ItemsSource = null;

            (vm.MediaItems[i], vm.MediaItems[i - 1]) = (vm.MediaItems[i - 1], vm.MediaItems[i]);
            pictureList.ItemsSource = vm.MediaItems;

            pictureList.SelectedIndex = i - 1;
            pictureList.Focus();
        }

        void playSlideshow () {
            if (vm.MediaItems.Count == 0 || pictureList.Items.Count == 0) {
                vm.MediaListHasContents = false;
                return;
            }
            if (pictureList.SelectedValue == null)
                pictureList.SelectedIndex = 0;
            vm.CurrentMediaItemIndex = pictureList.SelectedIndex;
            slideshow = new();
            SecondMonitor.ShowMediaWindow(slideshow, vm, (s, e) => { vm.MediaDisplayMode = false; });
            vm.CurrentPicture = vm.MediaItems[pictureList.SelectedIndex].Bitmap;
            vm.MediaDisplayMode = true;
        }

        void stopSlideshow () {
            slideshow?.Close();
            vm.MediaDisplayMode = false;
        }

        // Keyboard access key events

        private void KeyboardLeft_Executed (object sender, ExecutedRoutedEventArgs e) { vm.MoveToPreviousPicture(); }
        private void KeyboardRight_Executed (object sender, ExecutedRoutedEventArgs e) { vm.MoveToNextPicture(); }

        // Manage picture list

        private void AddPicture_Click (object sender, RoutedEventArgs e) { addPictureUsingFileDialog(); }
        private void AddPicture_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { addPictureUsingFileDialog(); }
        private void PictureList_SelectionChanged (object sender, SelectionChangedEventArgs e) { vm.MediaItemSelected = 0 < e.AddedItems.Count; }

        private void PictureList_Drop (object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var paths = (string[]) e.Data.GetData(DataFormats.FileDrop);
            foreach (var path in paths.Where(x => PictureExtensions.Contains(Path.GetExtension(x)))) {
                vm.AddPicture(new PictureItem {
                    Name = Path.GetFileName(path),
                    Path = path,
                    Bitmap = new BitmapImage(new Uri(path)) { DecodePixelHeight = 450 },
                });
            }
            if (0 < pictureList.Items.Count) {
                pictureList.SelectedIndex = 0;
                pictureList.Focus();
            }
        }

        private void PictureList_MouseDown (object sender, MouseButtonEventArgs e) {
            vm.MediaItemSelected = false;
            pictureList.SelectedIndex = -1;
        }

        // Navigate picture list

        private void MovePictureDown_Click (object sender, RoutedEventArgs e) { movePictureDown(); }
        private void MovePictureDown_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { movePictureDown(); }
        private void MovePictureLeft_Click (object sender, RoutedEventArgs e) { vm.MoveToPreviousPicture(); }
        private void MovePictureLeft_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { vm.MoveToPreviousPicture(); }
        private void MovePictureRight_Click (object sender, RoutedEventArgs e) { vm.MoveToNextPicture(); }
        private void MovePictureRight_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { vm.MoveToNextPicture(); }
        private void MovePictureUp_Click (object sender, RoutedEventArgs e) { movePictureUp(); }
        private void MovePictureUp_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { movePictureUp(); }

        // Navigate slideshow

        private void PlaySlideshow_Click (object sender, RoutedEventArgs e) { playSlideshow(); }
        private void PlaySlideshow_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { playSlideshow(); }
        private void StopSlideshow_Click (object sender, RoutedEventArgs e) { stopSlideshow(); }
        private void StopSlideshow_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { stopSlideshow(); }

        // Manage window

        private void Close_Click (object sender, RoutedEventArgs e) { Close(); }
        private void MainWindow_Closing (object? sender, System.ComponentModel.CancelEventArgs e) { slideshow?.Close(); }

        private void Picture_Click (object sender, RoutedEventArgs e) {
            if (!vm.MediaListMode)
                vm.MediaListMode = true;
        }

        private void Video_Click (object sender, RoutedEventArgs e) {
            if (vm.MediaListMode)
                vm.MediaListMode = false;
        }

        private void Menu_Click (object sender, RoutedEventArgs e) {
            var menu = new MenuWindow {
                Owner = this
            };
            menu.Show();
        }

        private void Window_MouseDown (object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
                try { DragMove(); } catch { }
            pictureList.Focus();
        }
    }
}
