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

        int _currentMediaItemIndex = -1;
        public int CurrentMediaItemIndex {
            get => _currentMediaItemIndex;
            set {
                Set(ref _currentMediaItemIndex, value);
                if (MediaItems[CurrentMediaItemIndex] is PictureItem a) {
                    CurrentPicture = a.Media;
                    IsPictureOnDisplay = true;
                }
                else {
                    CurrentVideo = ((VideoItem) MediaItems[CurrentMediaItemIndex]).Media;
                    IsPictureOnDisplay = false;
                }
            }
        }

        bool _isPictureOnDisplay = true;
        public bool IsPictureOnDisplay {
            get => _isPictureOnDisplay;
            set => Set(ref _isPictureOnDisplay, value);
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

        public void MoveToPreviousMediaItem () {
            if (MediaItems.Count == 0) return;
            if (CurrentMediaItemIndex == 0) return;
            --CurrentMediaItemIndex;
        }

        public void MoveToNextMediaItem () {
            if (MediaItems.Count == 0) return;
            if (MediaItems.Count - 1 <= CurrentMediaItemIndex) return;
            ++CurrentMediaItemIndex;
        }

        public void AddMediaItem (MediaItem a) {
            MediaItems.Add(a);
            MediaListHasContents = true;
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

        bool _mediaItemSelected = false;
        public bool MediaItemSelected {
            get => _mediaItemSelected;
            set => Set(ref _mediaItemSelected, value);
        }

        bool _mediaDisplayMode = false;
        public bool MediaDisplayMode {
            get => _mediaDisplayMode;
            set => Set(ref _mediaDisplayMode, value);
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
