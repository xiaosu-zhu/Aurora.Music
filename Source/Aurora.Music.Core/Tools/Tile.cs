using Aurora.Music.Core.Models;
using Aurora.Shared.Extensions;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace Aurora.Music.Core.Tools
{
    public static class Tile
    {
        public static void SendNormal(string title, string album, string artist, string image)
        {
            if (image.IsNullorEmpty())
                image = Consts.BlackPlaceholder;

            var tileContent = new TileContent()
            {
                Visual = new TileVisual()
                {
                    Branding = TileBranding.NameAndLogo,
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            TextStacking = TileTextStacking.Center,
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = title,
                                    HintStyle = AdaptiveTextStyle.Caption,
                                    HintWrap = true,
                                    HintAlign = AdaptiveTextAlign.Center
                                },
                                new AdaptiveText()
                                {
                                    Text = string.Format(Consts.Localizer.GetString("TileDesc"), album, artist),
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true,
                                    HintMaxLines = 2,
                                    HintAlign = AdaptiveTextAlign.Center
                                }
                            },
                            PeekImage = new TilePeekImage()
                            {
                                Source = image
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            TextStacking = TileTextStacking.Center,
                            Children =
                            {
                                new AdaptiveGroup()
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup()
                                        {
                                            HintWeight = 33,
                                            Children =
                                            {
                                                new AdaptiveImage()
                                                {
                                                    Source = image
                                                }
                                            }
                                        },
                                        new AdaptiveSubgroup()
                                        {
                                            Children =
                                            {
                                                new AdaptiveText()
                                                {
                                                    Text = title,
                                                    HintStyle = AdaptiveTextStyle.Base
                                                },
                                                new AdaptiveText()
                                                {
                                                    Text = album,
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                    HintMaxLines = 1
                                                },
                                                new AdaptiveText()
                                                {
                                                    Text = artist,
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                    HintMaxLines = 1
                                                }
                                            },
                                            HintTextStacking = AdaptiveSubgroupTextStacking.Center
                                        }
                                    }
                                }
                            },
                            PeekImage = new TilePeekImage()
                            {
                                Source = image
                            }
                        }
                    },
                    TileLarge = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            TextStacking = TileTextStacking.Center,
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = title,
                                    HintStyle = AdaptiveTextStyle.Base,
                                    HintWrap = true,
                                    HintAlign = AdaptiveTextAlign.Center
                                },
                                new AdaptiveText()
                                {
                                    Text = album,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintMaxLines = 1,
                                    HintAlign = AdaptiveTextAlign.Center
                                },
                                new AdaptiveText()
                                {
                                    Text = artist,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintMaxLines = 1,
                                    HintAlign = AdaptiveTextAlign.Center
                                },
                                new AdaptiveImage()
                                {
                                    Source = image
                                }
                            },
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = image,
                                HintOverlay = 75
                            }
                        }
                    }
                }
            };
            // Create the tile notification
            var tileNotif = new TileNotification(tileContent.GetXml());

            // And send the notification to the primary tile
            try
            {
                TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
            }
            catch (Exception)
            {

            }
        }

        public static void UpdatePodcast(string id, Podcast p)
        {
            // get newest 5 podcasts
            var preparetoShow = p.Count >= 5 ? (p.SortRevert ? p.TakeLast(5) : p.Take(5)) : p;
            var tileList = new List<TileContent>();
            foreach (var item in preparetoShow)
            {
                var tileContent = new TileContent()
                {
                    Visual = new TileVisual()
                    {
                        Branding = TileBranding.NameAndLogo,
                        DisplayName = p.Title,
                        TileSmall = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = p.HeroArtworks.FirstOrDefault()
                                },
                                PeekImage = new TilePeekImage()
                                {
                                    Source = item.PicturePath
                                }
                            }
                        },
                        TileMedium = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                TextStacking = TileTextStacking.Center,
                                Children =
                                {
                                    new AdaptiveText()
                                    {
                                        Text = item.Title,
                                        HintStyle = AdaptiveTextStyle.Caption,
                                        HintWrap = true,
                                        HintMaxLines = 1,
                                        HintAlign = AdaptiveTextAlign.Center
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = item.PubDate.PubDatetoString($"'{Consts.Today}'", "ddd", "M/dd ddd", "yy/M/dd", Consts.Next, Consts.Last),
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintAlign = AdaptiveTextAlign.Center
                                    }
                                },
                                BackgroundImage = new TileBackgroundImage()
                                {
                                    Source = p.HeroArtworks.FirstOrDefault(),
                                    HintOverlay = 75
                                },
                                PeekImage = new TilePeekImage()
                                {
                                    Source = item.PicturePath
                                }
                            }
                        },
                        TileWide = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                TextStacking = TileTextStacking.Center,
                                Children =
                                {
                                    new AdaptiveGroup()
                                    {
                                        Children =
                                        {
                                            new AdaptiveSubgroup()
                                            {
                                                HintWeight = 1,
                                                Children =
                                                {
                                                    new AdaptiveImage()
                                                    {
                                                        Source = item.PicturePath
                                                    }
                                                }
                                            },
                                            new AdaptiveSubgroup()
                                            {
                                                HintWeight = 2,
                                                Children =
                                                {
                                                    new AdaptiveText()
                                                    {
                                                        Text = item.Title,
                                                        HintStyle = AdaptiveTextStyle.Base,
                                                        HintMaxLines = 2,
                                                        HintWrap = true,
                                                        HintAlign = AdaptiveTextAlign.Left
                                                    },
                                                    new AdaptiveText()
                                                    {
                                                        Text = item.PubDate.PubDatetoString($"'{Consts.Today}'", "ddd", "M/dd ddd", "yy/M/dd", Consts.Next, Consts.Last),
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                        HintAlign = AdaptiveTextAlign.Left
                                                    },
                                                    new AdaptiveText()
                                                    {
                                                        Text = item.Album.Truncate(40),
                                                        HintStyle = AdaptiveTextStyle.Caption,
                                                        HintWrap = true,
                                                        HintMaxLines = 2,
                                                        HintAlign = AdaptiveTextAlign.Left
                                                    }
                                                },
                                                HintTextStacking = AdaptiveSubgroupTextStacking.Center
                                            }
                                        }
                                    }
                                },
                                PeekImage = new TilePeekImage()
                                {
                                    Source = p.HeroArtworks.FirstOrDefault()
                                }
                            }
                        },
                        TileLarge = new TileBinding()
                        {
                            Content = new TileBindingContentAdaptive()
                            {
                                TextStacking = TileTextStacking.Center,
                                Children =
                                {
                                    new AdaptiveGroup()
                                    {
                                        Children =
                                        {
                                            new AdaptiveSubgroup()
                                            {
                                                HintWeight = 1
                                            },
                                            new AdaptiveSubgroup()
                                            {
                                                HintWeight = 2,
                                                Children =
                                                {
                                                    new AdaptiveImage()
                                                    {
                                                        Source = item.PicturePath
                                                    }
                                                }
                                            },
                                            new AdaptiveSubgroup()
                                            {
                                                HintWeight = 1
                                            }
                                        }
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = item.Title,
                                        HintStyle = AdaptiveTextStyle.Base,
                                        HintMaxLines = 2,
                                        HintWrap = true,
                                        HintAlign = AdaptiveTextAlign.Center
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = item.PubDate.PubDatetoString($"'{Consts.Today}'", "ddd", "M/dd ddd", "yy/M/dd", Consts.Next, Consts.Last),
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                        HintAlign = AdaptiveTextAlign.Center
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = item.Album.Truncate(40),
                                        HintStyle = AdaptiveTextStyle.Caption,
                                        HintWrap = true,
                                        HintMaxLines = 3,
                                        HintAlign = AdaptiveTextAlign.Center
                                    }
                                },
                                PeekImage = new TilePeekImage()
                                {
                                    Source = p.HeroArtworks.FirstOrDefault()
                                }
                            }
                        }
                    }
                };
                tileList.Add(tileContent);
            }

            // If the secondary tile is pinned
            if (SecondaryTile.Exists(id))
            {
                // Get its updater
                var updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(id);

                // Clear all old tile
                var old = updater.GetScheduledTileNotifications();
                foreach (var o in old)
                {
                    updater.RemoveFromSchedule(o);
                }

                updater.EnableNotificationQueue(true);
                // using above to create new tile
                foreach (var item in tileList)
                {
                    var noti = new ScheduledTileNotification(item.GetXml(), DateTime.Now.AddSeconds(5));
                    // And send the notification
                    updater.AddToSchedule(noti);
                }
            }
        }

        public static void UpdateImage(string id, IList<string> images, string title, string desc)
        {
            var photosBinding = new TileBindingContentPhotos();
            foreach (var item in images)
            {
                photosBinding.Images.Add(new TileBasicImage()
                {
                    Source = item
                });
            }
            var imageTile = new TileContent()
            {
                Visual = new TileVisual()
                {
                    Branding = TileBranding.NameAndLogo,
                    DisplayName = Consts.Localizer.GetString("AppNameText"),
                    TileMedium = new TileBinding()
                    {
                        Content = photosBinding
                    },
                    TileLarge = new TileBinding()
                    {
                        Content = photosBinding
                    },
                    TileWide = new TileBinding()
                    {
                        Content = photosBinding
                    }
                }
            };



            var tileContent = new TileContent()
            {
                Visual = new TileVisual()
                {
                    DisplayName = Consts.Localizer.GetString("AppNameText"),
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            TextStacking = TileTextStacking.Center,
                            Children =
                            {
                                new AdaptiveText()
                                {
                                    Text = title,
                                    HintStyle = AdaptiveTextStyle.Caption,
                                    HintWrap = true,
                                    HintMaxLines = 2,
                                    HintAlign = AdaptiveTextAlign.Center
                                },
                                new AdaptiveText()
                                {
                                    Text = desc,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true,
                                    HintAlign = AdaptiveTextAlign.Center
                                }
                            }
                        }
                    },
                    TileWide = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            TextStacking = TileTextStacking.Center,
                            Children =
                            {
                                new AdaptiveGroup()
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup()
                                        {
                                            HintWeight = 1,
                                            Children =
                                            {
                                                new AdaptiveImage()
                                                {
                                                    Source = images[0]
                                                }
                                            }
                                        },
                                        new AdaptiveSubgroup()
                                        {
                                            HintWeight = 2,
                                            HintTextStacking = AdaptiveSubgroupTextStacking.Center,
                                            Children =
                                            {
                                                new AdaptiveText()
                                                {
                                                    Text = title,
                                                    HintStyle = AdaptiveTextStyle.Base,
                                                    HintWrap = true,
                                                    HintMaxLines = 2
                                                },
                                                new AdaptiveText()
                                                {
                                                    Text = desc,
                                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                                    HintWrap = true
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    TileLarge = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            TextStacking = TileTextStacking.Center,
                            Children =
                            {
                                new AdaptiveGroup()
                                {
                                    Children =
                                    {
                                        new AdaptiveSubgroup()
                                        {
                                            HintWeight = 1
                                        },
                                        new AdaptiveSubgroup()
                                        {
                                            HintWeight = 2,
                                            Children =
                                            {
                                                new AdaptiveImage()
                                                {
                                                    Source = images[0]
                                                }
                                            }
                                        },
                                        new AdaptiveSubgroup()
                                        {
                                            HintWeight = 1
                                        }
                                    }
                                },
                                new AdaptiveText()
                                {
                                    Text = title,
                                    HintStyle = AdaptiveTextStyle.Base,
                                    HintMaxLines = 2,
                                    HintWrap = true,
                                    HintAlign = AdaptiveTextAlign.Center
                                },
                                new AdaptiveText()
                                {
                                    Text = desc,
                                    HintStyle = AdaptiveTextStyle.CaptionSubtle,
                                    HintWrap = true,
                                    HintAlign = AdaptiveTextAlign.Center
                                }
                            }
                        }
                    }
                }
            };

            if (SecondaryTile.Exists(id))
            {
                // Get its updater
                var updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(id);

                // Clear all old tile
                var old = updater.GetScheduledTileNotifications();
                foreach (var o in old)
                {
                    updater.RemoveFromSchedule(o);
                }

                updater.EnableNotificationQueue(true);

                var noti = new ScheduledTileNotification(imageTile.GetXml(), DateTime.Now.AddSeconds(5));
                // And send the notification
                updater.AddToSchedule(noti);


                noti = new ScheduledTileNotification(tileContent.GetXml(), DateTime.Now.AddSeconds(5));
                // And send the notification
                updater.AddToSchedule(noti);
            }
        }
    }
}
