using Aurora.Shared.Helpers;
using System;
using System.Runtime.Serialization;
using Windows.ApplicationModel.Resources;
using Windows.Storage;

namespace Aurora.Music.Core
{
    public static class Consts
    {
        public static StorageFolder ArtworkFolder = AsyncHelper.RunSync(async () =>
        {
            return await ApplicationData.Current.LocalFolder.CreateFolderAsync("Artworks", CreationCollisionOption.OpenIfExists);
        });

        public const string ID = "ID";

        public const string SONG = "SONG";

        public const string BlackPlaceholder = "ms-appx:///Assets/Images/placeholder_b.png";
        public const string NowPlaceholder = "ms-appx:///Assets/Images/now_placeholder.png";

        public const string Duration = "Duration";

        public const string Artwork = "Artwork";

        public const string UnknownArtists = "Unknown Artists";
        public const string UnknownAlbum = "Unknown Album";

        public const string NowPlayingPageInAnimation = "NOW_PLAYING_IN";

        public const string ArtistPageInAnimation = "ARTIST_PAGE_IN";
        public const string AlbumDetailPageInAnimation = "ALBUM_DETAIL_IN";

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
    }
}
