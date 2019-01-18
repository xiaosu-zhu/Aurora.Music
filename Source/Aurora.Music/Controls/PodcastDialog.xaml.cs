// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;

using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
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
            RequestedTheme = Settings.Current.Theme;
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
                        await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
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
                    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
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
                        IsPrimaryButtonEnabled = true;
                        if (pod.Subscribed)
                        {
                            PrimaryButtonText = Consts.Localizer.GetString("PlayText");
                            DefaultButton = ContentDialogButton.Close;
                        }
                        else
                        {
                            DefaultButton = ContentDialogButton.Primary;
                        }
                    });
                }
                catch (Exception ex)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        Description.Text = "This may because of un-support rss format or network error.";
                        TitleText.Text = "Fetch failed";
                        Author.Text = string.Empty;
                        FetchingHeader.Visibility = Visibility.Collapsed;
                        FetchingProgress.IsIndeterminate = false;
                        FetchingProgress.Visibility = Visibility.Collapsed;
                    });
                    Core.Tools.Helper.Logging(ex, new Dictionary<string, string>()
                    {
                        ["Location"] = "PodcastDialog",
                        ["url"] = g.OnlineAlbumID
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
                        await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
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
                    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
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
                            PrimaryButtonText = Consts.Localizer.GetString("PlayText");
                            DefaultButton = ContentDialogButton.Close;
                        }
                        else
                        {
                            IsPrimaryButtonEnabled = true;
                        }
                    });
                }
                catch (Exception ex)
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                    {
                        Description.Text = "This may because of un-support rss format or network error.";
                        TitleText.Text = "Fetch failed";
                        Author.Text = string.Empty;
                        FetchingHeader.Visibility = Visibility.Collapsed;
                        FetchingProgress.IsIndeterminate = false;
                        FetchingProgress.Visibility = Visibility.Collapsed;
                    });
                    Core.Tools.Helper.Logging(ex, new Dictionary<string, string>()
                    {
                        ["Location"] = "PodcastDialog",
                        ["url"] = g.AbsoluteUri
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
            if (podcast.Subscribed)
            {
                if (podcast.Count < 1)
                {
                    MainPage.Current.PopMessage("No enough podcast");
                    return;
                }

                var def = args.GetDeferral();

                var i = 0;
                var s = podcast[i];
                if (s.IsVideo)
                {
                    var videoWindowID = 0;
                    await CoreApplication.CreateNewView().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        var frame = new Frame();
                        videoWindowID = ApplicationView.GetForCurrentView().Id;
                        frame.Navigate(typeof(VideoPodcast), s.FilePath);
                        Window.Current.Content = frame;
                        Window.Current.Activate();
                        ApplicationView.GetForCurrentView().Title = s.Title;
                        CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = true;
                        var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                        titleBar.ButtonBackgroundColor = Colors.Transparent;
                        titleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
                        titleBar.ButtonHoverBackgroundColor = Color.FromArgb(0x33, 0x00, 0x00, 0x00);
                        titleBar.ButtonForegroundColor = Colors.Black;
                        titleBar.ButtonHoverForegroundColor = Colors.White;
                        titleBar.ButtonInactiveForegroundColor = Colors.Gray;
                    });
                    bool viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(videoWindowID);
                    return;
                }
                if (podcast.Count < i + 20)
                {
                    var k = podcast.Count < 20 ? podcast.ToList() : podcast.GetRange(podcast.Count - 20, 20);
                    await MainPageViewModel.Current.InstantPlayAsync(k, k.IndexOf(s));
                }
                else
                {
                    await MainPageViewModel.Current.InstantPlayAsync(podcast.GetRange(i, 20), 0);
                }

                def.Complete();
            }
            else
            {
                podcast.Subscribed = true;
                await podcast.SaveAsync();
                MainPage.Current.PopMessage(string.Format(podcast.Subscribed ? Consts.Localizer.GetString("PodcastSubscribe") : Consts.Localizer.GetString("PodcastUnSubscribe"), podcast.Title));
            }
        }
    }
}
