using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using UI.ViewModel;

namespace UI {
    public partial class MediaWindow : Window {
        public MediaWindow () {
            InitializeComponent();
            DataContext = vm;
            Loaded += Window_Loaded;
            MouseDoubleClick += Window_MouseDoubleClick;
            MouseDown += Window_MouseDown;
            MouseMove += Window_MouseMove;
            PreviewKeyDown += Window_PreviewKeyDown;
            SizeChanged += Window_SizeChanged;
            vm.SetMediaListMedia += (_, _) => { setMedia(); };

            vm.GetVideoPositionEvent += (_, _) => vm.VideoPosition = video.Position;
            vm.SetVideoPositionEvent += (_, _) => video.Position = vm.VideoPosition;
            vm.PauseVideoEvent += (_, _) => pauseVideo();
            vm.PlayVideoEvent += (_, _) => playVideo();

            video.MediaOpened += (_, _) => {
                vm.VideoTotalLength = video.NaturalDuration.TimeSpan;
                video.Position = vm.VideoPosition;
            };
        }

        public MainViewModel vm => App.ViewModel;

        void fadeOutNavigation () {
            static DoubleAnimation animation () => new() {
                From = 1.0,
                To = 0.0,
                Duration = new(new TimeSpan(0, 0, 3)),
                EasingFunction = new CubicEase(),
            };

            DoubleAnimation a0 = animation();
            Storyboard topBackgroundTransparent = new();
            topBackgroundTransparent.Children.Add(a0);
            Storyboard.SetTargetName(a0, navigationTopBackground.Name);
            Storyboard.SetTargetProperty(a0, new PropertyPath(OpacityProperty));

            DoubleAnimation a1 = animation();
            Storyboard bottomBackgroundTransparent = new();
            bottomBackgroundTransparent.Children.Add(a1);
            Storyboard.SetTargetName(a1, navigationBottomBackground.Name);
            Storyboard.SetTargetProperty(a1, new PropertyPath(OpacityProperty));

            DoubleAnimation a2 = animation();
            Storyboard navigationTransparent = new();
            navigationTransparent.Children.Add(a2);
            Storyboard.SetTargetName(a2, navigation.Name);
            Storyboard.SetTargetProperty(a2, new PropertyPath(OpacityProperty));

            topBackgroundTransparent.Begin(this);
            bottomBackgroundTransparent.Begin(this);
            navigationTransparent.Begin(this);
        }

        void playVideo () {
            if (!vm.VideoDisplayedOnMediaWindow) return;
            video.Play();
            vm.VideoPaused = false;
        }

        void pauseVideo () {
            if (!vm.VideoDisplayedOnMediaWindow) return;
            video.Pause();
            vm.VideoPaused = true;
        }

        void setMedia () {
            vm.StopTimer();
            try { video.Stop(); } catch { }
            vm.VideoPaused = true;
            if (vm.InternetMode) {
                try { web.Source = vm.WebpageUrl; } catch { }
                return;
            }
            if (vm.MediaListMode) {
                if (vm.CurrentMediaItem.IsPicture)
                    picture.Source = ((PictureItem) vm.CurrentMediaItem).Source;
                else if (vm.CurrentMediaItem.IsVideo) {
                    video.Close();
                    video.Source = ((VideoItem) vm.CurrentMediaItem).Source;
                    playVideo();
                    vm.StartTimer();
                }
                else web.Source = ((PdfItem) vm.CurrentMediaItem).Source;
            }
            else {
                video.Close();
                video.Volume = 0;
                video.Source = vm.SingleVideo.Source;
                video.Position = vm.VideoPosition;
                video.Pause();
                video.Play();
                video.Volume = 1;
                vm.StartTimer();
            }
        }

        public void PlayPauseVideo () {
            if (!vm.VideoDisplayedOnMediaWindow) return;
            if (vm.InternetMode) return;
            if (vm.VideoPaused) playVideo();
            else pauseVideo();
        }

        public void SkipBackwardVideo () {
            if (!vm.VideoDisplayedOnMediaWindow) return;
            if (vm.InternetMode) return;
            video.Pause();
            video.Position = video.Position.Subtract(new TimeSpan(0, 0, 10));
            vm.VideoPosition = video.Position;
            video.Play();
            fadeOutNavigation();
        }

        public void SkipForwardVideo () {
            if (!vm.VideoDisplayedOnMediaWindow) return;
            if (vm.InternetMode) return;
            video.Pause();
            video.Position = video.Position.Add(new TimeSpan(0, 0, 30));
            vm.VideoPosition = video.Position;
            video.Play();
            fadeOutNavigation();
        }

        // Manage window

        void Window_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
            if (WindowState == WindowState.Maximized)
                SystemCommands.RestoreWindow(this);
            else
                SystemCommands.MaximizeWindow(this);
        }

        void Window_MouseMove (object sender, MouseEventArgs e) { fadeOutNavigation(); }

        void CloseBorder_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
            if (navigation.Opacity == 0.0) return;
            Close();
        }

        void Close_Click (object sender, RoutedEventArgs e) {
            if (navigation.Opacity == 0.0) return;
            Close();
        }

        void MaximizeBorder_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
            if (navigation.Opacity == 0.0) return;
            SystemCommands.MaximizeWindow(this);
        }

        void Maximize_Click (object sender, RoutedEventArgs e) {
            if (navigation.Opacity == 0.0) return;
            SystemCommands.MaximizeWindow(this);
        }

        void RestoreBorder_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
            if (navigation.Opacity == 0.0) return;
            SystemCommands.RestoreWindow(this);
        }

        void Restore_Click (object sender, RoutedEventArgs e) {
            if (navigation.Opacity == 0.0) return;
            SystemCommands.RestoreWindow(this);
        }

        void Window_Loaded (object sender, RoutedEventArgs e) {
            setMedia();
            fadeOutNavigation();
        }

        void Window_MouseDown (object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        void Window_SizeChanged (object sender, SizeChangedEventArgs e) {
            picture.Height = e.NewSize.Height;
            picture.Width = e.NewSize.Width;
            web.Height = e.NewSize.Height;
            web.Width = e.NewSize.Width;
            video.Width = e.NewSize.Width;
            web.Height = e.NewSize.Height;
            web.Width = e.NewSize.Width;
            navigationTopBackground.Width = e.NewSize.Width;

            // Work-around a crash which occurs when the media window is rendered on
            // the secondary display but no secondary display actually exists.
            // Assigning a negative value to Width is not allowed.
            var a = e.NewSize.Width - 20;
            navigationBottomBackground.Width = a < 0 ? 0 : a;

            navigation.Height = e.NewSize.Height;
            navigation.Width = e.NewSize.Width;
            fadeOutNavigation();

            if (WindowState == WindowState.Maximized) {
                maximizeBorder.Visibility = Visibility.Collapsed;
                restoreBorder.Visibility = Visibility.Visible;
            }
            else {
                restoreBorder.Visibility = Visibility.Collapsed;
                maximizeBorder.Visibility = Visibility.Visible;
            }
        }

        void Play_Click (object sender, RoutedEventArgs e) { playVideo(); }
        void Play_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { playVideo(); }

        void Pause_Click (object sender, RoutedEventArgs e) { pauseVideo(); }
        void Pause_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { pauseVideo(); }

        void SkipBackward_Click (object sender, RoutedEventArgs e) { SkipBackwardVideo(); }
        void SkipBackward_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { SkipBackwardVideo(); }

        void SkipForward_Click (object sender, RoutedEventArgs e) { SkipForwardVideo(); }
        void SkipForward_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { SkipForwardVideo(); }

        void Video_Click (object sender, MouseButtonEventArgs e) { PlayPauseVideo(); }

        void Window_PreviewKeyDown (object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Down: vm.MoveToNextMediaItem(); break;
                case Key.Escape: Close(); break;
                case Key.Left: SkipBackwardVideo(); break;
                case Key.Right: SkipForwardVideo(); break;
                case Key.Space:
                    if (!vm.VideoDisplayedOnMediaWindow) return;
                    PlayPauseVideo();
                    break;
                case Key.Up: vm.MoveToPreviousMediaItem(); break;
            }
        }
    }
}
