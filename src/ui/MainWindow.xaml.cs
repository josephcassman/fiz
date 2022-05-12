using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
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
        PictureSlideshow? slideshow;

        static readonly HashSet<string> PictureExtensions = new HashSet<string> {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
        };

        private void MainWindow_Closing (object? sender, System.ComponentModel.CancelEventArgs e) {
            slideshow?.Close();
        }

        private void Picture_Click (object sender, RoutedEventArgs e) {
            vm.PictureMode = false;
        }

        private void Video_Click (object sender, RoutedEventArgs e) {
            vm.PictureMode = true;
        }

        private void AddPicture_Click (object sender, RoutedEventArgs e) {
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

        private void PictureList_SelectionChanged (object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            vm.PictureSelected = 0 < e.AddedItems.Count;
        }

        private void PictureList_MouseDown (object sender, System.Windows.Input.MouseButtonEventArgs e) {
            vm.PictureSelected = false;
            pictureList.SelectedIndex = -1;
        }

        private void MoveDown_Click (object sender, RoutedEventArgs e) {
            if (pictureList.SelectedItem == null) return;
            if (vm.Pictures.Count == 0) return;
            if (pictureList.Items.Count - 1 <= pictureList.SelectedIndex) return;
            var i = pictureList.SelectedIndex;

            // Necessary to cause the picture thumbnail to update
            pictureList.ItemsSource = null;

            (vm.Pictures[i], vm.Pictures[i + 1]) = (vm.Pictures[i + 1], vm.Pictures[i]);
            pictureList.ItemsSource = vm.Pictures;

            pictureList.SelectedIndex = i + 1;
            pictureList.Focus();
        }

        private void MoveUp_Click (object sender, RoutedEventArgs e) {
            if (pictureList.SelectedItem == null) return;
            if (vm.Pictures.Count == 0) return;
            if (pictureList.SelectedIndex == 0) return;
            var i = pictureList.SelectedIndex;

            // Necessary to cause the picture thumbnail to update
            pictureList.ItemsSource = null;

            (vm.Pictures[i], vm.Pictures[i - 1]) = (vm.Pictures[i - 1], vm.Pictures[i]);
            pictureList.ItemsSource = vm.Pictures;

            pictureList.SelectedIndex = i - 1;
            pictureList.Focus();
        }

        private void KeyboardLeft_Executed (object sender, ExecutedRoutedEventArgs e) {
            vm.MovePrevious();
        }

        private void KeyboardRight_Executed (object sender, ExecutedRoutedEventArgs e) {
            vm.MoveNext();
        }

        private void MoveLeft_Click (object sender, RoutedEventArgs e) {
            vm.MovePrevious();
        }

        private void MoveRight_Click (object sender, RoutedEventArgs e) {
            vm.MoveNext();
        }

        private void PlaySlideshow_Click (object sender, RoutedEventArgs e) {
            if (vm.Pictures.Count == 0 || pictureList.Items.Count == 0) {
                vm.HasPictures = false;
                return;
            }
            if (pictureList.SelectedValue == null) pictureList.SelectedIndex = 0;
            vm.CurrentPictureIndex = pictureList.SelectedIndex;
            slideshow = new();
            Topmost = true;
            slideshow.Show();
            vm.CurrentPicture = vm.Pictures[pictureList.SelectedIndex].Bitmap;
            vm.PlayingPictureSlideshow = true;
        }

        private void StopSlideshow_Click (object sender, RoutedEventArgs e) {
            slideshow?.Close();
            vm.PlayingPictureSlideshow = false;
        }

        private void Window_MouseDown (object sender, MouseButtonEventArgs e) {
            pictureList.Focus();
        }
    }
}
