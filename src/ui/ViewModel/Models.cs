using System.Windows.Media.Imaging;

namespace UI.ViewModel {
    public class PictureItem {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public BitmapImage Bitmap { get; set; } = new();
    }
}
