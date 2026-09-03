using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UI.ViewModel;

namespace UI.Tests {
    [TestClass]
    public class MainViewModelTests {
        private string? _tempSettingsFile;

        [TestInitialize]
        public void Setup() {
            _tempSettingsFile = Path.Combine(Path.GetTempPath(), $"fiz_vm_test_{Guid.NewGuid():N}.json");
            SettingsStorage.UseCustomFilePath(_tempSettingsFile);
            SettingsStorage.ResetData();
        }

        [TestCleanup]
        public void Cleanup() {
            SettingsStorage.UseCustomFilePath(null);
            SettingsStorage.ResetData();
            if (_tempSettingsFile != null && File.Exists(_tempSettingsFile)) {
                try { File.Delete(_tempSettingsFile); } catch { }
            }
        }

        [TestMethod]
        [DataRow("image.bmp", MediaType.Picture)]
        [DataRow("image.gif", MediaType.Picture)]
        [DataRow("image.jpg", MediaType.Picture)]
        [DataRow("image.jpeg", MediaType.Picture)]
        [DataRow("image.ico", MediaType.Picture)]
        [DataRow("image.png", MediaType.Picture)]
        [DataRow("image.tif", MediaType.Picture)]
        [DataRow("image.tiff", MediaType.Picture)]
        [DataRow("image.webp", MediaType.Picture)]
        [DataRow("IMAGE.PNG", MediaType.Picture)]
        [DataRow("IMAGE.JPEG", MediaType.Picture)]
        [DataRow("video.avi", MediaType.Video)]
        [DataRow("video.mov", MediaType.Video)]
        [DataRow("video.mp4", MediaType.Video)]
        [DataRow("video.mpe", MediaType.Video)]
        [DataRow("video.mpeg", MediaType.Video)]
        [DataRow("video.mpg", MediaType.Video)]
        [DataRow("video.wmv", MediaType.Video)]
        [DataRow("VIDEO.MP4", MediaType.Video)]
        [DataRow("document.pdf", MediaType.Unused)]
        [DataRow("document.txt", MediaType.Unused)]
        [DataRow("archive.zip", MediaType.Unused)]
        [DataRow("file_without_extension", MediaType.Unused)]
        public void GetMediaType_IdentifiesExtensionsAccurately(string fileName, MediaType expected) {
            Assert.AreEqual(expected, MainViewModel.GetMediaType(fileName));
        }

        [STATestMethod]
        public void MainWindowMode_Switching_SetsRespectiveModeFlags() {
            var vm = new MainViewModel();
            try {
                vm.MainWindowMode = MainWindowMode.Internet;
                Assert.IsTrue(vm.InternetMode);
                Assert.IsFalse(vm.MediaListMode);
                Assert.IsFalse(vm.SingleVideoMode);

                vm.MainWindowMode = MainWindowMode.SingleVideo;
                Assert.IsTrue(vm.SingleVideoMode);
                Assert.IsFalse(vm.InternetMode);
                Assert.IsFalse(vm.MediaListMode);

                vm.MainWindowMode = MainWindowMode.MediaList;
                Assert.IsTrue(vm.MediaListMode);
                Assert.IsFalse(vm.InternetMode);
                Assert.IsFalse(vm.SingleVideoMode);
            }
            finally {
                vm.StopTimer();
            }
        }

        [STATestMethod]
        public void MediaItems_AddAndRemove_UpdatesCollectionAndFlags() {
            var vm = new MainViewModel();
            try {
                Assert.AreEqual(0, vm.MediaItems.Count);
                Assert.IsFalse(vm.MediaListHasContents);

                var item1 = new VideoItem { FileName = "vid1.mp4", FilePath = @"C:\vid1.mp4" };
                var item2 = new VideoItem { FileName = "vid2.mp4", FilePath = @"C:\vid2.mp4" };

                vm.AddMediaItem(item1);
                Assert.AreEqual(1, vm.MediaItems.Count);
                Assert.IsTrue(vm.MediaListHasContents);

                vm.AddMediaItem(item2);
                Assert.AreEqual(2, vm.MediaItems.Count);

                vm.RemoveMediaItem(0);
                Assert.AreEqual(1, vm.MediaItems.Count);
                Assert.IsTrue(vm.MediaListHasContents);
                Assert.AreEqual("vid2.mp4", vm.MediaItems[0].FileName);

                vm.RemoveMediaItem(0);
                Assert.AreEqual(0, vm.MediaItems.Count);
                Assert.IsFalse(vm.MediaListHasContents);
            }
            finally {
                vm.StopTimer();
            }
        }

        [STATestMethod]
        public void TimeProperties_FormatDisplayStringsCorrectly() {
            var vm = new MainViewModel();
            try {
                vm.VideoPosition = TimeSpan.FromSeconds(125);
                Assert.AreEqual("00:02:05", vm.VideoPositionText);
                Assert.AreEqual(125.0, vm.VideoPositionSeconds);

                vm.VideoTotalLength = new TimeSpan(1, 45, 30);
                Assert.AreEqual("01:45:30", vm.VideoTotalLengthText);
                Assert.AreEqual(6330.0, vm.VideoTotalLengthSeconds);

                vm.SingleVideoPreviewPosition = TimeSpan.FromSeconds(50);
                Assert.AreEqual("00:00:50", vm.SingleVideoPreviewPositionText);

                vm.SingleVideoPreviewTotalLength = TimeSpan.FromMinutes(12);
                Assert.AreEqual("00:12:00", vm.SingleVideoPreviewTotalLengthText);
            }
            finally {
                vm.StopTimer();
            }
        }

        [STATestMethod]
        public void SingleVideo_SettingDecodesAndTruncatesPreviewFileName() {
            var vm = new MainViewModel();
            try {
                var longName = "A Very Long Video Filename Exceeding Twenty Five Characters.mp4";
                vm.SingleVideo = new VideoItem { FileName = longName };

                // SingleVideoPreviewFileName should truncate to 25 chars + ellipsis (\u2026)
                Assert.AreEqual(longName[..25] + "\u2026", vm.SingleVideoPreviewFileName);
                Assert.AreEqual(TimeSpan.Zero, vm.SingleVideoPreviewPosition);
                Assert.AreEqual(TimeSpan.Zero, vm.SingleVideoPreviewTotalLength);
            }
            finally {
                vm.StopTimer();
            }
        }
    }
}
