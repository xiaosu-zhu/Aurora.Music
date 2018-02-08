// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Shared.Extensions;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppExtensions;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace Aurora.Music.Core.Models
{
    public class OnlineMetaExtension : Extension
    {
        public OnlineMetaExtension(AppExtension ext, IPropertySet properties) : base(ext, properties)
        {
        }

        public override async Task<object> ExecuteAsync(ValueSet parameters)
        {
            if (_serviceName.IsNullorEmpty())
            {
                throw new InvalidProgramException("Extension is not a service");
            }
            try
            {
                // do app service call
                using (var connection = new AppServiceConnection())
                {
                    // service name was in properties
                    connection.AppServiceName = _serviceName;

                    // package Family Name is in the extension
                    connection.PackageFamilyName = this.AppExtension.Package.Id.FamilyName;

                    // open connection
                    AppServiceConnectionStatus status = await connection.OpenAsync();
                    if (status != AppServiceConnectionStatus.Success)
                    {
                        throw new InvalidOperationException(status.ToString());
                    }
                    else
                    {
                        // send request to service
                        // get response
                        AppServiceResponse response = await connection.SendMessageAsync(parameters);
                        if (response.Status == AppServiceResponseStatus.Success)
                        {
                            ValueSet message = response.Message as ValueSet;
                            if (message.TryGetValue("status", out object stat) && (int)stat == 1)
                            {
                                if (message.ContainsKey("artist_result") && message["artist_result"] is string s)
                                {
                                    return GetArtistInfo(s);
                                }
                                if (message.ContainsKey("album_result") && message["album_result"] is string r)
                                {
                                    return GetAlbumInfo(r);
                                }
                            }
                        }
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private object GetAlbumInfo(string r)
        {
            var set = JsonConvert.DeserializeObject<PropertySet>(r);
            return new AlbumInfo()
            {
                Name = set["name"] as string,
                AltArtwork = (set["artwork"] as string).IsNullorEmpty() ? null : new Uri(set["artwork"] as string),
                Description = set["desc"] as string,
                Artist = set["artist"] as string,
                Year = Convert.ToUInt32(set["year"]),
            };
        }

        private object GetArtistInfo(string s)
        {
            var set = JsonConvert.DeserializeObject<PropertySet>(s);
            return new Artist()
            {
                AvatarUri = (set["avatar"] as string).IsNullorEmpty() ? null : new Uri(set["avatar"] as string),
                Name = set["name"] as string,
                Description = set["desc"] as string
            };
        }
    }
}
