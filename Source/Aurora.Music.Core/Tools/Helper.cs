// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Models;
using Aurora.Shared.Extensions;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace Aurora.Music.Core.Tools
{
    public static class Helper
    {
        public static async Task<RandomAccessStreamReference> UpdateSongAsync(Song song, StorageFile file)
        {

            using (var tag = TagLib.File.Create(file))
            {
                await song.UpdateAsync(tag.Tag, tag.Properties, file);

                var pictures = tag.Tag.Pictures;
                if (!pictures.IsNullorEmpty())
                {
                    // Is this right?
                    var randomAccessStream = new InMemoryRandomAccessStream();
                    await randomAccessStream.WriteAsync(pictures[0].Data.Data.AsBuffer());
                    randomAccessStream.Seek(0);
                    return RandomAccessStreamReference.CreateFromStream(randomAccessStream);
                }
                else
                {
                    // Find if there is a Cover.jpg / Cover.png
                    try
                    {
                        var folder = await StorageFolder.GetFolderFromPathAsync(Path.GetDirectoryName(file.Path));
                        var result = folder.CreateFileQueryWithOptions(new Windows.Storage.Search.QueryOptions()
                        {
                            FolderDepth = Windows.Storage.Search.FolderDepth.Shallow,
                            ApplicationSearchFilter = "System.FileName:\"cover\" System.FileExtension:=(\".jpg\" OR \".png\" OR \".jpeg\" OR \".gif\" OR \".tiff\" OR \".bmp\")"
                        });
                        var files = await result.GetFilesAsync();
                        if (files.Count > 0)
                        {
                            return RandomAccessStreamReference.CreateFromFile(files[0]);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    catch
                    {
                        return null;
                    }
                }
            }
        }

        private static ulong CreateHash64(string str)
        {
            byte[] utf8 = System.Text.Encoding.UTF8.GetBytes(str);

            ulong value = (ulong)utf8.Length;
            for (int n = 0; n < utf8.Length; n++)
            {
                value += (ulong)utf8[n] << ((n * 5) % 56);
            }
            return value;
        }
    }
}
