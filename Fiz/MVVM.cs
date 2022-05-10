using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Storage;

namespace Fiz {
    public abstract class BindableBase : INotifyPropertyChanged {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged ([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool Set<T> (ref T storage, T value,
            [CallerMemberName] String propertyName = null) {
            if (Equals(storage, value)) {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    public class MediaItem {
        public StorageFile File { get; set; } = null;
        public string FileName { get; set; } = "";
        public BitmapImage Preview { get; set; } = new();
    }

    public class MainViewModel : BindableBase {
        public ObservableCollection<MediaItem> MediaItems { get; } = new ObservableCollection<MediaItem>();

        private MediaItem _selectedMediaItem = new MediaItem();
        public MediaItem SelectedMediaItem {
            get => _selectedMediaItem;
            set => Set(ref _selectedMediaItem, value);
        }
    }

    public static class Converters {
        public static Visibility VisibleWhenEmpty (IList values) =>
         values == null ? Visibility.Visible : values.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        public static Visibility VisibleWhenNotEmpty (IList values) =>
         values == null ? Visibility.Collapsed : values.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }
}
