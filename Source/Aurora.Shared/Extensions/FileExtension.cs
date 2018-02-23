// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace Aurora.Shared.Extensions
{
    public static class FileExtension
    {
        public static async Task<List<StorageFile>> GetAllFilesAsync(this StorageFolder folder)
        {
            var list = new List<StorageFile>();
            list.AddRange(await folder.GetFilesAsync());
            var folders = await folder.GetFoldersAsync();
            if (!folders.IsNullorEmpty())
            {
                foreach (var item in folders)
                {
                    list.AddRange(await item.GetAllFilesAsync());
                }
            }
            return list;
        }

        public static async Task<ulong> FolderSize(this StorageFolder folder)
        {
            var options = new Windows.Storage.Search.QueryOptions
            {
                FolderDepth = Windows.Storage.Search.FolderDepth.Deep,
            };
            var query = folder.CreateFileQueryWithOptions(options);

            var files = await query.GetFilesAsync();
            ulong si = 0ul;
            foreach (var file in files)
            {
                si += (await file.GetBasicPropertiesAsync()).Size;
            }

            return si;
        }
    }
}
