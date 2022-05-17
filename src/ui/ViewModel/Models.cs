using System;
using System.Windows.Media.Imaging;

namespace UI.ViewModel {
    public abstract class MediaItem {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";
        public BitmapSource Preview { get; set; } = new BitmapImage();
        public bool IsPicture { get; set; } = true;
    }

    public class PictureItem : MediaItem {
        public BitmapImage Media { get; set; } = new();
    }

    public class VideoItem : MediaItem {
        public Uri Media { get; set; } = new("about:blank");
    }
}
