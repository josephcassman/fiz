using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace UI.ViewModel {
    public enum MainWindowMode {
        MediaList,
        SingleVideo,
        Internet,
    }

    public class MainViewModel : BindableBase {
        public MainViewModel() {
            SettingsStorage.Initialize();
            ShowMediaFullscreen = SettingsStorage.ShowMediaFullscreen;
            ShowMediaOnSecondMonitor = SettingsStorage.ShowMediaOnSecondMonitor;
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
                            Name = name,
                            Path = path,
                            Preview = bmp,
                            Media = bmp,
                        });
                    }
                    else if (VideoExtensions.Contains(extension)) {
                        AddMediaItem(new VideoItem {
                            Name = name,
                            Path = path,
                            Media = uri,
                        });
                    }
                    else if (extension == ".pdf") {
                        AddMediaItem(new PdfItem {
                            Name = name,
                            Path = path,
                            Media = uri,
                        });
                    }
                    else SettingsStorage.DeleteMediaListPath(path);
                }
            }
            var b = SettingsStorage.SingleVideoPath;
            if (File.Exists(b)) {
                Uri uri = new(b);
                SingleVideo = new VideoItem {
                    Name = Path.GetFileName(b),
                    Path = b,
                    Media = uri,
                };
            }
            if (0 < a.Count) MainWindowMode = MainWindowMode.MediaList;
            else if (!string.IsNullOrEmpty(SingleVideo.Path)) MainWindowMode = MainWindowMode.SingleVideo;
            else MainWindowMode = MainWindowMode.MediaList;
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

        public void MoveToPreviousMediaItem () { MoveUp?.Invoke(this, new()); }
        public void MoveToNextMediaItem () { MoveDown?.Invoke(this, new()); }

        public event EventHandler? MoveDown;
        public event EventHandler? MoveUp;
        public event EventHandler? SetMediaListMedia;

        public Uri WebpageUrl = new("about:blank");

        public void AddMediaItem (MediaItem a) {
            MediaItems.Add(a);
            MediaListHasContents = true;
        }

        public void BackupMediaItems () {
            SettingsStorage.ClearMediaListPaths();
            foreach (MediaItem a in MediaItems)
                SettingsStorage.SaveMediaListPath(a.Path);
            SettingsStorage.SingleVideoPath = SingleVideo.Path;
        }

        public void RemoveMediaItem (int i) {
            MediaItems.RemoveAt(i);
            MediaListHasContents = 0 < MediaItems.Count;
        }

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

                SetMediaListMedia?.Invoke(this, new());
            }
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

        bool _mediaDisplayed = false;
        public bool MediaDisplayed {
            get => _mediaDisplayed;
            set { Set(ref _mediaDisplayed, value); }
        }

        bool _pdfDisplayedOnMediaWindow = false;
        public bool PdfDisplayedOnMediaWindow {
            get => _pdfDisplayedOnMediaWindow;
            set { Set(ref _pdfDisplayedOnMediaWindow, value); }
        }

        bool _pictureDisplayedOnMediaWindow = false;
        public bool PictureDisplayedOnMediaWindow {
            get => _pictureDisplayedOnMediaWindow;
            set { Set(ref _pictureDisplayedOnMediaWindow, value); }
        }

        VideoItem _singleVideo = new();
        public VideoItem SingleVideo {
            get => _singleVideo;
            set => Set(ref _singleVideo, value);
        }

        bool _videoDisplayedOnMediaWindow = false;
        public bool VideoDisplayedOnMediaWindow {
            get => _videoDisplayedOnMediaWindow;
            set { Set(ref _videoDisplayedOnMediaWindow, value); }
        }

        bool _videoPaused = true;
        public bool VideoPaused {
            get => _videoPaused;
            set => Set(ref _videoPaused, value);
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
    }
}
