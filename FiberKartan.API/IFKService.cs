using System.ServiceModel;
using System.ServiceModel.Web;
using FiberKartan.Database.Models;
using FiberKartan.API.Responses;
using FiberKartan.API.Security;

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
namespace FiberKartan.API
{
    /// <summary>
    /// Möjliga felkoder man kan få när mar anropar REST-gränssnittet.
    /// </summary>
    public enum ErrorCode : int { Ok = 0, NotLoggedIn = 1, NoAccessToMap = 2, FailedToSave = 3, MissingInformation = 4, GenericError = 5 };

    [ServiceContract]
    public interface IFKService
    {
        /// <summary>
        /// Metod används för att logga in i tjänsten och få erfoderliga kakor satta som bevisar att man är inloggad.
        /// </summary>
        /// <param name="credentials">Inloggningsuppgifter</param>
        /// <returns>Uppgifter om inloggad användare vid lyckad inloggning</returns>
        [OperationContract]
        [WebInvoke(UriTemplate = "/sessions",
            Method = "POST",
            RequestFormat = WebMessageFormat.Json,
            BodyStyle = WebMessageBodyStyle.Bare)]
        GetLoginResponse Login(Credentials credentials);

        /// <summary>
        /// Metod används för att logga ut från tjänsten.
        /// </summary>
        [OperationContract]
        [WebInvoke(UriTemplate = "/sessions",
            Method = "DELETE")]
        void Logout();

        /// <summary>
        /// Metod som returnerar en lista på tillgängliga karttyper.
        /// </summary>
        /// <param name="orderBy">Fält som vi skall sortera efter, "Title" är standard</param>
        /// <param name="sortDescending">Sortera i stigande ordning, annars i fallande</param>
        /// <param name="offset">Från vilken post vi vill börja listan</param>
        /// <param name="count">Hur många poster vi är intresserade av</param>
        /// <returns>Lista på karttyper</returns>
        [OperationContract]
        [Security]
        [WebGet(UriTemplate = "/maptypes?orderBy={orderBy}&sortDescending={sortDescending}&offset={offset}&count={count}")]
        GetMapTypesResponse GetMapTypes(string orderBy = "Title", bool sortDescending = false, int offset = 0, int count = 20);

        /// <summary>
        /// Metod som returnerar en specifik karttyp.
        /// </summary>
        /// <param name="mapTypeId">Id på karttypen</param>
        /// <returns>Karttyp, eller null om karttyp inte finns</returns>
        [OperationContract]
        [Security]
        [WebGet(UriTemplate = "/maptypes/{mapTypeId}")]
        GetMapTypeResponse GetMapType(string mapTypeId);

        /// <summary>
        /// Metod som returnerar en lista på tillgängliga kartor.
        /// </summary>
        /// <param name="mapTypeId">Karta som efterfrågas</param>
        /// <param name="orderBy">Fält som vi skall sortera efter, "Ver" är standard</param>
        /// <param name="sortAscending">Sortera i fallande ordning, annars i stigande</param>
        /// <param name="offset">Från vilken post vi vill börja listan</param>
        /// <param name="count">Hur många poster vi är intresserade av</param>
        /// <returns>Lista på kartor</returns>
        [OperationContract]
        [Security]
        [WebGet(UriTemplate = "/maptypes/{mapTypeId}/maps?orderBy={orderBy}&sortAscending={sortAscending}&offset={offset}&count={count}")]
        GetMapsResponse GetMaps(string mapTypeId, string orderBy, bool sortAscending, int offset, int count);

        /// <summary>
        /// Metod som returnerar en karta.
        /// </summary>
        /// <param name="mapTypeId">Id på karttypen</param>
        /// <param name="version">Version av kartan som skall hämtas, 0 om senaste version</param>
        /// <returns>Karta, eller null om karta inte finns</returns>
        [OperationContract]
        [Security]
        [WebGet(UriTemplate = "/maptypes/{mapTypeId}/maps/{version}")]
        GetMapResponse GetMap(string mapTypeId, string version);

        /// <summary>
        /// Metod som sparar ner ändringar av en karta.
        /// </summary>
        /// <param name="mapContent">Kartans lager(markörer, kabelsträckor, osv)</param>
        /// <param name="mapTypeId">Id på karttypen</param>
        /// <returns>Returkod och id på nya markörer, sträckor, osv.</returns>
        [OperationContract]
        [Security]
        [WebInvoke(UriTemplate = "/maptypes/{mapTypeId}/maps",
         Method = "POST",
         RequestFormat = WebMessageFormat.Json,
         BodyStyle = WebMessageBodyStyle.WrappedRequest)]
        SaveMapResponse SaveMap(SaveMap mapContent, string mapTypeId);

        /// <summary>
        /// Metod som returnerar ett lager för en karta.
        /// </summary>
        /// <param name="mapTypeId">Id på karttypen</param>
        /// <param name="ids">Id på lagret som skall hämtas, kommaseparerad för flera</param>
        /// <param name="version">Version av kartan som skall användas, 0 om senaste version</param>
        /// <returns>Lista med kartlager</returns>
        [OperationContract]
        [Security]
        [WebGet(UriTemplate = "/maptypes/{mapTypeId}/maps/{version}/layers/{ids}")]
        GetLayersResponse GetLayers(string mapTypeId, string version, string ids);

        /// <summary>
        /// Metod som används för att rapportera fel på ett fibernätverk.
        /// </summary>
        /// <param name="mapTypeId">Id på karttypen</param>
        /// <param name="version">Version av kartan som skall användas, 0 om senaste version</param>
        /// <param name="report">Felrapport</param>
        [OperationContract]
        [Security]
        [WebInvoke(UriTemplate = "/maptypes/{mapTypeId}/maps/{version}/reportIncident",
         Method = "POST",
         RequestFormat = WebMessageFormat.Json)]
        Response ReportIncident(string mapTypeId, string version, IncidentReport report);

        /// <summary>
        /// Metod som används för att kontinuerligt anropa servern för att på så sätt påvisa att klienten ännu är ansluten.
        /// </summary>
        /// <returns>Ett dymmy-svar</returns>
        [OperationContract]
        [WebGet(UriTemplate = "/ping")]
        PingResponse Ping();
    }
}
