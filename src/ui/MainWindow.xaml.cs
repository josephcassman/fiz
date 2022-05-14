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
            pictureList.ItemsSource = vm.Pictures;
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

        void addPictureUsingFileDialog () {
            Microsoft.Win32.OpenFileDialog dialog = new() {
                FileName = "Document",
                Filter = "Pictures |*.jpg;*.jpeg;*.png;*.gif",
                Multiselect = true,
            };
            var result = dialog.ShowDialog();
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
            if (pictureList.SelectedItem == null)
                return;
            if (vm.Pictures.Count == 0)
                return;
            if (pictureList.Items.Count - 1 <= pictureList.SelectedIndex)
                return;
            var i = pictureList.SelectedIndex;

            // Necessary to cause the picture thumbnail to update
            pictureList.ItemsSource = null;

            (vm.Pictures[i], vm.Pictures[i + 1]) = (vm.Pictures[i + 1], vm.Pictures[i]);
            pictureList.ItemsSource = vm.Pictures;

            pictureList.SelectedIndex = i + 1;
            pictureList.Focus();
        }

        void movePictureUp () {
            if (pictureList.SelectedItem == null)
                return;
            if (vm.Pictures.Count == 0)
                return;
            if (pictureList.SelectedIndex == 0)
                return;
            var i = pictureList.SelectedIndex;

            // Necessary to cause the picture thumbnail to update
            pictureList.ItemsSource = null;

            (vm.Pictures[i], vm.Pictures[i - 1]) = (vm.Pictures[i - 1], vm.Pictures[i]);
            pictureList.ItemsSource = vm.Pictures;

            pictureList.SelectedIndex = i - 1;
            pictureList.Focus();
        }

        void playSlideshow () {
            if (vm.Pictures.Count == 0 || pictureList.Items.Count == 0) {
                vm.HasPictures = false;
                return;
            }
            if (pictureList.SelectedValue == null)
                pictureList.SelectedIndex = 0;
            vm.CurrentPictureIndex = pictureList.SelectedIndex;
            slideshow = new();
            SecondMonitor.ShowMediaWindow(slideshow, vm, (s, e) => { vm.PlayingPictureSlideshow = false; });
            vm.CurrentPicture = vm.Pictures[pictureList.SelectedIndex].Bitmap;
            vm.PlayingPictureSlideshow = true;
        }

        void stopSlideshow () {
            slideshow?.Close();
            vm.PlayingPictureSlideshow = false;
        }

        // Keyboard access key events

        private void KeyboardLeft_Executed (object sender, ExecutedRoutedEventArgs e) { vm.MoveToPreviousPicture(); }
        private void KeyboardRight_Executed (object sender, ExecutedRoutedEventArgs e) { vm.MoveToNextPicture(); }

        // Manage picture list

        private void AddPicture_Click (object sender, RoutedEventArgs e) { addPictureUsingFileDialog(); }
        private void AddPicture_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { addPictureUsingFileDialog(); }
        private void PictureList_SelectionChanged (object sender, SelectionChangedEventArgs e) { vm.PictureSelected = 0 < e.AddedItems.Count; }

        private void PictureList_Drop (object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;
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
            vm.PictureSelected = false;
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
            if (!vm.PictureMode)
                vm.PictureMode = true;
        }

        private void Video_Click (object sender, RoutedEventArgs e) {
            if (vm.PictureMode)
                vm.PictureMode = false;
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
