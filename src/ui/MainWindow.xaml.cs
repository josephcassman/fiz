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
                Filter = "Pictures |*.jpg;*.jpeg;*.png;*.gif"
            };
            var result = dialog.ShowDialog();
            if (result == true) {
                vm.Pictures.Add(new PictureItem {
                    Name = dialog.SafeFileName,
                    Path = dialog.FileName,
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
    }
}
