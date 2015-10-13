using System.Collections.Generic;
using System.Runtime.Serialization;

namespace FiberKartan.Database.Models
{
    [DataContract(Name = "saveMap")]
    public class SaveMap
    {
        [DataMember(Name = "previousVersion")]
        public int PreviousVersion { get; set; }

        [DataMember(Name = "publish")]
        public bool Publish { get; set; }
        
        [DataMember(Name = "layers")]
        public List<Layer> Layers { get; set; }
    }
}