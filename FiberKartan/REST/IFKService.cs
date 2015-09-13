using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

/*
Copyright (c) 2012, Henrik Östman.

This file is part of FiberKartan.

FiberKartan is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

FiberKartan is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with FiberKartan.  If not, see <http://www.gnu.org/licenses/>.
*/
namespace FiberKartan.REST
{
    /// <summary>
    /// Möjliga felkoder man kan få när mar anropar REST-gränssnittet.
    /// </summary>
    public enum ErrorCode : int { OK = 0, NotLoggedIn = 1, NoAccessToMap = 2, FailedToSave = 3, MissingInformation = 4 };

    [ServiceContract]
    public interface IFKService
    {
        /// <summary>
        /// Metod som sparar ner ändringar av en karta.
        /// </summary>
        /// <param name="mapContent">Kartans lager(markörer, kabelsträckor, osv)</param>
        /// <param name="mapTypeId">Id på karta</param>
        /// <returns>Returkod och id på nya markörer, sträckor, osv.</returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "/map/{mapTypeId}",
         Method = "POST",
         RequestFormat = WebMessageFormat.Json,
         BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        SaveMapResponse SaveMap(SaveMap mapContent, string mapTypeId);

        /// <summary>
        /// Metod som returnerar en karta.
        /// </summary>
        /// <param name="mapTypeId">Id på karta</param>
        /// <param name="version">[Frivilligt] Version av kartan som skall hämtas, om inget versionsnummer anges så antas den senaste versionen</param>
        /// <returns>Karta</returns>
        [OperationContract]
        [WebGet(UriTemplate = "/map/{mapTypeId}?ver={version}")]
        ViewMap GetMap(string mapTypeId, string version);

        /// <summary>
        /// Metod som används för att rapportera fel på ett fibernätverk.
        /// </summary>
        /// <param name="report">Felrapport</param>
        [OperationContract]
        [WebInvoke(UriTemplate = "/reportIncident",
         Method = "POST",
         RequestFormat = WebMessageFormat.Json)]
        Response ReportIncident(IncidentReport report);

        /// <summary>
        /// Metod som returnerar ett lager för en karta.
        /// </summary>
        /// <param name="mapTypeId">Id på karta</param>
        /// <param name="ids">Id på lagret som skall hämtas, kommaseparerad för flera</param>
        /// <param name="ver">[Frivilligt] Version av kartan som skall användas, om inget versionsnummer anges så antas den senaste versionen</param>
        /// <returns>Lista med kartlager</returns>
        [OperationContract]
        [WebGet(UriTemplate = "/map/{mapTypeId}/layers/{ids}?ver={version}")]
        GetLayersResponse GetLayers(string mapTypeId, string ids, string version);

        /// <summary>
        /// Metod som används för att kontinuerligt anropa servern för att på så sätt påvisa att klienten ännu är ansluten.
        /// </summary>
        /// <returns>Ett dymmy-svar</returns>
        [OperationContract]
        [WebGet(UriTemplate = "/ping")]
        PingResponse Ping();
    }

    #region Requests
    [DataContract]
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
    #endregion Requests

    #region Responses
    [DataContract]
    public class PingResponse
    {
        [DataMember(Name = "message")]
        public string Message { get; set; }
    }

    [DataContract]
    public class Response
    {
        [DataMember(Name = "errorcode", EmitDefaultValue = false)]
        public ErrorCode ErrorCode { get; set; }

        [DataMember(Name = "errormessage", EmitDefaultValue = false)]
        public string ErrorMessage { get; set; }
    }

    [DataContract(Name = "saveMapResponse")]
    public class SaveMapResponse : Response
    {
        [DataMember(Name = "newVersion")]
        public int NewVersionNumber { get; set; }
    }

    [DataContract(Name = "viewMap")]
    public class ViewMap
    {
        [DataMember(Name = "mapTypeId")]
        public int MapTypeId { get; set; }

        [DataMember(Name = "version")]
        public int Version { get; set; }

        [DataMember(Name = "previousVersion")]
        public int PreviousVersion { get; set; }

        [DataMember(Name = "created")]
        public DateTime Created { get; set; }

        [DataMember(Name = "published", EmitDefaultValue = false)]
        public DateTime Published { get; set; }

        [DataMember(Name = "views")]
        public int Views { get; set; }

        [DataMember(Name = "layers")]
        public List<LayerInfo> Layers { get; set; }
    }

    [DataContract(Name = "layerInfo")]
    public class LayerInfo
    {
        [DataMember(Name = "id")]
        public String Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }
    }

    [DataContract]
    public class GetLayersResponse : Response
    {
        public GetLayersResponse()
        {
            this.Layers = new List<Layer>();
        }

        [DataMember(Name = "layers")]
        public List<Layer> Layers { get; set; }
    }

    [DataContract(Name = "layer")]
    public class Layer
    {
        [DataMember(Name = "id")]
        public String Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "readonly")]
        public bool Readonly { get; set; }

        [DataMember(Name = "markers", EmitDefaultValue = false)]
        public List<Marker> Markers { get; set; }

        [DataMember(Name = "lines", EmitDefaultValue = false)]
        public List<Line> Lines { get; set; }

        [DataMember(Name = "polygons", EmitDefaultValue = false)]
        public List<Polygon> Polygons { get; set; }

        public override bool Equals(System.Object obj)
        {
            Layer l = obj as Layer;
            if ((object)l == null)
            {
                return false;
            }

            // Return true if the fields match:
            return base.Equals(obj) && Id == l.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }

    [DataContract(Name = "coord")]
    public class Coordinate
    {
        [DataMember(Name = "lat")]
        public string Latitude { get; set; }

        [DataMember(Name = "lng")]
        public string Longitude { get; set; }
    }

    [DataContract(Name = "marker")]
    public class Marker
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "desc", EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(Name = "type")]
        public string Type { get; set; }

        [DataMember(Name = "lat")]
        public string Latitude { get; set; }

        [DataMember(Name = "lng")]
        public string Longitude { get; set; }

        [DataMember(Name = "settings", EmitDefaultValue = false)]
        public string Settings { get; set; }
    }

    [DataContract(Name = "line")]
    public class Line
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "desc", EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(Name = "color", EmitDefaultValue = false)]
        public string Color { get; set; }

        [DataMember(Name = "width", EmitDefaultValue = false)]
        public string Width { get; set; }

        [DataMember(Name = "coord")]
        public List<Coordinate> Coord { get; set; }

        [DataMember(Name = "settings", EmitDefaultValue = false)]
        public string Settings { get; set; }
    }

    [DataContract(Name = "polygon")]
    public class Polygon
    {
        [DataMember(Name = "id")]
        public int Id { get; set; }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "desc", EmitDefaultValue = false)]
        public string Description { get; set; }

        [DataMember(Name = "border", EmitDefaultValue = false)]
        public string BorderColor { get; set; }

        [DataMember(Name = "fill", EmitDefaultValue = false)]
        public string FillColor { get; set; }

        [DataMember(Name = "coord")]
        public List<Coordinate> Coord { get; set; }

        [DataMember(Name = "settings", EmitDefaultValue = false)]
        public string Settings { get; set; }
    }
    #endregion Responses
}
