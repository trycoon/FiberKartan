using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FiberKartan.REST.Responses
{
    [DataContract]
    public class PingResponse
    {
        [DataMember(Name = "message")]
        public string Message { get; set; }
    }
}