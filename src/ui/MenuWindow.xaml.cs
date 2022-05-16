﻿using System.Windows;
using System.Windows.Input;
using UI.ViewModel;

namespace UI {
    public partial class MenuWindow : Window {
        public MenuWindow () {
            InitializeComponent();
            DataContext = vm;
        }

        public MainViewModel vm => App.ViewModel;

        void Close_Click (object sender, RoutedEventArgs e) { Close(); }

        void Window_MouseDown (object sender, MouseButtonEventArgs e) {
            if (e.ChangedButton == MouseButton.Left)
                try { DragMove(); } catch { }
        }
    }
}
