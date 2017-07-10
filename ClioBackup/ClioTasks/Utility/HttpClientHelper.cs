using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;

namespace ClioTasks.Utility
{
    public static class HttpClientHelper
    {
        /// <summary>
        /// get请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> GetResponseAsync(string url,string token)
        {
            if (url.StartsWith("https"))
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(
               new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

            HttpResponseMessage response = httpClient.GetAsync(url).Result;

            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                return result;
            }
            return null;
        }

        public static async Task<string> PatchAsync(string token, string url, string content)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpContent myContent = new StringContent(content, Encoding.UTF8, "application/json");
                Uri requestUri = new Uri(url);
                //var result = await PatchAsync(httpClient, requestUri, myContent);
                var method = new HttpMethod("PATCH");
                var request = new HttpRequestMessage(method, requestUri)
                {
                    Content = myContent
                };
                HttpResponseMessage response = new HttpResponseMessage();
                try
                {
                    response = await httpClient.SendAsync(request);
                }
                catch (TaskCanceledException e)
                {

                }
                return response.Content.ToString();
            }
        }

        public static async Task<string> PutAsync(string token, string url, string content)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                HttpContent myContent = new StringContent(content, Encoding.UTF8, "application/json");              

                HttpResponseMessage response = new HttpResponseMessage();
                try
                {
                    response = await httpClient.PutAsync(url,myContent);
                }
                catch (TaskCanceledException e)
                {

                }
                return response.Content.ToString();
            }
        }

        public static async Task<string> DeletetAsync(string token, string url)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));               

                HttpResponseMessage response = new HttpResponseMessage();
                try
                {
                    response = await httpClient.DeleteAsync(url);
                }
                catch (TaskCanceledException e)
                {

                }
                return response.Content.ToString();
            }
        }


        public static async Task<string> DeleteAsync(string token, string url)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = new HttpResponseMessage();
                try
                {
                    response = await httpClient.DeleteAsync(url);
                }
                catch (TaskCanceledException e)
                {

                }
                return response.Content.ToString();
            }

        }


        public static T GetResponse<T>(string url)
            where T : class, new()
        {
            if (url.StartsWith("https"))
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Accept.Add(
               new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = httpClient.GetAsync(url).Result;

            T result = default(T);

            if (response.IsSuccessStatusCode)
            {
                Task<string> t = response.Content.ReadAsStringAsync();
                string s = t.Result;

                result = JsonConvert.DeserializeObject<T>(s);
            }
            return result;
        }

        /// <summary>
        /// post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="postData">post数据</param>
        /// <returns></returns>
        public static string PostResponse(string url, string postData)
        {
            if (url.StartsWith("https"))
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            HttpContent httpContent = new StringContent(postData);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpClient httpClient = new HttpClient();

            HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;

            if (response.IsSuccessStatusCode)
            {
                string result = response.Content.ReadAsStringAsync().Result;
                return result;
            }
            return null;
        }

        /// <summary>
        /// 发起post请求
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">url</param>
        /// <param name="postData">post数据</param>
        /// <returns></returns>
        public static T PostResponse<T>(string url, string postData)
            where T : class, new()
        {
            if (url.StartsWith("https"))
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            HttpContent httpContent = new StringContent(postData);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpClient httpClient = new HttpClient();

            T result = default(T);

            HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;

            if (response.IsSuccessStatusCode)
            {
                Task<string> t = response.Content.ReadAsStringAsync();
                string s = t.Result;

                result = JsonConvert.DeserializeObject<T>(s);
            }
            return result;
        }

        /// <summary>
        /// V3接口全部为Xml形式，故有此方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static T PostXmlResponse<T>(string url, string xmlString)
            where T : class, new()
        {
            if (url.StartsWith("https"))
                System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;

            HttpContent httpContent = new StringContent(xmlString);
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            HttpClient httpClient = new HttpClient();

            T result = default(T);

            HttpResponseMessage response = httpClient.PostAsync(url, httpContent).Result;

            if (response.IsSuccessStatusCode)
            {
                Task<string> t = response.Content.ReadAsStringAsync();
                string s = t.Result;

                result = XmlDeserialize<T>(s);
            }
            return result;
        }

        /// <summary>
        /// 反序列化Xml
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static T XmlDeserialize<T>(string xmlString)
            where T : class, new()
        {
            try
            {
                XmlSerializer ser = new XmlSerializer(typeof(T));
                using (StringReader reader = new StringReader(xmlString))
                {
                    return (T)ser.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("XmlDeserialize发生异常：xmlString:" + xmlString + "异常信息：" + ex.Message);
            }

        }
    }
}