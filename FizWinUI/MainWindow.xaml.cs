using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Fiz {
    public sealed partial class MainWindow : Window {
        public MainWindow () {
            this.InitializeComponent();
        }

        public MainViewModel ViewModel => App.ViewModel;

        async Task addFiles (IEnumerable<IStorageItem> files) {
            static async Task processFile (IStorageItem file, ConcurrentBag<MediaItem> results) {
                var a = file as StorageFile;
                var thumbnail = await a.GetThumbnailAsync(ThumbnailMode.PicturesView, 150, ThumbnailOptions.UseCurrentScale);
                var bmp = new BitmapImage();
                bmp.SetSource(thumbnail);
                results.Add(new MediaItem {
                    File = a,
                    FileName = a.Name,
                    Preview = bmp,
                });
            }

            var results = new ConcurrentBag<MediaItem>();
            await Task.WhenAll(files.Select(file => processFile(file, results)));
            foreach (var result in results)
                ViewModel.MediaItems.Add(result);
        }

        private void DragOver (object sender, DragEventArgs e) {
            e.AcceptedOperation = DataPackageOperation.Copy;
        }

        private async void Drop (object sender, DragEventArgs e) {
            if (e.DataView.Contains(StandardDataFormats.StorageItems)) {
                try {
                    var items = await e.DataView.GetStorageItemsAsync();
                    await addFiles(items);
                    if (0 < ViewModel.MediaItems.Count)
                        ViewModel.SelectedMediaItem = ViewModel.MediaItems[^1];
                    FileList.SelectedIndex = FileList.Items.Count - 1;
                }
                catch (Exception) { }
            }
        }

        private void FileList_SelectionChanged (object sender, SelectionChangedEventArgs e) {
            if (FileList.SelectedItem == null) return;
            ViewModel.SelectedMediaItem = ViewModel.MediaItems[FileList.SelectedIndex];
        }

        private void ImageButton_PointerEntered (object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e) {
            var a = sender as Border;
            a.Background = new SolidColorBrush(Colors.LightGray);
        }

        private void ImageButton_PointerExited (object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e) {
            var a = sender as Border;
            a.Background = new SolidColorBrush(Colors.Transparent);
        }

        private async void Add_Tapped (object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e) {
            try {
                var window = new Window();
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
                var items = await picker.PickMultipleFilesAsync();
                await addFiles(items);
                if (0 < ViewModel.MediaItems.Count)
                    ViewModel.SelectedMediaItem = ViewModel.MediaItems[^1];
                FileList.SelectedIndex = FileList.Items.Count - 1;
            }
            catch (Exception) { }
        }

        private void Up_Tapped (object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e) {
            if (FileList.SelectedItem == null) return;
            if (ViewModel.MediaItems.Count == 0) return;
            if (FileList.SelectedIndex == 0) return;
            var i = FileList.SelectedIndex;
            (ViewModel.MediaItems[i], ViewModel.MediaItems[i - 1]) = (ViewModel.MediaItems[i - 1], ViewModel.MediaItems[i]);
            FileList.SelectedIndex = i - 1;
        }

        private void Down_Tapped (object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e) {
            if (FileList.SelectedItem == null) return;
            if (ViewModel.MediaItems.Count == 0) return;
            if (FileList.Items.Count - 1 <= FileList.SelectedIndex) return;
            var i = FileList.SelectedIndex;
            (ViewModel.MediaItems[i], ViewModel.MediaItems[i + 1]) = (ViewModel.MediaItems[i + 1], ViewModel.MediaItems[i]);
            FileList.SelectedIndex = i + 1;
        }
    }
}
