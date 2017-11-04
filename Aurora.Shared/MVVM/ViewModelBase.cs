// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;

namespace Aurora.Shared.MVVM
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingField, T Value, [CallerMemberName] string propertyName = null)
        {
            var changed = !EqualityComparer<T>.Default.Equals(backingField, Value);
            if (changed)
            {
                backingField = Value;
                this.RaisePropertyChanged(propertyName);
            }
            return changed;
        }

        public Visibility BooltoVisibility(bool b)
        {
            return b ? Visibility.Visible : Visibility.Collapsed;
        }
        public Visibility BoolNottoVisibility(bool b)
        {
            return !b ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
