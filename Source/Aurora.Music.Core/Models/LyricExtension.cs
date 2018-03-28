// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppExtensions;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;
using Windows.Storage;

namespace Aurora.Music.Core.Models
{
    public class LyricExtension : Extension
    {
        public LyricExtension(AppExtension ext, IPropertySet properties) : base(ext, properties)
        {

        }

        public async Task<string> GetLyricAsync(Song s, string online = null)
        {
            if (!s.IsOnline && !s.IsPodcast)
                try
                {
                    var lrcPath = Path.ChangeExtension(s.FilePath, ".lrc");
                    var lrcFile = await StorageFile.GetFileFromPathAsync(lrcPath);
                    var lrc = await FileIO.ReadTextAsync(lrcFile);
                    if (!string.IsNullOrWhiteSpace(lrc))
                        return lrc;
                }
                catch
                {
                }
            var args = new ValueSet()
            {
                new KeyValuePair<string, object>("q", "lyric"),
                new KeyValuePair<string, object>("title", s.Title),
                new KeyValuePair<string, object>("album", s.Album),
                new KeyValuePair<string, object>("artist", s.Performers.IsNullorEmpty() ? null : s.Performers[0]),
            };
            if (s.IsOnline)
            {
                args.Add("service", online ?? "Aurora.Music.Services");
                args.Add("ID", s.OnlineID);
            }
            return (await ExecuteAsync(args)) as string;
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
                            if (message.ContainsKey("status") && (int)message["status"] == 1)
                            {
                                return message["result"];
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
    }
}
