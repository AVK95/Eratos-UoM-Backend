using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System.IO;
using System.IO.Compression;

namespace UoM_Server
{
    class RequestHandler
    {
        string usingToken = Config.Access_Token;


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
            if(type != null)
            {
                requestPara += "?type=" + type;
            }

            if(detail != null)
            {
                if(requestPara == "")
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
