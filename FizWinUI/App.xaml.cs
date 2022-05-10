﻿using Microsoft.UI.Xaml;

namespace FizWinUI {
    public partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window m_window;

        public static MainViewModel ViewModel { get; } = new MainViewModel();
    }
}
