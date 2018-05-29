// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Music.Pages;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class AddPlayList : ContentDialog
    {
        ObservableCollection<PlayListViewModel> Playlists = new ObservableCollection<PlayListViewModel>();

        private int[] songID;

        public AddPlayList()
        {
            InitializeComponent();
            RequestedTheme = Settings.Current.Theme;
            Task.Run(async () =>
            {
                var list = await SQLOperator.Current().GetPlayListBriefAsync();
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
                {
                    foreach (var item in list)
                    {
                        Playlists.Add(new PlayListViewModel(item));
                    }
                    await Task.Delay(200);
                    if (list.Count > 0)
                    {
                        Main.SelectedIndex = 0;
                    }
                    else
                    {
                        IsPrimaryButtonEnabled = false;
                    }
                });
            });
        }

        public AddPlayList(IEnumerable<int> ID) : this()
        {
            songID = ID.ToArray();
            Title = string.Format(Consts.Localizer.GetString("AddToCollectionTitle"), SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), songID.Length));
        }

        public AddPlayList(int ID) : this()
        {
            songID = new int[] { ID };
            Title = string.Format(Consts.Localizer.GetString("AddToCollectionTitle"), SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), 1));
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (Main.SelectedIndex < 0 || Playlists.Count < 1 || Main.SelectedIndex > Playlists.Count)
            {

            }
            else
            {
                if (Main.SelectedIndex == 0)
                {
                    foreach (var item in songID)
                    {
                        await SQLOperator.Current().WriteFavoriteAsync(item, true);
                    }
                }
                else
                {
                    await Playlists[Main.SelectedIndex].AddAsync(songID);
                }
            }
            MainPage.Current.PopMessage($"Added {SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), songID.Length)} into collection");
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            AddCompleteBtn.IsEnabled = false;
            AddBtn.Visibility = Visibility.Collapsed;
            AddPanel.Visibility = Visibility.Visible;
        }

        private async void AddComplete(object sender, RoutedEventArgs e)
        {
            if (PlaylistTitle.Text.IsNullorEmpty())
            {
                return;
            }
            var p = new PlayListViewModel()
            {
                Title = PlaylistTitle.Text,
            };
            await p.SaveAsync();

            Playlists.Add(p);

            AddBtn.Visibility = Visibility.Visible;
            PlaylistTitle.Text = string.Empty;
            AddPanel.Visibility = Visibility.Collapsed;
            if (LibraryPage.Current != null)
                await LibraryPage.Current.AddPlayList(p);
        }

        private void PlaylistTitle_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PlaylistTitle.Text.IsNullorEmpty())
            {
                AddCompleteBtn.IsEnabled = false;
                return;
            }
            foreach (var item in Playlists)
            {
                if (item.Title == PlaylistTitle.Text)
                {
                    AddCompleteBtn.IsEnabled = false;
                    return;
                }
            }
            AddCompleteBtn.IsEnabled = true;
        }
    }
}
