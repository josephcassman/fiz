using System;
using System.Web;
using System.Windows.Media.Imaging;

namespace UI.ViewModel {
    public abstract class MediaItem {
        public string Name { get; set; } = "";
        public string DecodedName => HttpUtility.UrlDecode(Name);
        public string Path { get; set; } = "";
        public abstract bool IsPdf { get; }
        public abstract bool IsPicture { get; }
        public abstract bool IsVideo { get; }
    }

    public class PdfItem : MediaItem {
        public Uri Media { get; set; } = new("about:blank");
        public override bool IsPdf => true;
        public override bool IsPicture => false;
        public override bool IsVideo => false;
    }

    public class PictureItem : MediaItem {
        public BitmapSource Preview { get; set; } = new BitmapImage();
        public BitmapImage Media { get; set; } = new();
        public override bool IsPdf => false;
        public override bool IsPicture => true;
        public override bool IsVideo => false;
    }

    public class VideoItem : MediaItem {
        public Uri Media { get; set; } = new("about:blank");
        public TimeSpan Skip { get; set; } = TimeSpan.Zero;
        public TimeSpan TotalLength { get; set; } = TimeSpan.Zero;
        public override bool IsPdf => false;
        public override bool IsPicture => false;
        public override bool IsVideo => true;
    }
}
