using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using UI.ViewModel;

namespace UI {
    public partial class MainWindow : Window {
        public MainWindow () {
            InitializeComponent();
            DataContext = vm;
            PictureList.ItemsSource = vm.Pictures;
        }

        public MainViewModel vm => App.ViewModel;

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
                vm.Pictures.Add(new PictureItem {
                    Name = path.a,
                    Path = path.b,
                });
            }
        }

        static readonly HashSet<string> PictureExtensions = new HashSet<string> {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
        };

        private void PictureList_Drop (object sender, DragEventArgs e) {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var paths = (string[]) e.Data.GetData(DataFormats.FileDrop);
            foreach (var path in paths.Where(x => PictureExtensions.Contains(Path.GetExtension(x)))) {
                vm.Pictures.Add(new PictureItem {
                    Name = Path.GetFileName(path),
                    Path = path,
                });
            }
        }

        private void PictureList_SelectionChanged (object sender, System.Windows.Controls.SelectionChangedEventArgs e) {
            vm.PictureSelected = 0 < e.AddedItems.Count;
        }

        private void PictureList_MouseDown (object sender, System.Windows.Input.MouseButtonEventArgs e) {
            vm.PictureSelected = false;
            PictureList.SelectedIndex = -1;
        }

        private void MoveDown_Click (object sender, RoutedEventArgs e) {
            if (PictureList.SelectedItem == null) return;
            if (vm.Pictures.Count == 0) return;
            if (PictureList.Items.Count - 1 <= PictureList.SelectedIndex) return;
            var i = PictureList.SelectedIndex;

            // Necessary to cause the picture thumbnail to update
            PictureList.ItemsSource = null;

            (vm.Pictures[i], vm.Pictures[i + 1]) = (vm.Pictures[i + 1], vm.Pictures[i]);
            PictureList.ItemsSource = vm.Pictures;

            PictureList.SelectedIndex = i + 1;
            PictureList.Focus();
        }

        private void MoveUp_Click (object sender, RoutedEventArgs e) {
            if (PictureList.SelectedItem == null) return;
            if (vm.Pictures.Count == 0) return;
            if (PictureList.SelectedIndex == 0) return;
            var i = PictureList.SelectedIndex;

            // Necessary to cause the picture thumbnail to update
            PictureList.ItemsSource = null;

            (vm.Pictures[i], vm.Pictures[i - 1]) = (vm.Pictures[i - 1], vm.Pictures[i]);
            PictureList.ItemsSource = vm.Pictures;

            PictureList.SelectedIndex = i - 1;
            PictureList.Focus();
        }
    }
}
