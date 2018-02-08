// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Shared.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Aurora.Music.Core.Tools
{
    public static class Helper
    {
        public static async Task<string> GetBuiltInArtworkAsync(string id, StorageFile file)
        {
            var options = new Windows.Storage.Search.QueryOptions(Windows.Storage.Search.CommonFileQuery.DefaultQuery, new string[] { ".jpg", ".png", ".bmp" })
            {
                ApplicationSearchFilter = $"System.FileName:{id}.*"
            };

            var query = ApplicationData.Current.TemporaryFolder.CreateFileQueryWithOptions(options);
            var files = await query.GetFilesAsync();
            if (id != "0" && files.Count > 0)
            {
                return files[0].Path;
            }
            else
            {
                using (var tag = TagLib.File.Create(file))
                {
                    var pictures = tag.Tag.Pictures;
                    if (!pictures.IsNullorEmpty())
                    {
                        var fileName = $"{id}.{pictures[0].MimeType.Split('/').LastOrDefault().Replace("jpeg", "jpg")}";
                        try
                        {
                            var s = await ApplicationData.Current.TemporaryFolder.GetFileAsync(fileName);
                            if (id == "0" || s == null)
                            {
                                StorageFile cacheImg = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                                await FileIO.WriteBytesAsync(cacheImg, pictures[0].Data.Data);
                                return cacheImg.Path;
                            }
                            else
                            {
                                return s.Path;
                            }
                        }
                        catch (FileNotFoundException)
                        {
                            StorageFile cacheImg = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
                            await FileIO.WriteBytesAsync(cacheImg, pictures[0].Data.Data);
                            return cacheImg.Path;
                        }
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
        }
    }
}
