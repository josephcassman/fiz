using System;
using System.Windows;
using System.Windows.Data;

namespace UI.ViewModel {
    [ValueConversion(typeof(bool), typeof(bool))]
    public class InvertConverter : IValueConverter {
        public object Convert (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            bool a = (bool) value;
            return !a;
        }

        public object ConvertBack (object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            var a = (bool) value;
            return !a;
        }
    }

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
}
