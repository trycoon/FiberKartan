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
        /// <param name="mapContent">Kartans innehåll(markörer, kabelsträckor, osv)</param>
        /// <param name="publish">Om satt till sann så publiceras kartan också.</param>
        /// <returns>Returkod och id på nya markörer, sträckor, osv.</returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "/SaveMap",
         Method = "POST",
         RequestFormat = WebMessageFormat.Json,
         BodyStyle=WebMessageBodyStyle.WrappedRequest)]
        SaveMapResponse SaveMap(MapContent mapContent, bool publish = false);

        /// <summary>
        /// Metod som används för att rapportera fel på ett fibernätverk.
        /// </summary>
        /// <param name="report">Felrapport</param>
        [OperationContract]
        [WebInvoke(UriTemplate = "/ReportIncident",
         Method = "POST",
         RequestFormat = WebMessageFormat.Json)]
        Response ReportIncident(IncidentReport report);

        /// <summary>
        /// Metod som returnerar ett lager för en karta.
        /// </summary>
        /// <param name="mapId">Id på karta</param>
        /// <param name="names">Namn på lagret som skall hämtas, kommaseparerad för flera</param>
        /// <param name="ver">[Frivilligt] Version av kartan som skall användas, om inget versionsnummer anges så antas den senaste versionen</param>
        /// <returns>Lista med kartlager</returns>
        [OperationContract]
        [WebGet(UriTemplate = "/Layer/{mapId}/{names}/?ver={ver}")]
        GetLayerResponse GetLayer(string mapId, string names, string ver);

        /// <summary>
        /// Metod som returnerar beskrivningen för en markör.
        /// </summary>
        /// <param name="id">Markörens id</param>
        /// <returns>Markörens beskrivning, kan vara null om ingen finns</returns>
        [OperationContract]
        [WebGet(UriTemplate = "/MarkerDescription/{id}")]
        MarkerDescription MarkerDescription(string id);

        /// <summary>
        /// Metod som returnerar beskrivningen för en linje.
        /// </summary>
        /// <param name="id">Linjens id</param>
        /// <returns>Linjens beskrivning, kan vara null om ingen finns</returns>
        [OperationContract]
        [WebGet(UriTemplate = "/LineDescription/{id}")]
        LineDescription LineDescription(string id);

        /// <summary>
        /// Metod som returnerar beskrivningen för ett område.
        /// </summary>
        /// <param name="id">Områdets id</param>
        /// <returns>Områdets beskrivning, kan vara null om ingen finns</returns>
        [OperationContract]
        [WebGet(UriTemplate = "/RegionDescription/{id}")]
        RegionDescription RegionDescription(string id);

        /// <summary>
        /// Metod som används för att kontinuerligt anropa servern för att på så sätt påvisa att klienten ännu är ansluten.
        /// </summary>
        /// <returns>Ett dymmy-svar</returns>
        [OperationContract]
        [WebGet(UriTemplate = "/Ping")]
        PingResponse Ping();
    }

    #region RequestDataTypes
    [DataContract]
    public class MapContent
    {
        [DataMember]
        public int MapTypeId { get; set; }

        [DataMember]
        public int Ver { get; set; }

        [DataMember]
        public List<Marker> Markers { get; set; }

        [DataMember]
        public List<Cable> Cables { get; set; }

        [DataMember]
        public List<Region> Regions { get; set; }
    }

    [DataContract]
    public class IncidentReport
    {
        [DataMember]
        public int MapTypeId { get; set; }

        [DataMember]
        public int Ver { get; set; }

        [DataMember]
        public Coordinate Position { get; set; }

        [DataMember]
        public string Estate { get; set; }

        [DataMember]
        public string Description { get; set; }
    }
    #endregion RequestDataTypes

    #region ResponseDataTypes
    [DataContract]
    public class Response
    {
        [DataMember]
        public ErrorCode ErrorCode { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }
    }

    [DataContract]
    public class SaveMapResponse : Response
    {
        [DataMember]
        public int NewVersionNumber { get; set; }

        [DataMember]
        public List<Marker> AddedMarkers { get; set; }

        [DataMember]
        public List<Cable> AddedCables { get; set; }

        [DataMember]
        public List<Region> AddedRegions { get; set; }
    }

    [DataContract]
    public class GetLayerResponse : Response
    {
        public GetLayerResponse()
        {
            this.Layers = "{}";
        }

        [DataMember]
        public string Layers { get; set; }
    }

    [DataContract]
    public class PingResponse
    {
        [DataMember]
        public string Message { get; set; }
    }
    #endregion ResponseDataTypes

    #region ModelDataTypes
    [DataContract]
    public class Marker
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int Uid { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Desc { get; set; }

        [DataMember]
        public int MarkId { get; set; }

        [DataMember]
        public string Lat { get; set; }

        [DataMember]
        public string Lng { get; set; }

        [DataMember]
        public int Settings { get; set; }

        [DataMember]
        public string OptionalInfo { get; set; }
    }

    [DataContract]
    public class Cable
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int Uid { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Desc { get; set; }

        [DataMember]
        public string Color { get; set; }

        [DataMember]
        public string Width { get; set; }

        [DataMember]
        public List<Coordinate> Coord { get; set; }

        [DataMember]
        public int Type { get; set; }
    }

    [DataContract]
    public class Region
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int Uid { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Desc { get; set; }

        [DataMember]
        public string BorderColor { get; set; }

        [DataMember]
        public string FillColor { get; set; }

        [DataMember]
        public List<Coordinate> Coord { get; set; }
    }

    [DataContract]
    public class Coordinate
    {
        [DataMember]
        public string Lat { get; set; }

        [DataMember]
        public string Lng { get; set; }
    }
    
    public class MarkerDescription
    {
        [DataMember]
        public string Desc { get; set; }
    }

    public class LineDescription
    {
        [DataMember]
        public string Desc { get; set; }
    }

    public class RegionDescription
    {
        [DataMember]
        public string Desc { get; set; }
    }
    #endregion ModelDataTypes
}
