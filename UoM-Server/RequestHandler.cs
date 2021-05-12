using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using System.IO.Compression;

namespace UoM_Server
{
    public class TokenNodeResponse
    {
        public string token { get; set; }
    }
    public class GNTaskMeta
    {
        public string resource { get; set; }
    }
    public class GNTaskRequest
    {
        public string priority { get; set; }
        public string type { get; set; }
        public GNTaskMeta meta { get; set; }
    }
    public class GNTaskResponse
    {

        public string id { get; set; }
        public string createdAt { get; set; }
        public string lastUpdatedAt { get; set; }
        public string startedAt { get; set; }
        public string endedAt { get; set; }
        public string priority { get; set; }
        public string state { get; set; }
        public string type { get; set; }
        public string error { get; set; }
        public GNTaskMeta meta { get; set; }
    }

    public class PolicyResponse
    {

        public string id { get; set; }
        public string date { get; set; }
        public string owner { get; set; }
        public string fora { get; set; } //for variable is forbidden
        public string type { get; set; }
        public PolicyInvites invites { get; set; }
        public PolicyProperties[] links { get; set; }
        public PolicyRules rules { get; set; }
    }

    public class PolicyInvites
    {

        public string id { get; set; }
        public string createdAt { get; set; }
        public string createdBy { get; set; }
        public string email { get; set; }
        public string[] actions { get; set; }

    }

    public class PolicyProperties
    {

        public string email { get; set; }
        public string fullName { get; set; }

    }

    public class PolicyRules
    {

        public string[] actions { get; set; }
        public string effect { get; set; }
        public string actor { get; set; }

    }

    class RequestHandler
    {
        string usingToken = Config.Access_Token;

        enum Priority
        {
            Low,
            Normal,
            High
        }

        static string SHA256HashStringHex(string content)
        {
            byte[] hbytes;
            byte[] cbytes = Encoding.UTF8.GetBytes(content);
            using (SHA256 mySHA256 = SHA256.Create())
            {
                hbytes = mySHA256.ComputeHash(cbytes);
            }
            return BitConverter.ToString(hbytes).Replace("-", "").ToLower();
        }
        static string HMACHashStringHex(string content, string keyB64)
        {
            byte[] hbytes;
            byte[] cbytes = Encoding.UTF8.GetBytes(content);
            string normB64 = keyB64.Replace('_', '/').Replace('-', '+'); // URLSafe b64 -> standard b64.
            switch (normB64.Length % 4)
            { // Add b64 padding.
                case 2: normB64 += "=="; break;
                case 3: normB64 += "="; break;
            }
            byte[] kbytes = Convert.FromBase64String(normB64);
            using (HMACSHA256 hash = new HMACSHA256(kbytes))
            {
                hbytes = hash.ComputeHash(cbytes);
            }
            return BitConverter.ToString(hbytes).Replace("-", "").ToLower();
        }

        static string GetToken(string accessId, string accessSecret, string uri)
        {
            string curTime = DateTime.UtcNow.ToString("o");
            string canonReq = "GET\n";

            Uri uriObj = new Uri(uri);
            canonReq += uriObj.LocalPath + "\n";
            canonReq += uriObj.Query + "\n";

            canonReq += "host:" + uriObj.Host + "\n";
            canonReq += "x-eratos-date:" + curTime + "\n";

            string bodyHash = SHA256HashStringHex("");
            canonReq += bodyHash;

            string signedMAC = HMACHashStringHex(canonReq, accessSecret);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Headers.Set("X-Eratos-Date", curTime);
            request.Headers.Set("Authorization", "ERATOS-HMAC-SHA256 Credential=" + accessId + ", SignedHeaders=host;x-eratos-date, Signature=" + signedMAC);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                        throw e;
                    }
                }
            }
        }

        
        static GNTaskResponse CreateNewTask(string token, Priority priority, string resource)
        {
            GNTaskRequest req = new GNTaskRequest();
            req.priority = priority.ToString();
            req.type = "GenerateClimateInfo";
            req.meta = new GNTaskMeta();
            req.meta.resource = resource;
            byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize<GNTaskRequest>(req));
            string json = PostWithToken(token, Config.taskGNNodeDomain + "/tasks", body);
            return JsonSerializer.Deserialize<GNTaskResponse>(json);
        }
        static GNTaskResponse GetTask(string token, string id)
        {
            string json = GetWithToken(token, Config.Primary_Node_Domain + "/tasks/" + id);
            return JsonSerializer.Deserialize<GNTaskResponse>(json);
        }

        static PolicyResponse GetPolicy(string token, string id)
        {
            string json = GetWithToken(token, Config.taskGNNodeDomain + "/policies/" + id);
            return JsonSerializer.Deserialize<PolicyResponse>(json);
        }
        static void RemoveTask(string token, string id)
        {
            DeleteWithToken(token, Config.taskGNNodeDomain + "/tasks/" + id);
        }
        static string PostWithToken(string token, string uri, byte[] body)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "POST";
            request.ContentLength = body.Length;
            request.ContentType = "application/json";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Headers.Set("Authorization", "Bearer " + token);

            Stream dataStream = request.GetRequestStream();
            dataStream.Write(body, 0, body.Length);
            dataStream.Close();

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                        throw e;
                    }
                }
            }
        }
        static string GetWithToken(string token, string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Headers.Set("Authorization", "Bearer " + token);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                        throw e;
                    }
                }
            }
        }

        static string DeleteWithToken(string token, string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "DELETE";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Headers.Set("Authorization", "Bearer " + token);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    Console.WriteLine("Error code: {0}", httpResponse.StatusCode);
                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        string text = reader.ReadToEnd();
                        Console.WriteLine(text);
                        throw e;
                    }
                }
            }
        }
        public string CreateNewResource(string type, string name)
        {

            var client = new RestClient(Config.Primary_Node_Domain);
            client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", usingToken));

            var payload = string.Format("\"@type\":\"{0}\", \"name\":\"{1}\"", type, name);
            payload = "{" + payload + "}";
            var request = new RestRequest("resources", Method.POST, DataFormat.Json);
            request.AddJsonBody(payload);

            var response = client.Execute(request);
            return response.Content;
        }


        public string ShowResourceVersions(string resourceID)
        {
            var client = new RestClient(Config.Primary_Node_Domain);
            client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", usingToken));

            var request = new RestRequest("resources/" + resourceID + "/versions", Method.GET, DataFormat.None);

            var response = client.Execute(request);
            return response.Content;
        }

        public string DeleteResource(string resourceID)
        {
            var client = new RestClient(Config.Primary_Node_Domain);
            client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", usingToken));

            var request = new RestRequest("resources/" + resourceID, Method.DELETE, DataFormat.None);

            var response = client.Execute(request);
            return response.Content;
        }


        public string FetchGeometry(string resourceID, string type = null, string detail = null)
        {
            var client = new RestClient(Config.Primary_Node_Domain);
            client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", usingToken));

            var requestPara = "";
            if (type != null)
            {
                requestPara += "?type=" + type;
            }

            if (detail != null)
            {
                if (requestPara == "")
                {
                    requestPara += "?detail=" + detail;
                }
                else
                {
                    requestPara += "&detail=" + detail;
                }
            }

            var request = new RestRequest("resources/" + resourceID + "/geo" + requestPara, Method.GET, DataFormat.None);

            var response = client.Execute(request);
            return response.Content;
        }


        public static string AUTHORISE_USER(string token)
        {
            var client = new RestClient(Config.Tracker_Node_Domain);
            client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", token));

            var request = new RestRequest("auth/me", Method.GET, DataFormat.None);

            var response = client.Execute(request);
            return response.Content;
        }


    }
}
