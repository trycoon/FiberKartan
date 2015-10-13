using System.Runtime.Serialization;

namespace FiberKartan.Database.Models
{
    [DataContract(Name = "municipality")]
    public class Municipality
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "code")]
        public string Code { get; set; }

        [DataMember(Name = "centerLatitude")]
        public string CenterLatitude { get; set; }

        [DataMember(Name = "centerLongitude")]
        public string CenterLongitude { get; set; }

        [DataMember(Name = "referenceSystem")]
        public string Referencesystem { get; set; }
    }
}
