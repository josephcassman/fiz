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

        public static void SetWindowPosition (Window window, MainViewModel vm) {
            window.Left = vm.StartLocationLeft;
            window.Top = vm.StartLocationTop;
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
