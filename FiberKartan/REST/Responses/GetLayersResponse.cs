using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using FiberKartan.REST.Models;

namespace FiberKartan.REST.Responses
{
    [DataContract]
    public class GetLayersResponse : Response
    {
        public GetLayersResponse()
        {
            this.Layers = new List<Layer>();
        }

        [DataMember(Name = "layers")]
        public List<Layer> Layers { get; set; }
    }
}