using System.Collections.Generic;
using System.Runtime.Serialization;
using FiberKartan.Database.Models;

namespace FiberKartan.API.Responses
{
    [DataContract]
    public class GetMapTypesResponse : Response
    {
        public GetMapTypesResponse()
        {
            this.MapTypes = new List<ViewMapType>();
        }

        [DataMember(Name = "maptypes")]
        public List<ViewMapType> MapTypes { get; set; }
    }
}