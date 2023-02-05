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
            window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = vm.MediaWindowLeft;
            window.Top = vm.MediaWindowTop;
            window.Height = vm.MediaWindowHeight;
            window.Width = vm.MediaWindowWidth;
            window.Show();
        }

        public static Window ShowSampleMediaWindow (MainViewModel vm) {
            var height = SettingsStorage.MediaWindowHeight;
            var width = SettingsStorage.MediaWindowWidth;
            var r = new SampleMediaWindow() {
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = SettingsStorage.MediaWindowLeft,
                Top = SettingsStorage.MediaWindowTop,
                Height = height == 0.0 ? 500.0 : height,
                Width = width == 0.0 ? 800.0 : width,
            };
            r.Closing += (s, e) => {
                var b = (s as Window) ?? new();
                SettingsStorage.MediaWindowLeft = b.Left;
                SettingsStorage.MediaWindowTop = b.Top;
                SettingsStorage.MediaWindowHeight = b.Height;
                SettingsStorage.MediaWindowWidth = b.Width;
                vm.MediaWindowLeft = b.Left;
                vm.MediaWindowTop = b.Top;
                vm.MediaWindowHeight = b.Height;
                vm.MediaWindowWidth = b.Width;
            };
            r.Show();
            return r;
        }
    }
}
