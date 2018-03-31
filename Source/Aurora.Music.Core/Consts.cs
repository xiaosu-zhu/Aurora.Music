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
    public static class Consts
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

        private static string ommaSeparator = localizer.GetString("CommaSeparator");
        public static string CommaSeparator => ommaSeparator;

        public static string UpdateNote =>
                            "### Note: Thank you for supporting this app become better!\r\n\r\n---\r\n\r\n" +
                            "* Now, Aurora Music is one of four finalists in **Design Innovator** category of **[Windows Developer Awards](https://developer.microsoft.com/en-us/windows/projects/events/build/2018/awards)**. If you think our app is really excellent, please +1 for this app! Voting starts on April 2nd and ends on April 16th, We're appreciated for your votes!\r\n\r\n* 现在，极光音乐获得了 **[Windows Developer Awards](https://developer.microsoft.com/en-us/windows/projects/events/build/2018/awards)** 中的 **Design Innovator** 提名，如果你觉得这个播放器还不错，请在4月2日到4月16日之间到上面的网站投票，非常感谢您的支持！\r\n\r\n---\r\n\r\n" +
                            "* **Improve**: Merged pull requests [#6](https://github.com/pkzxs/Aurora.Music/pull/6) contributed by [OpportunityLiu](https://github.com/OpportunityLiu):\r\n\r\n" +
                           "\t1. Fixed searching items duplicated.\r\n" +
                            "\t2. Updated searching layout.\r\n" +
                            "* **Improve**: Now can play video podcasts(just a basic support).\r\n" +
                            "* **Improve**: Now can drag position slider in Mainpage.\r\n" +
                            "* **Improve**: Updated layouts.\r\n" +
                            "* **Improve**: Synced translations.\r\n" +
                            "* **Improve**: Bugs fixed.\r\n";

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
