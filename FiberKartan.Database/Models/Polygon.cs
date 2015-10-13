using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FiberKartan.Database.Models
{
    [DataContract(Name = "polygon")]
    public class Polygon
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "desc", EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(Name = "type")]
        public int Type { get; set; }

        [DataMember(Name = "coord")]
        public List<Coordinate> Coord { get; set; }

        [DataMember(Name = "settings", EmitDefaultValue = false)]
        public string Settings { get; set; }
    }
}