// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class BindedTemplates
    {
        public BindedTemplates()
        {
            this.InitializeComponent();
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["PointerOver"] as Storyboard).Begin();
            }
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (sender is Panel s)
            {
                (s.Resources["Normal"] as Storyboard).Begin();
            }
        }

        private void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            MainPageViewModel.Current.SkiptoItem((sender as Button).DataContext as SongViewModel);
        }

        private async void SongList_Play(object sender, RoutedEventArgs e)
        {
            await MainPageViewModel.Current.InstantPlay(await (await ((sender as FrameworkElement).DataContext as SongViewModel).GetAlbumAsync()).GetSongsAsync(), (int)((sender as FrameworkElement).DataContext as SongViewModel).Index);
        }
    }
}
