// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Web.Http;
using Windows.Web.Http.Filters;

namespace Aurora.Shared.Helpers
{
    //public static class BaiduRequestHelper
    //{
    // Use HttpGet();
    //}

    public static class ApiRequestHelper
    {
        /// <summary>
        /// See https://docs.microsoft.com/en-us/azure/architecture/antipatterns/improper-instantiation/
        /// </summary>
        private static readonly HttpClient httpClient = CreateHttpClient();

        /// <summary>
        /// resolve POST weird behavior 
        /// </summary>
        /// <returns></returns>
        private static HttpClient CreateHttpClient()
        {
            var httpClient = new HttpClient();

            // HttpClient functionality can be extended by plugging multiple filters together and providing
            // HttpClient with the configured filter pipeline.
            IHttpFilter filter = new HttpBaseProtocolFilter();
            httpClient = new HttpClient(filter);

            // The following line sets a "User-Agent" request header as a default header on the HttpClient instance.
            // Default headers will be sent with every request sent from this HttpClient instance.
            httpClient.DefaultRequestHeaders.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 Edge/16.16299");

            return httpClient;
        }

        public static async Task<string> HttpPostJson(string url, string postDataJson)
        {
            try
            {
                Uri requestUri = new Uri(url);
                var content = new HttpStringContent(postDataJson, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/json");
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

        public static async Task<string> HttpPostForm(string url, IEnumerable<KeyValuePair<string, string>> form, bool ignoreStatus = false)
        {
            try
            {
                Uri requestUri = new Uri(url);
                var content = new HttpFormUrlEncodedContent(form);
                using (var response = await httpClient.PostAsync(requestUri, content))
                {
                    if (ignoreStatus)
                    {

                    }
                    else
                    {
                        response.EnsureSuccessStatusCode();
                    }
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

        public static async Task<string> HttpGet(string url, IEnumerable<KeyValuePair<string, string>> getDataStr, IEnumerable<KeyValuePair<string, string>> addHeader = null, bool ignoreStatus = false)
        {
            //The safe way to add a header value is to use the TryParseAdd method and verify the return value is true,
            //especially if the header value is coming from user input.

            //header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
            //if (!headers.UserAgent.TryParseAdd(header))
            //{
            //    throw new Exception("Invalid header value: " + header);
            //}
            try
            {
                if (getDataStr == null || getDataStr.Count() == 0)
                {

                }
                else
                {
                    url += $"?{string.Join("&", getDataStr.Select(x => $"{x.Key}={x.Value}"))}";
                }

                Uri requestUri = new Uri(url);

                //Send the GET request asynchronously and retrieve the response as a string.
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                {
                    request.Headers.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 Edge/16.16299");
                    request.Headers.TryAppendWithoutValidation("Accept-Charset", "utf-8");
                    request.Headers.Connection.TryParseAdd("Keep-Alive");
                    request.Headers.AcceptEncoding.TryParseAdd("gzip, deflate, br");

                    if (addHeader != null)
                    {
                        foreach (var item in addHeader)
                        {
                            request.Headers.TryAppendWithoutValidation(item.Key, item.Value);
                        }
                    }

                    using (HttpResponseMessage response = await httpClient.SendRequestAsync(request))
                    {
                        if (ignoreStatus)
                        {

                        }
                        else
                        {
                            response.EnsureSuccessStatusCode();
                        }
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


        public static async Task<string> HttpGet(string url, NameValueCollection getDataStr = null, IEnumerable<KeyValuePair<string, string>> addHeader = null, bool ignoreStatus = false)
        {
            //The safe way to add a header value is to use the TryParseAdd method and verify the return value is true,
            //especially if the header value is coming from user input.

            //header = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
            //if (!headers.UserAgent.TryParseAdd(header))
            //{
            //    throw new Exception("Invalid header value: " + header);
            //}
            try
            {
                if (getDataStr == null)
                {

                }
                else
                {
                    url += $"?{getDataStr.ToString()}";
                }

                Uri requestUri = new Uri(url);
                //Send the GET request asynchronously and retrieve the response as a string.
                using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                {
                    request.Headers.UserAgent.TryParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 Edge/16.16299");
                    request.Headers.TryAppendWithoutValidation("Accept-Charset", "utf-8");
                    request.Headers.Connection.TryParseAdd("Keep-Alive");
                    request.Headers.AcceptEncoding.TryParseAdd("gzip, deflate, br");

                    if (addHeader != null)
                    {
                        foreach (var item in addHeader)
                        {
                            request.Headers.TryAppendWithoutValidation(item.Key, item.Value);
                        }
                    }

                    using (var response = await httpClient.SendRequestAsync(request))
                    {
                        if (ignoreStatus)
                        {

                        }
                        else
                        {
                            response.EnsureSuccessStatusCode();
                        }
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

    // ***********************************************************************************
    // ***********************************************************************************
    // ***********************************************************************************
    // *************************THESE STAFFS SHOULDN'T AT HERE****************************
    // ***********************************************************************************
    // ***********************************************************************************
    // ***********************************************************************************
    //public static class CaiyunRequestHelper
    //{
    //    private static readonly string nowUrl = "https://api.caiyunapp.com/v2/";
    //    public static async Task<string> RequestNowWithKeyAsync(float lon, float lat, string key)
    //    {
    //        var strURL = nowUrl + key + '/' + lon.ToString("0.0000") + ',' + lat.ToString("0.0000") + "/realtime.json";
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

    //    public static async Task<string> RequestForecastWithKeyAsync(float lon, float lat, string key)
    //    {
    //        var strURL = nowUrl + key + '/' + lon.ToString("0.0000") + ',' + lat.ToString("0.0000") + "/forecast.json";
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
    //}

    // ***********************************************************************************
    // ***********************************************************************************
    // ***********************************************************************************
    // *************************THESE STAFFS SHOULDN'T AT HERE****************************
    // ***********************************************************************************
    // ***********************************************************************************
    // ***********************************************************************************
    //public static class WundergroundRequestHelper
    //{
    //    private static readonly string url = "http://api.wunderground.com/api/{0}/geolookup/conditions/forecast/hourly/q";

    //    public static async Task<string> GeoLookup(string key, float lat, float lon)
    //    {
    //        var strURL = string.Format(url, key) + '/' + lat.ToString() + ',' + lon.ToString() + ".json";
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

    //    public static async Task<string> GetResult(string key, string zmw)
    //    {
    //        var strURL = string.Format(url, key) + '/' + zmw + ".json";
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
    //}


    public static class WebHelper
    {
        public static bool IsInternet()
        {
            var connections = NetworkInformation.GetInternetConnectionProfile();
            bool internet = connections != null && connections.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;
            return internet;
        }

        public static async Task<IAsyncOperationWithProgress<DownloadOperation, DownloadOperation>> DownloadFileAsync(string name, Uri uri, StorageFolder folder = null)
        {
            var downloader = new BackgroundDownloader();
            if (folder == null)
            {
                folder = ApplicationData.Current.LocalFolder;
            }
            var file = await folder.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);
            // Create a new download operation.
            var download = downloader.CreateDownload(uri, file);
            return download.StartAsync();
        }
    }
}
