using System.Runtime.Serialization;
using FiberKartan.Database.Internal;

namespace FiberKartan.Database.Models
{
    [DataContract(Name = "viewMapType")]
    public class ViewMapType
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ViewMapType()
        {
            // Nothing here.
        }

        /// <summary>
        /// Copy-constructor
        /// </summary>
        /// <param name="mapType">Database entity to copy</param>
        public ViewMapType(MapType mapType)
        {
            this.Id = mapType.Id;
            this.Title = mapType.Title;
            this.Creator = new Creator
            {
                Id = mapType.CreatorId,
                Name = mapType.User.Name
            };

            this.Municipality = new Municipality
            {
                Code = mapType.MunicipalityCode,
                Name = mapType.Municipality.Name,
                CenterLatitude = mapType.Municipality.CenterLatitude,
                CenterLongitude = mapType.Municipality.CenterLongitude,
                Referencesystem = mapType.Municipality.Referencesystem,
            };
        }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "creator")]
        public Creator Creator { get; set; }

        [DataMember(Name = "municipality")]
        public Municipality Municipality { get; set; }
    }
}