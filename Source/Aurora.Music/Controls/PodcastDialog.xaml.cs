// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;
using Aurora.Music.ViewModels;
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
                var pod = await Podcast.GetiTunesPodcast(g.Addtional);
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
                    if (pod.Subscribed)
                    {
                        PrimaryButtonText = "Undo Subscribe";
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
            var a = DateTime.Now;
            var k = (a - d);

            if (d.Year != d.Year)
            {
                return d.ToString("yy-M-dd ddd");
            }
            else
            {
                // use Date
                if (Math.Abs(k.TotalDays) > 7)
                {
                    return d.ToString("M-dd ddd");
                }
                // use day of week
                else
                {
                    if (d > a)
                    {
                        // this week
                        if (d.DayOfWeek > a.DayOfWeek)
                        {
                            return d.ToString("dddd");
                        }
                        // next week
                        else
                        {
                            return $"Next {d.ToString("dddd")}";
                        }
                    }
                    else
                    {
                        // last week
                        if (d.DayOfWeek > a.DayOfWeek)
                        {
                            return $"Last {d.ToString("dddd")}";
                        }
                        // this week
                        else
                        {
                            return d.ToString("dddd");
                        }
                    }
                }
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            podcast.Subscribed = !podcast.Subscribed;
            await podcast.SaveAsync();
        }
    }
}
