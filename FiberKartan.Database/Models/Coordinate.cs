using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

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