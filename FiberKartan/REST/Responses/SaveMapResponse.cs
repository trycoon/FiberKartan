using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FiberKartan.REST.Responses
{
    [DataContract(Name = "saveMapResponse")]
    public class SaveMapResponse : Response
    {
        [DataMember(Name = "newVersion")]
        public int NewVersionNumber { get; set; }
    }
}