using System.Windows;
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
    }
}
