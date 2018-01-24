using Aurora.Music.Controls;
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Models.Json;
using Aurora.Music.PlaybackEngine;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Controls;

namespace Aurora.Music.ViewModels
{
    class DoubanPageViewModel : ViewModelBase
    {
        public ObservableCollection<ChannelGroup> Channels { get; set; }
        private Uri artwork;
        public Uri Artwork
        {
            get { return artwork; }
            set { SetProperty(ref artwork, value); }
        }

        private string title;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set { SetProperty(ref description, value); }
        }

        public DoubanPageViewModel()
        {
            Channels = new ObservableCollection<ChannelGroup>();

            Task.Run(async () =>
            {
                await Init();
            });
        }

        internal void Detach()
        {
            Player.Current.StatusChanged -= Current_StatusChanged;
            Player.Current.PositionUpdated -= Current_PositionUpdated;
        }

        public DelegateCommand Delete
        {
            get => new DelegateCommand(() =>
            {

            });
        }

        public DelegateCommand PlayPause
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (IsPlaying is bool b)
                    {
                        if (b)
                        {
                            Player.Current?.Pause();
                        }
                        else
                        {
                            Player.Current?.Play();
                        }
                    }
                    else
                    {
                        Player.Current?.Play();
                    }
                });
            }
        }

        private bool? isPlaying;
        public bool? IsPlaying
        {
            get { return isPlaying; }
            set { SetProperty(ref isPlaying, value); }
        }

        public Symbol NullableBoolToSymbol(bool? b)
        {
            if (b is bool bb)
            {
                return bb ? Symbol.Pause : Symbol.Play;
            }
            return Symbol.Play;
        }

        public string NullableBoolToString(bool? b)
        {
            if (b is bool bb)
            {
                return bb ? Consts.Localizer.GetString("PauseText") : Consts.Localizer.GetString("PlayText");
            }
            return Consts.Localizer.GetString("PlayText");
        }

        public async Task Init()
        {
            var result = await ApiRequestHelper.HttpGet("https://api.douban.com/v2/fm/app_channels?alt=json&apikey=02646d3fb69a52ff072d47bf23cef8fd&app_name=radio_iphone&client=s%3Amobile%7Cy%3AiOS%2010.2%7Cf%3A115%7Cd%3Ab88146214e19b8a8244c9bc0e2789da68955234d%7Ce%3AiPhone7%2C1%7Cm%3Aappstore&douban_udid=b635779c65b816b13b330b68921c0f8edc049590&icon_cate=xlarge&udid=b88146214e19b8a8244c9bc0e2789da68955234d&version=115");
            var douban = JsonConvert.DeserializeObject<Douban>(result);

            Player.Current.StatusChanged += Current_StatusChanged;
            Player.Current.PositionUpdated += Current_PositionUpdated;

            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                foreach (var item in douban.groups)
                {
                    var g = new ChannelGroup()
                    {
                        Name = (item.group_name.IsNullorEmpty() ? (Settings.Current.DoubanToken.IsNullorEmpty() ? "Not Login" : Settings.Current.DoubanUserName) : item.group_name),
                        ID = item.group_id,
                    };
                    foreach (var c in item.chls)
                    {
                        g.Add(new ChannelViewModel()
                        {
                            Cover = new Uri(c.cover),
                            Description = c.intro,
                            Name = c.name,
                            ID = c.id,
                        });
                    }
                    Channels.Add(g);
                }
            });
        }

        private void Current_PositionUpdated(object sender, PositionUpdatedArgs e)
        {
            //throw new NotImplementedException();
        }

        private async void Current_StatusChanged(object sender, StatusChangedArgs e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                IsPlaying = Player.Current.IsPlaying;
                if (e.CurrentSong != null)
                {
                    Title = e.CurrentSong.Title;
                    Description = string.Format(Consts.Localizer.GetString("TileDesc"), e.CurrentSong.Album, string.Join(Consts.CommaSeparator, e.CurrentSong.Performers ?? new string[] { }));
                    Artwork = e.CurrentSong.PicturePath.IsNullorEmpty() ? null : new Uri(e.CurrentSong.PicturePath);
                }
            });
        }

        internal async void Switch(ChannelViewModel model)
        {
            if (model.ID == 0)
            {
                if (!Settings.Current.VerifyDoubanLogin())
                {
                    DoubanLogin d = new DoubanLogin();
                    var result = await d.ShowAsync();
                    if (result == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
                    {
                        model.Name = Settings.Current.DoubanUserName;
                    }
                    else
                    {
                        return;
                    }
                }

            }
            var liat = await model.RequestPlayListAsync();
            if (liat.r == 0)
            {
                await Player.Current.NewPlayList(liat.song.Select(a => new Core.Models.Song()
                {
                    Title = a.title,
                    Album = a.albumtitle,
                    OnlineUri = new Uri(a.url),
                    OnlineID = a.sid,
                    IsOnline = true,
                    PicturePath = a.picture,
                    Performers = a.singers.Select(s => s.name).ToArray(),
                    AlbumArtists = new string[] { a.artist },
                }).ToList());

                Player.Current.Play();
            }
        }
    }
}
