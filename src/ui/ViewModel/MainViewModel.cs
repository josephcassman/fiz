using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Web;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace UI.ViewModel {
    public enum MainWindowMode {
        MediaList,
        SingleVideo,
        Internet,
    }

    public sealed class MainViewModel : BindableBase {
        public MainViewModel() {
            SettingsStorage.Initialize();
            ShowMediaFullscreen = SettingsStorage.ShowMediaFullscreen;
            ShowMediaOnSecondMonitor = SettingsStorage.ShowMediaOnSecondMonitor;
            StartLocationLowerLeft = SettingsStorage.StartLocationLowerLeft;
            StartLocationUpperLeft = SettingsStorage.StartLocationUpperLeft;
            StartLocationUpperRight = SettingsStorage.StartLocationUpperRight;
            StartLocationLowerRight = SettingsStorage.StartLocationLowerRight;
            var a = SettingsStorage.MediaListPaths;
            foreach (var path in a) {
                if (!File.Exists(path)) SettingsStorage.DeleteMediaListPath(path);
                else {
                    var name = Path.GetFileName(path);
                    var extension = Path.GetExtension(path);
                    var uri = new Uri(path);
                    if (PictureExtensions.Contains(extension)) {
                        var bmp = new BitmapImage(uri);
                        AddMediaItem(new PictureItem {
                            FileName = name,
                            FilePath = path,
                            Source = bmp,
                        });
                    }
                    else if (VideoExtensions.Contains(extension)) {
                        AddMediaItem(new VideoItem {
                            FileName = name,
                            FilePath = path,
                            Source = uri,
                        });
                    }
                    else if (extension == ".pdf") {
                        AddMediaItem(new PdfItem {
                            FileName = name,
                            FilePath = path,
                            Source = uri,
                        });
                    }
                    else SettingsStorage.DeleteMediaListPath(path);
                }
            }

            if (0 < a.Count) MainWindowMode = MainWindowMode.MediaList;
            else if (!string.IsNullOrEmpty(SettingsStorage.SingleVideoPath))
                MainWindowMode = MainWindowMode.SingleVideo;
            else MainWindowMode = MainWindowMode.MediaList;

            updateVideoPositionTimer.Tick += (_, _) => GetVideoPositionEvent?.Invoke(this, EventArgs.Empty);
            updateVideoPositionTimer.Start();
        }

        public static readonly HashSet<string> PictureExtensions = new() {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
        };

        public static readonly HashSet<string> VideoExtensions = new() {
            ".mp4",
            ".wav",
        };

        public Uri WebpageUrl = new("about:blank");

        // Media items

        public event EventHandler? MoveDown;
        public event EventHandler? MoveUp;
        public event EventHandler? SetMediaListMedia;

        public void MoveToNextMediaItem () { MoveDown?.Invoke(this, EventArgs.Empty); }
        public void MoveToPreviousMediaItem () { MoveUp?.Invoke(this, EventArgs.Empty); }

        public ObservableCollection<MediaItem> MediaItems = new();

        public MediaItem CurrentMediaItem => MediaItems[MediaItemsCurrentIndex];

        int _mediaItemsCurrentIndex = -1;
        public int MediaItemsCurrentIndex {
            get => _mediaItemsCurrentIndex;
            set {
                Set(ref _mediaItemsCurrentIndex, value);

                PdfDisplayedOnMediaWindow = MediaItems[value].IsPdf;
                PictureDisplayedOnMediaWindow = MediaItems[value].IsPicture;
                VideoDisplayedOnMediaWindow = MediaItems[value].IsVideo;

                SetMediaListMedia?.Invoke(this, EventArgs.Empty);
            }
        }

        public void AddMediaItem (MediaItem a) {
            MediaItems.Add(a);
            MediaListHasContents = true;
        }

        public void BackupMediaItems () {
            SettingsStorage.ClearMediaListPaths();
            foreach (MediaItem a in MediaItems)
                SettingsStorage.SaveMediaListPath(a.FilePath);
            SettingsStorage.SingleVideoPath = SingleVideo.FilePath;
        }

        public void RemoveMediaItem (int i) {
            MediaItems.RemoveAt(i);
            MediaListHasContents = 0 < MediaItems.Count;
        }

        // Main window state

        bool _internetMode = false;
        public bool InternetMode {
            get => _internetMode;
            set => Set(ref _internetMode, value);
        }

        bool _internetNavigationFailed = false;
        public bool InternetNavigationFailed {
            get => _internetNavigationFailed;
            set => Set(ref _internetNavigationFailed, value);
        }

        MainWindowMode _mainWindowMode = MainWindowMode.MediaList;
        public MainWindowMode MainWindowMode {
            get => _mainWindowMode;
            set {
                switch (value) {
                    case MainWindowMode.MediaList:
                        SingleVideoMode = false;
                        InternetMode = false;
                        MediaListMode = true;
                        break;
                    case MainWindowMode.SingleVideo:
                        MediaListMode = false;
                        InternetMode = false;
                        SingleVideoMode = true;
                        break;
                    case MainWindowMode.Internet:
                        MediaListMode = false;
                        SingleVideoMode = false;
                        InternetMode = true;
                        break;
                    default: throw new NotImplementedException();
                }
            }
        }

        bool _mediaListHasContents = false;
        public bool MediaListHasContents {
            get => _mediaListHasContents;
            set => Set(ref _mediaListHasContents, value);
        }

        bool _mediaListMode = false;
        public bool MediaListMode {
            get => _mediaListMode;
            set => Set(ref _mediaListMode, value);
        }

        bool _minified = false;
        public bool Minified {
            get => _minified;
            set => Set(ref _minified, value);
        }

        bool _singleVideoMode = false;
        public bool SingleVideoMode {
            get => _singleVideoMode;
            set => Set(ref _singleVideoMode, value);
        }

        bool _singleVideoSkipUpdated = false;
        public bool SingleVideoSkipUpdated {
            get => _singleVideoSkipUpdated;
            set => Set(ref _singleVideoSkipUpdated, value);
        }

        // Media window state

        readonly DispatcherTimer updateVideoPositionTimer = new() { Interval = TimeSpan.FromSeconds(0.5) };

        public event EventHandler? GetVideoPositionEvent;
        public event EventHandler? SetVideoPositionEvent;
        public event EventHandler? PauseVideoEvent;
        public event EventHandler? PlayVideoEvent;

        bool _mediaDisplayed = false;
        public bool MediaDisplayed {
            get => _mediaDisplayed;
            set { Set(ref _mediaDisplayed, value); }
        }

        bool _pdfDisplayedOnMediaWindow = false;
        public bool PdfDisplayedOnMediaWindow {
            get => _pdfDisplayedOnMediaWindow;
            set {
                Set(ref _pdfDisplayedOnMediaWindow, value);
                if (value) {
                    PictureDisplayedOnMediaWindow = false;
                    VideoDisplayedOnMediaWindow = false;
                }
            }
        }

        bool _pictureDisplayedOnMediaWindow = false;
        public bool PictureDisplayedOnMediaWindow {
            get => _pictureDisplayedOnMediaWindow;
            set {
                Set(ref _pictureDisplayedOnMediaWindow, value);
                if (value) {
                    PdfDisplayedOnMediaWindow = false;
                    VideoDisplayedOnMediaWindow = false;
                }
            }
        }

        bool _setVideoPositionSliderActive = false;
        public bool SetVideoPositionSliderActive {
            get => _setVideoPositionSliderActive;
            set => Set(ref _setVideoPositionSliderActive, value);
        }

        string _setVideoPositionSliderPreviewPositionText = "00:00:00";
        public string SetVideoPositionSliderPreviewPositionText {
            get => _setVideoPositionSliderPreviewPositionText;
            set => Set(ref _setVideoPositionSliderPreviewPositionText, value);
        }

        VideoItem _singleVideo = new();
        public VideoItem SingleVideo {
            get => _singleVideo;
            set {
                Set(ref _singleVideo, value);
                var a = HttpUtility.UrlDecode(value.FileName);
                SingleVideoPreviewFileName = 25 < a.Length ? a[..25] + "\u2026" : a;
                SingleVideoPreviewPosition = TimeSpan.Zero;
                SingleVideoPreviewTotalLength = TimeSpan.Zero;
            }
        }

        string _singleVideoPreviewFileName = "";
        public string SingleVideoPreviewFileName {
            get => _singleVideoPreviewFileName;
            set => Set(ref _singleVideoPreviewFileName, value);
        }

        TimeSpan _singleVideoPreviewPosition = TimeSpan.Zero;
        public TimeSpan SingleVideoPreviewPosition {
            get => _singleVideoPreviewPosition;
            set {
                _singleVideoPreviewPosition = value;
                SingleVideoPreviewPositionText = value.ToString(@"hh\:mm\:ss");
            }
        }

        string _singleVideoPreviewPositionText = "00:00:00";
        public string SingleVideoPreviewPositionText {
            get => _singleVideoPreviewPositionText;
            set => Set(ref _singleVideoPreviewPositionText, value);
        }

        bool _singleVideoPreviewIsLoading = false;
        public bool SingleVideoPreviewIsLoading {
            get => _singleVideoPreviewIsLoading;
            set => Set(ref _singleVideoPreviewIsLoading, value);
        }

        TimeSpan _singleVideoPreviewTotalLength = TimeSpan.Zero;
        public TimeSpan SingleVideoPreviewTotalLength {
            get => _singleVideoPreviewTotalLength;
            set {
                _singleVideoPreviewTotalLength = value;
                SingleVideoPreviewTotalLengthText = value.ToString(@"hh\:mm\:ss");
            }
        }

        string _singleVideoPreviewTotalLengthText = "00:00:00";
        public string SingleVideoPreviewTotalLengthText {
            get => _singleVideoPreviewTotalLengthText;
            set => Set(ref _singleVideoPreviewTotalLengthText, value);
        }

        TimeSpan _videoPosition = TimeSpan.Zero;
        public TimeSpan VideoPosition {
            get => _videoPosition;
            set {
                _videoPosition = value;
                VideoPositionText = value.ToString(@"hh\:mm\:ss");
                VideoPositionSeconds = value.TotalSeconds;
            }
        }

        double _videoPositionSeconds = 0;
        public double VideoPositionSeconds {
            get => _videoPositionSeconds;
            set => Set(ref _videoPositionSeconds, value);
        }

        string _videoPositionText = "00:00:00";
        public string VideoPositionText {
            get => _videoPositionText;
            set => Set(ref _videoPositionText, value);
        }

        bool _videoDisplayedOnMediaWindow = false;
        public bool VideoDisplayedOnMediaWindow {
            get => _videoDisplayedOnMediaWindow;
            set {
                Set(ref _videoDisplayedOnMediaWindow, value);
                if (value) {
                    PdfDisplayedOnMediaWindow = false;
                    PictureDisplayedOnMediaWindow = false;
                }
            }
        }

        bool _videoPaused = true;
        public bool VideoPaused {
            get => _videoPaused;
            set => Set(ref _videoPaused, value);
        }

        TimeSpan _videoTotalLength = TimeSpan.Zero;
        public TimeSpan VideoTotalLength {
            get => _videoTotalLength;
            set {
                _videoTotalLength = value;
                VideoTotalLengthText = value.ToString(@"hh\:mm\:ss");
                VideoTotalLengthSeconds = value.TotalSeconds;
            }
        }

        double _videoTotalLengthSeconds = 0;
        public double VideoTotalLengthSeconds {
            get => _videoTotalLengthSeconds;
            set => Set(ref _videoTotalLengthSeconds, value);
        }

        string _videoTotalLengthText = "00:00:00";
        public string VideoTotalLengthText {
            get => _videoTotalLengthText;
            set => Set(ref _videoTotalLengthText, value);
        }

        public void PauseVideo () { PauseVideoEvent?.Invoke(this, EventArgs.Empty); }
        public void PlayVideo () { PlayVideoEvent?.Invoke(this, EventArgs.Empty); }
        public void StartTimer () { updateVideoPositionTimer.Start(); }
        public void StopTimer () { updateVideoPositionTimer.Stop(); }

        public void UpdateVideoPosition (TimeSpan a) {
            VideoPosition = a;
            SetVideoPositionEvent?.Invoke(this, EventArgs.Empty);
        }

        // Settings

        bool _showMediaFullscreen = true;
        public bool ShowMediaFullscreen {
            get => _showMediaFullscreen;
            set {
                Set(ref _showMediaFullscreen, value);
                SettingsStorage.ShowMediaFullscreen = value;
            }
        }

        bool _showMediaOnSecondMonitor = true;
        public bool ShowMediaOnSecondMonitor {
            get => _showMediaOnSecondMonitor;
            set {
                Set(ref _showMediaOnSecondMonitor, value);
                SettingsStorage.ShowMediaOnSecondMonitor = value;
            }
        }

        bool _startLocationLowerLeft = true;
        public bool StartLocationLowerLeft {
            get => _startLocationLowerLeft;
            set {
                Set(ref _startLocationLowerLeft, value);
                SettingsStorage.StartLocationLowerLeft = value;
            }
        }

        bool _startLocationLowerRight = false;
        public bool StartLocationLowerRight {
            get => _startLocationLowerRight;
            set {
                Set(ref _startLocationLowerRight, value);
                SettingsStorage.StartLocationLowerRight = value;
            }
        }

        bool _startLocationUpperLeft = false;
        public bool StartLocationUpperLeft {
            get => _startLocationUpperLeft;
            set {
                Set(ref _startLocationUpperLeft, value);
                SettingsStorage.StartLocationUpperLeft = value;
            }
        }

        bool _startLocationUpperRight = false;
        public bool StartLocationUpperRight {
            get => _startLocationUpperRight;
            set {
                Set(ref _startLocationUpperRight, value);
                SettingsStorage.StartLocationUpperRight = value;
            }
        }
    }
}
