﻿using System;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace UI.ViewModel {
    public class MainViewModel : BindableBase {
        public MainViewModel() {
            SettingsStorage.Initialize();
            ShowMediaOnSecondMonitor = SettingsStorage.ShowMediaOnSecondMonitor;
            ShowMediaFullscreen = SettingsStorage.ShowMediaFullscreen;
        }

        public ObservableCollection<MediaItem> MediaItems = new();

        private int _currentMediaItemIndex = -1;
        public int CurrentMediaItemIndex {
            get => _currentMediaItemIndex;
            set {
                Set(ref _currentMediaItemIndex, value);
                if (MediaItems[CurrentMediaItemIndex] is PictureItem a)
                    CurrentPicture = a.Media;
                else
                    CurrentVideo = ((VideoItem) MediaItems[CurrentMediaItemIndex]).Media;
            }
        }

        private BitmapImage _currentPicture = new();
        public BitmapImage CurrentPicture {
            get => _currentPicture;
            set => Set(ref _currentPicture, value);
        }

        private Uri _currentVideo = new("about:blank");
        public Uri CurrentVideo {
            get => _currentVideo;
            set => Set(ref _currentVideo, value);
        }

        public void MoveToPreviousMediaItem () {
            if (MediaItems.Count == 0) return;
            if (CurrentMediaItemIndex == 0) return;
            --CurrentMediaItemIndex;
        }

        public void MoveToNextMediaItem () {
            if (MediaItems.Count == 0) return;
            if (MediaItems.Count - 1 <= CurrentMediaItemIndex) return;
            ++CurrentMediaItemIndex;
        }

        public void AddMediaItem (MediaItem a) {
            MediaItems.Add(a);
            MediaListHasContents = true;
        }

        private bool _mediaListHasContents = false;
        public bool MediaListHasContents {
            get => _mediaListHasContents;
            set => Set(ref _mediaListHasContents, value);
        }

        private bool _mediaListMode = true;
        public bool MediaListMode {
            get => _mediaListMode;
            set => Set(ref _mediaListMode, value);
        }

        private bool _mediaItemSelected = false;
        public bool MediaItemSelected {
            get => _mediaItemSelected;
            set => Set(ref _mediaItemSelected, value);
        }

        private bool _mediaDisplayMode = false;
        public bool MediaDisplayMode {
            get => _mediaDisplayMode;
            set => Set(ref _mediaDisplayMode, value);
        }

        private bool _showMediaOnSecondMonitor = true;
        public bool ShowMediaOnSecondMonitor {
            get => _showMediaOnSecondMonitor;
            set {
                Set(ref _showMediaOnSecondMonitor, value);
                SettingsStorage.ShowMediaOnSecondMonitor = value;
            }
        }

        private bool _showMediaFullscreen = true;
        public bool ShowMediaFullscreen {
            get => _showMediaFullscreen;
            set {
                Set(ref _showMediaFullscreen, value);
                SettingsStorage.ShowMediaFullscreen = value;
            }
        }
    }
}
