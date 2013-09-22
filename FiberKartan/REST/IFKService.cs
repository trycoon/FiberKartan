using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

/*
The zlib/libpng License
Copyright (c) 2012 Henrik Östman

This software is provided 'as-is', without any express or implied warranty. In no event will the authors be held liable for any damages arising from the use of this software.
Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/
namespace FiberKartan.REST
{
    /// <summary>
    /// Möjliga felkoder man kan få när mar sparar ner en karta.
    /// </summary>
    public enum SaveMapErrorCode : int { OK = 0, NotLoggedIn = 1, NoAccessToMap = 2, FailedToSave = 3};

    [ServiceContract]
    public interface IFKService
    {
        /// <summary>
        /// Metod som sparar ner ändringar av en karta.
        /// </summary>
        /// <param name="mapContent">Kartans innehåll(markörer, kabelsträckor, osv)</param>
        /// <returns>Returkod och id på nya markörer, sträckor, osv.</returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "/SaveChanges",
         Method = "POST",
         RequestFormat = WebMessageFormat.Json)]
        SaveResponse SaveChanges(MapContent mapContent);

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

    [DataContract]
    public class SaveResponse
    {
        [DataMember]
        public SaveMapErrorCode ErrorCode { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public int NewVersionNumber { get; set; }

        [DataMember]
        public List<Marker> AddedMarkers { get; set; }

        [DataMember]
        public List<Cable> AddedCables { get; set; }

        [DataMember]
        public List<Region> AddedRegions { get; set; }
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

    [DataContract]
    public class PingResponse
    {
        [DataMember]
        public string Message { get; set; }
    }
}
