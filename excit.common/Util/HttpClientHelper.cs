using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace asi.excit.common.Util
{
    // This helper class will create on static HttpClient instance per domain
    // Reference: http://byterot.blogspot.com/2016/07/singleton-httpclient-dns.html
    public class HttpClientHelper
    {
        private static ConcurrentDictionary<string, HttpClient> _httpClients = new ConcurrentDictionary<string, HttpClient>();
        // asi.excit.common.Integration.AlphaBroder.OrderCreation.UploadFile() is currently the only one using this instance
        private static ConcurrentDictionary<string, HttpClient> _httpClientsNoRedirect = new ConcurrentDictionary<string, HttpClient>();

        public static HttpClient GetHttpClient(string url, string authHeader = null, bool allowRedirect = true)
        {
            var httpClients = allowRedirect ? _httpClients : _httpClientsNoRedirect;
            var secondDomain = string.Empty;

            // get second domain
            if( !string.IsNullOrEmpty(url))
            {
                var strArrays = System.Text.RegularExpressions.Regex.Split(url, @"://");
                secondDomain = strArrays.Length > 1 ? strArrays[1] : strArrays[0];
                secondDomain = secondDomain.Split('/')[0];
                strArrays = secondDomain.Split('.');
                var len = strArrays.Length;
                secondDomain = len > 1 ? $"{strArrays[len-2]}.{strArrays[len - 1]}" : strArrays[0];
                secondDomain = secondDomain.ToLower();
            }

            if( !httpClients.ContainsKey(secondDomain))
            {
                httpClients.TryAdd(secondDomain, allowRedirect ? new HttpClient() : new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }));
                var sp = ServicePointManager.FindServicePoint(new Uri(url));
                sp.ConnectionLeaseTimeout = 60 * 2000;  // set ConnectionLeaseTimeout for 2 minutes
            }

            var httpClient = httpClients[secondDomain];

            if (!string.IsNullOrEmpty(authHeader))
            {
                var byteArray = new UTF8Encoding().GetBytes(authHeader);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
            else
            {
                httpClient.DefaultRequestHeaders.Authorization = null;
            }

            return httpClient;
        }

        public static string SubmitWebRequest(string url, IDictionary<string, string> headerParam, string content, bool post = true)
        {
            return SubmitWebRequestAsync(url, headerParam, content, post).Result;
        }

        public async static Task<string> SubmitWebRequestAsync(string url, IDictionary<string, string> headerParam, string content, bool post = true)
        {
            var resultContent = string.Empty;
            var client = GetHttpClient(url);            
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                request.RequestUri = new Uri(url);
                request.Method = new HttpMethod(post ? "POST" : "GET");

                request.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/535.19 (KHTML, like Gecko) Chrome/18.0.1025.168 Safari/535.19");
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //set the content into the request if available
                if (!string.IsNullOrEmpty(content))
                {
                    UTF8Encoding encoding = new UTF8Encoding();
                    byte[] postBytes = encoding.GetBytes(content);
                    request.Content = new StreamContent(new MemoryStream(postBytes));
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

                    request.RegisterForDispose(request.Content);
                }

                if (headerParam != null)
                {
                    foreach (string key in headerParam.Keys)
                    {
                        switch (key.ToLower())
                        {
                            case "contenttype":
                                if (request.Content != null)
                                {
                                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(headerParam[key]);
                                }
                                break;
                            case "authorization-scheme"://ignored, can only be used with authorization
                                break;
                            case "authorization":
                                request.Headers.Authorization = new AuthenticationHeaderValue(headerParam["Authorization-scheme"], headerParam[key]);
                                break;
                            default:
                                request.Headers.Add(key, headerParam[key]);
                                break;
                        }
                    }
                }

                // Execute the request
                if (ServicePointManager.Expect100Continue) ServicePointManager.Expect100Continue = false;

                using (var response = await client.SendAsync(request).ConfigureAwait(false))
                {
                    resultContent = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode) return resultContent.ToString();
                    else if (string.IsNullOrEmpty(resultContent)) throw new Exception(string.Format("The web request was not successfully completed: (code {0})", response.StatusCode));
                    else throw new Exception(string.Format("The web request was not successfully completed: (code {0}) with error {1}", response.StatusCode, resultContent));
                }
            }            
        }

        public static string SubmitForm(string url, IDictionary<string, string> parameters, bool post = false)
        {
            //construct the url
            var webParams = new StringBuilder();
            if (!post && parameters.Count > 0)
            {
                webParams.Append("?");
            }

            //build query string
            if (parameters != null)
            {
                foreach (string key in parameters.Keys)
                {
                    if (webParams.Length > 1)
                    {
                        webParams.Append("&");
                    }
                    
                    webParams.Append(key)
                        .Append("=")
                        .Append(System.Web.HttpUtility.UrlEncode(parameters[key]));

                }
            }
            //create the web request
            var finalUrl = (post ? url : url + webParams);
            var content = (post ? webParams.ToString() : null);
            return SubmitWebRequest(finalUrl, null, content, post);
        }
    }
}
