using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FiberKartan.Database.Models
{
    [DataContract(Name = "saveMap")]
    public class SaveMap
    {
        [DataMember(Name = "publish")]
        public bool Publish { get; set; }

        [DataMember(Name = "mapTypeId")]
        public int MapTypeId { get; set; }

        [DataMember(Name = "previousVersion")]
        public int PreviousVersion { get; set; }

        [DataMember(Name = "layers")]
        public List<Layer> Layers { get; set; }
    }
}