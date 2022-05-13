using System.Windows;
using UI.ViewModel;

namespace UI {
    public partial class MenuWindow : Window {
        public MenuWindow () {
            InitializeComponent();
            DataContext = vm;
        }

        public MainViewModel vm => App.ViewModel;
    }
}
