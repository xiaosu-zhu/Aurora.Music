// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Aurora.Shared.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Web.Http;

namespace Aurora.Shared.Helpers
{
    public static class BaiduRequestHelper
    {
        /// <summary>
        /// 发送带有 ApiKey 的 HTTP 请求
        /// </summary>
        /// <param name="url">请求的 URL</param>
        /// <param name="pars">请求的参数</param>
        /// <param name="apikey">百度 API Key</param>
        /// <returns>请求结果</returns>
        public static async Task<string> RequestWithKeyAsync(string url, string[] pars, string apikey)
        {
            var strURL = url;
            if (!pars.IsNullorEmpty())
            {
                strURL += '?';
                foreach (var param in pars)
                {
                    strURL += param + '&';
                }
                strURL = strURL.Remove(strURL.Length - 1);
            }
            try
            {
                WebRequest request;
                request = WebRequest.Create(strURL);
                request.Method = "GET";
                // 添加header
                request.Headers["apikey"] = apikey;
                WebResponse response;
                response = await request.GetResponseAsync();
                Stream s;
                s = response.GetResponseStream();
                string StrDate = "";
                string strValue = "";
                StreamReader Reader = new StreamReader(s, Encoding.UTF8);
                while ((StrDate = Reader.ReadLine()) != null)
                {
                    strValue += StrDate + "\r\n";
                }

                return strValue;
            }
            catch (System.Exception)
            {
                return null;
            }
        }
    }

    public static class ApiRequestHelper
    {
        ///// <summary>
        ///// 发送 HTTP 请求
        ///// </summary>
        ///// <param name="url">请求的 URL</param>
        ///// <param name="pars">请求的参数</param>
        ///// <param name="apikey">API Key</param>
        ///// <returns>请求结果</returns>
        //public static async Task<string> RequestIncludeKeyAsync(string url, string[] pars, string apikey)
        //{
        //    try
        //    {
        //        var strURL = url;
        //        if (!pars.IsNullorEmpty())
        //        {
        //            strURL += '?';
        //            foreach (var param in pars)
        //            {
        //                strURL += param + '&';
        //            }
        //            strURL += "key=" + apikey;
        //        }
        //        WebRequest request;
        //        request = WebRequest.Create(strURL);
        //        request.Method = "GET";
        //        WebResponse response;
        //        response = await request.GetResponseAsync();
        //        Stream s;
        //        s = response.GetResponseStream();
        //        string StrDate = "";
        //        string strValue = "";
        //        StreamReader Reader = new StreamReader(s, Encoding.UTF8);
        //        while ((StrDate = Reader.ReadLine()) != null)
        //        {
        //            strValue += StrDate + "\r\n";
        //        }
        //        return strValue;
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }

        //}

        //public static async Task<string> RequestWithFormattedUrlAsync(string requestUrl, params string[] v)
        //{
        //    try
        //    {
        //        WebRequest request;
        //        request = WebRequest.Create(string.Format(requestUrl, v));
        //        request.Method = "GET";
        //        WebResponse response;
        //        response = await request.GetResponseAsync();
        //        Stream s;
        //        s = response.GetResponseStream();
        //        string StrDate = "";
        //        string strValue = "";
        //        StreamReader Reader = new StreamReader(s, Encoding.UTF8);
        //        while ((StrDate = Reader.ReadLine()) != null)
        //        {
        //            strValue += StrDate + "\r\n";
        //        }
        //        return strValue;
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}

        public static async Task<string> HttpPostJson(string url, string postDataJson)
        {
            using (var httpClient = new HttpClient())
            {
                Uri requestUri = new Uri(url);
                try
                {
                    var content = new HttpStringContent(postDataJson, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
                    content.Headers.ContentType.MediaType = "application/json";
                    content.Headers.ContentType.CharSet = "utf-8";
                    using (HttpResponseMessage response = await httpClient.PostAsync(requestUri, content))
                    {
                        response.EnsureSuccessStatusCode();
                        var buffer = await response.Content.ReadAsBufferAsync();
                        var byteArray = buffer.ToArray();
                        return Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
                    }
                }
                catch (Exception)
                {
                    //Could not connect to server
                    //Use more specific exception handling, this is just an example
                    return null;
                }
            }
        }

        public static async Task<string> HttpGet(string url, IEnumerable<KeyValuePair<string, string>> getDataStr, string encode = "utf-8", CookieCollection cc = null)
        {
            try
            {
                if (!WebHelper.IsInternet())
                {
                    return null;
                }
                //Create an HTTP client object
                using (HttpClient httpClient = new HttpClient())
                {
                    //The safe way to add a header value is to use the TryParseAdd method and verify the return value is true,
                    //especially if the header value is coming from user input.

                    //header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
                    //if (!headers.UserAgent.TryParseAdd(header))
                    //{
                    //    throw new Exception("Invalid header value: " + header);
                    //}


                    if (getDataStr == null || getDataStr.Count() == 0)
                    {

                    }
                    else
                    {
                        url += $"?{string.Join("&", getDataStr.Select(x => $"{x.Key}={x.Value}"))}";
                    }

                    Uri requestUri = new Uri(url);

                    try
                    {
                        //Send the GET request asynchronously and retrieve the response as a string.
                        using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                        {
                            request.Headers.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 Edge/16.16299");
                            request.Headers.TryAppendWithoutValidation("Accept-Charset", "utf-8");
                            request.Headers.Connection.TryParseAdd("Keep-Alive");
                            request.Headers.AcceptEncoding.TryParseAdd("gzip, deflate, br");
                            using (HttpResponseMessage response = await httpClient.SendRequestAsync(request))
                            {
                                response.EnsureSuccessStatusCode();
                                var buffer = await response.Content.ReadAsBufferAsync();
                                var byteArray = buffer.ToArray();
                                return Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }
            }
            catch (WebException)
            {
                return null;
            }
        }


        public static async Task<string> HttpGet(string url, NameValueCollection getDataStr = null)
        {
            if (!WebHelper.IsInternet())
            {
                return null;
            }
            //Create an HTTP client object
            using (HttpClient httpClient = new HttpClient())
            {
                //The safe way to add a header value is to use the TryParseAdd method and verify the return value is true,
                //especially if the header value is coming from user input.

                //header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
                //if (!headers.UserAgent.TryParseAdd(header))
                //{
                //    throw new Exception("Invalid header value: " + header);
                //}


                if (getDataStr == null)
                {

                }
                else
                {
                    url += $"?{getDataStr.ToString()}";
                }

                Uri requestUri = new Uri(url);

                try
                {
                    //Send the GET request asynchronously and retrieve the response as a string.
                    using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                    {
                        request.Headers.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 Edge/16.16299");
                        request.Headers.TryAppendWithoutValidation("Accept-Charset", "utf-8");
                        request.Headers.Connection.TryParseAdd("Keep-Alive");
                        request.Headers.AcceptEncoding.TryParseAdd("gzip, deflate, br");
                        using (HttpResponseMessage response = await httpClient.SendRequestAsync(request))
                        {
                            response.EnsureSuccessStatusCode();
                            var buffer = await response.Content.ReadAsBufferAsync();
                            var byteArray = buffer.ToArray();
                            return Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
                        }
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }
    }

    public static class CaiyunRequestHelper
    {
        private static readonly string nowUrl = "https://api.caiyunapp.com/v2/";
        public static async Task<string> RequestNowWithKeyAsync(float lon, float lat, string key)
        {
            var strURL = nowUrl + key + '/' + lon.ToString("0.0000") + ',' + lat.ToString("0.0000") + "/realtime.json";
            WebRequest request;
            request = WebRequest.Create(strURL);
            request.Method = "GET";
            WebResponse response;
            response = await request.GetResponseAsync();
            Stream s;
            s = response.GetResponseStream();
            string StrDate = "";
            string strValue = "";
            StreamReader Reader = new StreamReader(s, Encoding.UTF8);
            while ((StrDate = Reader.ReadLine()) != null)
            {
                strValue += StrDate + "\r\n";
            }
            return strValue;
        }

        public static async Task<string> RequestForecastWithKeyAsync(float lon, float lat, string key)
        {
            var strURL = nowUrl + key + '/' + lon.ToString("0.0000") + ',' + lat.ToString("0.0000") + "/forecast.json";
            WebRequest request;
            request = WebRequest.Create(strURL);
            request.Method = "GET";
            WebResponse response;
            response = await request.GetResponseAsync();
            Stream s;
            s = response.GetResponseStream();
            string StrDate = "";
            string strValue = "";
            StreamReader Reader = new StreamReader(s, Encoding.UTF8);
            while ((StrDate = Reader.ReadLine()) != null)
            {
                strValue += StrDate + "\r\n";
            }
            return strValue;
        }
    }

    public static class WundergroundRequestHelper
    {
        private static readonly string url = "http://api.wunderground.com/api/{0}/geolookup/conditions/forecast/hourly/q";

        public static async Task<string> GeoLookup(string key, float lat, float lon)
        {
            var strURL = string.Format(url, key) + '/' + lat.ToString() + ',' + lon.ToString() + ".json";
            WebRequest request;
            request = WebRequest.Create(strURL);
            request.Method = "GET";
            WebResponse response;
            response = await request.GetResponseAsync();
            Stream s;
            s = response.GetResponseStream();
            string StrDate = "";
            string strValue = "";
            StreamReader Reader = new StreamReader(s, Encoding.UTF8);
            while ((StrDate = Reader.ReadLine()) != null)
            {
                strValue += StrDate + "\r\n";
            }
            return strValue;
        }

        public static async Task<string> GetResult(string key, string zmw)
        {
            var strURL = string.Format(url, key) + '/' + zmw + ".json";
            WebRequest request;
            request = WebRequest.Create(strURL);
            request.Method = "GET";
            WebResponse response;
            response = await request.GetResponseAsync();
            Stream s;
            s = response.GetResponseStream();
            string StrDate = "";
            string strValue = "";
            StreamReader Reader = new StreamReader(s, Encoding.UTF8);
            while ((StrDate = Reader.ReadLine()) != null)
            {
                strValue += StrDate + "\r\n";
            }
            return strValue;
        }
    }


    public static class WebHelper
    {
        public static bool IsInternet()
        {
            ConnectionProfile connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return internet;
        }

        public static async Task<IAsyncOperationWithProgress<HttpResponseMessage, HttpProgress>> PostAsync(Uri uri, StorageFile file)
        {
            using (HttpClient client = new HttpClient())
            {
                using (var content = new HttpBufferContent(await FileIOHelper.GetBuffer(file)))
                {
                    var response = client.PostAsync(uri, content);
                    return response;
                }
            }
        }

        public static async Task<IAsyncOperationWithProgress<DownloadOperation, DownloadOperation>> DownloadFileAsync(string name, Uri uri, StorageFolder folder = null)
        {
            var downloader = new BackgroundDownloader();
            if (folder == null)
            {
                folder = ApplicationData.Current.LocalFolder;
            }
            var file = await folder.CreateFileAsync(name, CreationCollisionOption.OpenIfExists);
            // Create a new download operation.
            var download = downloader.CreateDownload(uri, file);

            return download.StartAsync();
        }


        /// <summary>
        /// Creates HTTP POST request & uploads database to server. Author : Farhan Ghumra
        /// </summary>
        public static void UploadFilesToServer(Uri uri, Dictionary<string, string> data, string fileName, string fileContentType, byte[] fileData)
        {
            string boundary = "----------" + Guid.NewGuid().ToString();
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.ContentType = "multipart/form-data; boundary=" + boundary;
            httpWebRequest.Method = "POST";
            httpWebRequest.BeginGetRequestStream((result) =>
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)result.AsyncState;
                    using (Stream requestStream = request.EndGetRequestStream(result))
                    {
                        WriteMultipartForm(requestStream, boundary, data, fileName, fileContentType, fileData);
                    }
                    request.BeginGetResponse(async a =>
                    {
                        try
                        {
                            var response = request.EndGetResponse(a);
                            var responseStream = response.GetResponseStream();
                            using (var sr = new StreamReader(responseStream))
                            {
                                using (StreamReader streamReader = new StreamReader(response.GetResponseStream()))
                                {
                                    string responseString = streamReader.ReadToEnd();
                                    //responseString is depend upon your web service.
                                    if (responseString == "Success")
                                    {
                                        await FileIOHelper.DeleteLogAsync(fileName);
                                    }
                                    else
                                    {
                                    }
                                }
                            }
                        }
                        catch (Exception)
                        {

                        }
                    }, null);
                }
                catch (Exception)
                {

                }
            }, httpWebRequest);
        }

        /// <summary>
        /// Writes multi part HTTP POST request. Author : Farhan Ghumra
        /// </summary>
        private static void WriteMultipartForm(Stream s, string boundary, Dictionary<string, string> data, string fileName, string fileContentType, byte[] fileData)
        {
            /// The first boundary
            byte[] boundarybytes = Encoding.UTF8.GetBytes("--" + boundary + "\r\n");
            /// the last boundary.
            byte[] trailer = Encoding.UTF8.GetBytes("\r\n--" + boundary + "–-\r\n");
            /// the form data, properly formatted 
            /// Content-Disposition: form-data; name="text1" 
            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            /// the form-data file upload, properly formatted
            /// Content-Disposition: form-data; name="userfile1"; filename="E:/s" 
            /// Content-Type: application/octet-stream 
            string fileheaderTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\";\r\nContent-Type: {2}\r\n\r\n";

            /// Added to track if we need a CRLF or not.
            bool bNeedsCRLF = false;

            if (data != null)
            {
                foreach (string key in data.Keys)
                {
                    /// if we need to drop a CRLF, do that.
                    if (bNeedsCRLF)
                        FileIOHelper.WriteToStream(s, "\r\n");

                    /// Write the boundary.
                    FileIOHelper.WriteToStream(s, boundarybytes);

                    /// Write the key.
                    FileIOHelper.WriteToStream(s, string.Format(formdataTemplate, key, data[key]));
                    bNeedsCRLF = true;
                }
            }

            /// If we don't have keys, we don't need a crlf.
            if (bNeedsCRLF)
                FileIOHelper.WriteToStream(s, "\r\n");

            FileIOHelper.WriteToStream(s, boundarybytes);
            FileIOHelper.WriteToStream(s, string.Format(fileheaderTemplate, "file", fileName, fileContentType));
            /// Write the file data to the stream.
            FileIOHelper.WriteToStream(s, fileData);
            FileIOHelper.WriteToStream(s, trailer);
        }

    }
}
