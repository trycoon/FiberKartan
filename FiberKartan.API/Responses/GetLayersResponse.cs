using System.Collections.Generic;
using System.Runtime.Serialization;
using FiberKartan.Database.Models;

namespace FiberKartan.API.Responses
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