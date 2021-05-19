using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.Json;

namespace UoM_Server.Models
{
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

    public struct ResourceTable
    {
        public int ResourceID { get; set; }
        public string EratosResourceID { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string Policy { get; set; }
        public string Geo { get; set; }
        public string Meta { get; set; }

        public ResourceTable(int resourceID, string eratosResourceID, string type, string name, DateTime date, string policy, string geo, string meta)
        {
            ResourceID = resourceID;
            EratosResourceID = eratosResourceID;
            Type = type;
            Name = name;
            Date = date;
            Policy = policy;
            Geo = geo;
            Meta = meta;
        }
    }

    [DataContract]
    public class PersonResource : Resource
    {
        [DataMember(Name = "description", EmitDefaultValue = false)]
        public string description { get; set; }


    }
}
