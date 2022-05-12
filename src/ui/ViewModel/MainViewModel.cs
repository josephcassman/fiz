using System;
using System.Collections.ObjectModel;

namespace UI.ViewModel {
    public class PictureItem {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
    }

    public class MainViewModel : BindableBase {
        public event EventHandler<MessageEventArgs>? Message;

        public void SendMessage (object sender, MessageEventArgs args) {
            Message?.Invoke(sender, args);
        }

        public ObservableCollection<PictureItem> Pictures = new();

        public void AddPicture (PictureItem picture) {
            Pictures.Add(picture);
            HasPictures = true;
        }

        private bool _hasPictures = false;
        public bool HasPictures {
            get => _hasPictures;
            set => Set(ref _hasPictures, value);
        }

        private bool _playingPictureSlideshow = false;
        public bool PlayingPictureSlideshow {
            get => _playingPictureSlideshow;
            set => Set(ref _playingPictureSlideshow, value);
        }

        public int CurrentPictureIndex;

        private PictureItem _currentPicture = new();
        public PictureItem CurrentPicture {
            get => _currentPicture;
            set => Set(ref _currentPicture, value);
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
    }
}
