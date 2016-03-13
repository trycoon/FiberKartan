using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FiberKartan.Database.Models
{
    [DataContract(Name = "layer")]
    public class Layer
    {
        [DataMember(Name = "id")]
        public string Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "readonly")]
        public bool Readonly { get; set; }

        [DataMember(Name = "markers", EmitDefaultValue = false)]
        public List<Marker> Markers { get; set; }

        [DataMember(Name = "lines", EmitDefaultValue = false)]
        public List<Line> Lines { get; set; }

        [DataMember(Name = "polygons", EmitDefaultValue = false)]
        public List<Polygon> Polygons { get; set; }

        public override bool Equals(object obj)
        {
            var l = obj as Layer;
            if ((object)l == null)
            {
                return false;
            }

            // Return true if the fields match:
            return base.Equals(obj) && Id == l.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}