// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Shared.Extensions;
using Aurora.Shared.MVVM;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.System.Threading;
using Windows.UI.Xaml.Media.Imaging;

namespace Aurora.Music.ViewModels
{
    class ArtistViewModel : ViewModelBase, IKey
    {
        public string RawName;

        private string description;
        public string Description
        {
            get { return description.IsNullorEmpty() ? Name : description; }
            set { SetProperty(ref description, value); }
        }

        private Uri avatar;
        public Uri Avatar
        {
            get { return avatar; }
            set
            {
                if (avatar?.OriginalString == value?.OriginalString)
                {
                    return;
                }
                SetProperty(ref avatar, value);
#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
                {
                    if (value == null)
                    {
                        AvatarImage = null;
                    }
                    else
                    {
                        AvatarImage = await ImageCache.Instance.GetFromCacheAsync(value);
                    }
                });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                var t = ThreadPool.RunAsync(async x =>
                {
                    await SQLOperator.Current().UpdateAvatarAsync(RawName, value.OriginalString);
                });
            }
        }

        private BitmapImage avatarImage;
        public BitmapImage AvatarImage
        {
            get { return avatarImage; }
            set { SetProperty(ref avatarImage, value); }
        }

        public bool NightModeEnabled { get; set; } = Settings.Current.NightMode;

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value.IsNullorWhiteSpace())
                {
                    SetProperty(ref name, Consts.UnknownArtists);
                    RawName = string.Empty;
                }
                else
                {
                    SetProperty(ref name, value.Replace(Consts.ArraySeparator, Consts.CommaSeparator));
                    RawName = value;
                }
            }
        }

        private int albumCount;
        public int SongsCount
        {
            get { return albumCount; }
            set { SetProperty(ref albumCount, value); }
        }

        public string Key
        {
            get
            {
                if (Name.IsNullorEmpty())
                {
                    return " ";
                }
                if (Name.StartsWith("The ", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    return Name.Substring(4);
                }
                if (Name.StartsWith("A ", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    return Name.Substring(2);
                }
                if (Name.StartsWith("An ", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    return Name.Substring(3);
                }
                return Name;

            }
        }

        public string CountToString(int count)
        {
            return SmartFormat.Smart.Format(Consts.Localizer.GetString("SmartSongs"), count);
        }

        internal async Task<IList<Song>> GetSongsAsync()
        {
            var opr = SQLOperator.Current();
            var albums = await opr.GetAlbumsOfArtistAsync(RawName);
            return await opr.GetSongsAsync(albums.SelectMany(s => s.Songs));
        }
    }
}
