using System.ComponentModel;
using System.Windows;

namespace UI.ViewModel {
    public static class WindowManager {
        public static void SetWindowPosition (Window window, MainViewModel vm) {
            if (vm.StartLocationLowerLeft) {
                window.Left = 0;
                window.Top = SystemParameters.WorkArea.Top + SystemParameters.WorkArea.Height - window.Height;
                return;
            }

            if (vm.StartLocationUpperLeft) {
                window.Left = 0;
                window.Top = 0;
                return;
            }

            if (vm.StartLocationUpperRight) {
                window.Left = SystemParameters.WorkArea.Left + SystemParameters.WorkArea.Width - window.Width;
                window.Top = 0;
                return;
            }

            // Lower-right
            window.Left = SystemParameters.WorkArea.Left + SystemParameters.WorkArea.Width - window.Width;
            window.Top = SystemParameters.WorkArea.Top + SystemParameters.WorkArea.Height - window.Height;
        }

        public static void ShowMediaWindow (Window window, MainViewModel vm, CancelEventHandler closing) {
            window.Closing += closing;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            if (vm.ShowMediaOnSecondMonitor) {
                var width = SystemParameters.VirtualScreenWidth - SystemParameters.PrimaryScreenWidth;
                if (vm.ShowMediaFullscreen) {
                    window.Left = SystemParameters.PrimaryScreenWidth;
                    window.Top = 0;
                    window.Height = SystemParameters.VirtualScreenHeight;
                    window.Width = width;
                    window.Loaded += (s, e) => {
                        var a = (s as Window) ?? new();
                        a.WindowState = WindowState.Maximized;
                    };
                }
                else {
                    window.Left = SystemParameters.PrimaryScreenWidth + width * 0.025;
                    window.Top = SystemParameters.VirtualScreenHeight * 0.025;
                    window.Height = SystemParameters.VirtualScreenHeight * 0.95;
                    window.Width = width * 0.95;
                }
                window.Show();
            }
            else {
                if (vm.ShowMediaFullscreen) {
                    window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    window.WindowState = WindowState.Maximized;
                    window.Show();
                }
                else {
                    window.Top = SystemParameters.PrimaryScreenHeight * 0.2;
                    window.Left = SystemParameters.PrimaryScreenWidth * 0.2;
                    window.Height = SystemParameters.WorkArea.Height * 0.6;
                    window.Width = SystemParameters.PrimaryScreenWidth * 0.6;
                    window.Show();
                }
            }
        }
    }
}
