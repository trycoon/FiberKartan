using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FiberKartan.Database.Models
{
    [DataContract(Name = "incidentReport")]
    public class IncidentReport
    {
        [DataMember(Name = "mapTypeId")]
        public int MapTypeId { get; set; }

        [DataMember(Name = "version")]
        public int Version { get; set; }

        [DataMember(Name = "position")]
        public Coordinate Position { get; set; }

        [DataMember(Name = "estate")]
        public string Estate { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }
    }
}