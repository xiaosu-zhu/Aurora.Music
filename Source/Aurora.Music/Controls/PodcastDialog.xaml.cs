// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class PodcastDialog : ContentDialog
    {
        private List<SongViewModel> Posts = new List<SongViewModel>();
        private Podcast podcast;

        public PodcastDialog(GenericMusicItemViewModel g)
        {
            InitializeComponent();
            TitleText.Text = g.Title;
            Author.Text = g.Description;
            Artwork.Source = new BitmapImage(g.Artwork);
            Task.Run(async () =>
            {
                try
                {
                    var pod = await Podcast.GetiTunesPodcast(g.OnlineAlbumID);
                    if (pod == null)
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            Description.Text = "This may because of un-support rss format or network error.";
                            TitleText.Text = "Fetch failed";
                            Author.Text = string.Empty;
                            FetchingHeader.Visibility = Visibility.Collapsed;
                            FetchingProgress.IsIndeterminate = false;
                            FetchingProgress.Visibility = Visibility.Collapsed;
                        });
                        return;
                    }
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
                }
                catch (Exception)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        Description.Text = "This may because of un-support rss format or network error.";
                        TitleText.Text = "Fetch failed";
                        Author.Text = string.Empty;
                        FetchingHeader.Visibility = Visibility.Collapsed;
                        FetchingProgress.IsIndeterminate = false;
                        FetchingProgress.Visibility = Visibility.Collapsed;
                    });
                }

            });
        }

        public PodcastDialog(Uri g)
        {
            InitializeComponent();
            Task.Run(async () =>
            {
                try
                {
                    var pod = await Podcast.GetiTunesPodcast(g.AbsoluteUri);
                    if (pod == null)
                    {
                        await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            Description.Text = "This may because of un-support rss format or network error.";
                            TitleText.Text = "Fetch failed";
                            Author.Text = string.Empty;
                            FetchingHeader.Visibility = Visibility.Collapsed;
                            FetchingProgress.IsIndeterminate = false;
                            FetchingProgress.Visibility = Visibility.Collapsed;
                        });
                        return;
                    }
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
                }
                catch (Exception)
                {
                    await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                    {
                        Description.Text = "This may because of un-support rss format or network error.";
                        TitleText.Text = "Fetch failed";
                        Author.Text = string.Empty;
                        FetchingHeader.Visibility = Visibility.Collapsed;
                        FetchingProgress.IsIndeterminate = false;
                        FetchingProgress.Visibility = Visibility.Collapsed;
                    });
                }
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
