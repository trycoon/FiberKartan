using System.Runtime.Serialization;
using FiberKartan.Database.Models;

namespace FiberKartan.API.Responses
{
    [DataContract]
    public class GetMapResponse : Response
    {
        [DataMember(Name = "map")]
        public ViewMap Map { get; set; }
    }
}