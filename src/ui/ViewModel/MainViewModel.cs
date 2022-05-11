using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace UI.ViewModel {
    public class PictureItem {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
    }
    public class MainViewModel : BindableBase {
        public ObservableCollection<PictureItem> Pictures = new();

        private bool _pictureMode = true;
        public bool PictureMode {
            get => _pictureMode;
            set => Set(ref _pictureMode, value);
        }
    }
}
