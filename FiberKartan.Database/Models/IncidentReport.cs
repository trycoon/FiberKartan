using System.Runtime.Serialization;

namespace FiberKartan.Database.Models
{
    [DataContract(Name = "incidentReport")]
    public class IncidentReport
    {
        [DataMember(Name = "position")]
        public Coordinate Position { get; set; }

        [DataMember(Name = "estate")]
        public string Estate { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }
    }
}