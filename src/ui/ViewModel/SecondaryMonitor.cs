using System.Linq;
using System.Windows;

namespace UI.ViewModel {
    public static class SecondMonitor {
        public static void ShowMaximizedOnSecondScreen (Window window) {
            var two = System.Windows.Forms.Screen.AllScreens.Where(a => !a.Primary).FirstOrDefault();
            
            if (two != null) {
                if (!window.IsLoaded)
                    window.WindowStartupLocation = WindowStartupLocation.Manual;

                window.Left = two.WorkingArea.Left;
                window.Top = two.WorkingArea.Top;
                window.Width = two.WorkingArea.Width;
                window.Height = two.WorkingArea.Height;
                
                window.Show();

                if (window.IsLoaded)
                    window.WindowState = WindowState.Maximized;
            }
        }
    }
}
