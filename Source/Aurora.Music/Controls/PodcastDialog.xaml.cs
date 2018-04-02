// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using System;
using System.Collections.Generic;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class PodcastDialog : ContentDialog
    {
        private List<SongViewModel> Posts = new List<SongViewModel>();
        private Podcast podcast;

        public PodcastDialog(GenericMusicItemViewModel g)
        {
            this.InitializeComponent();
            TitleText.Text = g.Title;
            Author.Text = g.Description;
            Artwork.Source = new BitmapImage(g.Artwork);
            Task.Run(async () =>
            {
                var pod = await Podcast.GetiTunesPodcast(g.OnlineAlbumID);
                podcast = pod;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    Description.Text = pod.Description;
                    TitleText.Text = pod.Title;
                    Author.Text = pod.Author;
                    Artwork.Source = new BitmapImage(new Uri(pod.HeroArtworks[0]));
                    Updated.Text = PubDatetoString(pod.LastUpdate);
                    Posts.AddRange(pod.Select(a => new SongViewModel(a)));
                    Posts.OrderByDescending(a => a.PubDate);
                    PodList.ItemsSource = Posts;
                    FetchingHeader.Visibility = Visibility.Collapsed;
                    FetchingProgress.IsIndeterminate = false;
                    FetchingProgress.Visibility = Visibility.Collapsed;
                    if (pod.Subscribed)
                    {
                        PrimaryButtonText = Consts.Localizer.GetString("UndoSubscribeText");
                        DefaultButton = ContentDialogButton.Close;
                    }
                    else
                    {
                    }
                    IsPrimaryButtonEnabled = true;
                });
            });
        }

        public PodcastDialog(Uri g)
        {
            this.InitializeComponent();
            Task.Run(async () =>
            {
                var pod = await Podcast.GetiTunesPodcast(g.AbsoluteUri);
                podcast = pod;
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    Description.Text = pod.Description;
                    TitleText.Text = pod.Title;
                    Author.Text = pod.Author;
                    Artwork.Source = new BitmapImage(new Uri(pod.HeroArtworks[0]));
                    Updated.Text = PubDatetoString(pod.LastUpdate);
                    Posts.AddRange(pod.Select(a => new SongViewModel(a)));
                    Posts.OrderByDescending(a => a.PubDate);
                    PodList.ItemsSource = Posts;
                    FetchingHeader.Visibility = Visibility.Collapsed;
                    FetchingProgress.IsIndeterminate = false;
                    FetchingProgress.Visibility = Visibility.Collapsed;
                    if (pod.Subscribed)
                    {
                        PrimaryButtonText = Consts.Localizer.GetString("UndoSubscribeText");
                        DefaultButton = ContentDialogButton.Close;
                    }
                    else
                    {
                        IsPrimaryButtonEnabled = true;
                    }
                });
            });
        }

        public string PubDatetoString(DateTime d)
        {
            return d.PubDatetoString($"'{Consts.Today}'", "ddd", "M/dd ddd", "yy/M/dd", Consts.Next, Consts.Last);
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            podcast.Subscribed = !podcast.Subscribed;
            await podcast.SaveAsync();
            MainPage.Current.PopMessage(string.Format(podcast.Subscribed ? Consts.Localizer.GetString("PodcastSubscribe") : Consts.Localizer.GetString("PodcastUnSubscribe"), podcast.Title));
        }
    }
}
