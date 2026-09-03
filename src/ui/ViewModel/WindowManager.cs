using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace UI.ViewModel {
    public static partial class WindowManager {
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
            window.Closing += closing;
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

            public static implicit operator Rect (RECTANGLE r) => new(r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top);
            public static implicit operator RECTANGLE (Rect r) => new() {
                Left = (int)Math.Round(r.Left),
                Top = (int)Math.Round(r.Top),
                Right = (int)Math.Round(r.Right),
                Bottom = (int)Math.Round(r.Bottom)
            };
        }

        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool GetWindowRect (IntPtr hWnd, out RECTANGLE lpRect);

        [LibraryImport("user32.dll")]
        private static partial uint GetDpiForWindow (IntPtr hWnd);

        public static Rect GetWindowRectangle (Window window) {
            var handle = new WindowInteropHelper(window).Handle;
            GetWindowRect(handle, out RECTANGLE r);

            double dpiX = 0;
            double dpiY = 0;

            try {
                var dpi = VisualTreeHelper.GetDpi(window);
                dpiX = dpi.DpiScaleX;
                dpiY = dpi.DpiScaleY;
            }
            catch { }

            if ((dpiX <= 0 || (dpiX == 1.0 && dpiY == 1.0)) && handle != IntPtr.Zero) {
                uint dpiForWindow = GetDpiForWindow(handle);
                if (dpiForWindow > 0) {
                    dpiX = dpiForWindow / 96.0;
                    dpiY = dpiForWindow / 96.0;
                }
            }

            if (dpiX <= 0) dpiX = 1.0;
            if (dpiY <= 0) dpiY = 1.0;

            return new Rect(
                r.Left / dpiX,
                r.Top / dpiY,
                (r.Right - r.Left) / dpiX,
                (r.Bottom - r.Top) / dpiY
            );
        }
    }
}
