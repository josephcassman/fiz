using System.Windows;
using UI.ViewModel;

namespace UI {
    public partial class MainWindow : Window {
        public MainWindow () {
            InitializeComponent();
            DataContext = vm;
        }

        public MainViewModel vm => App.ViewModel;

        private void Picture_Click (object sender, RoutedEventArgs e) {
            vm.PictureMode = false;
        }

        private void Video_Click (object sender, RoutedEventArgs e) {
            vm.PictureMode = true;
        }

        private void AddPicture_Click (object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFileDialog dialog = new();
            dialog.FileName = "Document";
            dialog.Filter = "Pictures |*.jpg;*.jpeg;*.png;*.gif";

            var result = dialog.ShowDialog();

            if (result == true) {
                var filename = dialog.FileName;
            }
        }
    }
}
