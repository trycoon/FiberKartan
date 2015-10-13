using System.Runtime.Serialization;

namespace FiberKartan.API.Responses
{
    [DataContract]
    public class PingResponse
    {
        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}