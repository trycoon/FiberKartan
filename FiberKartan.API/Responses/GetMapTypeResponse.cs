using System.Runtime.Serialization;
using FiberKartan.Database.Models;

namespace FiberKartan.API.Responses
{
    [DataContract]
    public class GetMapTypeResponse : Response
    {
        [DataMember(Name = "maptype")]
        public ViewMapType MapType { get; set; }
    }
}