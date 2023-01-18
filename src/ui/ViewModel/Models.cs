using System;
using System.Windows.Media.Imaging;

namespace UI.ViewModel {
    public abstract class MediaItem {
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
        public abstract bool IsPicture { get; }
        public abstract bool IsVideo { get; }
    }

    public sealed class PictureItem : MediaItem {
        public BitmapImage Source { get; set; } = new();
        public override bool IsPicture => true;
        public override bool IsVideo => false;
    }

    public sealed class VideoItem : MediaItem {
        public Uri Source { get; set; } = new("about:blank");
        public override bool IsPicture => false;
        public override bool IsVideo => true;
    }
}
