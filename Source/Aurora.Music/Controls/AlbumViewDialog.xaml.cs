using Aurora.Music.Core;
using Aurora.Music.Core.Storage;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class AlbumViewDialog : ContentDialog
    {
        internal ObservableCollection<SongViewModel> SongList = new ObservableCollection<SongViewModel>();
        private AlbumViewModel album;

        public AlbumViewDialog()
        {
            this.InitializeComponent();
        }

        public event EventHandler<Uri> UpdateArtwork;

        internal AlbumViewDialog(AlbumViewModel album)
        {
            this.InitializeComponent();
            if (album == null)
            {
                Title = "Oops!";
                IsPrimaryButtonEnabled = false;
                Album.Text = "Cant't find it";
                Artist.Visibility = Visibility.Collapsed;
                Brief.Visibility = Visibility.Collapsed;
                Descriptions.Visibility = Visibility.Collapsed;
            }
            this.album = album;
            var songs = AsyncHelper.RunSync(async () => { return await album.GetSongsAsync(); });
            uint i = 0;
            foreach (var item in songs)
            {
                SongList.Add(new SongViewModel(item)
                {
                    Index = i++,
                });
            }
            Album.Text = album.Name;
            Artwork.Source = new BitmapImage(album.Artwork ?? new Uri(Consts.NowPlaceholder));
            Artist.Text = album.GetFormattedArtists();
            Brief.Text = album.GetBrief();
            if (album.Description.IsNullorEmpty())
            {
                var t = ThreadPool.RunAsync(async x =>
                {
                    var info = await MainPageViewModel.Current.GetAlbumInfoAsync(album.Name, album.AlbumArtists.FirstOrDefault());
                    await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        if (info != null)
                        {
                            if (album.Artwork == null && info.AltArtwork != null)
                            {
                                Artwork.Source = new BitmapImage(info.AltArtwork);
                                UpdateArtwork?.Invoke(this, info.AltArtwork);
                                var task = ThreadPool.RunAsync(async k =>
                                {
                                    if (!album.IsOnline)
                                    {
                                        await SQLOperator.Current().UpdateAlbumArtworkAsync(album.ID, info.AltArtwork.OriginalString);
                                    }
                                });
                            }
                            Descriptions.Text = info.Description;
                        }
                        else
                        {
                            Descriptions.Text = "# Local Album";
                        }
                    });
                });
            }
            else
            {
                Descriptions.Text = album.Description;
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            await MainPageViewModel.Current.InstantPlay(await album.GetSongsAsync());
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Touch)
            { return; }
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

        private async void PlayBtn_Click(object sender, RoutedEventArgs e)
        {
            await MainPageViewModel.Current.InstantPlay(await album.GetSongsAsync(), (int)((sender as FrameworkElement).DataContext as SongViewModel).Index);
        }

        private void DetailPanel_Click(object sender, RoutedEventArgs e)
        {
            if (DescriIndicator.Glyph == "\uE018")
            {
                DetailPanel.SetValue(Grid.ColumnProperty, 1);
                DetailPanel.SetValue(Grid.ColumnSpanProperty, 1);
                DescriIndicator.Glyph = "\uE09D";
                Descriptions.Height = 75;
            }
            else
            {
                DetailPanel.SetValue(Grid.ColumnProperty, 0);
                DetailPanel.SetValue(Grid.ColumnSpanProperty, 2);
                DescriIndicator.Glyph = "\uE018";
                Descriptions.Height = double.NaN;
            }
        }

        private async void Descriptions_LinkClicked(object sender, Microsoft.Toolkit.Uwp.UI.Controls.LinkClickedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(e.Link));
        }
    }
}
