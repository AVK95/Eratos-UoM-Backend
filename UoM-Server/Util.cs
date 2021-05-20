using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UoM_Server.Models;

namespace UoM_Server
{
    public class Util
    {
        #region Hash Functions
        public static string SHA256HashBytesHex(byte[] content)
        {
            byte[] hbytes;
            using (SHA256 mySHA256 = SHA256.Create())
            {
                hbytes = mySHA256.ComputeHash(content);
            }
            return BitConverter.ToString(hbytes).Replace("-", "").ToLower();
        }
        public static string SHA256HashStringHex(string content)
        {
            byte[] hbytes;
            byte[] cbytes = Encoding.UTF8.GetBytes(content);
            using (SHA256 mySHA256 = SHA256.Create())
            {
                hbytes = mySHA256.ComputeHash(cbytes);
            }
            return BitConverter.ToString(hbytes).Replace("-", "").ToLower();
        }
        public static string HMACHashStringHex(string content, string keyB64)
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
        #endregion

        #region Json Converter
        public static string WriteObjToJSON<T>(T obj)
        {
            var ms = new MemoryStream();
            var ser = new DataContractJsonSerializer(typeof(T));
            ser.WriteObject(ms, obj);
            byte[] json = ms.ToArray();
            ms.Close();
            return Encoding.UTF8.GetString(json, 0, json.Length);
        }
        public static T ReadObjFromJSON<T>(string json) where T : class, new()
        {
            var dObj = new T();
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
            var ser = new DataContractJsonSerializer(dObj.GetType());
            dObj = ser.ReadObject(ms) as T;
            ms.Close();
            return dObj;
        }
        #endregion

        #region Database Model Mapping Functions

        public static UserTable MAP_TO_TABLE(User user, string userName)
        {
            DateTime now = DateTime.Now;
            UserTable userTable = new UserTable(0, user.id, "", userName, user.auth0Id, now.ToString(), user.info,false);
            return userTable;
        }

        public static ResourceTable MAP_TO_TABLE(Resource rsc)
        {
            DateTime now = DateTime.Now;
            ResourceTable rscTable = new ResourceTable(0, rsc.id, rsc.type,rsc.name, now.ToString(), rsc.policy, rsc.geo, rsc.data);
            return rscTable;
        }

        public static TaskTable MAP_TO_TABLE(GNTaskResponse task, int userUri, int orderId, string name)
        {
            TaskTable taskTable = new TaskTable(0, task.id, task.createdAt, task.lastUpdatedAt, task.startedAt, task.endedAt, task.priority,name, task.state, task.type, task.meta.resource, task.error, userUri, orderId);
            return taskTable;
        }
        #endregion
    }
}
