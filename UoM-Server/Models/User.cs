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
    public class User
    {
        [DataMember (Name = "auth0Id", EmitDefaultValue = false)]
        public string auth0Id { get; set; }

        [DataMember (Name = "id", EmitDefaultValue = false)]
        public string id { get; set; }

        [DataMember (Name = "info", EmitDefaultValue = false)]
        public string info { get; set; }

        [DataMember (Name = "resourcePolicy", EmitDefaultValue = false)]
        public string resourcePolicy { get; set; }

        [DataMember (Name = "termsLastAcceptedDate", EmitDefaultValue = false)]
        public string termsLastAcceptedDate { get; set; }

        [DataMember (Name = "termsLastAcceptedVersion", EmitDefaultValue = false)]
        public int termsLastAcceptedVersion { get; set; }
    }
}
