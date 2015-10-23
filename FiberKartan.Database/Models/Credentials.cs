using System.Runtime.Serialization;

namespace FiberKartan.Database.Models
{
    [DataContract(Name = "credentials")]
    public class Credentials
    {
        [DataMember(Name = "username")]
        public string Username { get; set; }

        [DataMember(Name = "password")]
        public string Password { get; set; }
    }
}