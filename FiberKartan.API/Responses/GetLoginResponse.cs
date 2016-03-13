using System;
using System.Runtime.Serialization;

namespace FiberKartan.API.Responses
{
    [DataContract]
    public class GetLoginResponse : Response
    {
        [DataMember(Name = "user")]
        public UserResponse User { get; set; }
    }
}