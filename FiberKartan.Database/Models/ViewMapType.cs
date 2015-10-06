using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

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

            this.Municipality = new FiberKartan.Database.Models.Municipality
            {
                Code = mapType.MunicipalityCode,
                CenterLatitude = mapType.Municipality.CenterLatitude,
                CenterLongitude = mapType.Municipality.CenterLongitude,
                Referencesystem = mapType.Municipality.Referencesystem,
            };
        }

        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "title")]
        public String Title { get; set; }

        [DataMember(Name = "creator")]
        public Creator Creator { get; set; }

        [DataMember(Name = "municipality")]
        public Municipality Municipality { get; set; }
    }
}