using System;
using System.Globalization;
using System.Linq;
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

    [ValueConversion(typeof(bool), typeof(double))]
    public class BoolToOpaqueConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
            bool a = (bool) value;
            return a ? 0.5 : 1.0;
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

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InvertConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
            bool a = (bool) value;
            return !a;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(double))]
    public class MainContentHeightConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
            bool a = (bool) value;
            return a ? 0 : 530;
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

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class MultiBoolToCollapsedConverter : IMultiValueConverter {
        public object Convert (object[] values, Type targetType, object parameter, CultureInfo culture) {
            return values.Where(value => value is bool).Any(value => (bool) value == false) ?
                Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack (object value, Type[] targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(double))]
    public class MultiBoolToVisibleConverter : IMultiValueConverter {
        public object Convert (object[] values, Type targetType, object parameter, CultureInfo culture) {
            return values.Where(value => value is bool).Any(value => (bool) value == false) ?
                  Visibility.Collapsed : Visibility.Visible;
        }

        public object[] ConvertBack (object value, Type[] targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(double))]
    public class OpacityConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, CultureInfo culture) {
            bool a = (bool) value;
            return a ? 0.15 : 1.0;
        }

        public object ConvertBack (object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
