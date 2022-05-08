using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

namespace Fiz {
    public sealed partial class MainWindow : Window {
        public MainWindow () {
            this.InitializeComponent();
        }

        public ObservableCollection<MediaItem> MediaItems { get; set; } = new ObservableCollection<MediaItem>();

        private void DragOver (object sender, DragEventArgs e) {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void Drop (object sender, DragEventArgs e) {
            if (e.DataView.Contains(StandardDataFormats.StorageItems)) {
                var items = await e.DataView.GetStorageItemsAsync();
                if (0 < items.Count) {
                    var file = items[0] as StorageFile;
                    MediaItems.Add(new MediaItem {
                        File = file,
                        FileName = file.Name,
                    });
                }
            }
            setVisibility();
        }

        void setVisibility () {
            var empty = MediaItems.Count == 0;
            DropInstructions.Visibility = empty ? Visibility.Visible : Visibility.Collapsed;
            Contents.Visibility = empty ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void AppBarButton_Click (object sender, RoutedEventArgs e) {
            var a = e.OriginalSource as AppBarButton;
            switch (a.Label) {
                case "Add":
                    var window = new Microsoft.UI.Xaml.Window();
                    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
                    var picker = new Windows.Storage.Pickers.FileOpenPicker();
                    WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
                    picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                    picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
                    picker.FileTypeFilter.Add(".jpg");
                    picker.FileTypeFilter.Add(".jpeg");
                    picker.FileTypeFilter.Add(".png");
                    picker.FileTypeFilter.Add(".gif");
                    picker.FileTypeFilter.Add(".mp4");
                    picker.FileTypeFilter.Add(".mp3");
                    var file = await picker.PickSingleFileAsync();
                    if (file != null) {
                        MediaItems.Add(new MediaItem {
                            File = file,
                            FileName = file.Name,
                        });
                    }
                    else { }
                    setVisibility();
                    break;
                case "Clear":
                    MediaItems.Clear();
                    setVisibility();
                    break;
            }
        }
    }

    public class MediaItem {
        public StorageFile File { get; set; }
        public string FileName { get; set; } = "";
    }
}
