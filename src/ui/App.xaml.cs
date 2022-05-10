using System.Windows;
using UI.ViewModel;

namespace UI {
    public partial class App : Application {
        public static MainViewModel ViewModel { get; } = new MainViewModel();
    }
}
