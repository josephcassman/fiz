using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace UI.ViewModel {
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BoolToCollapsedConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
            bool a = (bool) value;
            return a ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(double))]
    public sealed class BoolToNotActiveOpacityConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
            bool a = (bool) value;
            return a ? 0.08 : 1.0;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BoolToVisibleConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
            bool a = (bool) value;
            return a ? Visibility.Visible: Visibility.Collapsed;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(double))]
    public sealed class DisjunctionVisibleConverter : IMultiValueConverter {
        public object Convert (object[] values, Type targetType, object parameter, CultureInfo culture) {
            return values.Where(value => value is bool).Any(value => (bool) value == true) ?
                  Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack (object value, Type[] targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public sealed class InvertConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
            bool a = (bool) value;
            return !a;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(double))]
    public sealed class MainContentHeightConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
            bool a = (bool) value;
            return a ? 0 : 530;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(MediaItem), typeof(Visibility))]
    public sealed class MediaItemToCollapsedWhenTrueConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
            var a = (MediaItem) value;
            return string.IsNullOrEmpty(a.FileName) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(MediaItem), typeof(Visibility))]
    public sealed class MediaItemToVisibleWhenTrueConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
            var a = (MediaItem) value;
            return string.IsNullOrEmpty(a.FileName) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class MultiBoolToCollapsedConverter : IMultiValueConverter {
        public object Convert (object[] values, Type targetType, object parameter, CultureInfo culture) {
            return values.Where(value => value is bool).Any(value => (bool) value == false) ?
                Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack (object value, Type[] targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(double))]
    public sealed class MultiBoolToVisibleConverter : IMultiValueConverter {
        public object Convert (object[] values, Type targetType, object parameter, CultureInfo culture) {
            return values.Where(value => value is bool).Any(value => (bool) value == false) ?
                  Visibility.Collapsed : Visibility.Visible;
        }

        public object[] ConvertBack (object value, Type[] targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(string), typeof(Visibility))]
    public sealed class TextToVisibleConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
            var a = (string) value;
            return string.IsNullOrEmpty(a) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
