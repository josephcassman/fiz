using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace UI.ViewModel {
    public static partial class WindowManager {
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
            window.Height = vm.MediaWindowHeight == 0.0 ? 500.0 : vm.MediaWindowHeight;
            window.Width = vm.MediaWindowWidth == 0.0 ? 800.0 : vm.MediaWindowWidth;
            if (vm.MediaWindowMaximized) {
                window.Loaded += (_, _) => {
                    window.WindowState = WindowState.Maximized;
                };
            }
            window.Show();
        }

        public static Window ShowSampleMediaWindow (MainViewModel vm) {
            var height = vm.MediaWindowHeight;
            var width = vm.MediaWindowWidth;
            var r = new SampleMediaWindow() {
                WindowStartupLocation = WindowStartupLocation.Manual,
                Left = vm.MediaWindowLeft,
                Top = vm.MediaWindowTop,
                Height = height == 0.0 ? 500.0 : height,
                Width = width == 0.0 ? 800.0 : width,
            };
            if (vm.MediaWindowMaximized) {
                r.Loaded += (_, _) => {
                    r.WindowState = WindowState.Maximized;
                };
            }
            r.Closing += (s, e) => {
                var b = (s as Window) ?? new();
                double left = 0, top = 0, height = 0, width = 0;
                if (b.WindowState == WindowState.Maximized) {
                    var c = GetWindowRectangle(b);
                    left = c.Left;
                    top = c.Top;
                    height = c.Bottom - c.Top;
                    width = c.Right - c.Left;
                }
                else {
                    left = b.Left;
                    top = b.Top;
                    height = b.ActualHeight;
                    width = b.ActualWidth;
                }
                vm.MediaWindowMaximized = b.WindowState == WindowState.Maximized;
                vm.MediaWindowLeft = left;
                vm.MediaWindowTop = top;
                vm.MediaWindowHeight = height;
                vm.MediaWindowWidth = width;
                SettingsStorage.MediaWindowMaximized = vm.MediaWindowMaximized;
                SettingsStorage.MediaWindowLeft = left;
                SettingsStorage.MediaWindowTop = top;
                SettingsStorage.MediaWindowHeight = height;
                SettingsStorage.MediaWindowWidth = width;
            };
            r.Show();
            return r;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECTANGLE {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetWindowRect (IntPtr hWnd, out RECTANGLE lpRect);

        public static RECTANGLE GetWindowRectangle (Window window) {
            RECTANGLE r;
            GetWindowRect((new WindowInteropHelper(window)).Handle, out r);
            return r;
        }
    }
}
