// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;

using Aurora.Shared.MVVM;

using Windows.UI.Xaml.Media;

namespace Aurora.Music.ViewModels
{
    public class CategoryListItem : ViewModelBase
    {
        public string Title { get; set; }
        public string Glyph { get; set; }

        public string Desc { get; set; }

        public int Index { get; set; }

        public Type NavigatType { get; set; }

        public string Parameter { get; set; }

        public double GetHeight(bool b)
        {
            return b ? 240d : 120d;
        }

        public double GetVerticalShift(bool b)
        {
            return b ? 0d : -320d;
        }
        public int ID { get; internal set; }

        public double BoolToOpacity(bool a)
        {
            return a ? 1.0 : 0.333333333333;
        }

        public SolidColorBrush ChangeTextForeground(bool b)
        {
            return (SolidColorBrush)(b ? MainPage.Current.Resources["SystemControlForegroundBaseHighBrush"] : MainPage.Current.Resources["ButtonDisabledForegroundThemeBrush"]);
        }
    }
}
