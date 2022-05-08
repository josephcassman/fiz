using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Fiz {
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window {
        public MainWindow()
        {
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
    }

    public class MediaItem {
        public StorageFile File { get; set; }
    }
}
