using System.Runtime.Serialization;

namespace FiberKartan.API.Responses
{
    [DataContract]
    public class Response
    {
        [DataMember(Name = "errorcode", EmitDefaultValue = false)]
        public ErrorCode ErrorCode { get; set; }

        [DataMember(Name = "errormessage", EmitDefaultValue = false)]
        public string ErrorMessage { get; set; }
    }
}