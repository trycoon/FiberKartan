using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using FiberKartan.REST.Models;

namespace FiberKartan.REST.Responses
{
    [DataContract]
    public class GetMapsResponse : Response
    {
        public GetMapsResponse()
        {
            this.Maps = new List<ViewMapType>();
        }

        [DataMember(Name = "maps")]
        public List<ViewMapType> Maps { get; set; }
    }
}