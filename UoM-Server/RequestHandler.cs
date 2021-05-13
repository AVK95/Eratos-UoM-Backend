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
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO.Compression;
using System.Collections.Generic;
using System.Text.Json.Serialization;


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
        [DataContract]
    public class PersonResource : Resource
    {
        [DataMember(Name = "description", EmitDefaultValue=false)]
        public string description { get; set; }

        
    }
    [DataContract]
    public class Resource
    {
        [DataMember(Name = "@id", EmitDefaultValue=false)]
        public string id { get; set; }

        [DataMember(Name = "@geo", EmitDefaultValue=false)]
        public string geo { get; set; }

        [DataMember(Name = "@data", EmitDefaultValue=false)]
        public string data { get; set; }

        [DataMember(Name = "@date", EmitDefaultValue=false)]
        public string date { get; set; }

        [DataMember(Name = "@owner", EmitDefaultValue=false)]
        public string owner { get; set; }

        [DataMember(Name = "@policy", EmitDefaultValue=false)]
        public string policy { get; set; }

        [DataMember(Name = "@type")]
        public string type { get; set; }

        [DataMember(Name = "@public", EmitDefaultValue=false)]
        public bool isPublic { get; set; }

        [DataMember(Name = "@userPerms", EmitDefaultValue=false)]
        public List<string> userPerms { get; set; }

        [DataMember(Name = "@externalSources", EmitDefaultValue=false)]
        public List<string> externalSources { get; set; }

        [DataMember(Name = "name", EmitDefaultValue=false)]
        public string name { get; set; }
    }
 [DataContract]
    public class ResourcePolicyRule
    {
        [DataMember(Name = "actor")]
        public string actor { get; set; }

        [DataMember(Name = "effect")]
        public string effect { get; set; }

        [DataMember(Name = "actions")]
        public List<string> actions { get; set; }
    }
    
    [DataContract]
    public class ResourcePolicy
    {
        [DataMember(Name = "@id", EmitDefaultValue=false)]
        public string id { get; set; }

        [DataMember(Name = "@owner", EmitDefaultValue=false)]
        public string owner { get; set; }

        [DataMember(Name = "@for", EmitDefaultValue=false)]
        public string resource { get; set; }

        [DataMember(Name = "@type")]
        public string type { get; set; }

        [DataMember(Name = "@date", EmitDefaultValue=false)]
        public string date { get; set; }

        [DataMember(Name = "@invites", EmitDefaultValue=false)]
        public List<object> invites { get; set; }

        [DataMember(Name = "@links", EmitDefaultValue=false)]
        public Dictionary<string, object> links { get; set; }

        [DataMember(Name = "rules")]
        public List<ResourcePolicyRule> rules { get; set; }
    }

    public class PolicyInvites
    {
        [JsonPropertyName("$id")]
        public string id { get; set; }
        public string createdAt { get; set; }
        public string createdBy { get; set; }
        public string email { get; set; }
        public List<string> actions { get; set; }

    }

    public class PolicyProperties
    {

        public string email { get; set; }
        public string fullName { get; set; }

    }

    public class PolicyRules
    {

        public List<string> actions { get; set; }
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
        static string SHA256HashBytesHex(byte[] content) {
            byte[] hbytes;
            using (SHA256 mySHA256 = SHA256.Create())
            {
                hbytes = mySHA256.ComputeHash(content);
            }
            return BitConverter.ToString(hbytes).Replace("-", "").ToLower();
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
        static string EratosSignedRequest(string accessId, string accessSecret, string method, string uri, string reqJSON)
        {
            byte[] body = Encoding.UTF8.GetBytes(reqJSON);
            string curTime = DateTime.UtcNow.ToString("o");
            string canonReq = method + "\n";

            Uri uriObj = new Uri(uri);
            canonReq += uriObj.LocalPath+"\n";
            canonReq += uriObj.Query+"\n";

            canonReq += "host:"+uriObj.Host+"\n";
            canonReq += "x-eratos-date:"+curTime+"\n";
            if (body.Length > 0) {
                canonReq += "content-type:application/json\n";
            }

            string bodyHash = SHA256HashBytesHex(body);
            canonReq += bodyHash;

            string signedMAC = HMACHashStringHex(canonReq, accessSecret);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            if (body.Length > 0) {
                request.ContentLength = body.Length;
                request.ContentType = "application/json";
            }
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Headers.Set("X-Eratos-Date", curTime);
            string signedHeaders = "host;x-eratos-date";
            if (body.Length > 0) {
                signedHeaders += ";content-type";
            }
            request.Headers.Set("Authorization", "ERATOS-HMAC-SHA256 Credential="+accessId+", SignedHeaders="+signedHeaders+", Signature="+signedMAC);

            if (body.Length > 0) {
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(body, 0, body.Length);
                dataStream.Close();
            }

            try {
                using(HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using(Stream stream = response.GetResponseStream())
                using(StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse) response;
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
        static string WriteObjToJSON<T>(T obj) {
            var ms = new MemoryStream();
            var ser = new DataContractJsonSerializer(typeof(T));
            ser.WriteObject(ms, obj);
            byte[] json = ms.ToArray();
            ms.Close();
            return Encoding.UTF8.GetString(json, 0, json.Length);
        }
        static T ReadObjFromJSON<T>(string json) where T : class, new() {
            var dObj = new T();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var ser = new DataContractJsonSerializer(dObj.GetType());
            dObj = ser.ReadObject(ms) as T;
            ms.Close();
            return dObj;
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

        public string CreatePersonResource(string userName, string description)
        {
            PersonResource ps = new PersonResource();
            ps.type = "https://schemas.eratos.ai/json/person";
            ps.name = userName;
            ps.description = description;

            string psReq = WriteObjToJSON<PersonResource>(ps);
            string json = EratosSignedRequest(accessId, accessSecret, "POST", "https://staging.e-pn.io/resources", psReq);
            return json;

        }

        public ResourcePolicy GetPolicy(string personResource)
        {
            ps = ReadObjFromJSON<PersonResource>(personResource);
            json = EratosSignedRequest(accessId, accessSecret, "GET", ps.policy, "");
            ResourcePolicy pol = ReadObjFromJSON<ResourcePolicy>(json);
            return pol;
        }

        public string AddUserToPolicy(ResourcePolicy pol, string userURI)
        {
            ResourcePolicyRule newRule = new ResourcePolicyRule();
            newRule.actor = userURI; 
            newRule.effect = "Allow";
            newRule.actions = new List<string>() {
                "Get", "Download"
            };
            pol.rules.Add(newRule);
            string polReq = WriteObjToJSON<ResourcePolicy>(pol);
            string response = EratosSignedRequest(accessId, accessSecret, "PUT", pol.id, polReq);
            return response;
        }

        static Resource GetResource(string token, string id)
        {
            string json = GetWithToken(token, Config.Primary_Node_Domain + "/resources/" + id);
            return JsonSerializer.Deserialize<Resource>(json);
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
