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

        void Close_Click (object sender, RoutedEventArgs e) { Close(); }

        void DecreaseWebPageScaleFactor_Click (object sender, RoutedEventArgs e) {
            vm.WebPageScaleFactor -= 0.2;
        }

        void IncreaseWebPageScaleFactor_Click (object sender, RoutedEventArgs e) {
            vm.WebPageScaleFactor += 0.2;
        }

        void Window_MouseDown (object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
                try { DragMove(); } catch { }
        }
    }
}
