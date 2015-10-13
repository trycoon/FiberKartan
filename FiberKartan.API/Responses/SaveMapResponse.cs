using System.Runtime.Serialization;

namespace FiberKartan.API.Responses
{
    [DataContract(Name = "saveMapResponse")]
    public class SaveMapResponse : Response
    {
        [DataMember(Name = "newVersion")]
        public int NewVersionNumber { get; set; }
    }
}