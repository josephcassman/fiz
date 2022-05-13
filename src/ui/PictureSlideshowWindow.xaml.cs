using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using UI.ViewModel;

namespace UI {
    public partial class PictureSlideshowWindow : Window {
        public PictureSlideshowWindow () {
            InitializeComponent();
            DataContext = vm;
            Loaded += PictureSlideshow_Loaded;
            MouseMove += PictureSlideshow_MouseMove;
            SizeChanged += PictureSlideshow_SizeChanged;
        }

        public MainViewModel vm => App.ViewModel;

        void hideChrome () {
            if (vm.ShowMediaOnSecondMonitor) return;

            DoubleAnimation a0 = new() { From = 1.0, To = 0.0, Duration = new(new TimeSpan(0, 0, 3)), EasingFunction = new CubicEase() };
            Storyboard makeTransparentTitleBar = new();
            makeTransparentTitleBar.Children.Add(a0);
            Storyboard.SetTargetName(a0, titleBar?.Name);
            Storyboard.SetTargetProperty(a0, new PropertyPath(OpacityProperty));

            DoubleAnimation a1 = new() { From = 1.0, To = 0.0, Duration = new(new TimeSpan(0, 0, 3)), EasingFunction = new QuadraticEase() };
            Storyboard makeTransparentMaximizeButton = new();
            makeTransparentMaximizeButton.Children.Add(a1);
            Storyboard.SetTargetName(a1, maximizeBorder?.Name);
            Storyboard.SetTargetProperty(a1, new PropertyPath(OpacityProperty));

            DoubleAnimation a2 = new() { From = 1.0, To = 0.0, Duration = new(new TimeSpan(0, 0, 3)), EasingFunction = new QuadraticEase() };
            Storyboard makeTransparentCloseButton = new();
            makeTransparentCloseButton.Children.Add(a2);
            Storyboard.SetTargetName(a2, closeBorder?.Name);
            Storyboard.SetTargetProperty(a2, new PropertyPath(OpacityProperty));

            makeTransparentTitleBar.Begin(this);
            makeTransparentMaximizeButton.Begin(this);
            makeTransparentCloseButton.Begin(this);

            ObjectAnimationUsingKeyFrames makeInvisible = new();
            makeInvisible.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Hidden, new TimeSpan(0, 0, 5)));

            titleBar?.BeginAnimation(VisibilityProperty, makeInvisible);
            maximizeBorder?.BeginAnimation(VisibilityProperty, makeInvisible);
            closeBorder?.BeginAnimation(VisibilityProperty, makeInvisible);
        }

        void showChrome () {
            titleBar.Visibility = Visibility.Visible;

            maximizeBorder.Visibility = Visibility.Visible;
            maximize.Visibility = Visibility.Visible;

            closeBorder.Visibility = Visibility.Visible;
            close.Visibility = Visibility.Visible;

            DoubleAnimation a3 = new() { From = 0.0, To = 1.0, Duration = new(new TimeSpan(0, 0, 1)), EasingFunction = new BackEase() };
            Storyboard makeOpaqueTitleBar = new();
            makeOpaqueTitleBar.Children.Add(a3);
            Storyboard.SetTargetName(a3, titleBar?.Name);
            Storyboard.SetTargetProperty(a3, new PropertyPath(OpacityProperty));

            DoubleAnimation a4 = new() { From = 0.0, To = 1.0, Duration = new(new TimeSpan(0, 0, 1)), EasingFunction = new BackEase() };
            Storyboard makeOpaqueMaximizeButton = new();
            makeOpaqueMaximizeButton.Children.Add(a4);
            Storyboard.SetTargetName(a4, maximizeBorder?.Name);
            Storyboard.SetTargetProperty(a4, new PropertyPath(OpacityProperty));

            DoubleAnimation a5 = new() { From = 0.0, To = 1.0, Duration = new(new TimeSpan(0, 0, 1)), EasingFunction = new BackEase() };
            Storyboard makeOpaqueCloseButton = new();
            makeOpaqueCloseButton.Children.Add(a5);
            Storyboard.SetTargetName(a5, closeBorder?.Name);
            Storyboard.SetTargetProperty(a5, new PropertyPath(OpacityProperty));

            makeOpaqueTitleBar.Begin(this);
            makeOpaqueMaximizeButton.Begin(this);
            makeOpaqueCloseButton.Begin(this);
        }

        void showHideChrome () {
            if (vm.ShowMediaOnSecondMonitor) return;
            showChrome();
            hideChrome();
        }

        void toggleMaximize () {
            if (WindowState == WindowState.Maximized) {
                SystemCommands.RestoreWindow(this);
            }
            else
                SystemCommands.MaximizeWindow(this);
        }

        // Keyboard access key events
        
        private void KeyboardLeft_Executed (object sender, ExecutedRoutedEventArgs e) { vm.MoveToPreviousPicture(); }
        private void KeyboardRight_Executed (object sender, ExecutedRoutedEventArgs e) { vm.MoveToNextPicture(); }

        // Slideshow

        private void PictureSlideshow_Loaded (object sender, RoutedEventArgs e) {
            if (vm.ShowMediaOnSecondMonitor) {
                titleBar.Visibility = Visibility.Hidden;
                maximizeBorder.Visibility = Visibility.Hidden;
                maximize.Visibility = Visibility.Hidden;
                closeBorder.Visibility = Visibility.Hidden;
                close.Visibility = Visibility.Hidden;
                return;
            }

            hideChrome();
        }

        private void PictureSlideshow_MouseMove (object sender, MouseEventArgs e) {
            showHideChrome();
        }

        private void PictureSlideshow_SizeChanged (object sender, SizeChangedEventArgs e) {
            titleBar.Width = e.NewSize.Width;
            picture.Height = e.NewSize.Height;
            picture.Width = e.NewSize.Width;
            showHideChrome();
        }

        // Manage window

        private void CloseBorder_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) { Close(); }
        private void Close_Click (object sender, RoutedEventArgs e) { Close(); }

        private void MaximizeBorder_MouseLeftButtonDown (object sender, MouseButtonEventArgs e) {
            if (vm.ShowMediaOnSecondMonitor) return;
            toggleMaximize();
        }

        private void Maximize_Click (object sender, RoutedEventArgs e) {
            if (vm.ShowMediaOnSecondMonitor) return;
            toggleMaximize();
        }

        private void Window_MouseDown (object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
            showHideChrome();
        }

        private void Window_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
            if (vm.ShowMediaOnSecondMonitor)
                return;
            toggleMaximize();
        }
    }
}
