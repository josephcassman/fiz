using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
            MouseUp += Window_MouseUp;
            MouseWheel += Window_MouseWheel;
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

            web.CoreWebView2InitializationCompleted += async (_, _) => {
                var a = $$"""
                let a, ax = 0, ay = 0, dragable, dragging, curx, cury;

                function mouse_down (e) {
                    curx = e.clientX;
                    cury = e.clientY;
                    dragging = true;
                }

                function mouse_move (e) {
                    if (dragable !== true) return;
                    if (dragging !== true) return;
                    if (!e) { e = window.event; }
                    let dx = curx - e.clientX;
                    let dy = cury - e.clientY;
                    ax -= dx;
                    ay -= dy;
                    curx = e.clientX;
                    cury = e.clientY;
                    a.style.left = ax + "px";
                    a.style.top = ay + "px";
                }

                function mouse_up (e) {
                    dragging = false;
                }

                function keyboard_down (e) {
                    if (e.key === "Control") {
                        dragable = true;
                        removeUserSelect();
                    }
                }

                function keyboard_up (e) {
                    if (e.key === "Control") {
                        dragable = false;
                        dragging = false;
                        addUserSelect();
                    }
                }

                function addUserSelect () {
                    document.querySelectorAll("*").forEach(a => a.style.userSelect = "auto");
                }

                function removeUserSelect () {
                    document.querySelectorAll("*").forEach(a => a.style.userSelect = "none");
                }

                function wrap (element) {
                    element.addEventListener('mousedown', mouse_down);
                    element.addEventListener('mousemove', mouse_move);
                    element.addEventListener('mouseup', mouse_up);
                    element.addEventListener('keydown', keyboard_down);
                    element.addEventListener('keyup', keyboard_up);
                }

                window.addEventListener("load", function() {
                    const html = document.getElementsByTagName("html")[0];
                    html.style.width = "100%";
                    html.style.height = "100%";
                    html.style.margin = "0";
                    html.style.scale = "{{vm.WebPageScaleFactor}}";
                    html.style.willChange = "scale";
                    html.style.transformOrigin = "top center";
                    html.style.transition = "all 0.35s cubic-bezier(0.33, 1, 0.68, 1)";

                    const body = document.getElementsByTagName("body")[0];
                    body.style.width = "100%";
                    body.style.height = "100%";
                    body.style.margin = "0";

                    a = document.createElement("div");
                    a.style.position = "absolute";
                    a.style.width = "99vw";
                    a.style.height = "99vh";

                    while (0 < document.body.childNodes.length)
                        a.appendChild(document.body.childNodes[0]);

                    document.body.appendChild(a);

                    document.querySelectorAll("*").forEach(a => wrap(a));
                });
                """;
                await web.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(a);
            };
        }

        // State for panning and zooming picture media
        const float ZoomInFactor = 1.1f;
        const float ZoomOutFactor = 1f / 1.1f;
        MatrixTransform zoomTransform = new();
        bool panning = false;
        Vector panningDelta = new();

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
                if (vm.CurrentMediaItem.IsPicture) {
                    // Reset the picture layout.
                    // Undo the panning translation and
                    // zooming scale factors.
                    panningDelta = new();
                    Canvas.SetLeft(picture, 0);
                    Canvas.SetTop(picture, 0);
                    zoomTransform = new();
                    picture.RenderTransform = zoomTransform;

                    picture.Source = ((PictureItem) vm.CurrentMediaItem).Source;
                }
                else {
                    video.Close();
                    video.Source = ((VideoItem) vm.CurrentMediaItem).Source;
                    playVideo();
                    vm.StartTimer();
                }
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

        void Window_MouseDoubleClick (object sender, MouseButtonEventArgs e) {
            if (WindowState == WindowState.Maximized)
                SystemCommands.RestoreWindow(this);
            else
                SystemCommands.MaximizeWindow(this);
        }

        void Window_MouseDown (object sender, MouseButtonEventArgs e) {
            if (Keyboard.Modifiers == ModifierKeys.Control) {
                var mouse = e.GetPosition(this);
                Point canvas = new(Canvas.GetLeft(picture), Canvas.GetTop(picture));
                panningDelta = canvas - mouse;
                panning = true;
            }
            else if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        void Window_MouseMove (object sender, MouseEventArgs e) {
            if (Keyboard.Modifiers == ModifierKeys.Control) {
                if (panning && e.LeftButton == MouseButtonState.Pressed) {
                    Canvas.SetLeft(picture, e.GetPosition(this).X + panningDelta.X);
                    Canvas.SetTop(picture, e.GetPosition(this).Y + panningDelta.Y);
                }
            }
            else fadeOutNavigation();
        }

        void Window_MouseUp (object sender, MouseButtonEventArgs e) {
            panning = false;
        }

        void Window_MouseWheel (object sender, MouseWheelEventArgs e) {
            if (Keyboard.Modifiers == ModifierKeys.Control) {
                var scale = e.Delta < 0 ? ZoomOutFactor : ZoomInFactor;

                // Adjust the canvas to the new size of the picture.
                Canvas.SetLeft(picture, Canvas.GetLeft(picture) * scale);
                Canvas.SetTop(picture, Canvas.GetTop(picture) * scale);

                // Incrementally zoom by adjusting the current transform state.
                Matrix b = zoomTransform.Matrix;
                b.ScaleAt(scale, scale, e.GetPosition(this).X, e.GetPosition(this).Y);
                zoomTransform.Matrix = b;
                picture.RenderTransform = zoomTransform;
            }
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
