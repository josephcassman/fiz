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
                    MediaItems.Add(new MediaItem {
                        File = items[0] as StorageFile,
                    });
                }
            }
            setVisibility();
        }

        void setVisibility () {
            DropInstructions.Visibility = MediaItems.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void AppBarButton_Click (object sender, RoutedEventArgs e) {
            var a = e.OriginalSource as AppBarButton;
            if (a.Label == "Add") {
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
                    });
                }
                else {}
                setVisibility();
            }
        }
    }

    public class MediaItem {
        public StorageFile File { get; set; }
    }
}
