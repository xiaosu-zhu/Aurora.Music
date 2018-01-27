// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Shared.Helpers;
using System;
using System.Runtime.Serialization;
using Windows.ApplicationModel.Resources;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace Aurora.Music.Core
{
    public static class Consts
    {
        public static StorageFolder ArtworkFolder = AsyncHelper.RunSync(async () =>
        {
            return await ApplicationData.Current.LocalFolder.CreateFolderAsync("Artworks", CreationCollisionOption.OpenIfExists);
        });
        
        public const string SONG = "SONG";

        public const string BlackPlaceholder = "ms-appx:///Assets/Images/placeholder_b.png";
        public const string NowPlaceholder = "ms-appx:///Assets/Images/now_placeholder.png";

        public const string UnknownArtists = "Unknown Artists";
        public const string UnknownAlbum = "Unknown Album";

        public const string NowPlayingPageInAnimation = "NOW_PLAYING_IN";

        public const string ArtistPageInAnimation = "ARTIST_PAGE_IN";
        public const string AlbumItemConnectedAnimation = "ALBUM_DETAIL_IN";

        public static readonly string[] FileTypes = { ".flac", ".wav", ".m4a", ".aac", ".mp3", ".wma" };

        public const string ExtensionContract = "Aurora.Music.Extensions";
        public const string AppUserModelId = "6727Aurora-ZXS.10476770C0EE5_fxqtv0574xgme!App";
        public const string PackageFamilyName = "6727Aurora-ZXS.10476770C0EE5_fxqtv0574xgme";

        public const string OnlineAddOnStoreID = "9N8LMDXLQQ8V";

        public const string ProductID = "9NBLGGH6JVDT";

        public const string Github = "https://github.com/pkzxs/Aurora.Music";

        private static ResourceLoader localizer = ResourceLoader.GetForViewIndependentUse();
        public static ResourceLoader Localizer
        {
            get => localizer;
        }

        public static string CommaSeparator => localizer.GetString("CommaSeparator");

        public static string UpdateNote => localizer.GetString("UpdateNote");

        public static string UpdateNoteTitle => localizer.GetString("UpdateNoteTitle");

        public const string ArraySeparator = "$|$";

        public static string GetHourString(this DateTime time)
        {
            if (time.Hour < 5)
            {
                return Localizer.GetString("MidnightText");
            }
            else if (time.Hour < 10)
            {
                return Localizer.GetString("MorningText");
            }
            else if (time.Hour < 14)
            {
                return Localizer.GetString("NoonText");
            }
            else if (time.Hour < 19)
            {
                return Localizer.GetString("AfternoonText");
            }
            else if (time.Hour < 23)
            {
                return Localizer.GetString("EveningText");
            }
            return Localizer.GetString("MidnightText");
        }
    }
}
