using Aurora.Music.Core.Storage;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class AddPlayList : ContentDialog
    {
        ObservableCollection<PlayListViewModel> Playlists = new ObservableCollection<PlayListViewModel>();

        private int[] songID;

        public AddPlayList()
        {
            this.InitializeComponent();
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
            Title = $"Add {songID.Length} " + (songID.Length == 1 ? "song" : "songs") + " into collection";
        }

        public AddPlayList(int ID) : this()
        {
            songID = new int[] { ID };
            Title = $"Add 1 song into collection";
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

        private async void Button_Click_1(object sender, RoutedEventArgs e)
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
