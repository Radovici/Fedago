using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DataBazrPeer.Helpers
{
    public static class RestExtensions
    {
        public class GenericDataset
        {
            [JsonProperty("columns")]
            public List<string> Columns { get; set; }

            [JsonProperty("values")]
            public List<List<object>> Values { get; set; }
        }

        public static IEnumerable<IDictionary<string, object>> GetData(string host, string type, string querystring, HttpMethod httpMethod)
        {
            if (IsValidJson(querystring))
            {
                httpMethod = HttpMethod.Post;
            }
            string url = string.Format("{0}/{1}", host, type);
            string response = Rest(url, querystring, httpMethod);
            List<IDictionary<string, object>> results = new List<IDictionary<string, object>>();
            GenericDataset dataset = JsonConvert.DeserializeObject<GenericDataset>(response); // GenericDataset.FromJson(response);
            if (dataset.Values != null && dataset.Values.Any())
            {
                foreach (var values in dataset.Values)
                {
                    Dictionary<string, object> result = new Dictionary<string, object>();
                    for (int index = 0; index < values.Count; index++)
                    {
                        result[dataset.Columns[index]] = values[index];
                    }
                    results.Add(result);
                }
            }
            return results;
        }

        public static object GetValue(string host, string type, string querystring, HttpMethod httpMethod)
        {
            string url = string.Format("{0}/{1}", host, type);
            string response = Rest(url, querystring, httpMethod);
            object result = JsonConvert.DeserializeObject(response); // GenericDataset.FromJson(response);
            return result;
        }

        public static string Rest(string url, string content, HttpMethod httpMethod)
        {
            var responseTask = RestAsync(url, content, httpMethod);
            responseTask.Wait();
            var response = responseTask.Result;
            return response;
        }

        public static async Task<string> RestAsync(string url, string content, HttpMethod httpMethod)
        {
            ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

            using (var handler = new HttpClientHandler() { })
            {
                using (var client = new HttpClient(handler))
                {
                    client.Timeout = new TimeSpan(0, 30, 0);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpRequestMessage httpRequestMessage = new HttpRequestMessage();
                    httpRequestMessage.Method = httpMethod; // HttpMethod.Post;

                    if (httpMethod == HttpMethod.Get)
                    {
                        url = string.Format("{0}?{1}", url, content);
                    }
                    else
                    {
                        var json = content; //Newtonsoft.Json.JsonConvert.SerializeObject(request);                    
                        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                        httpRequestMessage.Content = httpContent;
                        httpRequestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        //httpRequestMessage.Headers.Add("x-api-token", "284949e6a933c5d0223747906843291a36a6c861");
                    }

                    httpRequestMessage.RequestUri = new Uri(url);

                    string responseBody;
                    //try
                    //{
                        var processResult = await client.SendAsync(httpRequestMessage);
                        responseBody = processResult.Content.ReadAsStringAsync().Result;
                        
                    //}
                    //catch(TaskCanceledException ex)
                    //{
                    //    // Check ex.CancellationToken.IsCancellationRequested here.
                    //    // If false, it's pretty safe to assume it was a timeout.
                    //    if (ex.CancellationToken.IsCancellationRequested)
                    //    {
                    //        responseBody = "CancelRequested";
                    //    }
                    //    else
                    //    {
                    //        responseBody = ex.Message;
                    //    }
                    //}
                    return responseBody;
                }
            }
        }

        public static bool IsValidJson(this string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}