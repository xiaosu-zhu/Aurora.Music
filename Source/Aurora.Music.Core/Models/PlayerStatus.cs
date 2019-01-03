// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core.Tools;
using Aurora.Shared.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Aurora.Music.Core.Models
{
    /// <summary>
    /// Save Player Status
    /// </summary>
    public class PlayerStatus
    {
        public List<Song> Songs { get; set; }
        public int Index { get; set; }
        public TimeSpan Position { get; set; }

        public PlayerStatus()
        {

        }

        public PlayerStatus(IEnumerable<Song> enumerable, int currentIndex, TimeSpan currentPosition)
        {
            Songs = enumerable.ToList();
            Index = currentIndex;
            Position = currentPosition;
        }

        public async Task SaveAsync()
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("CheckPoint", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(this));
            }
            catch (Exception)
            {

            }
        }

        public async Task RoamingSaveAsync()
        {
            try
            {
                if (await ApplicationData.Current.RoamingFolder.TryGetItemAsync("CheckPoint") is StorageFile old)
                {
                    await old.DeleteAsync();
                }
                // experiment how to sync
                var file = await ApplicationData.Current.RoamingFolder.CreateFileAsync("CheckPoint.txt", CreationCollisionOption.ReplaceExisting);
                // reduce size
                var json = JsonConvert.SerializeObject(this, Formatting.None,
                    new JsonSerializerSettings
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                    });
                await FileIO.WriteTextAsync(file, json);

                try
                {
                    ApplicationData.Current.RoamingSettings.Values["HighPriority"] = true;
                    var r = ProjectRome.Current;
                    var hasDevice = await r.WaitForFirstDeviceAsync();
                    if (hasDevice)
                    {
                        var romeQ = new ValueSet
                        {
                            { "q", "push" },
                            { "json", json }
                        };
                        await r.RequestRemoteResponseAsync(romeQ, true);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
            catch (Exception)
            {

            }
        }

        public static async Task<PlayerStatus> LoadAsync()
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("CheckPoint", CreationCollisionOption.OpenIfExists);
                var json = await FileIO.ReadTextAsync(file);
                if (json.IsNullorEmpty())
                {
                    return null;
                }
                return JsonConvert.DeserializeObject<PlayerStatus>(json);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static async Task ClearRoamingAsync()
        {
            try
            {
                if (await ApplicationData.Current.RoamingFolder.TryGetItemAsync("CheckPoint.txt") is StorageFile file)
                {
                    await file.DeleteAsync();
                }

                ApplicationData.Current.RoamingSettings.Values["HighPriority"] = false;
            }
            catch (Exception)
            {
            }
        }
    }
}
