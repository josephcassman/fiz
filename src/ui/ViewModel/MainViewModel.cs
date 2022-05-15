using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace UI.ViewModel {
    public class MainViewModel : BindableBase {
        public MainViewModel() {
            SettingsStorage.Initialize();
            ShowMediaOnSecondMonitor = SettingsStorage.ShowMediaOnSecondMonitor;
            ShowMediaFullscreen = SettingsStorage.ShowMediaFullscreen;
        }

        public ObservableCollection<PictureItem> MediaItems = new();
        public int CurrentPictureIndex;

        private BitmapImage _currentPicture = new();
        public BitmapImage CurrentPicture {
            get => _currentPicture;
            set => Set(ref _currentPicture, value);
        }

        public void MoveToPreviousPicture () {
            if (MediaItems.Count == 0) return;
            if (CurrentPictureIndex == 0) return;
            CurrentPicture = MediaItems[--CurrentPictureIndex].Bitmap;
        }

        public void MoveToNextPicture () {
            if (MediaItems.Count == 0) return;
            if (MediaItems.Count - 1 <= CurrentPictureIndex) return;
            CurrentPicture = MediaItems[++CurrentPictureIndex].Bitmap;
        }

        public void AddPicture (PictureItem picture) {
            MediaItems.Add(picture);
            MediaListHasContents = true;
        }

        private bool _mediaListHasContents = false;
        public bool MediaListHasContents {
            get => _mediaListHasContents;
            set => Set(ref _mediaListHasContents, value);
        }

        private bool _mediaListMode = true;
        public bool MediaListMode {
            get => _mediaListMode;
            set => Set(ref _mediaListMode, value);
        }

        private bool _mediaItemSelected = false;
        public bool MediaItemSelected {
            get => _mediaItemSelected;
            set => Set(ref _mediaItemSelected, value);
        }

        private bool _mediaDisplayMode = false;
        public bool MediaDisplayMode {
            get => _mediaDisplayMode;
            set => Set(ref _mediaDisplayMode, value);
        }

        private bool _showMediaOnSecondMonitor = true;
        public bool ShowMediaOnSecondMonitor {
            get => _showMediaOnSecondMonitor;
            set {
                Set(ref _showMediaOnSecondMonitor, value);
                SettingsStorage.ShowMediaOnSecondMonitor = value;
            }
        }

        private bool _showMediaFullscreen = true;
        public bool ShowMediaFullscreen {
            get => _showMediaFullscreen;
            set {
                Set(ref _showMediaFullscreen, value);
                SettingsStorage.ShowMediaFullscreen = value;
            }
        }
    }
}
