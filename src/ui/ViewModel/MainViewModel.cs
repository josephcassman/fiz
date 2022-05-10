namespace UI.ViewModel {
    public class MainViewModel : BindableBase {
        private bool _pictureMode = true;
        public bool PictureMode {
            get => _pictureMode;
            set => Set(ref _pictureMode, value);
        }
    }
}
