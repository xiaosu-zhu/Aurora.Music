using Aurora.Music.Core;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace Aurora.Music.Controls
{
    static class Tile
    {
        public static void SendNormal(string title, string album, string artist, string image)
        {
            var tileContent = new TileContent()
            {
                Visual = new TileVisual()
                {
                    Branding = TileBranding.None,
                    TileMedium = new TileBinding()
                    {
                        Content = new TileBindingContentAdaptive()
                        {
                            Children =
                {
                    new AdaptiveText()
                    {
                        Text = title,
                        HintStyle = AdaptiveTextStyle.Caption,
                        HintWrap = true,
                        HintMaxLines = 2
                    },
                    new AdaptiveText()
                    {
                        Text = string.Format(Consts.Localizer.GetString("TileDesc"), album, artist),
                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                        HintWrap = true
                    }
                },
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = image,
                                HintOverlay = 80
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
                                        HintStyle = AdaptiveTextStyle.Caption,
                                        HintWrap = true,
                                        HintMaxLines = 2
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = album,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    },
                                    new AdaptiveText()
                                    {
                                        Text = artist,
                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                    }
                                }
                            }
                        }
                    }
                },
                            BackgroundImage = new TileBackgroundImage()
                            {
                                Source = image,
                                HintOverlay = 80
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
                            Children =
                {
                    new AdaptiveText()
                    {
                        Text = title,
                        HintStyle = AdaptiveTextStyle.Caption,
                        HintAlign = AdaptiveTextAlign.Center
                    },
                    new AdaptiveText()
                    {
                        Text = string.Format(Consts.Localizer.GetString("TileDesc"), album, artist),
                        HintStyle = AdaptiveTextStyle.CaptionSubtle,
                        HintWrap = true,
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
                                HintOverlay = 80
                            },
                            PeekImage = new TilePeekImage()
                            {
                                Source = image
                            }
                        }
                    }
                }
            };

            // Create the tile notification
            var tileNotif = new TileNotification(tileContent.GetXml());

            // And send the notification to the primary tile
            TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotif);
        }
    }
}
