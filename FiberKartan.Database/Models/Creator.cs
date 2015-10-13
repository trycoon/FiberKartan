using System.Runtime.Serialization;

namespace FiberKartan.Database.Models
{
    [DataContract(Name = "creator")]
    public class Creator
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }
}
