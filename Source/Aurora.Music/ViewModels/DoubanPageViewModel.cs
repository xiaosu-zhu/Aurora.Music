// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using AudioVisualizer;
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
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Aurora.Music.ViewModels
{
    enum ReportType
    {
        bye, end, @new, playing, skip, rate, unrate
    }
    class DoubanPageViewModel : ViewModelBase
    {
        Queue<string> playedQueue = new Queue<string>();

        public ObservableCollection<ChannelGroup> Channels { get; set; }
        private Uri artwork;
        public Uri Artwork
        {
            get { return artwork; }
            set { SetProperty(ref artwork, value); }
        }

        public bool NightModeEnabled { get; set; } = Settings.Current.NightMode;

        private string sid;
        private string title;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private List<Color> palette;
        public List<Color> Palette
        {
            get { return palette; }
            set { SetProperty(ref palette, value); }
        }

        private bool rateToggle;
        public bool RateToggle
        {
            get { return rateToggle; }
            set
            {
                Task.Run(async () =>
                {
                    var list = await Report(value ? ReportType.rate : ReportType.unrate, channel.ToString(), sid);
                    if (list.r == 0)
                    {
                        await PlaybackEngine.PlaybackEngine.Current.AddtoNextPlay(list.song.Select(a => new Core.Models.Song()
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

                        PlaybackEngine.PlaybackEngine.Current.Play();
                    }
                });

                SetProperty(ref rateToggle, value);
            }
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
            PlaybackEngine.PlaybackEngine.Current.ItemsChanged -= Current_StatusChanged;
            PlaybackEngine.PlaybackEngine.Current.PositionUpdated -= Current_PositionUpdated;
            PlaybackEngine.PlaybackEngine.Current.PlaybackStatusChanged -= Current_PlaybackStatusChanged;
            Palette?.Clear();
            Palette = null;
            Channels?.Clear();
            Channels = null;
        }

        private double olume = Settings.Current.PlayerVolume;
        public double Volume
        {
            get { return olume; }
            set
            {
                SetProperty(ref olume, value);
                Settings.Current.PlayerVolume = value;
                PlaybackEngine.PlaybackEngine.Current.ChangeVolume(value);
            }
        }

        public string VolumeToSymbol(double d)
        {
            if (d.AlmostEqualTo(0))
            {
                return "\uE198";
            }
            if (d < 33.333333)
            {
                return "\uE993";
            }
            if (d < 66.66666667)
            {
                return "\uE994";
            }
            return "\uE995";
        }

        public string VolumeToString(double d)
        {
            return d.ToString("0");
        }

        public DelegateCommand Delete
        {
            get => new DelegateCommand(async () =>
            {
                var list = await Report(ReportType.bye, channel.ToString(), sid);
                if (list.r == 0)
                {
                    await PlaybackEngine.PlaybackEngine.Current.NewPlayList(list.song.Select(a => new Core.Models.Song()
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

                    PlaybackEngine.PlaybackEngine.Current.Play();
                }
                else
                {
                    MainPage.Current.PopMessage($"Playing error: {list.err}");
                }
            });
        }

        public DelegateCommand Next
        {
            get => new DelegateCommand(async () =>
            {
                PlaybackEngine.PlaybackEngine.Current?.Next();
                if (lastProgress < 0.9)
                {
                    var list = await Report(ReportType.skip, channel.ToString(), sid);
                    if (list.r == 0)
                    {
                        await PlaybackEngine.PlaybackEngine.Current.AddtoNextPlay(list.song.Select(a => new Core.Models.Song()
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
                        if (PlaybackEngine.PlaybackEngine.Current?.IsPlaying == null || !(bool)PlaybackEngine.PlaybackEngine.Current?.IsPlaying)
                        {
                            PlaybackEngine.PlaybackEngine.Current?.Play();
                        }
                    }
                }

            });
        }

        async Task<playlist> Report(ReportType type, string channel, string sid = null)
        {
            var args = new Dictionary<string, string>()
            {
                ["channel"] = channel,
                ["from"] = "mainsite",
                ["pt"] = "0.0",
                ["kbps"] = "128",
                ["formats"] = "aac",
                ["alt"] = "json",
                ["app_name"] = "radio_iphone",
                ["apikey"] = "02646d3fb69a52ff072d47bf23cef8fd",
                ["client"] = "s:mobile|y:iOS 10.2|f:115|d:b88146214e19b8a8244c9bc0e2789da68955234d|e:iPhone7,1|m:appstore",
                ["client_id"] = "02646d3fb69a52ff072d47bf23cef8fd",
                ["icon_cate"] = "xlarge",
                ["udid"] = "b88146214e19b8a8244c9bc0e2789da68955234d",
                ["douban_udid"] = "b635779c65b816b13b330b68921c0f8edc049590",
                ["version"] = "100",
                //["type"] = "n"
            };

            switch (type)
            {
                case ReportType.bye:
                    args.Add("type", "b");
                    if (sid == null)
                        throw new ArgumentException("sid is null");
                    args.Add("sid", sid);
                    break;
                case ReportType.end:
                    args.Add("type", "e");
                    if (sid == null)
                        throw new ArgumentException("sid is null");
                    args.Add("sid", sid);
                    break;
                case ReportType.@new:
                    args.Add("type", "n");
                    break;
                case ReportType.playing:
                    args.Add("type", "p");
                    break;
                case ReportType.skip:
                    args.Add("type", "s");
                    if (sid == null)
                        throw new ArgumentException("sid is null");
                    args.Add("sid", sid);
                    break;
                case ReportType.rate:
                    args.Add("type", "r");
                    if (sid == null)
                        throw new ArgumentException("sid is null");
                    args.Add("sid", sid);
                    break;
                case ReportType.unrate:
                    args.Add("type", "u");
                    if (sid == null)
                        throw new ArgumentException("sid is null");
                    args.Add("sid", sid);
                    break;
                default:
                    throw new ArgumentException("ReportType mismatch");
            }

            string h = string.Empty;

            while (playedQueue.TryDequeue(out string songID))
            {
                h += $"sid:{songID}|";
            }
            if (!h.IsNullorEmpty())
            {
                h.Remove(h.Length - 1);
                args.Add("h", h);
            }

            Dictionary<string, string> addHeader = null;

            if (Settings.Current.VerifyDoubanLogin())
            {
                addHeader = new Dictionary<string, string>()
                {
                    ["Authorization"] = $"Bearer {Settings.Current.DoubanToken}"
                };
                args.Add("user_id", Settings.Current.DoubanUserID);
                args.Add("expire", Settings.Current.DoubanExpireTime.ToString("0"));
                args.Add("token", Settings.Current.DoubanToken);
            }

            var json = await ApiRequestHelper.HttpGet("https://api.douban.com/v2/fm/playlist", args, addHeader, true);
            if (!json.IsNullorEmpty())
            {
                return JsonConvert.DeserializeObject<playlist>(json);
            }
            return null;
        }

        public DelegateCommand PlayPause
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (sid == null)
                    {
                        Switch(Channels.First().First());
                        return;
                    }
                    if (IsPlaying is bool b)
                    {
                        if (b)
                        {
                            PlaybackEngine.PlaybackEngine.Current?.Pause();
                        }
                        else
                        {
                            PlaybackEngine.PlaybackEngine.Current?.Play();
                        }
                    }
                    else
                    {
                        PlaybackEngine.PlaybackEngine.Current?.Play();
                    }
                });
            }
        }

        private bool? isPlaying;
        private int channel;
        private double lastProgress;

        public bool? IsPlaying
        {
            get { return isPlaying; }
            set { SetProperty(ref isPlaying, value); }
        }

        private CustomVisualizer isualizer;
        public CustomVisualizer Visualizer
        {
            get => isualizer;
            set
            {
                isualizer = value;
                MainPageViewModel.Current.VisualizerSource.SourceChanged += VisualizerSource_SourceChanged;
                if (MainPageViewModel.Current.VisualizerSource.Source != null)
                {
                    isualizer.Source = MainPageViewModel.Current.VisualizerSource.Source;
                }
            }
        }



        private void VisualizerSource_SourceChanged(object sender, IVisualizationSource args)
        {
            if (isualizer != null)
                isualizer.Source = MainPageViewModel.Current.VisualizerSource.Source;
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

            PlaybackEngine.PlaybackEngine.Current.ItemsChanged += Current_StatusChanged;
            PlaybackEngine.PlaybackEngine.Current.PlaybackStatusChanged += Current_PlaybackStatusChanged;
            PlaybackEngine.PlaybackEngine.Current.PositionUpdated += Current_PositionUpdated;

            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                foreach (var item in douban.groups)
                {
                    var g = new ChannelGroup()
                    {
                        Name = (item.group_name.IsNullorEmpty() ? (Settings.Current.VerifyDoubanLogin() ? Settings.Current.DoubanUserName : Consts.Localizer.GetString("NotLogin")) : item.group_name),
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

        private async void Current_PlaybackStatusChanged(object sender, PlaybackStatusChangedArgs e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                IsPlaying = e.PlaybackStatus == Windows.Media.Playback.MediaPlaybackState.Playing;
                if (e.PlaybackStatus == Windows.Media.Playback.MediaPlaybackState.Playing)
                {
                    MainPageViewModel.Current.IsVisualizing = true;
                }
            });
        }

        private void Current_PositionUpdated(object sender, PositionUpdatedArgs e)
        {
            lastProgress = e.Current / e.Total;
        }

        private async void Current_StatusChanged(object sender, PlayingItemsChangedArgs e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                if (e.CurrentSong != null)
                {
                    if (e.CurrentSong.OnlineID != sid)
                    {
                        if (lastProgress >= 0.9)
                        {
                            var p = string.Copy(sid);
                            playedQueue.Enqueue(p);
                            Task.Run(async () =>
                            {
                                var list = await Report(ReportType.end, channel.ToString(), p);
                                if (list.r == 0)
                                {
                                    await PlaybackEngine.PlaybackEngine.Current.AddtoNextPlay(list.song.Select(a => new Core.Models.Song()
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

                                    if (PlaybackEngine.PlaybackEngine.Current?.IsPlaying == null || !(bool)PlaybackEngine.PlaybackEngine.Current?.IsPlaying)
                                    {
                                        PlaybackEngine.PlaybackEngine.Current?.Play();
                                    }
                                }
                            });
                        }

                        Task.Run(async () =>
                        {
                            try
                            {
                                var pal = await ImagingHelper.GetColorPalette(e.CurrentSong.PicturePath.IsNullorEmpty() ? new Uri(Consts.NowPlaceholder) : new Uri(e.CurrentSong.PicturePath));
                                await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                                {
                                    var list = new List<Color>();
                                    for (int i = 0; i < 32; i++)
                                    {
                                        int j = i;
                                        while (j >= pal.Count)
                                        {
                                            j -= pal.Count;
                                        }
                                        list.Add(pal[j]);
                                    }
                                    list.Shuffle();
                                    Palette = list;
                                });
                            }
                            catch (Exception)
                            {
                                var pal = await ImagingHelper.GetColorPalette(new Uri(Consts.NowPlaceholder));
                                await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                                {
                                    var list = new List<Color>();
                                    for (int i = 0; i < 32; i++)
                                    {
                                        int j = i;
                                        while (j >= pal.Count)
                                        {
                                            j -= pal.Count;
                                        }
                                        list.Add(pal[j]);
                                    }
                                    list.Shuffle();
                                    Palette = list;
                                });
                            }

                        });
                        rateToggle = false;
                        RaisePropertyChanged("RateToggle");

                        Title = e.CurrentSong.Title;
                        Description = string.Format(Consts.Localizer.GetString("TileDesc"), e.CurrentSong.Album, string.Join(Consts.CommaSeparator, e.CurrentSong.Performers ?? new string[] { }));
                        Artwork = e.CurrentSong.PicturePath.IsNullorEmpty() ? null : new Uri(e.CurrentSong.PicturePath);
                        sid = e.CurrentSong.OnlineID;

                    }
                }
                else
                {
                    if (IsPlaying is bool aaa && aaa)
                    {
                        Task.Run(async () =>
                        {
                            if (lastProgress >= 0.9)
                            {
                                var p = string.Copy(sid);
                                playedQueue.Enqueue(p);

                                var list = await Report(ReportType.end, channel.ToString(), p);
                                if (list.r == 0)
                                {
                                    await PlaybackEngine.PlaybackEngine.Current.AddtoNextPlay(list.song.Select(a => new Core.Models.Song()
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

                                    if (PlaybackEngine.PlaybackEngine.Current?.IsPlaying == null || !(bool)PlaybackEngine.PlaybackEngine.Current?.IsPlaying)
                                    {
                                        PlaybackEngine.PlaybackEngine.Current?.Play();
                                    }
                                }

                            }
                            else
                            {
                                var list = await Report(ReportType.skip, channel.ToString(), sid);
                                if (list.r == 0)
                                {
                                    await PlaybackEngine.PlaybackEngine.Current.AddtoNextPlay(list.song.Select(a => new Core.Models.Song()
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
                                    if (PlaybackEngine.PlaybackEngine.Current?.IsPlaying == null || !(bool)PlaybackEngine.PlaybackEngine.Current?.IsPlaying)
                                    {
                                        PlaybackEngine.PlaybackEngine.Current?.Play();
                                    }
                                }
                            }
                        });
                    }
                    else
                    {
                        MainPageViewModel.Current.IsVisualizing = false;
                    }
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
                    if (result == ContentDialogResult.Primary)
                    {
                        Channels.Where(a => a.Contains(model)).First().Name = Settings.Current.DoubanUserName;
                    }
                    else
                    {
                        return;
                    }
                }

            }
            channel = model.ID;
            var liat = await Report(ReportType.@new, model.ID.ToString());
            if (liat.r == 0)
            {
                await PlaybackEngine.PlaybackEngine.Current.NewPlayList(liat.song.Select(a => new Core.Models.Song()
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

                PlaybackEngine.PlaybackEngine.Current.Play();
            }
            else
            {
                MainPage.Current.PopMessage($"Playing error: {liat.err}");
            }
        }
    }
}
