using System.Linq;
using System.Windows;

namespace UI.ViewModel {
    public static class SecondMonitor {
        public static void ShowMediaWindow (Window window, MainViewModel vm) {
            void showOnPrimaryMonitor () {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                window.Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2;
                window.Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2;
                window.Show();
            }

            var a = System.Windows.Forms.Screen.AllScreens.Where(a => !a.Primary);
            if (a.Any()) {
                if (!vm.ShowMediaOnSecondMonitor) showOnPrimaryMonitor();
                else {
                    if (!window.IsLoaded)
                        window.WindowStartupLocation = WindowStartupLocation.Manual;

                    var b = a.First();

                    window.Left = b.WorkingArea.Left;
                    window.Top = b.WorkingArea.Top;
                    window.Width = b.WorkingArea.Width;
                    window.Height = b.WorkingArea.Height;

                    window.Show();

                    if (window.IsLoaded)
                        window.WindowState = WindowState.Maximized;
                }
            }
            else {
                vm.ShowMediaOnSecondMonitor = false;
                showOnPrimaryMonitor();
            }
        }
    }
}
