// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Com.Aurora.AuWeather.Shared;
using Aurora.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;

namespace Aurora.Shared.Helpers
{
    public class StorageFileComparer : IEqualityComparer<StorageFile>
    {
        public bool Equals(StorageFile x, StorageFile y)
        {
            return x.Path.Equals(y.Path);
        }

        public int GetHashCode(StorageFile obj)
        {
            return obj.GetHashCode();
        }
    }

    public class FileIOHelper
    {
        /// <summary>
        /// Convert UTF-8 encoding stream to string, sync
        /// </summary>
        /// <param name="stream">source</param>
        /// <returns></returns>
        public static string StreamToString(Stream src)
        {
            src.Position = 0;
            using (StreamReader reader = new StreamReader(src, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public static async Task AppendLogtoCacheAsync(string LOG, string name = "BGLOG")
        {
            var cache = ApplicationData.Current.LocalCacheFolder;
            var log = await cache.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            await FileIO.AppendTextAsync(log, DateTime.Now.ToString("G") + ":  " + LOG + Environment.NewLine);
        }

        /// <summary>
        /// Saves a SoftwareBitmap to the specified StorageFile
        /// </summary>
        /// <param name="bitmap">SoftwareBitmap to save</param>
        /// <param name="file">Target StorageFile to save to</param>
        /// <returns></returns>
        private static async Task SaveSoftwareBitmapAsync(SoftwareBitmap bitmap, StorageFile file)
        {
            using (var outputStream = await file.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, outputStream);

                // Grab the data from the SoftwareBitmap
                encoder.SetSoftwareBitmap(bitmap);
                await encoder.FlushAsync();
            }
        }



        public static async Task<StorageFile> CreateCacheFileAsync(string name)
        {
            var cache = ApplicationData.Current.LocalCacheFolder;
            var log = await cache.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
            await FileIO.AppendTextAsync(log, DateTime.Now.ToString("G") + Environment.NewLine);
            return log;
        }


        public static async Task<StorageFile> CreateWallPaperFileAsync(string name)
        {
            var local = ApplicationData.Current.LocalFolder;
            var folder = await local.CreateFolderAsync("WallPaper", CreationCollisionOption.OpenIfExists);
            var files = await folder.GetFilesAsync();
            var fs = files.ToArray();
            if (!fs.IsNullorEmpty())
            {
                foreach (var item in fs)
                {
                    await item.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
            }
            var file = await folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);
            //await FileIO.AppendTextAsync(file, "nimabi");
            return file;
        }

        public static async Task<StorageFile> AppendLogtoCacheAsync(CrashLog exception, string name = "crashLOG")
        {
            var cache = ApplicationData.Current.LocalCacheFolder;
            var log = await cache.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            var sb = new StringBuilder(DateTime.Now.ToString("G") + Environment.NewLine);
            var info = SystemInfoHelper.GetSystemInfo();
            foreach (var i in info)
            {
                sb.Append(i + Environment.NewLine);
            }
            sb.Append("Error Code = " + exception.HResult.ToHexString() + Environment.NewLine);
            sb.Append("Exception = " + exception.Exception + Environment.NewLine);
            sb.Append("Message = " + exception.Message + Environment.NewLine);
            sb.Append("StackTrace = " + exception.StackTrace + Environment.NewLine);
            sb.Append("Source = " + exception.Source + Environment.NewLine);
            await FileIO.AppendTextAsync(log, sb.ToString());
            return log;
        }


        public static async Task DeleteLogAsync(string fileName = "crashLOG")
        {
            try
            {
                var cache = ApplicationData.Current.LocalCacheFolder;
                var log = await cache.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
                if (log != null)
                {
                    await log.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
            }
            catch (Exception)
            {

            }
        }

        public static async Task<IReadOnlyList<StorageFile>> GetFilesFromAssetsAsync(string path)
        {
            StorageFolder installedLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;
            var assets = await installedLocation.GetFolderAsync("Assets");
            try
            {
                var folder = await assets.GetFolderAsync(path);
                var files = await folder.GetFilesAsync();
                return files;
            }
            catch (Exception)
            {
                return null;
            }

        }

        /// <summary>
        /// Returns byte array from StorageFile. Author : Farhan Ghumra
        /// </summary>
        public async static Task<byte[]> GetBytesAsync(StorageFile file)
        {
            byte[] fileBytes = null;
            using (var stream = await file.OpenReadAsync())
            {
                fileBytes = new byte[stream.Size];
                using (var reader = new DataReader(stream))
                {
                    await reader.LoadAsync((uint)stream.Size);
                    reader.ReadBytes(fileBytes);
                }
            }

            return fileBytes;
        }

        /// <summary>
        /// Writes string to stream. Author : Farhan Ghumra
        /// </summary>
        public static void WriteToStream(Stream s, string txt)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(txt);
            s.Write(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Writes byte array to stream. Author : Farhan Ghumra
        /// </summary>
        public static void WriteToStream(Stream s, byte[] bytes)
        {
            s.Write(bytes, 0, bytes.Length);
        }


        /// <summary>
        /// Convert UTF-8 encoding string to stream, sync
        /// </summary>
        /// <param name="src">source</param>
        /// <returns></returns>
        public static Stream StringToStream(string src)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(src);
            return new MemoryStream(byteArray);
        }

        public static async Task<List<KeyValuePair<Uri, string>>> GetThumbnailUrisFromAssetsAsync(string path)
        {
            var files = await GetFilesFromAssetsAsync(path);
            var restul = new List<KeyValuePair<Uri, string>>();
            if (files != null && files.Count > 0)
                foreach (var file in files)
                {
                    if (file.DisplayName.EndsWith("_t"))
                    {
                        restul.Add(new KeyValuePair<Uri, string>
                            (new Uri("ms-appx:///Assets/" + path.Replace("\\", "/") + '/' + file.Name),
                            (file.DisplayName.Remove(file.DisplayName.Length - 2)).Substring(3)));
                    }
                }
            return restul;
        }

        public static async Task<IReadOnlyList<StorageFile>> GetFilesFromLocalAsync(string path)
        {
            var local = ApplicationData.Current.LocalFolder;
            try
            {
                var folder = await local.GetFolderAsync(path);
                return await folder.GetFilesAsync();
            }
            catch (Exception)
            {
                return null;
            }

        }


        public static async Task<StorageFile> GetFileFromLocalAsync(string name)
        {
            var local = ApplicationData.Current.LocalFolder;
            try
            {
                return await local.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            }
            catch (Exception)
            {
                return null;
            }

        }

        public static async Task<Uri> GetFileUriFromLocalAsync(string path, string fileName)
        {
            var local = ApplicationData.Current.LocalFolder;
            try
            {
                var folder = await local.GetFolderAsync(path);
                var files = await folder.GetFilesAsync();
                var fs = files.ToArray();
                if (!fs.IsNullorEmpty())
                {
                    foreach (var item in fs)
                    {
                        if (item.DisplayName == fileName)
                        {
                            return new Uri("ms-appdata:///local/" + path.Replace("\\", "/") + '/' + item.Name);
                        }
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }


        public static async Task<Uri> SaveFiletoLocalAsync(string path, StorageFile file, string desiredName)
        {
            var local = ApplicationData.Current.LocalFolder;
            try
            {
                var folder = await local.CreateFolderAsync(path, CreationCollisionOption.OpenIfExists);
                await file.CopyAsync(folder, desiredName + file.FileType, NameCollisionOption.ReplaceExisting);
                return new Uri("ms-appdata:///local/" + path.Replace("\\", "/") + '/' + desiredName + file.FileType);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<StorageFile> SaveFiletoLocalAsync(StorageFile file, string desiredName)
        {
            var local = ApplicationData.Current.LocalFolder;
            try
            {
                return await file.CopyAsync(local, desiredName + file.FileType, NameCollisionOption.ReplaceExisting);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Find file from localfolder, and read it
        /// </summary>
        /// <param name="fileName">file total name</param>
        /// <returns></returns>
        public static async Task<string> ReadStringFromAssetsAsync(string fileName)
        {
            if (fileName == null)
                return null;
            var sFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/" + fileName));
            return await FileIO.ReadTextAsync(sFile);
        }

        /// <summary>
        /// 从安装目录读取文件，返回 byte[]
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<byte[]> ReadAllBytesFromInstallAsync(string fileName)
        {
            var uri = new Uri("ms-appx:///" + fileName);
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var buffer = await FileIO.ReadBufferAsync(file);

            return buffer.ToArray();
        }

        /// <summary>
        /// 将缓冲区写入存储目录（覆盖）
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="buffer">要存储的缓冲区</param>
        public static async Task SaveBuffertoStorageAsync(string fileName, IBuffer buffer)
        {
            if (fileName.IsNullorEmpty())
                throw new ArgumentException("fileName can't be null or empty");
            var storeFolder = ApplicationData.Current.LocalFolder;
            var file = await storeFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);

            await FileIO.WriteBufferAsync(file, buffer);
        }

        public static async Task<IBuffer> GetBuffer(StorageFile file)
        {
            return await FileIO.ReadBufferAsync(file);
        }

        /// <summary>
        /// 将文本写入存储目录（覆盖）
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static async Task SaveStringtoLocalAsync(string fileName, string content)
        {
            if (fileName.IsNullorEmpty())
                throw new ArgumentException("fileName can't be null or empty");
            var storeFolder = ApplicationData.Current.LocalFolder;
            var file = await storeFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, content);
        }

        /// <summary>
        /// 将文本追加到指定的文件，若不存在则新建此文件
        /// </summary>
        /// <param name="v"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        public static async Task AppendStringtoLocalAsync(string fileName, string str)
        {
            if (fileName.IsNullorEmpty())
                throw new ArgumentException("fileName can't be null or empty");
            var storeFolder = ApplicationData.Current.LocalFolder;
            var file = await storeFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.AppendTextAsync(file, str);
        }

        public static async Task RemoveLocalFilesWithKeywordAsync(string key)
        {
            if (key.IsNullorEmpty())
                throw new ArgumentException("Keyword can't be null or empty");
            var storeFolder = ApplicationData.Current.LocalFolder;
            var files = await storeFolder.GetFilesAsync();
            foreach (var file in files)
            {
                if (file.Name.Contains(key))
                {
                    await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
            }
        }


        public static async Task RemoveFileFromLocalAsync(string path, string key)
        {
            if (key.IsNullorEmpty())
                throw new ArgumentException("Keyword can't be null or empty");
            var storeFolder = ApplicationData.Current.LocalFolder;
            try
            {
                var f = await storeFolder.GetFolderAsync(path);
                var files = await f.GetFilesAsync();
                var sf = files.ToArray();
                if (!sf.IsNullorEmpty())
                {
                    foreach (var file in sf)
                    {
                        if (file.Name.Contains(key))
                        {
                            await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 从存储目录读取文本
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<IBuffer> ReadBufferFromLocalAsync(string fileName)
        {
            if (fileName == null)
                return null;
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await storageFolder.GetFileAsync(fileName);

            return await FileIO.ReadBufferAsync(file);
        }

        public static async Task<string> ReadStringFromLocalAsync(string fileName)
        {
            if (fileName == null)
                return null;
            var storageFolder = ApplicationData.Current.LocalFolder;
            var file = await storageFolder.TryGetItemAsync(fileName);
            if (file is StorageFile f)
            {
                return await FileIO.ReadTextAsync(f);
            }
            return null;
        }

        public static async Task<string> ReadLastFromLocalAsync(string fileName)
        {
            if (fileName == null)
                return null;
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists);
            return (await FileIO.ReadLinesAsync(file)).Last();
        }

        public static async Task<Uri> GetFileUriFromAssetsAsync(string path, int index, bool isShuffle = false)
        {
            try
            {
                var files = await GetFilesFromAssetsAsync(path);
                List<StorageFile> result = new List<StorageFile>();
                result.AddRange(files);
                for (int i = result.Count - 1; i > -1; i--)
                {
                    if ((result[i]).DisplayName.EndsWith("_t"))
                    {
                        result.RemoveAt(i);
                    }
                }
                if (isShuffle)
                {
                    index = Tools.Random.Next(result.Count);
                }
                return new Uri("ms-appx:///Assets/" + path.Replace("\\", "/") + '/' + result[index].Name);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task<StorageFile> GetFilebyUriAsync(Uri uri)
        {
            return await StorageFile.GetFileFromApplicationUriAsync(uri);
        }

        /// <summary>
        /// 从Assets读取Buffer
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static async Task<IBuffer> ReadBufferFromAssetsAsync(string fileName)
        {
            if (fileName == null)
                return null;
            StorageFile sFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/" + fileName));
            return await FileIO.ReadBufferAsync(sFile);
        }

        public static async Task<IRandomAccessStream> ReadRandomAccessStreamFromAssetsAsync(string fileName)
        {
            var uri = new Uri("ms-appx:///Assets/" + fileName);
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var stream = await file.OpenAsync(FileAccessMode.Read);
            stream.Seek(0);
            return stream;
        }

        public static async Task<IRandomAccessStream> ReadRandomAccessStreamByUriAsync(Uri uri)
        {
            var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            var stream = await file.OpenAsync(FileAccessMode.Read);
            stream.Seek(0);
            return stream;
        }

        public static async Task<IRandomAccessStream> ToIRandomAccessStreamAsync(byte[] bytestream)
        {

            InMemoryRandomAccessStream memoryStream = new InMemoryRandomAccessStream();

            DataWriter datawriter = new DataWriter(memoryStream.GetOutputStreamAt(0));

            datawriter.WriteBytes(bytestream);

            await datawriter.StoreAsync();

            memoryStream.Seek(0);

            return memoryStream;
        }


    }
}
