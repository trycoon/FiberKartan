using System;
using System.Runtime.Serialization;

namespace FiberKartan.API.Responses
{
    [DataContract]
    public class UserResponse : Response
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "lastLoggedOn", IsRequired = false, EmitDefaultValue = false)]
        public string LastLoggedOn { get; set; }

        [DataMember(Name = "isAdmin")]
        public bool IsAdmin { get; set; }

        [DataMember(Name = "lastNotificationMessage")]
        public int LastNotificationMessage { get; set; }
    }
}