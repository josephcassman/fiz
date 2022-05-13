﻿using System.Linq;
using System.Windows;

namespace UI.ViewModel {
    public static class SecondMonitor {
        public static void ShowMaximizedOnSecondScreen (Window window, MainViewModel vm) {
            var a = System.Windows.Forms.Screen.AllScreens.Where(a => !a.Primary);
            if (a.Any()) {
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
            else {
                vm.ShowMediaOnSecondMonitor = false;
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                window.Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 2;
                window.Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 2;
                window.Show();
            }
        }
    }
}
