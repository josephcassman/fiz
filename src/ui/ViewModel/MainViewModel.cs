using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace UI.ViewModel {
    public class MainViewModel : BindableBase {
        public MainViewModel() {
            SettingsStorage.Initialize();
            ShowMediaOnSecondMonitor = SettingsStorage.ShowMediaOnSecondMonitor;
            ShowMediaFullscreen = SettingsStorage.ShowMediaFullscreen;
        }

        public ObservableCollection<MediaItem> MediaItems = new();

        public event EventHandler? StopVideo;
        public event EventHandler? MoveUp;
        public event EventHandler? MoveDown;

        int _mediaItemsCurrentIndex = -1;
        public int MediaItemsCurrentIndex {
            get => _mediaItemsCurrentIndex;
            set {
                if (0 <= _mediaItemsCurrentIndex && MediaItems[_mediaItemsCurrentIndex] is VideoItem)
                    StopVideo?.Invoke(this, new());
                Set(ref _mediaItemsCurrentIndex, value);
                if (MediaItems[value] is PictureItem a) {
                    CurrentPicture = a.Media;
                    PictureDisplayedOnMediaWindow = true;
                }
                else {
                    CurrentVideo = ((VideoItem) MediaItems[value]).Media;
                    PictureDisplayedOnMediaWindow = false;
                }
            }
        }

        bool _pictureDisplayedOnMediaWindow = true;
        public bool PictureDisplayedOnMediaWindow {
            get => _pictureDisplayedOnMediaWindow;
            set => Set(ref _pictureDisplayedOnMediaWindow, value);
        }

        BitmapImage _currentPicture = new();
        public BitmapImage CurrentPicture {
            get => _currentPicture;
            set => Set(ref _currentPicture, value);
        }

        Uri _currentVideo = new("about:blank");
        public Uri CurrentVideo {
            get => _currentVideo;
            set => Set(ref _currentVideo, value);
        }

        VideoItem _singleVideo = new();
        public VideoItem SingleVideo {
            get => _singleVideo;
            set => Set(ref _singleVideo, value);
        }

        public void MoveToPreviousMediaItem () { MoveUp?.Invoke(this, new()); }
        public void MoveToNextMediaItem () { MoveDown?.Invoke(this, new()); }

        public void AddMediaItem (MediaItem a) {
            MediaItems.Add(a);
            MediaListHasContents = true;
        }

        bool _videoPaused = true;
        public bool VideoPaused {
            get => _videoPaused;
            set => Set(ref _videoPaused, value);
        }

        bool _mediaListHasContents = false;
        public bool MediaListHasContents {
            get => _mediaListHasContents;
            set => Set(ref _mediaListHasContents, value);
        }

        bool _mediaListMode = true;
        public bool MediaListMode {
            get => _mediaListMode;
            set => Set(ref _mediaListMode, value);
        }

        bool _mediaDisplayed = false;
        public bool MediaDisplayed {
            get => _mediaDisplayed;
            set => Set(ref _mediaDisplayed, value);
        }

        bool _showMediaOnSecondMonitor = true;
        public bool ShowMediaOnSecondMonitor {
            get => _showMediaOnSecondMonitor;
            set {
                Set(ref _showMediaOnSecondMonitor, value);
                SettingsStorage.ShowMediaOnSecondMonitor = value;
            }
        }

        bool _showMediaFullscreen = true;
        public bool ShowMediaFullscreen {
            get => _showMediaFullscreen;
            set {
                Set(ref _showMediaFullscreen, value);
                SettingsStorage.ShowMediaFullscreen = value;
            }
        }
    }
}
