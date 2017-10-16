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

        public static async Task Read(StorageFolder folder)
        {
            var opr = await SQLOperator.CurrentAsync();
            var files = await GetFilesAsync(folder);
            var count = (int)Math.Ceiling(files.Count / 500d);
            var list = new List<Task>();
            for (int i = 0; i < count; i++)
            {
                list.Add(ReadFileandSave(files.GetRange(i * count, count), opr));
            }
            await Task.WhenAll(list);
        }

        public static async Task ReadFileandSave(IEnumerable<StorageFile> files, SQLOperator opr)
        {

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
                    await opr.UpdateAsync(await Models.Song.Create(tagTemp.Tag, file.Path));
                }
            }
        }

        public static async Task<List<Song>> ReadTags()
        {
            var opr = await SQLOperator.CurrentAsync();

            return await opr.GetAllAsync<Song>();
        }
    }
}
