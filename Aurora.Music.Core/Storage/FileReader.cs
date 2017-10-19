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

        public static async Task<List<Models.Album>> ReadAlbumsAsync()
        {
            var opr = await SQLOperator.CurrentAsync();
            var albums = await opr.GetAllAsync<Album>();
            return albums.ConvertAll(a => new Models.Album(a));
        }

        public static async Task Read(StorageFolder folder)
        {
            var opr = await SQLOperator.CurrentAsync();
            var files = await GetFilesAsync(folder);
            await ReadFileandSave(files, opr);
        }

        public static async Task ReadFileandSave(IEnumerable<StorageFile> files, SQLOperator opr)
        {
            List<Song> tempList = new List<Song>();
            foreach (var file in files)
            {
                bool b = false;

                foreach (var item in types)
                {
                    if (item.Equals(file.FileType, StringComparison.InvariantCultureIgnoreCase))
                    {
                        b = true;
                    }
                }
                if (!b)
                {
                    continue;
                }

                using (var tagTemp = File.Create(file.Path))
                {
                    tempList.Add(new Song(await Models.Song.Create(tagTemp.Tag, file.Path)));
                }
            }
            await opr.UpdateSongListAsync(tempList);
        }

        public static async Task<List<Song>> ReadTags()
        {
            var opr = await SQLOperator.CurrentAsync();

            return await opr.GetAllAsync<Song>();
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
