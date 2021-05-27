using System.Runtime.Serialization;

namespace EratosUoMBackend.Models
{
    [DataContract]
    public class User
    {
        [DataMember(Name = "auth0Id", EmitDefaultValue = false)]
        public string auth0Id { get; set; }

        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string id { get; set; }

        [DataMember(Name = "info", EmitDefaultValue = false)]
        public string info { get; set; }

        [DataMember(Name = "resourcePolicy", EmitDefaultValue = false)]
        public string resourcePolicy { get; set; }

        [DataMember(Name = "termsLastAcceptedDate", EmitDefaultValue = false)]
        public string termsLastAcceptedDate { get; set; }

        [DataMember(Name = "termsLastAcceptedVersion", EmitDefaultValue = false)]
        public int termsLastAcceptedVersion { get; set; }
    }

    public struct UserTable
    {
        public int UserID { get; set; }
        public string EratosUserID { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Auth0ID { get; set; }
        public string CreatedAt { get; set; }
        public string Info { get; set; }
        public bool isAdmin { get; set; }

        public UserTable(int userID, string eratosUserID, string email, string name, string auth0ID, string createdAt, string info, bool isAdmin)
        {
            UserID = userID;
            EratosUserID = eratosUserID;
            Email = email;
            Name = name;
            Auth0ID = auth0ID;
            CreatedAt = createdAt;
            Info = info;
            this.isAdmin = isAdmin;
        }
    }
}
