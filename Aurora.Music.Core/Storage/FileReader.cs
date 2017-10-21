using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Shared.Helpers;
using TagLib;
using Windows.Storage;

namespace Aurora.Music.Core.Storage
{
    public static class FileReader
    {
        private static readonly string[] types = new[] { ".mp3", ".m4a", ".flac", ".wav" };

        public static async Task<List<StorageFile>> GetFilesAsync(StorageFolder folder)
        {
            var files = new List<StorageFile>();
            files.AddRange(await folder.GetFilesAsync());
            var folders = await folder.GetFoldersAsync();
            foreach (var item in folders)
            {
                files.AddRange(await GetFilesAsync(item));
            }
            return files;
        }

        public static async Task<List<Models.Album>> GetAlbumsAsync()
        {
            var opr = await SQLOperator.CurrentAsync();
            var albums = await opr.GetAllAsync<Album>();
            return albums.ConvertAll(a => new Models.Album(a));
        }

        public static async Task Read(StorageFolder folder)
        {
            var files = await GetFilesAsync(folder);
            await ReadFileandSave(files);
        }

        public static async Task ReadFileandSave(IEnumerable<StorageFile> files)
        {
            var opr = await SQLOperator.CurrentAsync();
            List<Song> tempList = new List<Song>();
            foreach (var file in files)
            {
                bool b = false;

                foreach (var item in types)
                {
                    if (item.Equals(file.FileType, StringComparison.InvariantCultureIgnoreCase))
                    {
                        b = true;
                        break;
                    }
                }
                if (!b)
                {
                    continue;
                }

                using (var tagTemp = File.Create(file.Path))
                {
                    await opr.UpdateAsync(await Models.Song.Create(tagTemp.Tag, file.Path));
                    //tempList.Add(new Song(await Models.Song.Create(tagTemp.Tag, file.Path)));
                }
            }
            //await opr.UpdateSongListAsync(tempList);
        }

        public static async Task<List<Models.Song>> GetSongsAsync()
        {
            var opr = await SQLOperator.CurrentAsync();
            var songs = await opr.GetAllAsync<Song>();
            return songs.ConvertAll(a => new Models.Song(a));
        }

        public static async Task AddToAlbums(IEnumerable<Song> songs)
        {
            await Task.Run(async () =>
            {
                var albums = from song in songs group song by song.Album into album select album;
                var opr = await SQLOperator.CurrentAsync();
                await opr.AddtoAlbumAsync(albums);
            });
        }

    }
}
