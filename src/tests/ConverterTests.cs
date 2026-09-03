using System.Globalization;
using System.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UI.ViewModel;

namespace UI.Tests {
    [TestClass]
    public class ConverterTests {
        private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        [TestMethod]
        public void BoolToVisibleConverter_ReturnsExpectedVisibility() {
            var converter = new BoolToVisibleConverter();
            Assert.AreEqual(Visibility.Visible, converter.Convert(true, typeof(Visibility), null!, Culture));
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(false, typeof(Visibility), null!, Culture));
        }

        [TestMethod]
        public void BoolToCollapsedConverter_ReturnsExpectedVisibility() {
            var converter = new BoolToCollapsedConverter();
            Assert.AreEqual(Visibility.Collapsed, converter.Convert(true, typeof(Visibility), null!, Culture));
            Assert.AreEqual(Visibility.Visible, converter.Convert(false, typeof(Visibility), null!, Culture));
        }

        [TestMethod]
        public void BoolToNotActiveOpacityConverter_ReturnsExpectedOpacity() {
            var converter = new BoolToNotActiveOpacityConverter();
            Assert.AreEqual(0.08, converter.Convert(true, typeof(double), null!, Culture));
            Assert.AreEqual(1.0, converter.Convert(false, typeof(double), null!, Culture));
        }

        [TestMethod]
        public void InvertConverter_InvertsBoolean() {
            var converter = new InvertConverter();
            Assert.AreEqual(false, converter.Convert(true, typeof(bool), null!, Culture));
            Assert.AreEqual(true, converter.Convert(false, typeof(bool), null!, Culture));
        }

        [TestMethod]
        public void MainContentHeightConverter_ReturnsCorrectHeight() {
            var converter = new MainContentHeightConverter();
            Assert.AreEqual(0, converter.Convert(true, typeof(double), null!, Culture));
            Assert.AreEqual(530, converter.Convert(false, typeof(double), null!, Culture));
        }

        [TestMethod]
        public void TextToVisibleConverter_EmptyReturnsVisible_NonEmptyReturnsCollapsed() {
            var converter = new TextToVisibleConverter();
            Assert.AreEqual(Visibility.Visible, converter.Convert("", typeof(Visibility), null!, Culture));
            Assert.AreEqual(Visibility.Visible, converter.Convert(null!, typeof(Visibility), null!, Culture));
            Assert.AreEqual(Visibility.Collapsed, converter.Convert("hello", typeof(Visibility), null!, Culture));
        }

        [TestMethod]
        public void MediaItemConverters_EvaluateFileName() {
            var collapsedConverter = new MediaItemToCollapsedWhenTrueConverter();
            var visibleConverter = new MediaItemToVisibleWhenTrueConverter();

            var emptyItem = new PictureItem { FileName = "" };
            var populatedItem = new VideoItem { FileName = "video.mp4" };

            // When empty: MediaItemToCollapsedWhenTrueConverter returns Visible
            Assert.AreEqual(Visibility.Visible, collapsedConverter.Convert(emptyItem, typeof(Visibility), null!, Culture));
            Assert.AreEqual(Visibility.Collapsed, collapsedConverter.Convert(populatedItem, typeof(Visibility), null!, Culture));

            // When populated: MediaItemToVisibleWhenTrueConverter returns Visible
            Assert.AreEqual(Visibility.Collapsed, visibleConverter.Convert(emptyItem, typeof(Visibility), null!, Culture));
            Assert.AreEqual(Visibility.Visible, visibleConverter.Convert(populatedItem, typeof(Visibility), null!, Culture));
        }

        [TestMethod]
        public void DisjunctionVisibleConverter_ReturnsVisibleIfAnyTrue() {
            var converter = new DisjunctionVisibleConverter();

            object[] allFalse = [false, false];
            object[] oneTrue = [false, true];
            object[] allTrue = [true, true];

            Assert.AreEqual(Visibility.Collapsed, converter.Convert(allFalse, typeof(Visibility), null!, Culture));
            Assert.AreEqual(Visibility.Visible, converter.Convert(oneTrue, typeof(Visibility), null!, Culture));
            Assert.AreEqual(Visibility.Visible, converter.Convert(allTrue, typeof(Visibility), null!, Culture));
        }

        [TestMethod]
        public void MultiBoolConverters_EvaluateArrayOfBooleans() {
            var multiCollapsed = new MultiBoolToCollapsedConverter();
            var multiVisible = new MultiBoolToVisibleConverter();

            object[] allTrue = [true, true];
            object[] withFalse = [true, false];

            // MultiBoolToCollapsedConverter: Visible if ANY is false, else Collapsed
            Assert.AreEqual(Visibility.Collapsed, multiCollapsed.Convert(allTrue, typeof(Visibility), null!, Culture));
            Assert.AreEqual(Visibility.Visible, multiCollapsed.Convert(withFalse, typeof(Visibility), null!, Culture));

            // MultiBoolToVisibleConverter: Collapsed if ANY is false, else Visible
            Assert.AreEqual(Visibility.Visible, multiVisible.Convert(allTrue, typeof(Visibility), null!, Culture));
            Assert.AreEqual(Visibility.Collapsed, multiVisible.Convert(withFalse, typeof(Visibility), null!, Culture));
        }
    }
}
