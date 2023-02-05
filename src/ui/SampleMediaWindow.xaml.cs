using System.Windows;
using System.Windows.Input;
using UI.ViewModel;

namespace UI {
    public partial class SampleMediaWindow : Window {
        public SampleMediaWindow () {
            InitializeComponent();
            MouseDoubleClick += Window_MouseDoubleClick;
            MouseDown += Window_MouseDown;
            SizeChanged += Window_SizeChanged;
        }

        public MainViewModel vm => App.ViewModel;

        void Close_Click (object sender, RoutedEventArgs e) {
            Close();
        }

        void CloseBorder_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
            Close();
        }

        void Maximize_Click (object sender, RoutedEventArgs e) {
            SystemCommands.MaximizeWindow(this);
        }

        void MaximizeBorder_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
            SystemCommands.MaximizeWindow(this);
        }

        void Restore_Click (object sender, RoutedEventArgs e) {
            SystemCommands.RestoreWindow(this);
        }

        void RestoreBorder_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
            SystemCommands.RestoreWindow(this);
        }

        void Window_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
            if (WindowState == WindowState.Maximized)
                SystemCommands.RestoreWindow(this);
            else
                SystemCommands.MaximizeWindow(this);
        }

        void Window_MouseDown (object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        void Window_SizeChanged (object sender, SizeChangedEventArgs e) {
            grid.Height = e.NewSize.Height;
            grid.Width = e.NewSize.Width;
            navigationTopBackground.Width = e.NewSize.Width - 20;
            navigation.Height = e.NewSize.Height - 20;
            navigation.Width = e.NewSize.Width - 20;
            if (WindowState == WindowState.Maximized) {
                maximizeBorder.Visibility = Visibility.Collapsed;
                restoreBorder.Visibility = Visibility.Visible;
            }
            else {
                restoreBorder.Visibility = Visibility.Collapsed;
                maximizeBorder.Visibility = Visibility.Visible;
            }
        }
    }
}
