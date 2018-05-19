// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Windows.UI.Xaml;

namespace Aurora.Shared.Controls
{
    public interface IChangeTheme
    {
        void ChangeTheme();
        void ChangeTheme(ElementTheme theme);
    }
}
