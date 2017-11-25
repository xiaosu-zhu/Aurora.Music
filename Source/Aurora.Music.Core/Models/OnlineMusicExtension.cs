using Aurora.Shared.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppExtensions;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace Aurora.Music.Core.Models
{
    public class OnlineMusicExtension : Extension
    {
        public OnlineMusicExtension(AppExtension ext, IPropertySet properties) : base(ext, properties)
        {
        }

        public override async Task<object> ExecuteAsync(params KeyValuePair<string, object>[] parameters)
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
                        var request = new ValueSet
                        {
                        };

                        foreach (var item in parameters)
                        {
                            request.Add(item);
                        }

                        // get response
                        AppServiceResponse response = await connection.SendMessageAsync(request);
                        if (response.Status == AppServiceResponseStatus.Success)
                        {
                            ValueSet message = response.Message as ValueSet;
                            if (message.ContainsKey("status") && (int)message["status"] == 1)
                            {
                                if (message.ContainsKey("search_result") && message["search_result"] is string s)
                                {
                                    return GetGenericMusicItem(s);
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

        private IEnumerable<OnlineMusicItem> GetGenericMusicItem(string result)
        {
            var set = JsonConvert.DeserializeObject<PropertySet[]>(result);
            var res = new List<OnlineMusicItem>();
            foreach (var p in set)
            {
                MediaType t;
                switch (p["type"])
                {
                    case "song":
                        t = MediaType.Song;
                        break;
                    case "album":
                        t = MediaType.Album;
                        break;
                    case "artist":
                        t = MediaType.Artist;
                        break;
                    case "playlist":
                        t = MediaType.PlayList;
                        break;
                    default:
                        t = MediaType.Song;
                        break;
                }
                res.Add(new OnlineMusicItem(p["title"] as string, p["description"] as string, p["addtional"] as string, p["id"] as string[])
                {
                    InnerType = t,
                    PicturePath = p["picture_path"] as string,
                });
            }
            return res;
        }
    }
}
