using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace UoM_Server.Models
{
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

}
