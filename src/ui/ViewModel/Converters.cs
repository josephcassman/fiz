using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UI.ViewModel {
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToCollapsedConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
            bool a = (bool) value;
            return a ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibleConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
            bool a = (bool) value;
            return a ? Visibility.Visible: Visibility.Collapsed;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(MediaItem), typeof(Visibility))]
    public class MediaItemToVisibleWhenTrueConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
            var a = (MediaItem) value;
            return string.IsNullOrEmpty(a.Name) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(MediaItem), typeof(Visibility))]
    public class MediaItemToCollapsedWhenTrueConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
            var a = (MediaItem) value;
            return string.IsNullOrEmpty(a.Name) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
