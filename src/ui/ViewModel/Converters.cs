using System;
using System.Windows;
using System.Windows.Data;

namespace UI.ViewModel {
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class VisibleWhenFalseConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            bool a = (bool) value;
            return a ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var a = (Visibility) value;
            return a == Visibility.Collapsed;
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class VisibleWhenTrueConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            bool a = (bool) value;
            return a ? Visibility.Visible: Visibility.Collapsed;
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var a = (Visibility) value;
            return a == Visibility.Visible;
        }
    }

    [ValueConversion(typeof(MediaItem), typeof(Visibility))]
    public class MediaItemToVisibleWhenTrueConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var a = (MediaItem) value;
            return string.IsNullOrEmpty(a.Name) ? Visibility.Hidden : Visibility.Visible;
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(MediaItem), typeof(Visibility))]
    public class MediaItemToHiddenWhenTrueConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var a = (MediaItem) value;
            return string.IsNullOrEmpty(a.Name) ? Visibility.Visible : Visibility.Hidden;
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
