// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Shared.Extensions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Aurora.Shared.MVVM
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool isAccentDark = Palette.IsDarkColor(new UISettings().GetColorValue(UIColorType.Accent));
        public bool IsAccentDark
        {
            get { return isAccentDark; }
            set { SetProperty(ref isAccentDark, value); }
        }

        protected void RaisePropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingField, T Value, [CallerMemberName] string propertyName = null)
        {
            var changed = !EqualityComparer<T>.Default.Equals(backingField, Value);
            if (changed)
            {
                backingField = Value;
                RaisePropertyChanged(propertyName);
            }
            return changed;
        }

        public bool NullableBooltoBool(bool? b)
        {
            if (b is bool a)
            {
                return a;
            }
            return false;
        }

        public bool BoolNot(bool b)
        {
            return !b;
        }

        public Visibility BooltoVisibility(bool b)
        {
            return b ? Visibility.Visible : Visibility.Collapsed;
        }
        public Visibility BoolNottoVisibility(bool b)
        {
            return !b ? Visibility.Visible : Visibility.Collapsed;
        }
        public Visibility NullableBooltoVisibility(bool? b)
        {
            if (b is bool a)
            {
                return a ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }
        public Visibility NullableBoolNottoVisibility(bool? b)
        {
            if (b is bool a)
            {
                return !a ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }
        public SolidColorBrush AccentForeground(bool isAccentDark)
        {
            return isAccentDark ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
        }
        public Visibility CollapseIfEmpty(string s)
        {
            return s.IsNullorEmpty() ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfNotEmpty(string s)
        {
            return !s.IsNullorEmpty() ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfNull(object s)
        {
            return s == null ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfNotNull(object s)
        {
            return s != null ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfZero(int count)
        {
            return count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfZero(uint count)
        {
            return count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfNotZero(int count)
        {
            return count != 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfNotZero(uint count)
        {
            return count != 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
