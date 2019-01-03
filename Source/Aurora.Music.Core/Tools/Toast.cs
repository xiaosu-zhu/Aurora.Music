// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Linq;
using Windows.UI.Notifications;

namespace Aurora.Music.Core.Tools
{
    public static class Toast
    {
        public static void SendPodcast(Podcast p)
        {
            var toastContent = new ToastContent()
            {
                Header = new ToastHeader("Podcast", Consts.Localizer.GetString("PodcastText"), "as-music:///library/podcast"),
                ActivationType = ToastActivationType.Foreground,
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = string.Format(Consts.Localizer.GetString("PodcastToastTitle"), p.Title),
                                HintMaxLines = 1
                            },
                            new AdaptiveText()
                            {
                                Text = string.Format(Consts.Localizer.GetString("PodcastToastDesc"), string.Join(Consts.CommaSeparator, p.Take(p.Count > 3 ? 3 : p.Count).Select(a=>a.Title)))
                            },
                            new AdaptiveGroup()
                            {
                                Children =
                                {
                                    new AdaptiveSubgroup()
                                    {
                                        HintWeight = 3,
                                        Children =
                                        {
                                            new AdaptiveText()
                                            {
                                                Text = p[0].Title,
                                                HintStyle = AdaptiveTextStyle.Body
                                            },
                                            new AdaptiveText()
                                            {
                                                Text = p[0].Album.Truncate(40),
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            }
                                        }
                                    },
                                    new AdaptiveSubgroup()
                                    {
                                        HintWeight = 1,
                                        Children =
                                        {
                                            new AdaptiveText()
                                            {
                                                Text = p.Count > 1 ? string.Format(Consts.Localizer.GetString("AndMore"), p.Count - 1) : string.Empty,
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        HeroImage = new ToastGenericHeroImage()
                        {
                            Source = p[0].PicturePath
                        },
                        AppLogoOverride = new ToastGenericAppLogo()
                        {
                            Source = p.HeroArtworks[0]
                        }
                    }
                },
                Launch = $"as-music:///library/podcast/id/{p.ID}"
            };

            // Create the toast notification
            var toastNotif = new ToastNotification(toastContent.GetXml())
            {
                Tag = p.ID.ToString()
            };
            // Remove old (if have)
            ToastNotificationManager.History.Remove(p.ID.ToString());
            // And send the notification
            ToastNotificationManager.CreateToastNotifier().Show(toastNotif);
        }

        public static ToastNotification GetDownload(DownloadDesc des)
        {
            var toastContent = new ToastContent()
            {
                Visual = new ToastVisual()
                {
                    BindingGeneric = new ToastBindingGeneric()
                    {
                        Children =
                        {
                            new AdaptiveText()
                            {
                                Text = $"{des.Title} - {des.Description} has downloaded"
                            }
                        },
                        Attribution = new ToastGenericAttributionText()
                        {
                            Text = "Aurora Music"
                        }
                    }
                }
            };

            // Create the toast notification
            return new ToastNotification(toastContent.GetXml())
            {
                Tag = des.Guid.ToString()
            };
        }

        public static void SendDownload(DownloadDesc des)
        {
            var toast = GetDownload(des);
            // Remove old (if have)
            ToastNotificationManager.History.Remove(toast.Tag);
            // And send the notification
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }
    }
}
