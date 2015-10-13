using System.Collections.Generic;
using System.Runtime.Serialization;
using FiberKartan.Database.Models;

namespace FiberKartan.API.Responses
{
    [DataContract]
    public class GetMapsResponse : Response
    {
        public GetMapsResponse()
        {
            this.Maps = new List<ViewMap>();
        }

        [DataMember(Name = "maps")]
        public List<ViewMap> Maps { get; set; }
    }
}