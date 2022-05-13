using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace UI.ViewModel {
    public class PictureItem {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public BitmapImage Bitmap { get; set; } = new();
    }

    public class MainViewModel : BindableBase {
        public ObservableCollection<PictureItem> Pictures = new();
        public int CurrentPictureIndex;

        private BitmapImage _currentPicture = new();
        public BitmapImage CurrentPicture {
            get => _currentPicture;
            set => Set(ref _currentPicture, value);
        }

        public void MoveToPreviousPicture () {
            if (Pictures.Count == 0) return;
            if (CurrentPictureIndex == 0) return;
            CurrentPicture = Pictures[--CurrentPictureIndex].Bitmap;
        }

        public void MoveToNextPicture () {
            if (Pictures.Count == 0) return;
            if (Pictures.Count - 1 <= CurrentPictureIndex) return;
            CurrentPicture = Pictures[++CurrentPictureIndex].Bitmap;
        }

        public void AddPicture (PictureItem picture) {
            Pictures.Add(picture);
            HasPictures = true;
        }

        private bool _hasPictures = false;
        public bool HasPictures {
            get => _hasPictures;
            set => Set(ref _hasPictures, value);
        }

        private bool _pictureMode = true;
        public bool PictureMode {
            get => _pictureMode;
            set => Set(ref _pictureMode, value);
        }

        private bool _pictureSelected = false;
        public bool PictureSelected {
            get => _pictureSelected;
            set => Set(ref _pictureSelected, value);
        }

        private bool _playingPictureSlideshow = false;
        public bool PlayingPictureSlideshow {
            get => _playingPictureSlideshow;
            set => Set(ref _playingPictureSlideshow, value);
        }

        private bool _showMediaOnSecondMonitor = true;
        public bool ShowMediaOnSecondMonitor {
            get => _showMediaOnSecondMonitor;
            set => Set(ref _showMediaOnSecondMonitor, value);
        }
    }
}
