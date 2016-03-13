using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Threading;
using System.Web;
using System.Web.Security;
using FiberKartan.Database;
using FiberKartan.Database.Models;
using FiberKartan.API.Responses;
using FiberKartan.API.Security;
using log4net;

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

// The following line sets the default namespace for DataContract serialized typed to be ""
[assembly: ContractNamespace("", ClrNamespace = "FiberKartan.API")]

namespace FiberKartan.API
{
    // AspNetCompatibility is needed to access and set Cookies!
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class FKService : IFKService
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Database reference.
        /// </summary>
        private readonly MsSQL db = new MsSQL(); //TODO: Switch to interface instead of concrete class.

        /// <summary>
        /// Default constructor
        /// </summary>
        public FKService()
        {
            // Nothing here.
        }
        /// <summary>
        /// Method för att logga in användare.
        /// </summary>
        /// <param name="credentials">Inloggningsuppgifter</param>
        /// <returns>Användaruppgifter om lyckad inloggning</returns>
        public GetLoginResponse Login(Credentials credentials)
        {
            var response = new GetLoginResponse();

            try
            {
                var user = SecurityHandler.Validate(credentials.Username, credentials.Password);

                var ticket = new FormsAuthenticationTicket(1, "username", DateTime.Now,
                   DateTime.Now.AddHours(5), credentials.IsPersistent, "{\"user\": \"aaa\"}", FormsAuthentication.FormsCookiePath);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket))
                {
                    Path = FormsAuthentication.FormsCookiePath,
                    Expires = DateTime.Now.AddHours(5),
                    HttpOnly = true     // Prevent Javascript on client to access cookie.
                };

                HttpContext.Current.Response.AppendCookie(cookie);
                log.InfoFormat("User successfully logged in with id: {0}, username: {1}, and name: {2}.", user.Id, user.Username, user.Name);

                db.SetLastLoggedOn(user.Id);

                response.User = new UserResponse()
                {
                    Id = user.Id,
                    Name = user.Name,
                    Username = user.Username,
                    LastLoggedOn = user.LastLoggedOn.ToLocalTime().ToString(),
                    IsAdmin = user.IsAdmin,
                    LastNotificationMessage = user.LastNotificationMessage
                };
            }
            catch (SecurityTokenException ex)
            {
                Thread.Sleep(100);  // Fördröjning så att man inte kan bygga ett program som söker efter lösenord.
                response.ErrorCode = ErrorCode.NotLoggedIn;
                response.ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                Thread.Sleep(100);  // Fördröjning så att man inte kan bygga ett program som söker efter lösenord.
                response.ErrorCode = ErrorCode.GenericError;
                response.ErrorMessage = "Fel vid validering av användarnamn och lösenord, var god försök igen senare.";
            }

            return response;
        }

        /// <summary>
        /// Metod används för att logga ut från tjänsten.
        /// </summary>
        public void Logout()
        {
            // db.Logout(1);
           // log.InfoFormat("User successfully logged out with id: {0}, username: {1}, and name: {2}.", user.Id, user.Username, user.Name);
            FormsAuthentication.SignOut();
        }

        /// <summary>
        /// Metod som returnerar en lista på tillgängliga karttyper.
        /// </summary>
        /// <param name="orderBy">Fält som vi skall sortera efter, "Title" är standard</param>
        /// <param name="sortDescending">Sortera i stigande ordning, annars i fallande</param>
        /// <param name="offset">Från vilken post vi vill börja listan</param>
        /// <param name="count">Hur många poster vi är intresserade av</param>
        /// <returns>Lista på karttyper</returns>
        public GetMapTypesResponse GetMapTypes(string orderBy, bool sortDescending, int offset, int count)
        {
            var response = new GetMapTypesResponse();

            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                response.ErrorCode = ErrorCode.NotLoggedIn;
                response.ErrorMessage = "Du måste vara inloggad för att hämta lista på karttyper.";
                log.Info(response.ErrorMessage);

                return response;
            }

            if (string.IsNullOrEmpty(orderBy))
            {
                orderBy = "Title";
            }
            if (count < 1)
            {
                count = 20;
            }
            //TODO: Fixa userId
            response.MapTypes = db.GetMapTypes(1, orderBy, sortDescending, offset, count);

            return response;
        }

        /// <summary>
        /// Metod som returnerar en specifik karttyp.
        /// </summary>
        /// <param name="mapTypeId">Id på karttypen</param>
        /// <returns>Karttyp, eller null om karttyp inte finns</returns>
        public GetMapTypeResponse GetMapType(string mapTypeId)
        {
            var response = new GetMapTypeResponse();

            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                response.ErrorCode = ErrorCode.NotLoggedIn;
                response.ErrorMessage = "Du måste vara inloggad för att hämta karttyp.";
                log.Info(response.ErrorMessage);

                return response;
            }

            var mapId = 0;
            if (!int.TryParse(mapTypeId, out mapId))
            {
                response.ErrorCode = ErrorCode.MissingInformation;
                response.ErrorMessage = "mapTypeId saknas eller är ogilltigt i anrop.";
                log.Info(response.ErrorMessage + " mapTypeId=" + mapTypeId);

                return response;
            }

            //TODO: Fixa userId
            response.MapType = db.GetMapType(1, mapId);

            return response;
        }

        /// <summary>
        /// Metod som returnerar en lista på tillgängliga kartor.
        /// </summary>
        /// <param name="mapTypeId">Karta som efterfrågas</param>
        /// <param name="orderBy">Fält som vi skall sortera efter, "Ver" är standard</param>
        /// <param name="sortAscending">Sortera i fallande ordning, annars i stigande</param>
        /// <param name="offset">Från vilken post vi vill börja listan</param>
        /// <param name="count">Hur många poster vi är intresserade av</param>
        /// <returns>Lista på kartor</returns>
        public GetMapsResponse GetMaps(string mapTypeId, string orderBy, bool sortAscending, int offset, int count)
        {
            var response = new GetMapsResponse();

            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                response.ErrorCode = ErrorCode.NotLoggedIn;
                response.ErrorMessage = "Du måste vara inloggad för att hämta lista på kartor.";
                log.Info(response.ErrorMessage);

                return response;
            }

            var mapId = 0;
            if (!int.TryParse(mapTypeId, out mapId))
            {
                response.ErrorCode = ErrorCode.MissingInformation;
                response.ErrorMessage = "mapTypeId saknas eller är ogilltigt i anrop.";
                log.Info(response.ErrorMessage + " mapTypeId=" + mapTypeId);

                return response;
            }

            if (string.IsNullOrEmpty(orderBy))
            {
                orderBy = "Ver";
            }
            if (count < 1)
            {
                count = 20;
            }

            //TODO: Fixa userId
            response.Maps = db.GetMapVersions(1, mapId, orderBy, sortAscending, offset, count);

            return response;
        }

        /// <summary>
        /// Metod som returnerar en karta.
        /// </summary>
        /// <param name="mapTypeId">Id på karttypen</param>
        /// <param name="version">Version av kartan som skall användas, 0 om senaste version</param>
        /// <returns>Karta</returns>
        public GetMapResponse GetMap(string mapTypeId, string version)
        {
            var response = new GetMapResponse();

            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                response.ErrorCode = ErrorCode.NotLoggedIn;
                response.ErrorMessage = "Du måste vara inloggad för att hämta kartversion.";
                log.Info(response.ErrorMessage);

                return response;
            }

            var mapId = 0;
            var ver = 0;

            if (!int.TryParse(mapTypeId, out mapId))
            {
                response.ErrorCode = ErrorCode.MissingInformation;
                response.ErrorMessage = "mapTypeId saknas eller är ogilltigt i anrop.";
                log.Info(response.ErrorMessage + " mapTypeId=" + mapTypeId);

                return response;
            }

            if (!int.TryParse(version, out ver) || ver < 0)
            {
                response.ErrorCode = ErrorCode.MissingInformation;
                response.ErrorMessage = "version saknas eller är ogilltigt i anrop.";
                log.Info(response.ErrorMessage + " version=" + version);

                return response;
            }

            //TODO: Fixa userId
            response.Map = db.GetMapVersion(1, mapId, ver);

            return response;
        }

        /// <summary>
        /// Metod som sparar ner ändringar av en karta.
        /// </summary>
        /// <param name="mapContent">Kartans innehåll(markörer, kabelsträckor, osv)</param>
        /// <param name="mapTypeId">Id på karttypen</param>
        /// <returns>Versionsnummer på den sparade kartan</returns>
        public SaveMapResponse SaveMap(SaveMap mapContent, string mapTypeId)
        {
            var response = new SaveMapResponse();

            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                response.ErrorCode = ErrorCode.NotLoggedIn;
                response.ErrorMessage = "Du måste vara inloggad för att spara karta.";
                log.Info(response.ErrorMessage);

                return response;
            }
            //TODO: Fixa userId


            return response;
            /*
            Utils.Log("SaveMap anropad för användare=" + HttpContext.Current.User.Identity.Name + ".", System.Diagnostics.EventLogEntryType.Information, 126);
            var response = new SaveMapResponse();

            try
            {
                if (!HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    return new SaveMapResponse() { ErrorCode = ErrorCode.NotLoggedIn, ErrorMessage = "Du måste vara inloggad för att spara karta." };
                }

                if (!Utils.GetMapAccessRights(mapContent.MapTypeId).HasFlag(MapAccessRights.Write))
                {
                    return new SaveMapResponse() { ErrorCode = ErrorCode.NoAccessToMap, ErrorMessage = "Du saknar behörighet för att spara karta." };
                }

                Utils.Log("Ny version av karta skall sparas " + (mapContent.Publish ? "och publiceras " : string.Empty) + "(MapTypeId=" + mapContent.MapTypeId + " för användare=" + HttpContext.Current.User.Identity.Name + ").", System.Diagnostics.EventLogEntryType.Information, 126);

                var fiberDb = new FiberDataContext();

                var existingMap = (from m in fiberDb.Maps where (m.MapTypeId == mapContent.MapTypeId && m.Ver == mapContent.PreviousVersion) select m).SingleOrDefault();

                if (existingMap == null)
                {
                    Utils.Log("Misslyckades med att spara karta, kunde inte hitta karta med MapTypeId=" + mapContent.MapTypeId + ", Version=" + mapContent.PreviousVersion + " för användare=" + HttpContext.Current.User.Identity.Name, System.Diagnostics.EventLogEntryType.Warning, 109);
                    return new SaveMapResponse() { ErrorCode = ErrorCode.FailedToSave, ErrorMessage = "Karta kunde inte hittas." };
                }

                var map = new FiberKartan.Map();
                var lastMap = existingMap.MapType.Maps.OrderByDescending(x => x.Ver).FirstOrDefault();
                if (lastMap == null)    // Kollar om detta är första versionen av kartan.
                {
                    map.PreviousVer = 0;
                    map.Ver = 1;
                }
                else
                {
                    map.Ver = lastMap.Ver + 1;
                    map.PreviousVer = existingMap.Ver;  // Föregående version skall vara den version som vi här redigerar. (Det behöver ju inte alltid vara den senaste versionen som vi justerar)
                }

                map.Created = DateTime.Now;
                if (mapContent.Publish)
                {
                    map.Published = map.Created;
                }

                map.MapType = existingMap.MapType;

                // Påvisa vem som skapade denna nya version.
                var user = (from u in fiberDb.Users where (u.Username == HttpContext.Current.User.Identity.Name) select u).FirstOrDefault();
                map.User = user;
                map.CreatorId = user.Id;

                existingMap.MapType.Maps.Add(map);  // Lägger till kartan.

                #region Layers

                // * För att ta bort ett lager så skickar vi inte med den i kollektionen vid sparandet.
                // * För att ha det kvar så skickar vi in lagret med Id, men markers, lines, polygons är toma (såvida vi inte vill ändra någon av dessa). Dessa kopieras här över från existerande karta.
                // * För att ändra ett lager så skickar vi in det ändrade lagret komplett med markers, lines och polygons.
                // * För att lägga till ett nytt lager så skicka in det med ev. markers, lines och polygons, men med ett Id som är -1 (ett Id kommer då att genereras).

                List<Layer> existingLayers = new List<Layer>();
                List<Layer> newLayers = new List<Layer>();

                if (!string.IsNullOrEmpty(existingMap.Layers))
                {
                    existingLayers = JsonConvert.DeserializeObject<List<Layer>>(existingMap.Layers);
                }

                foreach (var layer in existingLayers)
                {
                    if (layer.Markers == null)
                    {
                        layer.Markers = new List<FiberKartan.REST.Models.Marker>();
                    }
                    if (layer.Lines == null)
                    {
                        layer.Lines = new List<FiberKartan.REST.Models.Line>();
                    }
                    if (layer.Polygons == null)
                    {
                        layer.Polygons = new List<Polygon>();
                    }

                    // Ta bort gamla lager som inte existerar längre (genom att kontrollera att de finns med i den nya listan).
                    if (mapContent.Layers.Contains(layer))
                    {
                        newLayers.Add(layer);
                    }
                }

                SHA1Managed hasher = new SHA1Managed();

                // Lägg sedan till nya eller modifierade lager.
                for (var i = 1; i < mapContent.Layers.Count; i++)
                {
                    var newLayer = mapContent.Layers[i];
                    var existingLayer = existingLayers.First(l => l.Id == newLayer.Id);     // Om ovanstående kod fungerar korrekt så skall vi aldrig få null här!

                    if (String.IsNullOrEmpty(newLayer.Name))
                    {
                        newLayer.Name = "Lager " + i;
                    }

                    if (String.IsNullOrEmpty(newLayer.Id) || newLayer.Id == "-1")
                    {
                        newLayer.Id = BitConverter.ToString(hasher.ComputeHash(Encoding.UTF8.GetBytes(DateTime.Now.ToString()))).Replace("-", "");  // Tar tiden och får fram en unik hashnyckel.
                        newLayers.Add(newLayer);
                    }

                    AddMarkers(newLayer.Markers, existingLayer.Markers);

                    AddLines(newLayer.Lines, existingLayer.Lines);

                    AddPolygons(newLayer.Polygons, existingLayer.Polygons);
                }

                map.Layers = JsonConvert.SerializeObject(newLayers);

                #endregion Layers

                Utils.Log("Submittar ändringar till databas.", System.Diagnostics.EventLogEntryType.Information, 126);

                fiberDb.SubmitChanges();    // Sparar till databasen.

                response.NewVersionNumber = map.Ver;

                // Om denna karta visas i regionsvyn(TotalMap.aspx) så måste cachen för denna tömmas så att den nya versionen visas.
                if (((MapViewSettings)map.MapType.ViewSettings).HasFlag(MapViewSettings.AllowViewAggregatedMaps))
                {
                    HttpContext.Current.Cache.Remove("CachedTotalMap_" + map.MapType.Municipality.Code);
                    HttpContext.Current.Cache.Remove("CachedTotalMap_null"); // Hela Sverige.
                }

                Utils.Log("Ny version av karta sparad " + (mapContent.Publish ? "och publicerad " : string.Empty) + "(MapTypeId=" + map.MapTypeId + ", Version=" + map.Ver + " för användare=" + HttpContext.Current.User.Identity.Name + ").", System.Diagnostics.EventLogEntryType.Information, 125);

                Utils.NotifyEmailSubscribers(map.MapTypeId, map.Ver);
            }
            catch (Exception exception)
            {
                try
                {
                    var browserCapabilities = HttpContext.Current.Request.Browser;
                    Utils.Log("Misslyckades med att spara en ny version av karta för användare=" + HttpContext.Current.User.Identity.Name + " med webbläsare: " + browserCapabilities.Type + ", " + browserCapabilities.Browser + ", " + browserCapabilities.Version + ", " + browserCapabilities.Platform + ". Error=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 125);
                }
                catch (Exception)
                {
                    // Ignorera.
                }

                throw;
            }

            return response;*/
        }


        /// <summary>
        /// Metod som returnerar ett eller flera lager för en karta.
        /// </summary>
        /// <param name="mapTypeId">Id på karttypen</param>
        /// <param name="version">Version av kartan som skall användas, 0 om senaste version</param>
        /// <param name="ids">Id på lagret som skall hämtas, kommaseparerad för flera</param>
        /// <returns>Lista med kartlager</returns>
        public GetLayersResponse GetLayers(string mapTypeId, string version, string ids)
        {
            var response = new GetLayersResponse();

            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                response.ErrorCode = ErrorCode.NotLoggedIn;
                response.ErrorMessage = "Du måste vara inloggad för att hämta kartlager.";
                log.Info(response.ErrorMessage);

                return response;
            }

            var mapId = 0;
            var ver = 0;

            if (!int.TryParse(mapTypeId, out mapId))
            {
                response.ErrorCode = ErrorCode.MissingInformation;
                response.ErrorMessage = "mapTypeId saknas eller är ogilltigt i anrop.";
                log.Info(response.ErrorMessage + " mapTypeId=" + mapTypeId);

                return response;
            }

            if (!int.TryParse(version, out ver) || ver < 0)
            {
                response.ErrorCode = ErrorCode.MissingInformation;
                response.ErrorMessage = "version saknas eller är ogilltigt i anrop.";
                log.Info(response.ErrorMessage + " version=" + version);

                return response;
            }

            //TODO: Fixa userId
            response.Layers = db.GetLayers(1, mapId, ids, ver);

            return response;
        }

        /// <summary>
        /// Metod som används för att rapportera fel på ett fibernätverk.
        /// </summary>
        /// <param name="mapTypeId">Id på karttypen</param>
        /// <param name="version">Version av kartan som skall användas, 0 om senaste version</param>
        /// <param name="report">Felrapport</param>
        public Response ReportIncident(string mapTypeId, string version, IncidentReport report)
        {
            var response = new Response();

            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                response.ErrorCode = ErrorCode.NotLoggedIn;
                response.ErrorMessage = "Du måste vara inloggad för att rapportera incidenter.";
                log.Info(response.ErrorMessage);

                return response;
            }
            //TODO: Fixa userId
            //db.GetLayers(1, mapTypeId, ids, version);

            return response;

            /*Utils.Log("ReportIncident anropad för användare=" + HttpContext.Current.User.Identity.Name + ".", System.Diagnostics.EventLogEntryType.Information, 127);
            var response = new Response() { ErrorCode = 0, ErrorMessage = string.Empty };

            try
            {
                if (!HttpContext.Current.User.Identity.IsAuthenticated)
                {
                    return new Response() { ErrorCode = ErrorCode.NotLoggedIn, ErrorMessage = "Du måste vara inloggad för att rapportera incidenter." };
                }

                if (!Utils.GetMapAccessRights(report.MapTypeId).HasFlag(MapAccessRights.Write))
                {
                    return new Response() { ErrorCode = ErrorCode.NoAccessToMap, ErrorMessage = "Du saknar behörighet för att rapportera incidenter." };
                }

                Utils.Log("Ny incident skapas (MapTypeId=" + report.MapTypeId + ", Description=" + report.Description + ") för användare=" + HttpContext.Current.User.Identity.Name + ".", System.Diagnostics.EventLogEntryType.Information, 126);

                var fiberDb = new FiberDataContext();

                var existingMap = (from m in fiberDb.Maps where (m.MapTypeId == report.MapTypeId && m.Ver == report.Version) select m).SingleOrDefault();

                if (existingMap == null)
                {
                    Utils.Log("Misslyckades med att rapportera incident, kunde inte hitta karta med MapTypeId=" + report.MapTypeId + ", Version=" + report.Version + " för användare=" + HttpContext.Current.User.Identity.Name, System.Diagnostics.EventLogEntryType.Warning, 109);
                    return new Response() { ErrorCode = ErrorCode.FailedToSave, ErrorMessage = "Karta kunde inte hittas." };
                }

                if (!existingMap.MapType.ServiceCompanyId.HasValue)
                {
                    return new Response() { ErrorCode = ErrorCode.MissingInformation, ErrorMessage = "Karta saknar Serviceleverantör, ingen felrapportering kan därför ske." };
                }

                if (!string.IsNullOrEmpty(report.Estate))
                {
                    report.Estate = report.Estate.Trim();
                }

                if (!string.IsNullOrEmpty(report.Description))
                {
                    report.Description = report.Description.Trim();
                }

                if (string.IsNullOrEmpty(report.Description))
                {
                    return new Response() { ErrorCode = ErrorCode.MissingInformation, ErrorMessage = "En felbeskrivning måste anges." };
                }

                // Påvisa vem som skapade denna rapport.
                var user = (from u in fiberDb.Users where (u.Username == HttpContext.Current.User.Identity.Name) select u).FirstOrDefault();

                var incidentReport = new FiberKartan.IncidentReport()
                {
                    MapTypeId = existingMap.MapTypeId,
                    MapVer = existingMap.Ver,
                    CreatorId = user.Id,
                    Created = DateTime.Now,
                    ServiceCompanyId = existingMap.MapType.ServiceCompanyId.Value,
                    ReportStatus = 1,
                    Latitude = report.Position.Latitude,
                    Longitude = report.Position.Longitude,
                    Estate = report.Estate,
                    Description = report.Description
                };

                fiberDb.IncidentReports.InsertOnSubmit(incidentReport);

                fiberDb.SubmitChanges();    // Sparar till databasen.

                #region SendMail

                var body = new StringBuilder("<html><head><title>Felrapport</title></head><body><h1>Felrapport</h1>");
                body.Append(string.Format("<p>{0} har rapporterat följande fel på fibernätverk <a href=\"{1}/admin/MapAdmin.aspx?mid={2}&ver={3}&marker={4}\">\"{5}\"</a>:</p>", user.Name, ConfigurationManager.AppSettings["ServerAdress"], existingMap.MapTypeId, existingMap.Ver, report.Position.Latitude + "x" + report.Position.Longitude, existingMap.MapType.Title));
                body.Append("Felrapport skapad: " + incidentReport.Created + "<br/>");
                body.Append("Till serviceföretag: " + existingMap.MapType.ServiceCompany.Name + "<br/>");
                body.Append("Position(WGS84) latitud: <strong>" + report.Position.Latitude + "</strong> longitud: <strong>" + report.Position.Longitude + "</strong><br/>");
                body.Append("Fastighet: <strong>" + incidentReport.Estate + "</strong><br/>");
                body.Append("Felbeskrivning: " + incidentReport.Description + "<br/>");

                using (var mail = new MailMessage()
                {
                    From = new MailAddress("noreply@fiberkartan.se", "FiberKartan"),
                    Sender = new MailAddress(user.Username, user.Name),
                    ReplyTo = new MailAddress(user.Username, user.Name),
                    Subject = "Felrapport-Fiberkartan",
                    IsBodyHtml = true,
                    DeliveryNotificationOptions = DeliveryNotificationOptions.Never
                })
                {
                    // HTML-innehåll måste kodas så här
                    mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(body.ToString(), Encoding.UTF8, MediaTypeNames.Text.Html));

                    mail.Bcc.Add(new MailAddress(existingMap.MapType.ServiceCompany.ServiceEmail));

                    using (var SMTPServer = new SmtpClient())
                    {
                        SMTPServer.Send(mail);
                    }
                }

                #endregion SendMail
            }
            catch (Exception exception)
            {
                try
                {
                    var browserCapabilities = HttpContext.Current.Request.Browser;
                    Utils.Log("Misslyckades med att rapportera incident med MapTypeId=" + report.MapTypeId + ", Version=" + report.Version + " för användare=" + HttpContext.Current.User.Identity.Name + " med webbläsare: " + browserCapabilities.Type + ", " + browserCapabilities.Browser + ", " + browserCapabilities.Version + ", " + browserCapabilities.Platform + ". Error=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 125);

                }
                catch (Exception)
                {
                    // Ignorera.
                }

                throw;
            }

            return response;*/
        }

        /// <summary>
        /// Metod som används för att kontinuerligt anropa servern för att på så sätt påvisa att klienten ännu är ansluten.
        /// </summary>
        /// <returns>Ett dymmy-svar</returns>
        public PingResponse Ping()
        {
            /* try
             {
                 // Kolla om man har en sessionskaka satt så att vi kan se vilken användare det är.
                 if (HttpContext.Current.User != null && HttpContext.Current.User.Identity != null)
                 {
                     var fiberDb = new FiberDataContext();
                     var user = fiberDb.Users.Where(u => u.Username == HttpContext.Current.User.Identity.Name).SingleOrDefault();
                     if (user != null)
                     {
                         user.LastActivity = DateTime.Now;   // Uppdaterar tidsstämpeln för användaren.
                         fiberDb.SubmitChanges();
                     }
                 }
             }
             catch (Exception)
             {
                 // Vi bryr oss inte om fel. Det är inte en så viktig funktion.
             }*/

            return new PingResponse()
            {
                Message = "pong"
            };
        }
        /*
        /// <summary>
        /// Lägger till nya/uppdaterar befintliga markörer till ett lager.
        /// </summary>
        /// <param name="newMarkers">lista med nya/uppdaterade markörer.</param>
        /// <param name="existingMarkers">existerande lista med markörer i lagret.</param>
        private void AddMarkers(List<FiberKartan.REST.Models.Marker> newMarkers, List<FiberKartan.REST.Models.Marker> existingMarkers)
        {
            Utils.Log("Lägger till markörer till kart-lager.", System.Diagnostics.EventLogEntryType.Information, 126);

            List<Marker> resultList = new List<Marker>(existingMarkers.Count);

            var lastId = existingMarkers.Max(m => m.Id);    // Hämta ut senaste använda Id. Så att nya markörer kan ta vid därifrån.
            var lookup = existingMarkers.ToDictionary(x => x.Id, x => x);

            foreach (var marker in newMarkers)
            {
                // Ny markör?
                if (marker.Id < 0)
                {
                    resultList.Add(new Marker()
                    {
                        Id = ++lastId,
                        Name = Utils.RemoveInvalidXmlChars(marker.Name, true) ?? string.Empty,
                        Description = Utils.RemoveInvalidXmlChars(marker.Description, true) ?? string.Empty,
                        Type = marker.Type,
                        Latitude = marker.Latitude,
                        Longitude = marker.Longitude,
                        Settings = (marker.Settings ?? "{}").Trim()
                    });
                }
                else
                {
                    Marker oldMarker;
                    lookup.TryGetValue(marker.Id, out oldMarker);

                    if (oldMarker != null)
                    {
                        resultList.Add(new Marker()
                        {
                            Id = marker.Id,
                            Name = Utils.RemoveInvalidXmlChars(marker.Name, true) ?? string.Empty,
                            Description = Utils.RemoveInvalidXmlChars(marker.Description, true) ?? Utils.RemoveInvalidXmlChars(marker.Description, true),
                            Type = marker.Type,
                            Latitude = marker.Latitude,
                            Longitude = marker.Longitude,
                            Settings = (marker.Settings ?? "{}").Trim()
                        });
                    }
                }
            }
        }

        /// <summary>
        /// Lägger till nya/uppdaterar befintliga linjer till ett lager.
        /// </summary>
        /// <param name="newLines">lista med nya/uppdaterade linjer.</param>
        /// <param name="existingLines">existerande lista med linjer i lagret.</param>
        private void AddLines(List<Line> newLines, List<Line> existingLines)
        {
            Utils.Log("Kopierar befintliga linjer till ny kartversion.", System.Diagnostics.EventLogEntryType.Information, 126);

            // Uppdaterar grävsträckor med eventuell ny information, hämtar bara ut de grävsträckor som fortfarande finns, övriga har blivit borttagna.
            /* foreach (var existingLine in existingMap.Lines.Where(el => mapContent.Cables.Exists(l => l.Id == el.Id)))
             {
                 var updatedLine = (from ul in mapContent.Cables where ul.Id == existingLine.Id select ul).Single();

                 var updatedCoordinates = new StringBuilder();
                 for (var i = 0; i < updatedLine.Coord.Count; i++)
                 {
                     updatedCoordinates.Append(updatedLine.Coord[i].Lat.ToString().Replace(",", "."));
                     updatedCoordinates.Append(":");
                     updatedCoordinates.Append(updatedLine.Coord[i].Lng.ToString().Replace(",", "."));
                     if (i < updatedLine.Coord.Count - 1)
                     {
                         updatedCoordinates.Append("|");
                     }
                 }

                 map.Lines.Add(new FiberKartan.Line()
                 {
                     Uid = updatedLine.Uid,
                     Name = Utils.RemoveInvalidXmlChars(updatedLine.Name, true) ?? string.Empty,
                     Description = Utils.RemoveInvalidXmlChars(updatedLine.Desc, true) ?? Utils.RemoveInvalidXmlChars(existingLine.Description, true),
                     LineColor = updatedLine.Color,
                     Width = int.Parse(updatedLine.Width),
                     Coordinates = updatedCoordinates.ToString(),
                     Type = updatedLine.Type
                 });
             }

             Utils.Log("Lägger till nya linjer till ny kartversion.", System.Diagnostics.EventLogEntryType.Information, 126);

             // Lägger till nya grävsträckor. Dessa kommer in med Id som är negativa.
             foreach (var newLine in (from nl in mapContent.Cables where nl.Id < 0 select nl))
             {
                 var newCoordinates = new StringBuilder();
                 for (var i = 0; i < newLine.Coord.Count; i++)
                 {
                     newCoordinates.Append(newLine.Coord[i].Lat.ToString().Replace(",", "."));
                     newCoordinates.Append(":");
                     newCoordinates.Append(newLine.Coord[i].Lng.ToString().Replace(",", "."));
                     if (i < newLine.Coord.Count - 1)
                     {
                         newCoordinates.Append("|");
                     }
                 }

                 map.Lines.Add(new FiberKartan.Line()
                 {
                     Uid = 0,    // Ny linje får Uid satt till Id av databasen.
                     Name = Utils.RemoveInvalidXmlChars(newLine.Name, true) ?? string.Empty,
                     Description = Utils.RemoveInvalidXmlChars(newLine.Desc, true) ?? string.Empty,
                     LineColor = newLine.Color,
                     Width = int.Parse(newLine.Width),
                     Coordinates = newCoordinates.ToString(),
                     Type = newLine.Type
                 });
             }*/
        //}

        /// <summary>
        /// Lägger till nya/uppdaterar befintliga polygoner till ett lager.
        /// </summary>
        /// <param name="newPolygons">lista med nya/uppdaterade polygoner.</param>
        /// <param name="existingPolygons">existerande lista med polygoner i lagret.</param>
        /*private void AddPolygons(List<Polygon> newPolygons, List<Polygon> existingPolygons)
        {
            Utils.Log("Kopierar befintliga områden till ny kartversion.", System.Diagnostics.EventLogEntryType.Information, 126);

            // Uppdaterar områden med eventuell ny information, hämtar bara ut de områden som fortfarande finns, övriga har blivit borttagna.
            /* foreach (var existingRegion in existingMap.Regions.Where(er => mapContent.Regions.Exists(l => l.Id == er.Id)))
             {
                 var updatedRegion = (from ur in mapContent.Regions where ur.Id == existingRegion.Id select ur).Single();

                 var updatedCoordinates = new StringBuilder();
                 for (var i = 0; i < updatedRegion.Coord.Count; i++)
                 {
                     updatedCoordinates.Append(updatedRegion.Coord[i].Lat.ToString().Replace(",", "."));
                     updatedCoordinates.Append(":");
                     updatedCoordinates.Append(updatedRegion.Coord[i].Lng.ToString().Replace(",", "."));
                     if (i < updatedRegion.Coord.Count - 1)
                     {
                         updatedCoordinates.Append("|");
                     }
                 }

                 map.Regions.Add(new FiberKartan.Region()
                 {
                     Uid = updatedRegion.Uid,
                     Name = Utils.RemoveInvalidXmlChars(updatedRegion.Name, true) ?? string.Empty,
                     Description = Utils.RemoveInvalidXmlChars(updatedRegion.Desc, true) ?? Utils.RemoveInvalidXmlChars(existingRegion.Description, true),
                     FillColor = updatedRegion.FillColor,
                     LineColor = updatedRegion.BorderColor,
                     Coordinates = updatedCoordinates.ToString()
                 });
             }

             Utils.Log("Lägger till nya områden till ny kartversion.", System.Diagnostics.EventLogEntryType.Information, 126);

             // Lägger till nya områden. Dessa kommer in med Id som är negativa.
             foreach (var newRegion in (from nr in mapContent.Regions where nr.Id < 0 select nr))
             {
                 var newCoordinates = new StringBuilder();
                 for (var i = 0; i < newRegion.Coord.Count; i++)
                 {
                     newCoordinates.Append(newRegion.Coord[i].Lat.ToString().Replace(",", "."));
                     newCoordinates.Append(":");
                     newCoordinates.Append(newRegion.Coord[i].Lng.ToString().Replace(",", "."));
                     if (i < newRegion.Coord.Count - 1)
                     {
                         newCoordinates.Append("|");
                     }
                 }

                 map.Regions.Add(new FiberKartan.Region()
                 {
                     Uid = 0,    // Nytt område får Uid satt till Id av databasen.
                     Name = Utils.RemoveInvalidXmlChars(newRegion.Name, true) ?? string.Empty,
                     Description = Utils.RemoveInvalidXmlChars(newRegion.Desc, true) ?? string.Empty,
                     FillColor = newRegion.FillColor,
                     LineColor = newRegion.BorderColor,
                     Coordinates = newCoordinates.ToString()
                 });
             }*/
        // }
    }
}
