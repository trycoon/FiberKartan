using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace FiberKartan.Database.Models
{
    [DataContract(Name = "viewMap")]
    public class ViewMap
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public ViewMap()
        {
            // Nothing here.
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        /// <param name="map">Database Map</param>
        /// <param name="withLayerInfo">Om lagerinformation skall laddas in, detta är en rätt kostsam operation och bör bara användas då absolut nödvändigt.</param>
        public ViewMap(Map map, bool withLayerInfo)
        {
            this.MapType = new ViewMapType(map.MapType);
            this.Version = map.Ver;
            this.PreviousVersion = map.PreviousVer.HasValue ? map.PreviousVer.Value : -1;
            this.Created = map.Created;
            this.Creator = this.Creator = new Creator
            {
                Id = map.CreatorId,
                Name = map.User.Name
            };
            this.Published = map.Published;
            this.Views = map.Views;

            if (withLayerInfo)
            {
                this.Layers = loadLayerInfo(map.Layers);
            }
        }

        [DataMember(Name = "mapType")]
        public ViewMapType MapType { get; set; }

        [DataMember(Name = "version")]
        public int Version { get; set; }

        [DataMember(Name = "previousVersion")]
        public int PreviousVersion { get; set; }

        [DataMember(Name = "created")]
        public DateTime Created { get; set; }

        [DataMember(Name = "creator")]
        public Creator Creator { get; set; }

        [DataMember(Name = "published", EmitDefaultValue = false)]
        public DateTime? Published { get; set; }

        [DataMember(Name = "views")]
        public int Views { get; set; }

        [DataMember(Name = "layers")]
        public List<LayerInfo> Layers { get; set; }

        private static List<LayerInfo> loadLayerInfo(string layerString) {
            return null; //TODO: implement!
        }
    }
}