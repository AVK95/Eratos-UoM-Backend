using System;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Runtime.Serialization;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System.Runtime.Serialization.Json;
using System.Collections.Generic;

namespace UoM_Server
{
    public class TokenNodeResponse
    {
        public string token { get; set; }
    }

    [DataContract]
    public class Resource
    {
        [DataMember(Name = "@id", EmitDefaultValue = false)]
        public string id { get; set; }

        [DataMember(Name = "@geo", EmitDefaultValue = false)]
        public string geo { get; set; }

        [DataMember(Name = "@data", EmitDefaultValue = false)]
        public string data { get; set; }

        [DataMember(Name = "@date", EmitDefaultValue = false)]
        public string date { get; set; }

        [DataMember(Name = "@owner", EmitDefaultValue = false)]
        public string owner { get; set; }

        [DataMember(Name = "@policy", EmitDefaultValue = false)]
        public string policy { get; set; }

        [DataMember(Name = "@type")]
        public string type { get; set; }

        [DataMember(Name = "@public", EmitDefaultValue = false)]
        public bool isPublic { get; set; }

        [DataMember(Name = "@userPerms", EmitDefaultValue = false)]
        public List<string> userPerms { get; set; }

        [DataMember(Name = "@externalSources", EmitDefaultValue = false)]
        public List<string> externalSources { get; set; }

        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string name { get; set; }
    }

    [DataContract]
    public class GenericResource : Resource
    {
        [DataMember(Name = "description", EmitDefaultValue = false)]
        public string description { get; set; }
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
        [DataMember(Name = "@id", EmitDefaultValue = false)]
        public string id { get; set; }

        [DataMember(Name = "@owner", EmitDefaultValue = false)]
        public string owner { get; set; }

        [DataMember(Name = "@for", EmitDefaultValue = false)]
        public string resource { get; set; }

        [DataMember(Name = "@type")]
        public string type { get; set; }

        [DataMember(Name = "@date", EmitDefaultValue = false)]
        public string date { get; set; }

        [DataMember(Name = "@invites", EmitDefaultValue = false)]
        public List<object> invites { get; set; }

        [DataMember(Name = "@links", EmitDefaultValue = false)]
        public Dictionary<string, object> links { get; set; }

        [DataMember(Name = "rules")]
        public List<ResourcePolicyRule> rules { get; set; }
    }

    public class UserDetails
    {
        public string id { get; set; }
        public string email { get; set; }
        public string name { get; set; }
        public string auth0Id { get; set; }
        public string createdAt { get; set; }
        public string info { get; set; }
        public string resourcePolicy { get; set; }

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

    class RequestHandlerTested
    {
        enum Priority
        {
            Low,
            Normal,
            High
        }
       
        
        static string SHA256HashBytesHex(byte[] content)
        {
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
            canonReq += uriObj.LocalPath + "\n";
            canonReq += uriObj.Query + "\n";

            canonReq += "host:" + uriObj.Host + "\n";
            canonReq += "x-eratos-date:" + curTime + "\n";
            if (body.Length > 0)
            {
                canonReq += "content-type:application/json\n";
            }

            string bodyHash = SHA256HashBytesHex(body);
            canonReq += bodyHash;

            string signedMAC = HMACHashStringHex(canonReq, accessSecret);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            if (body.Length > 0)
            {
                request.ContentLength = body.Length;
                request.ContentType = "application/json";
            }
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Headers.Set("X-Eratos-Date", curTime);
            string signedHeaders = "host;x-eratos-date";
            if (body.Length > 0)
            {
                signedHeaders += ";content-type";
            }
            request.Headers.Set("Authorization", "ERATOS-HMAC-SHA256 Credential=" + accessId + ", SignedHeaders=" + signedHeaders + ", Signature=" + signedMAC);

            if (body.Length > 0)
            {
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(body, 0, body.Length);
                dataStream.Close();
            }

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
        static string WriteObjToJSON<T>(T obj)
        {
            var ms = new MemoryStream();
            var ser = new DataContractJsonSerializer(typeof(T));
            ser.WriteObject(ms, obj);
            byte[] json = ms.ToArray();
            ms.Close();
            return Encoding.UTF8.GetString(json, 0, json.Length);
        }
        static T ReadObjFromJSON<T>(string json) where T : class, new()
        {
            var dObj = new T();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var ser = new DataContractJsonSerializer(dObj.GetType());
            dObj = ser.ReadObject(ms) as T;
            ms.Close();
            return dObj;
        }

        static GNTaskResponse CreateNewTask(string token, Priority priority, string resource)
        {
            GNTaskRequest req = new GNTaskRequest();
            req.priority = priority.ToString();
            req.type = "GenerateClimateInfo";
            req.meta = new GNTaskMeta();
            req.meta.resource = resource;
            byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize<GNTaskRequest>(req));
            string json = PostWithToken(token, taskGNNodeDomain + "/tasks", body);
            return JsonSerializer.Deserialize<GNTaskResponse>(json);
        }
        static GNTaskResponse GetTask(string token, string id)
        {
            string json = GetWithToken(token, Config.taskGNNodeDomain + "/tasks/" + id);
            return JsonSerializer.Deserialize<GNTaskResponse>(json);
        }

        static UserDetails GetUserDetails(string token)
        {
            string json = GetWithToken(token, Config.Tracker_Node_Domain + "/auth/me");
            return JsonSerializer.Deserialize<UserDetails>(json);
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
        static string CreateResource(string name, string description, string type, string accessId, string accessSecret)
        {
            GenericResource ps = new GenericResource();
            ps.type = type;
            ps.name = name;
            ps.description = description;

            string psReq = WriteObjToJSON<GenericResource>(ps);
            string json = EratosSignedRequest(accessId, accessSecret, "POST", "https://staging.e-pn.io/resources", psReq);
            return json;

        }

        static ResourcePolicy GetPolicy(string personResource, string accessId, string accessSecret)
        {
            var ps = ReadObjFromJSON<GenericResource>(personResource);
            var json = EratosSignedRequest(accessId, accessSecret, "GET", ps.policy, "");
            ResourcePolicy pol = ReadObjFromJSON<ResourcePolicy>(json);
            return pol;
        }

        static string AddUserToPolicy(ResourcePolicy pol, string userURI, string accessId, string accessSecret)
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

        //static Resource GetResource(string token, string id)
        //{
        //    string json = GetWithToken(token, Config.Primary_Node_Domain + "/resources/" + id);
        //    return JsonSerializer.Deserialize<Resource>(json);
        //}

        //public string ShowResourceVersions(string resourceID)
        //{
        //    var client = new RestClient(Config.Primary_Node_Domain);
        //    client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", usingToken));

        //    var request = new RestRequest("resources/" + resourceID + "/versions", Method.GET, DataFormat.None);

        //    var response = client.Execute(request);
        //    return response.Content;
        //}

        //public string DeleteResource(string resourceID)
        //{
        //    var client = new RestClient(Config.Primary_Node_Domain);
        //    client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", usingToken));

        //    var request = new RestRequest("resources/" + resourceID, Method.DELETE, DataFormat.None);

        //    var response = client.Execute(request);
        //    return response.Content;
        //}

        //public string FetchGeometry(string resourceID, string type = null, string detail = null)
        //{
        //    var client = new RestClient(Config.Primary_Node_Domain);
        //    client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", usingToken));

        //    var requestPara = "";
        //    if (type != null)
        //    {
        //        requestPara += "?type=" + type;
        //    }

        //    if (detail != null)
        //    {
        //        if (requestPara == "")
        //        {
        //            requestPara += "?detail=" + detail;
        //        }
        //        else
        //        {
        //            requestPara += "&detail=" + detail;
        //        }
        //    }

        //    var request = new RestRequest("resources/" + resourceID + "/geo" + requestPara, Method.GET, DataFormat.None);

        //    var response = client.Execute(request);
        //    return response.Content;
        //}


        //public static string AUTHORISE_USER(string token)
        //{
        //    var client = new RestClient(Tracker_Node_Domain);
        //    client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", token));

        //    var request = new RestRequest("auth/me", Method.GET, DataFormat.None);

        //    var response = client.Execute(request);
        //    return response.Content;
        //}
        //static void Main(string[] args)
        //{
        //    string html = string.Empty;
        //    string accessId = "prm3f5kedubl5p633mp5ntab";
        //    string accessSecret = "j3U21gw8Ldk-IstOc8vb3plybaGL7iTwikA2sccArBOPHpESCJiunnYLJRhFZ6vmJbLzi2gBkSyVHtgqSqojjQ";
        //    string trackerAuthURL = "https://staging.e-tr.io/token/node";

        //    string json = GetToken(accessId, accessSecret, trackerAuthURL);
        //    var resp = JsonSerializer.Deserialize<TokenNodeResponse>(json);
        //    Console.WriteLine("Token: " + resp.token);


        //    //GenericResource ps = new GenericResource();
        //    //ps.type = "https://schemas.eratos.ai/json/person";
        //    //ps.name = "Paul Stegeman";
        //    //ps.description = "CTO of Eratos";

        //    //Console.WriteLine("Creating a person resource!");
        //    //string psReq = WriteObjToJSON<GenericResource>(ps);
        //    //string json2 = EratosSignedRequest(accessId, accessSecret, "POST", "https://staging.e-pn.io/resources", psReq);
        //    string respo = CreateResource("Paul Stegemen", "CTO", "https://schemas.eratos.ai/json/person", accessId, accessSecret);
        //    Console.WriteLine("Resource Content:\n---\n" + respo + "\n---\n");
        //    GenericResource ps = new GenericResource();
        //    ps = ReadObjFromJSON<GenericResource>(respo);
        //    Console.WriteLine("Resource ID: " + ps.id);
        //    Console.WriteLine("Policy ID:   " + ps.policy);

        //    Console.WriteLine("\nGetting the resource's policy!");
        //    //json = EratosSignedRequest(accessId, accessSecret, "GET", ps.policy, "");
        //    ResourcePolicy jsonz = GetPolicy(respo, accessId, accessSecret);
        //    //Console.WriteLine("Policy Content:\n---\n"+json+"\n---");
        //    //ResourcePolicy pol = ReadObjFromJSON<ResourcePolicy>(json);

        //    AddUserToPolicy(jsonz, "https://staging.e-pn.io/users/f4jg4ai6guuiy2ldq26fpieq", accessId, accessSecret);
        //    Console.WriteLine("\nModifying the resource's policy!");
        //    //ResourcePolicyRule newRule = new ResourcePolicyRule();
        //    //newRule.actor = "https://staging.e-pn.io/users/f4jg4ai6guuiy2ldq26fpieq"; // testing@eratos.ai
        //    //newRule.effect = "Allow";
        //    //newRule.actions = new List<string>() {
        //    //    "Get", "Download"
        //    //};
        //    //pol.rules.Add(newRule);
        //    //string polReq = WriteObjToJSON<ResourcePolicy>(pol);
        //    //json = EratosSignedRequest(accessId, accessSecret, "PUT", pol.id, polReq);
        //    //pol = ReadObjFromJSON<ResourcePolicy>(json);
        //    //Console.WriteLine("New policy Content:\n---\n"+polReq+"\n---");

        //    //Console.WriteLine("Creating a task!");
        //    var task = CreateNewTask(resp.token, Priority.Low, ps.id);

        //    Console.WriteLine("Task:");
        //    Console.WriteLine("  id:            " + task.id);
        //    Console.WriteLine("  createdAt:     " + task.createdAt);
        //    Console.WriteLine("  lastUpdatedAt: " + task.lastUpdatedAt);
        //    Console.WriteLine("  priority:      " + task.priority);
        //    Console.WriteLine("  type:          " + task.type);
        //    Console.WriteLine("  state:         " + task.state);

        //    Console.WriteLine("Getting the task again!");


        //    try
        //    {
        //        var task2 = GetTask(resp.token, task.id);

        //        Console.WriteLine("Task2:");
        //        Console.WriteLine("  id:            " + task2.id);
        //        Console.WriteLine("  createdAt:     " + task2.createdAt);
        //        Console.WriteLine("  lastUpdatedAt: " + task2.lastUpdatedAt);
        //        Console.WriteLine("  priority:      " + task2.priority);
        //        Console.WriteLine("  type:          " + task2.type);
        //        Console.WriteLine("  state:         " + task2.state);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Something went wrong.");
        //    }
        //    finally
        //    {
        //        Console.WriteLine("Removing the task!");
        //        RemoveTask(resp.token, task.id);
        //        Console.WriteLine("\nRemoving the person resource!");
        //        EratosSignedRequest(accessId, accessSecret, "DELETE", ps.id, "");
        //    }

        //    //Console.WriteLine("Task2:");
        //    //Console.WriteLine("  id:            " + task2.id);
        //    //Console.WriteLine("  createdAt:     " + task2.createdAt);
        //    //Console.WriteLine("  lastUpdatedAt: " + task2.lastUpdatedAt);
        //    //Console.WriteLine("  priority:      " + task2.priority);
        //    //Console.WriteLine("  type:          " + task2.type);
        //    //Console.WriteLine("  state:         " + task2.state);

        //    //Console.WriteLine("Removing the task!");



        //    //Console.WriteLine("\nRemoving the person resource!");
        //    //EratosSignedRequest(accessId, accessSecret, "DELETE", ps.id, "");
        //}
    }
}
