using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ZPrinterConfig.Controllers
{
    public class V275_NodeController
    {
        public enum Actions
        {
            GET,
            PUT,
            POST,
            DELETE,
            STREAM
        }

        public class Command
        {
            public Type JSONType { get; set; }
            public string TypeName { get; set; }
            public Actions Action { get; set; }
            public List<string> Resources { get; set; }
        }

        public string ConnectionString(string host) => $"https://{host}:8080";

        
        public string ConfigurationURI(int nodeNumber) => $"/api/printinspection/{nodeNumber}/configuration/camera/";

        ///api/printinspection/1/configuration/camera/peelAndPresentMode?save=1
        public string ConfigurationPUT(string host, int nodeNumber, string parameter) => $"{ConnectionString(host)}{ConfigurationURI(nodeNumber)}{parameter}?save=1";

        public bool IsException { get; private set; }
        public Exception RESTException { get; private set; }
        public bool IsHttpStatus { get; private set; }
        public HttpStatusCode HttpStatusCode { get; private set; }


        private void Reset()
        {
            RESTException = null;
            IsException = false;

            IsHttpStatus = false;
        }

        public async Task<string> Post(string url, string jSONData)
        {
            Reset();


            try
            {
                //byte[] cred = UTF8Encoding.UTF8.GetBytes($"{UserName}:{pass}");
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new System.Uri(url);
                    //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(cred));
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    HttpContent content = new StringContent(jSONData, UTF8Encoding.UTF8, "application/json");

                    return await client.PostAsync(url, content).Result.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                RESTException = ex;
                IsException = true;
                return string.Empty;
            }
            finally
            {
                ServicePointManager.ServerCertificateValidationCallback -= (sender1, certificate, chain, sslPolicyErrors) => true;
            }

        }
        public async Task<string> Put(string url, string jSONData)
        {
            Reset();

            //ServicePointManager.ServerCertificateValidationCallback += (sender1, certificate, chain, sslPolicyErrors) => true;
            try
            {
                //byte[] cred = UTF8Encoding.UTF8.GetBytes($"{UserName}:{pass}");
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new System.Uri(url);
                    //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(cred));
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    HttpContent content = new StringContent(jSONData, UTF8Encoding.UTF8, "application/json");

                    return await client.PutAsync(url, content).Result.Content.ReadAsStringAsync();
                }
            }
            catch (Exception ex)
            {
                RESTException = ex;
                IsException = true;
                return string.Empty;
            }
            finally
            {
                //ServicePointManager.ServerCertificateValidationCallback -= (sender1, certificate, chain, sslPolicyErrors) => true;
            }

        }
        public async Task<string> Delete(string url, string pass)
        {
            Reset();

            //ServicePointManager.ServerCertificateValidationCallback += (sender1, certificate, chain, sslPolicyErrors) => true;
            try
            {
                //byte[] cred = UTF8Encoding.UTF8.GetBytes($"{UserName}:{pass}");
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new System.Uri(url);
                    //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(cred));
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    return await client.DeleteAsync(url).Result.Content.ReadAsStringAsync();

                }

            }
            catch (Exception ex)
            {
                RESTException = ex;
                IsException = true;
                return string.Empty;
            }
            finally
            {
                //ServicePointManager.ServerCertificateValidationCallback -= (sender1, certificate, chain, sslPolicyErrors) => true;
            }

        }
        public async Task<string> Get(string url, string pass)
        {
            Reset();

            //ServicePointManager.ServerCertificateValidationCallback += (sender1, certificate, chain, sslPolicyErrors) => true;
            try
            {
                //byte[] cred = UTF8Encoding.UTF8.GetBytes($"{UserName}:{pass}");
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new System.Uri(url);
                    //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(cred));
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                    string res = await client.GetAsync(url).Result.Content.ReadAsStringAsync();

                    if (!res.StartsWith("["))
                    {
                        if (res.StartsWith("<html>"))
                        {
                            Match match = Regex.Match(res, @"(?:<h1>)[0-9]+");
                            if (match.Success)
                            {
                                IsHttpStatus = true;
                                HttpStatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), match.Value.Replace("<h1>", ""));
                            }
                        }
                        else
                            throw new Exception(res);
                    }


                    return res;

                }
            }
            catch (Exception ex)
            {
                RESTException = ex;
                IsException = true;
                return string.Empty;
            }
            finally
            {
                //ServicePointManager.ServerCertificateValidationCallback -= (sender1, certificate, chain, sslPolicyErrors) => true;
            }
        }
        public async Task<Stream> Stream(string url, string pass)
        {
            Reset();

            //ServicePointManager.ServerCertificateValidationCallback += (sender1, certificate, chain, sslPolicyErrors) => true;
            try
            {
                //byte[] cred = UTF8Encoding.UTF8.GetBytes($"{UserName}:{pass}");
                using (HttpClient client = new HttpClient())
                {
                    client.BaseAddress = new System.Uri(url);
                    //client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(cred));
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));


                    return await client.GetStreamAsync(url);
                }
            }
            catch (Exception ex)
            {
                RESTException = ex;
                IsException = true;
                return null;
            }
            finally
            {
                //ServicePointManager.ServerCertificateValidationCallback -= (sender1, certificate, chain, sslPolicyErrors) => true;
            }


        }

    }
}
