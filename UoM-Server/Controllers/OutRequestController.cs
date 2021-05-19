using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System.IO;
using System.Text.Json;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.IO.Compression;
using System.Text.Json.Serialization;
using System.Text;
using System.Collections.Generic;
using UoM_Server.Models;


namespace UoM_Server.Controllers
{

    class OutRequestController
    {
        static string SendRequest(HttpWebRequest request)
        {
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


        #region Resource
        public Resource CreateResource(string type, string name)
        {
            Resource res = new Resource();
            res.type = type;
            res.name = name;

            string req = Util.WriteObjToJSON<Resource>(res);
            HttpWebRequest request = EratosSignedRequest(Config.accessId, Config.accessSecret, "POST", "https://staging.e-pn.io/resources", req);
            string json = SendRequest(request);
            Resource resultRes = Util.ReadObjFromJSON<Resource>(json);
            return resultRes;
        }

        public Resource GetResource(string resourceUri)
        {
            HttpWebRequest request = EratosSignedRequest(Config.accessId, Config.accessSecret, "GET", resourceUri, "");
            request.Accept = "application/json";
            string json = SendRequest(request);
            Resource res = Util.ReadObjFromJSON<Resource>(json);
            return res;
        }

        public string ShowResourceVersions(string resourceUri)
        {
            HttpWebRequest request = EratosSignedRequest(Config.accessId, Config.accessSecret, "GET", resourceUri + "/versions", "");
            string json = SendRequest(request);
            return json;
        }

        public string DeleteResource(string resourceUri)
        {
            HttpWebRequest request = EratosSignedRequest(Config.accessId, Config.accessSecret, "DELETE", resourceUri, "");
            string json = SendRequest(request);
            return json;
        }
        #endregion

        #region Policy
        public ResourcePolicy GetPolicy(string resourceId)
        {
            Resource res = GetResource(resourceId);
            HttpWebRequest request = EratosSignedRequest(Config.accessId, Config.accessSecret, "GET", res.policy, "");
            string json = SendRequest(request);
            ResourcePolicy pol = Util.ReadObjFromJSON<ResourcePolicy>(json);
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
            string polReq = Util.WriteObjToJSON<ResourcePolicy>(pol);
            HttpWebRequest request = EratosSignedRequest(Config.accessId, Config.accessSecret, "PUT", pol.id, polReq);
            string response = SendRequest(request);
            return response;
        }

        #endregion

        #region Geometry
        public string FetchGeometry(string resourceId, string type = null, string detail = null) 
        {
            // Having some error when giving parameters, probably because some process is needed for the parameters in the signed header.
            // It shouldn't matter because we're not going to call it when processing tasks. This function is just for test.
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

            HttpWebRequest request = EratosSignedRequest(Config.accessId, Config.accessSecret, "GET", Config.Primary_Node_Domain + "resources/" + resourceId + "/geo" + requestPara, "");
            string geo = SendRequest(request);
            return geo;
        }

        public string UpdateGeometry(string resourceUri,string geometry)
        {
            HttpWebRequest request = EratosSignedRequest(Config.accessId, Config.accessSecret, "POST", resourceUri + "/geo", geometry, "application/vnd.eratos.geo+wkt", "identity");
            string response = SendRequest(request);
            return response;
        }

        #endregion

        #region Task

        public GNTaskResponse CreateNewTask(Priority priority, string resource)
        {
            string token = GetToken();
            GNTaskRequest req = new GNTaskRequest();
            req.priority = priority.ToString();
            req.type = "GenerateClimateInfo";
            req.meta = new GNTaskMeta();
            req.meta.resource = resource;
            byte[] body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize<GNTaskRequest>(req));
            string json = PostWithToken(token, Config.Gateway_Node_Domain + "/tasks", body);
            return JsonSerializer.Deserialize<GNTaskResponse>(json);
        }
        public GNTaskResponse GetTask(string id)
        {
            string token = GetToken();
            string json = GetWithToken(token, Config.Gateway_Node_Domain + "/tasks/" + id);

            return JsonSerializer.Deserialize<GNTaskResponse>(json);
        }

        public string RemoveTask(string id)
        {
            string token = GetToken();
            string response = DeleteWithToken(token, Config.Gateway_Node_Domain + "/tasks/" + id);
            return response;
        }

        public string GetToken()
        {
            HttpWebRequest request = EratosSignedRequest(Config.accessId, Config.accessSecret, "GET", Config.Tracker_Node_Domain + "token/node", "");
            string json = SendRequest(request);
            TokenNodeResponse tokenResponse = Util.ReadObjFromJSON<TokenNodeResponse>(json);
            return tokenResponse.token;
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

        #endregion

        #region Authorisation
        static HttpWebRequest EratosSignedRequest(string accessId, string accessSecret, string method, string uri, string reqJSON, string contentType = "application/json", string encoding = null)
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
                canonReq += "content-type:" + contentType + "\n";
                if (encoding != null)
                {
                    canonReq += "content-encoding:" + encoding + "\n";
                }
            }

            string bodyHash = Util.SHA256HashBytesHex(body);
            canonReq += bodyHash;
            string signedMAC = Util.HMACHashStringHex(canonReq, accessSecret);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = method;
            if (body.Length > 0)
            {
                request.ContentLength = body.Length;
                request.ContentType = contentType;
                if (encoding != null)
                {
                    request.Headers.Add("Content-Encoding", encoding);
                }
            }
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            request.Headers.Set("X-Eratos-Date", curTime);
            string signedHeaders = "host;x-eratos-date";
            if (body.Length > 0)
            {
                signedHeaders += ";content-type";
                if (encoding != null)
                {
                    signedHeaders += ";content-encoding";
                }
            }
            request.Headers.Set("Authorization", "ERATOS-HMAC-SHA256 Credential=" + accessId + ", SignedHeaders=" + signedHeaders + ", Signature=" + signedMAC);

            if (body.Length > 0)
            {
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(body, 0, body.Length);
                dataStream.Close();
            }
            return request;
        }

        public static string GET_USER_DETAIL(string token)
        {
            var client = new RestClient(Config.Tracker_Node_Domain);
            client.AddDefaultHeader("Authorization", string.Format("Bearer {0}", token));

            var request = new RestRequest("auth/me", Method.GET, DataFormat.None);

            var response = client.Execute(request);
            return response.Content;
        }
        #endregion
 
    }
}
