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
    }
}
