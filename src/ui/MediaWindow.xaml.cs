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
            Loaded += MediaWindow_Loaded;
            MouseMove += MediaWindow_MouseMove;
            SizeChanged += MediaWindow_SizeChanged;
        }

        public MainViewModel vm => App.ViewModel;

        bool navigationHidden => vm.ShowMediaFullscreen && vm.ShowMediaOnSecondMonitor;
        bool videoStarted = false;

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

        void toggleMaximize () {
            if (WindowState == WindowState.Maximized) {
                SystemCommands.RestoreWindow(this);
            }
            else
                SystemCommands.MaximizeWindow(this);
        }

        public void PlayPauseVideo () {
            if (!videoStarted) {
                video.Play();
                videoStarted = true;
                vm.VideoPaused = false;
                return;
            }
            if (vm.VideoPaused) video.Play();
            else video.Pause();
            vm.VideoPaused = !vm.VideoPaused;
        }

        public void SkipBackwardVideo () {
            if (vm.IsPictureOnDisplay) return;

            video.Pause();
            var a = video.Position;
            video.Position = a.Subtract(new TimeSpan(0, 0, 10));
            video.Play();

            hideNavigation();
        }

        public void SkipForwardVideo () {
            if (vm.IsPictureOnDisplay) return;

            video.Pause();
            var a = video.Position;
            video.Position = a.Add(new TimeSpan(0, 0, 30));
            video.Play();

            hideNavigation();
        }

        // Keyboard access key events
        
        void KeyboardLeft_Executed (object sender, ExecutedRoutedEventArgs e) { SkipBackwardVideo(); }
        void KeyboardRight_Executed (object sender, ExecutedRoutedEventArgs e) { SkipForwardVideo(); }
        void KeyboardSpace_Executed (object sender, ExecutedRoutedEventArgs e) { PlayPauseVideo(); }
        void KeyboardEscape_Executed (object sender, ExecutedRoutedEventArgs e) { Close(); }

        // Manage window

        void CloseBorder_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
            if (navigation.Opacity == 0.0) return;
            Close();
        }

        void Close_Click (object sender, RoutedEventArgs e) {
            if (navigation.Opacity == 0.0) return;
            Close();
        }

        void MediaWindow_Loaded (object sender, RoutedEventArgs e) {
            if (navigationHidden) {
                navigationTopBackground.Visibility = Visibility.Hidden;
                navigationBottomBackground.Visibility = Visibility.Hidden;
                navigation.Visibility = Visibility.Hidden;
                return;
            }
            hideNavigation();
        }

        void MediaWindow_MouseDown (object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        void MediaWindow_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
            toggleMaximize();
        }

        void MediaWindow_MouseMove (object sender, MouseEventArgs e) { hideNavigation(); }

        void MediaWindow_SizeChanged (object sender, SizeChangedEventArgs e) {
            picture.Height = e.NewSize.Height;
            picture.Width = e.NewSize.Width;
            video.Width = e.NewSize.Width;
            navigationTopBackground.Width = e.NewSize.Width;
            navigationBottomBackground.Width = e.NewSize.Width - 20;
            navigation.Height = e.NewSize.Height;
            navigation.Width = e.NewSize.Width;
        }

        void MaximizeBorder_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
            if (navigation.Opacity == 0.0) return;
            toggleMaximize();
        }

        void Maximize_Click (object sender, RoutedEventArgs e) {
            if (navigation.Opacity == 0.0) return;
            toggleMaximize();
        }
    }
}
