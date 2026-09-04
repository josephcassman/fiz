// Copyright 2026 Joseph Cassman
// SPDX-License-Identifier: Apache-2.0

using System.IO;
using System.Text.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UI.ViewModel;

namespace UI.Tests {
    [TestClass]
    public class SettingsStorageTests {
        private string? _tempSettingsFile;

        [TestInitialize]
        public void Setup() {
            _tempSettingsFile = Path.Combine(Path.GetTempPath(), $"fiz_test_settings_{Guid.NewGuid():N}.json");
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
        public void SettingsStorage_SavesAndPersistsProperties() {
            SettingsStorage.MediaWindowHeight = 720.0;
            SettingsStorage.MediaWindowWidth = 1280.0;
            SettingsStorage.MediaWindowLeft = 100.0;
            SettingsStorage.MediaWindowTop = 200.0;
            SettingsStorage.MediaWindowMaximized = true;
            SettingsStorage.WebPageScaleFactor = 1.5;
            SettingsStorage.SingleVideoPath = @"C:\test\video.mp4";

            Assert.AreEqual(720.0, SettingsStorage.MediaWindowHeight);
            Assert.AreEqual(1280.0, SettingsStorage.MediaWindowWidth);
            Assert.AreEqual(100.0, SettingsStorage.MediaWindowLeft);
            Assert.AreEqual(200.0, SettingsStorage.MediaWindowTop);
            Assert.IsTrue(SettingsStorage.MediaWindowMaximized);
            Assert.AreEqual(1.5, SettingsStorage.WebPageScaleFactor);
            Assert.AreEqual(@"C:\test\video.mp4", SettingsStorage.SingleVideoPath);

            // Re-initialize from the persisted file to verify disk round-trip
            SettingsStorage.ResetData();
            SettingsStorage.Initialize();

            Assert.AreEqual(720.0, SettingsStorage.MediaWindowHeight);
            Assert.AreEqual(1280.0, SettingsStorage.MediaWindowWidth);
            Assert.AreEqual(100.0, SettingsStorage.MediaWindowLeft);
            Assert.AreEqual(200.0, SettingsStorage.MediaWindowTop);
            Assert.IsTrue(SettingsStorage.MediaWindowMaximized);
            Assert.AreEqual(1.5, SettingsStorage.WebPageScaleFactor);
            Assert.AreEqual(@"C:\test\video.mp4", SettingsStorage.SingleVideoPath);
        }

        [TestMethod]
        public void SettingsStorage_StartLocations_ClampNegativeToZero() {
            SettingsStorage.StartLocationLeft = -50.0;
            SettingsStorage.StartLocationTop = -100.0;

            Assert.AreEqual(0.0, SettingsStorage.StartLocationLeft);
            Assert.AreEqual(0.0, SettingsStorage.StartLocationTop);

            SettingsStorage.StartLocationLeft = 150.0;
            SettingsStorage.StartLocationTop = 250.0;

            Assert.AreEqual(150.0, SettingsStorage.StartLocationLeft);
            Assert.AreEqual(250.0, SettingsStorage.StartLocationTop);
        }

        [TestMethod]
        public void SettingsStorage_MediaListPaths_ManagesListCorrectly() {
            SettingsStorage.ClearMediaListPaths();
            Assert.AreEqual(0, SettingsStorage.MediaListPaths.Count);

            SettingsStorage.SaveMediaListPath(@"C:\media\a.png");
            SettingsStorage.SaveMediaListPath(@"C:\media\b.mp4");
            // Duplicate should not add twice
            SettingsStorage.SaveMediaListPath(@"C:\media\a.png");

            Assert.AreEqual(2, SettingsStorage.MediaListPaths.Count);
            CollectionAssert.AreEqual(new[] { @"C:\media\a.png", @"C:\media\b.mp4" }, SettingsStorage.MediaListPaths);

            SettingsStorage.DeleteMediaListPath(@"C:\media\a.png");
            Assert.AreEqual(1, SettingsStorage.MediaListPaths.Count);
            Assert.AreEqual(@"C:\media\b.mp4", SettingsStorage.MediaListPaths[0]);

            SettingsStorage.ClearMediaListPaths();
            Assert.AreEqual(0, SettingsStorage.MediaListPaths.Count);
        }

        [TestMethod]
        public void SettingsData_SerializesAndDeserializesCorrectly() {
            var data = new SettingsData {
                WebPageScaleFactor = 1.25,
                MediaWindowMaximized = true,
                MediaListPaths = [@"C:\1.png", @"C:\2.mp4"]
            };

            var json = JsonSerializer.Serialize(data);
            var deserialized = JsonSerializer.Deserialize<SettingsData>(json);

            Assert.IsNotNull(deserialized);
            Assert.AreEqual(1.25, deserialized.WebPageScaleFactor);
            Assert.IsTrue(deserialized.MediaWindowMaximized);
            CollectionAssert.AreEqual(new[] { @"C:\1.png", @"C:\2.mp4" }, deserialized.MediaListPaths);
        }
    }
}
