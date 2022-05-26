﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace UI.ViewModel {
    public static class WindowManager {
        // Must run on the UI thread
        public static RenderTargetBitmap GenerateSingleVideoThumbnail (Uri a, TimeSpan skip) {
            MediaPlayer player = new() {
                ScrubbingEnabled = true,
                Volume = 0,
            };
            player.Open(a);
            player.Position = skip;
            System.Threading.Thread.Sleep(2000);

            DrawingVisual dv = new();
            DrawingContext dc = dv.RenderOpen();
            dc.DrawVideo(player, new Rect(0, 0, 330, 330));
            dc.Close();

            RenderTargetBitmap bmp = new(330, 330, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(dv);
            player.Close();
            return bmp;
        }

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