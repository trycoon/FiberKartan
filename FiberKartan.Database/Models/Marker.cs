using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FiberKartan.Database.Models
{
    [DataContract(Name = "marker")]
    public class Marker
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "desc", EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(Name = "type")]
        public int Type { get; set; }

        [DataMember(Name = "lat")]
        public Double Latitude { get; set; }

        [DataMember(Name = "lng")]
        public Double Longitude { get; set; }

        [DataMember(Name = "settings", EmitDefaultValue = false)]
        public string Settings { get; set; }
    }
}