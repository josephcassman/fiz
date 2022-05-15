using System.Windows.Media.Imaging;

namespace UI.ViewModel {
    public class PictureItem {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public BitmapImage Preview { get; set; } = new();
        public BitmapImage Media { get; set; } = new();
    }
}
