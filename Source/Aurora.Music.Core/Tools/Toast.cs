// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace Aurora.Music.Core.Tools
{
    public static class Toast
    {
        public static void SendPodcast(Podcast p)
        {
            var toastContent = new ToastContent()
            {
                Header = new ToastHeader("Podcast", string.Format(Consts.Localizer.GetString("PodcastToastTitle"), ""), null),
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
                                Text = string.Format(Consts.Localizer.GetString("PodcastToastDesc"), string.Join(Consts.CommaSeparator, p.Select(a=>a.Title)))
                            },
                            new AdaptiveGroup()
                            {
                                Children =
                                {
                                    new AdaptiveSubgroup()
                                    {
                                        Children =
                                        {
                                            new AdaptiveText()
                                            {
                                                Text = p[0].Title,
                                                HintStyle = AdaptiveTextStyle.Body
                                            },
                                            new AdaptiveText()
                                            {
                                                Text = p[0].Album.Replace('\r', ' ').Replace('\n', ' '),
                                                HintStyle = AdaptiveTextStyle.CaptionSubtle
                                            }
                                        }
                                    },
                                    new AdaptiveSubgroup()
                                    {
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
                Launch = $"Action=ShowPodcast&ID={p.ID}"
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
    }
}
