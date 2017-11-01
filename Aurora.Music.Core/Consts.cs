using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Aurora.Music.Core
{
    public static class Consts
    {
        public static StorageFolder ArtworkFolder = AsyncHelper.RunSync(async () =>
        {
            return await ApplicationData.Current.LocalFolder.CreateFolderAsync("Artworks", CreationCollisionOption.OpenIfExists);
        });

        public const string ArtistPageInAnimation = "ARTIST_PAGE_IN";
    }
}
