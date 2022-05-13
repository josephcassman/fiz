using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using UI.ViewModel;

namespace UI {
    public partial class PictureSlideshow : Window {
        public PictureSlideshow () {
            InitializeComponent();
            DataContext = vm;
            Loaded += PictureSlideshow_Loaded;
            MouseMove += PictureSlideshow_MouseMove;
            SizeChanged += PictureSlideshow_SizeChanged;

            makeInvisible.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Hidden, new TimeSpan(0, 0, 5)));
        }

        public MainViewModel vm => App.ViewModel;

        readonly Storyboard makeTransparentTitleBar = new();
        readonly Storyboard makeTransparentMaximizeButton = new();
        readonly Storyboard makeTransparentCloseButton = new();
        readonly ObjectAnimationUsingKeyFrames makeInvisible = new();

        readonly Storyboard makeOpaqueTitleBar = new();
        readonly Storyboard makeOpaqueMaximizeButton = new();
        readonly Storyboard makeOpaqueCloseButton = new();

        void hideChrome () {
            if (vm.SlideshowOnSecondMonitor) return;

            makeTransparentTitleBar.Begin(this);
            makeTransparentMaximizeButton.Begin(this);
            makeTransparentCloseButton.Begin(this);

            titleBar.BeginAnimation(StackPanel.VisibilityProperty, makeInvisible);
            maximizeBorder.BeginAnimation(Border.VisibilityProperty, makeInvisible);
            closeBorder.BeginAnimation(Border.VisibilityProperty, makeInvisible);
        }

        void showHideChrome () {
            if (vm.SlideshowOnSecondMonitor) return;

            titleBar.Visibility = Visibility.Visible;

            maximizeBorder.Visibility = Visibility.Visible;
            maximize.Visibility = Visibility.Visible;

            closeBorder.Visibility = Visibility.Visible;
            close.Visibility = Visibility.Visible;

            makeOpaqueTitleBar.Begin(this);
            makeOpaqueMaximizeButton.Begin(this);
            makeOpaqueCloseButton.Begin(this);

            hideChrome();
        }

        void toggleMaximize () {
            if (WindowState == WindowState.Maximized) {
                SystemCommands.RestoreWindow(this);
            }
            else
                SystemCommands.MaximizeWindow(this);
        }

        private void PictureSlideshow_Loaded (object sender, RoutedEventArgs e) {
            if (vm.SlideshowOnSecondMonitor) {
                titleBar.Visibility = Visibility.Hidden;
                maximizeBorder.Visibility = Visibility.Hidden;
                maximize.Visibility = Visibility.Hidden;
                closeBorder.Visibility = Visibility.Hidden;
                close.Visibility = Visibility.Hidden;
                return;
            }

            DoubleAnimation a0 = new() { From = 1.0, To = 0.0, Duration = new(new TimeSpan(0, 0, 3)), EasingFunction = new CubicEase() };
            makeTransparentTitleBar.Children.Add(a0);
            Storyboard.SetTargetName(a0, titleBar.Name);
            Storyboard.SetTargetProperty(a0, new PropertyPath(StackPanel.OpacityProperty));

            DoubleAnimation a1 = new() { From = 1.0, To = 0.0, Duration = new(new TimeSpan(0, 0, 3)), EasingFunction = new QuadraticEase() };
            makeTransparentMaximizeButton.Children.Add(a1);
            Storyboard.SetTargetName(a1, maximizeBorder.Name);
            Storyboard.SetTargetProperty(a1, new PropertyPath(Border.OpacityProperty));

            DoubleAnimation a2 = new() { From = 1.0, To = 0.0, Duration = new(new TimeSpan(0, 0, 3)), EasingFunction = new QuadraticEase() };
            makeTransparentCloseButton.Children.Add(a2);
            Storyboard.SetTargetName(a2, closeBorder.Name);
            Storyboard.SetTargetProperty(a2, new PropertyPath(Border.OpacityProperty));

            hideChrome();

            DoubleAnimation a3 = new() { From = 0.0, To = 1.0, Duration = new(new TimeSpan(0, 0, 1)), EasingFunction = new BackEase() };
            makeOpaqueTitleBar.Children.Add(a3);
            Storyboard.SetTargetName(a3, titleBar.Name);
            Storyboard.SetTargetProperty(a3, new PropertyPath(StackPanel.OpacityProperty));

            DoubleAnimation a4 = new() { From = 0.0, To = 1.0, Duration = new(new TimeSpan(0, 0, 1)), EasingFunction = new BackEase() };
            makeOpaqueMaximizeButton.Children.Add(a4);
            Storyboard.SetTargetName(a4, maximizeBorder.Name);
            Storyboard.SetTargetProperty(a4, new PropertyPath(Border.OpacityProperty));

            DoubleAnimation a5 = new() { From = 0.0, To = 1.0, Duration = new(new TimeSpan(0, 0, 1)), EasingFunction = new BackEase()};
            makeOpaqueCloseButton.Children.Add(a5);
            Storyboard.SetTargetName(a5, closeBorder.Name);
            Storyboard.SetTargetProperty(a5, new PropertyPath(Border.OpacityProperty));
        }

        private void PictureSlideshow_MouseMove (object sender, MouseEventArgs e) {
            showHideChrome();
        }

        private void Left_Executed (object sender, ExecutedRoutedEventArgs e) {
            vm.MovePrevious();
        }

        private void Right_Executed (object sender, ExecutedRoutedEventArgs e) {
            vm.MoveNext();
        }

        private void Window_MouseDown (object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
            showHideChrome();
        }

        private void Maximize_Click (object sender, RoutedEventArgs e) {
            if (vm.SlideshowOnSecondMonitor) return;
            toggleMaximize();
        }

        private void Close_Click (object sender, RoutedEventArgs e) {
            Close();
        }

        private void PictureSlideshow_SizeChanged (object sender, SizeChangedEventArgs e) {
            titleBar.Width = e.NewSize.Width;
            picture.Height = e.NewSize.Height;
            picture.Width = e.NewSize.Width;
            showHideChrome();
        }

        private void Window_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
            if (vm.SlideshowOnSecondMonitor) return;
            toggleMaximize();
        }
    }
}
