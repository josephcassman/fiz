﻿using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace UI.ViewModel {
    public abstract class BindableBase : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged ([CallerMemberName] string? propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool Set<T> (ref T storage, T value,
            [CallerMemberName] string? propertyName = null) {
            if (Equals(storage, value)) {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
