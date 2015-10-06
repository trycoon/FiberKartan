using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using FiberKartan.REST.Models;
using FiberKartan.REST.Responses;

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
        /// Metod som returnerar en lista på tillgängliga kartor.
        /// </summary>
        /// <returns>Lista på kartor</returns>
        [OperationContract]
        [WebGet(UriTemplate = "/maps")]
        GetMapsResponse GetMaps();

        /// <summary>
        /// Metod som returnerar en karta.
        /// </summary>
        /// <param name="mapTypeId">Id på karta</param>
        /// <param name="version">[Frivilligt] Version av kartan som skall hämtas, om inget versionsnummer anges så antas den senaste versionen</param>
        /// <returns>Karta</returns>
        [OperationContract]
        [WebGet(UriTemplate = "/maps/{mapTypeId}?ver={version}")]
        GetMapResponse GetMap(string mapTypeId, string version);

        /// <summary>
        /// Metod som sparar ner ändringar av en karta.
        /// </summary>
        /// <param name="mapContent">Kartans lager(markörer, kabelsträckor, osv)</param>
        /// <param name="mapTypeId">Id på karta</param>
        /// <returns>Returkod och id på nya markörer, sträckor, osv.</returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "/maps/{mapTypeId}",
         Method = "POST",
         RequestFormat = WebMessageFormat.Json,
         BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        SaveMapResponse SaveMap(SaveMap mapContent, string mapTypeId);

        /// <summary>
        /// Metod som returnerar ett lager för en karta.
        /// </summary>
        /// <param name="mapTypeId">Id på karta</param>
        /// <param name="ids">Id på lagret som skall hämtas, kommaseparerad för flera</param>
        /// <param name="ver">[Frivilligt] Version av kartan som skall användas, om inget versionsnummer anges så antas den senaste versionen</param>
        /// <returns>Lista med kartlager</returns>
        [OperationContract]
        [WebGet(UriTemplate = "/maps/{mapTypeId}/layers/{ids}?ver={version}")]
        GetLayersResponse GetLayers(string mapTypeId, string ids, string version);

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
        /// Metod som används för att kontinuerligt anropa servern för att på så sätt påvisa att klienten ännu är ansluten.
        /// </summary>
        /// <returns>Ett dymmy-svar</returns>
        [OperationContract]
        [WebGet(UriTemplate = "/ping")]
        PingResponse Ping();
    }
}
