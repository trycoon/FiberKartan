using System;
using System.Runtime.Serialization;

namespace FiberKartan.Database.Models
{
    [DataContract(Name = "coord")]
    public class Coordinate
    {
        [DataMember(Name = "lat")]
        public Double Latitude { get; set; }

        [DataMember(Name = "lng")]
        public Double Longitude { get; set; }
    }
}