// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Shared.Helpers;
using System;
using Windows.ApplicationModel.Resources;
using Windows.Storage;

namespace Aurora.Music.Core
{
    public enum SleepAction { Pause, Stop, Shutdown }
    public static partial class Consts
    {
        public const uint SpectrumBarCount = 16;

        public static StorageFolder ArtworkFolder = AsyncHelper.RunSync(async () =>
        {
            return await ApplicationData.Current.LocalFolder.CreateFolderAsync("Artworks", CreationCollisionOption.OpenIfExists);
        });

        public const string SONG = "SONG";

        public const string PodcastTaskName = "Aurora Music Podcasts Fetcher";

        public const string BlackPlaceholder = "ms-appx:///Assets/Images/placeholder_b.png";
        public const string NowPlaceholder = "ms-appx:///Assets/Images/now_placeholder.png";

        public const string UnknownArtists = "Unknown Artists";
        public const string UnknownAlbum = "Unknown Album";

        public const string NowPlayingPageInAnimation = "NOW_PLAYING_IN";

        public const string ArtistPageInAnimation = "ARTIST_PAGE_IN";
        public const string AlbumItemConnectedAnimation = "ALBUM_DETAIL_IN";

        public static readonly string[] FileTypes = { ".flac", ".wav", ".m4a", ".aac", ".mp3", ".wma" };
        public static readonly string[] PlaylistType = { "m3u", ".m3u8", ".wpl", ".zpl" };

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

        private static string commaSeparator = localizer.GetString("CommaSeparator");
        public static string CommaSeparator => commaSeparator;

        public static string UpdateNote =>
                            "### We are going to do a UI/UX renewal, please head to [UI/UX Renewal](https://github.com/pkzxs/Aurora.Music/issues/26) and leave your comment!\r\n---\r\n" +
                            "Now, with April 2018 Update, we have a basic cross-device experience. when you play online songs or podcasts, the app can push them to the roaming storage, and next time you tap the card in Timeline, you can then restore what you listened last time in other devices.";

        public static string UpdateNoteTitle => localizer.GetString("UpdateNoteTitle");

        private static string today = localizer.GetString("TodayText");
        public static string Today => today;
        private static string next = localizer.GetString("NextDayText");
        public static string Next => next;
        private static string last = localizer.GetString("LastDayText");
        public static string Last => last;

        public const string OPMLTemplate = "OPMLTemplate.xml";

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
