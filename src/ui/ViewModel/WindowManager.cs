using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace UI.ViewModel {
    public static class WindowManager {
        public static void LetUIUpdate () {
            DispatcherFrame frame = new();
            DispatcherOperationCallback callback = new(delegate (object parameter) {
                frame.Continue = false;
                return null;
            });
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, callback, null);
            Dispatcher.PushFrame(frame);
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new Action(delegate { }));
        }

        public static void SetWindowPosition (Window window, MainViewModel vm) => SetWindowPosition(window, vm, window.Height);
        public static void SetWindowPosition (Window window, MainViewModel vm, double windowHeight) {
            if (vm.StartLocationLowerLeft) {
                window.Left = 0;
                window.Top = SystemParameters.WorkArea.Top + SystemParameters.WorkArea.Height - windowHeight;
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
            window.Top = SystemParameters.WorkArea.Top + SystemParameters.WorkArea.Height - windowHeight;
        }

        public static void ShowMediaWindow (Window window, MainViewModel vm, CancelEventHandler closing) {
            window.Closing += closing;
            window.WindowStartupLocation = WindowStartupLocation.Manual;
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
    }
}
