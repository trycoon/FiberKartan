using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using FiberKartan.REST.Models;

namespace FiberKartan.REST.Responses
{
    [DataContract]
    public class GetMapResponse : Response
    {
        [DataMember(Name = "map")]
        public ViewMap Map { get; set; }
    }
}