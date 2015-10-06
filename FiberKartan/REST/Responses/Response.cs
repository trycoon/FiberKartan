using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FiberKartan.REST.Responses
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