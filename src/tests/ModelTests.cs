using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UI.ViewModel;

namespace UI.Tests {
    [TestClass]
    public class ModelTests {
        [TestMethod]
        public void PictureItem_ReportsCorrectMediaTypes() {
            var item = new PictureItem {
                FileName = "photo.jpg",
                FilePath = @"C:\media\photo.jpg"
            };

            Assert.IsTrue(item.IsPicture);
            Assert.IsFalse(item.IsVideo);
            Assert.AreEqual("photo.jpg", item.FileName);
            Assert.AreEqual(@"C:\media\photo.jpg", item.FilePath);
        }

        [TestMethod]
        public void VideoItem_ReportsCorrectMediaTypes() {
            var item = new VideoItem {
                FileName = "video.mp4",
                FilePath = @"C:\media\video.mp4"
            };

            Assert.IsFalse(item.IsPicture);
            Assert.IsTrue(item.IsVideo);
            Assert.AreEqual("video.mp4", item.FileName);
            Assert.AreEqual(@"C:\media\video.mp4", item.FilePath);
            Assert.AreEqual("about:blank", item.Source.ToString());
        }

        private class TestBindable : BindableBase {
            private string _name = "";
            public string Name {
                get => _name;
                set => Set(ref _name, value);
            }

            private int _count;
            public int Count {
                get => _count;
                set => Set(ref _count, value);
            }
        }

        [TestMethod]
        public void BindableBase_Set_RaisesPropertyChangedWhenValueChanges() {
            var target = new TestBindable();
            var changedProperties = new List<string>();
            target.PropertyChanged += (_, e) => {
                if (e.PropertyName != null) {
                    changedProperties.Add(e.PropertyName);
                }
            };

            target.Name = "New Value";
            target.Count = 42;

            CollectionAssert.AreEqual(new[] { "Name", "Count" }, changedProperties);
            Assert.AreEqual("New Value", target.Name);
            Assert.AreEqual(42, target.Count);
        }

        [TestMethod]
        public void BindableBase_Set_DoesNotRaisePropertyChangedWhenValueIsIdentical() {
            var target = new TestBindable { Name = "Same" };
            var raised = false;
            target.PropertyChanged += (_, _) => raised = true;

            target.Name = "Same";

            Assert.IsFalse(raised);
        }
    }
}
