using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using UI.ViewModel;

namespace UI {
    public partial class MenuWindow : Window {
        public MenuWindow () {
            InitializeComponent();
            DataContext = vm;
            MouseDown += Window_MouseDown;
        }

        public MainViewModel vm => App.ViewModel;

        void updateWindowLocation () {
            WindowManager.SetWindowPosition(Application.Current.MainWindow, vm);
            Left = Application.Current.MainWindow.Left + 15;
            Top = Application.Current.MainWindow.Top + 15;
        }

        void Close_Click (object sender, RoutedEventArgs e) { Close(); }

        void Window_MouseDown (object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
                try { DragMove(); } catch { }
        }

        void StartLocationLowerLeft_Click (object sender, RoutedEventArgs e) {
            var a = (CheckBox) sender;
            if (a == null || a.IsChecked == false) {
                vm.StartLocationLowerLeft = true;
                e.Handled = true;
                return;
            }
            vm.StartLocationLowerLeft = true;
            vm.StartLocationUpperLeft = false;
            vm.StartLocationUpperRight = false;
            vm.StartLocationLowerRight = false;
            updateWindowLocation();
        }

        void StartLocationUpperLeft_Click (object sender, RoutedEventArgs e) {
            var a = (CheckBox) sender;
            if (a == null || a.IsChecked == false) {
                vm.StartLocationUpperLeft = true;
                e.Handled = true;
                return;
            }
            vm.StartLocationLowerLeft = false;
            vm.StartLocationUpperLeft = true;
            vm.StartLocationUpperRight = false;
            vm.StartLocationLowerRight = false;
            updateWindowLocation();
        }

        void StartLocationUpperRight_Click (object sender, RoutedEventArgs e) {
            var a = (CheckBox) sender;
            if (a == null || a.IsChecked == false) {
                vm.StartLocationUpperRight = true;
                e.Handled = true;
                return;
            }
            vm.StartLocationLowerLeft = false;
            vm.StartLocationUpperLeft = false;
            vm.StartLocationUpperRight = true;
            vm.StartLocationLowerRight = false;
            updateWindowLocation();
        }

        void StartLocationLowerRight_Click (object sender, RoutedEventArgs e) {
            var a = (CheckBox) sender;
            if (a == null || a.IsChecked == false) {
                vm.StartLocationLowerRight = true;
                e.Handled = true;
                return;
            }
            vm.StartLocationLowerLeft = false;
            vm.StartLocationUpperLeft = false;
            vm.StartLocationUpperRight = false;
            vm.StartLocationLowerRight = true;
            updateWindowLocation();
        }

    }
}
