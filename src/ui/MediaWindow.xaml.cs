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
            MouseMove += Window_MouseMove;
            SizeChanged += Window_SizeChanged;

            vm.SetMedia += (_, _) => { setMedia(); };
        }

        public MainViewModel vm => App.ViewModel;

        bool navigationHidden => vm.ShowMediaFullscreen && vm.ShowMediaOnSecondMonitor;

        void hideNavigation () {
            static DoubleAnimation animation () => new() {
                From = 1.0,
                To = 0.0,
                Duration = new(new TimeSpan(0, 0, 3)),
                EasingFunction = new CubicEase()
            };

            if (navigationHidden) return;

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
            video.Play();
            vm.VideoPaused = false;
        }

        void pauseVideo () {
            video.Pause();
            vm.VideoPaused = true;
        }

        void setMedia () {
            try { video.Stop(); } catch { }
            vm.VideoPaused = true;
            if (vm.MediaListMode) {
                if (vm.CurrentMediaItem is PictureItem a) {
                    picture.Source = a.Media;
                    vm.PictureDisplayedOnMediaWindow = true;
                }
                else {
                    video.Source = ((VideoItem) vm.CurrentMediaItem).Media;
                    vm.PictureDisplayedOnMediaWindow = false;
                    playVideo();
                }
            }
            else {
                vm.PictureDisplayedOnMediaWindow = false;
                video.Source = vm.SingleVideo.Media;
                video.Position = vm.SingleVideo.Skip;
                playVideo();
            }
        }

        void toggleMaximize () {
            if (WindowState == WindowState.Maximized)
                SystemCommands.RestoreWindow(this);
            else
                SystemCommands.MaximizeWindow(this);
        }

        public void PlayPauseVideo () {
            if (vm.PictureDisplayedOnMediaWindow) return;
            if (vm.VideoPaused) playVideo();
            else pauseVideo();
        }

        public void SkipBackwardVideo () {
            if (vm.PictureDisplayedOnMediaWindow) return;
            video.Pause();
            var a = video.Position;
            video.Position = a.Subtract(new TimeSpan(0, 0, 10));
            video.Play();
            hideNavigation();
        }

        public void SkipForwardVideo () {
            if (vm.PictureDisplayedOnMediaWindow) return;
            video.Pause();
            var a = video.Position;
            video.Position = a.Add(new TimeSpan(0, 0, 30));
            video.Play();
            hideNavigation();
        }

        // Manage window

        void Window_MouseDoubleClick (object sender, MouseButtonEventArgs e) { toggleMaximize(); }
        void Window_MouseMove (object sender, MouseEventArgs e) { hideNavigation(); }

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
            toggleMaximize();
        }

        void Maximize_Click (object sender, RoutedEventArgs e) {
            if (navigation.Opacity == 0.0) return;
            toggleMaximize();
        }

        void Window_Loaded (object sender, RoutedEventArgs e) {
            if (navigationHidden) {
                navigationTopBackground.Visibility = Visibility.Hidden;
                navigationBottomBackground.Visibility = Visibility.Hidden;
                navigation.Visibility = Visibility.Hidden;
                return;
            }
            hideNavigation();
            setMedia();
        }

        void Window_MouseDown (object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        void Window_SizeChanged (object sender, SizeChangedEventArgs e) {
            picture.Height = e.NewSize.Height;
            picture.Width = e.NewSize.Width;
            video.Width = e.NewSize.Width;
            navigationTopBackground.Width = e.NewSize.Width;
            navigationBottomBackground.Width = e.NewSize.Width - 20;
            navigation.Height = e.NewSize.Height;
            navigation.Width = e.NewSize.Width;
        }

        void Play_Click (object sender, RoutedEventArgs e) { playVideo(); }
        void Play_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { playVideo(); }

        void Pause_Click (object sender, RoutedEventArgs e) { pauseVideo(); }
        void Pause_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { pauseVideo(); }

        void SkipBackward_Click (object sender, RoutedEventArgs e) { SkipBackwardVideo(); }
        void SkipBackward_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { SkipBackwardVideo(); }

        void SkipForward_Click (object sender, RoutedEventArgs e) { SkipForwardVideo(); }
        void SkipForward_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { SkipForwardVideo(); }

        void Window_PreviewKeyDown (object sender, KeyEventArgs e) {
            switch (e.Key) {
                case Key.Down: vm.MoveToNextMediaItem(); break;
                case Key.Escape: Close(); break;
                case Key.Left: SkipBackwardVideo(); break;
                case Key.Right: SkipForwardVideo(); break;
                case Key.Space:
                    if (vm.PictureDisplayedOnMediaWindow) return;
                    PlayPauseVideo();
                    break;
                case Key.Up: vm.MoveToPreviousMediaItem(); break;
            }
        }
    }
}
