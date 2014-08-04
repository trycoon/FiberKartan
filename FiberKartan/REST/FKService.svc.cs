﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Security.Principal;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using FiberKartan;

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
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class FKService : IFKService
    {
        /// <summary>
        /// Metod som sparar ner ändringar av en karta.
        /// </summary>
        /// <param name="mapContent">Kartans innehåll(markörer, kabelsträckor, osv)</param>
        /// <param name="publish">Om satt till sann så publiceras kartan också.</param>
        /// <returns>Returkod och id på nya markörer, sträckor, osv.</returns>
        public SaveMapResponse SaveMap(MapContent mapContent, bool publish = false)
        {
            Utils.Log("SaveMap anropad för användare=" + HttpContext.Current.User.Identity.Name + ".", System.Diagnostics.EventLogEntryType.Information, 126);
            var response = new SaveMapResponse() { ErrorCode = 0, ErrorMessage = string.Empty };

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

                Utils.Log("Ny version av karta skall sparas " + (publish ? "och publiceras " : string.Empty) + "(MapTypeId=" + mapContent.MapTypeId + " för användare=" + HttpContext.Current.User.Identity.Name + ").", System.Diagnostics.EventLogEntryType.Information, 126);

                var fiberDb = new FiberDataContext();

                var existingMap = (from m in fiberDb.Maps where (m.MapTypeId == mapContent.MapTypeId && m.Ver == mapContent.Ver) select m).SingleOrDefault();

                if (existingMap == null)
                {
                    Utils.Log("Misslyckades med att spara karta, kunde inte hitta karta med MapTypeId=" + mapContent.MapTypeId + ", Version=" + mapContent.Ver + " för användare=" + HttpContext.Current.User.Identity.Name, System.Diagnostics.EventLogEntryType.Warning, 109);
                    return new SaveMapResponse() { ErrorCode = ErrorCode.FailedToSave, ErrorMessage = "Karta kunde inte hittas." };
                }

                // Förhindrar att det smäller om parametrar inte är satta.
                if (mapContent.Markers == null)
                {
                    mapContent.Markers = new List<Marker>();
                }
                if (mapContent.Cables == null)
                {
                    mapContent.Cables = new List<Cable>();
                }
                if (mapContent.Regions == null)
                {
                    mapContent.Regions = new List<Region>();
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

                var hash = BitConverter.ToString(new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(DateTime.Now.ToString()))).Replace("-", "");  // Tar tiden och får fram en unik hashnyckel.

                map.Created = DateTime.Now;
                map.MapType = existingMap.MapType;

                map.KML_Hash = hash;
                map.SourceKML = string.Empty;   // Vet inte om vi behöver sätta denna, man kanske bara vill ha den för importerade kartor.

                // Påvisa vem som skapade denna nya version.
                var user = (from u in fiberDb.Users where (u.Username == HttpContext.Current.User.Identity.Name) select u).FirstOrDefault();
                map.User = user;
                map.CreatorId = user.Id;

                map.Layers = "[]";

                existingMap.MapType.Maps.Add(map);  // Lägger till kartan.

                #region Markers

                Utils.Log("Kopierar befintliga markörer till ny kartversion.", System.Diagnostics.EventLogEntryType.Information, 126);

                // Uppdaterar markörer med eventuell ny information, hämtar bara ut de markörer som fortfarande finns, övriga har blivit borttagna.
                foreach (var existingMarker in existingMap.Markers.Where(em => mapContent.Markers.Exists(m => m.Id == em.Id)))
                {
                    var updatedMarker = (from um in mapContent.Markers where um.Id == existingMarker.Id select um).Single();

                    map.Markers.Add(new FiberKartan.Marker()
                    {
                        Uid = updatedMarker.Uid,
                        Name = Utils.RemoveInvalidXmlChars(updatedMarker.Name, true) ?? string.Empty,
                        Description = Utils.RemoveInvalidXmlChars(updatedMarker.Desc, true) ?? Utils.RemoveInvalidXmlChars(existingMarker.Description, true),
                        MarkerTypeId = updatedMarker.MarkId,
                        Latitude = double.Parse(updatedMarker.Lat, CultureInfo.InvariantCulture.NumberFormat),
                        Longitude = double.Parse(updatedMarker.Lng, CultureInfo.InvariantCulture.NumberFormat),
                        Settings = updatedMarker.Settings,
                        OptionalInfo = (updatedMarker.OptionalInfo ?? "{}").Trim()
                    });
                }

                Utils.Log("Lägger till nya markörer till ny kartversion.", System.Diagnostics.EventLogEntryType.Information, 126);

                // Lägger till nya markörer. Dessa kommer in med Id som är negativa.
                foreach (var newMarker in (from nm in mapContent.Markers where nm.Id < 0 select nm))
                {
                    map.Markers.Add(new FiberKartan.Marker()
                    {
                        Uid = 0,    // Ny markör får Uid satt till Id av databasen.
                        Name = Utils.RemoveInvalidXmlChars(newMarker.Name, true) ?? string.Empty,
                        Description = Utils.RemoveInvalidXmlChars(newMarker.Desc, true) ?? string.Empty,
                        MarkerTypeId = newMarker.MarkId,
                        Latitude = double.Parse(newMarker.Lat, CultureInfo.InvariantCulture.NumberFormat),
                        Longitude = double.Parse(newMarker.Lng, CultureInfo.InvariantCulture.NumberFormat),
                        Settings = newMarker.Settings,
                        OptionalInfo = (newMarker.OptionalInfo ?? "{}").Trim()
                    });
                }

                #endregion Markers

                #region Cables

                Utils.Log("Kopierar befintliga linjer till ny kartversion.", System.Diagnostics.EventLogEntryType.Information, 126);

                // Uppdaterar grävsträckor med eventuell ny information, hämtar bara ut de grävsträckor som fortfarande finns, övriga har blivit borttagna.
                foreach (var existingLine in existingMap.Lines.Where(el => mapContent.Cables.Exists(l => l.Id == el.Id)))
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
                }

                #endregion Cables

                #region Regions

                Utils.Log("Kopierar befintliga områden till ny kartversion.", System.Diagnostics.EventLogEntryType.Information, 126);

                // Uppdaterar områden med eventuell ny information, hämtar bara ut de områden som fortfarande finns, övriga har blivit borttagna.
                foreach (var existingRegion in existingMap.Regions.Where(er => mapContent.Regions.Exists(l => l.Id == er.Id)))
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
                }

                #endregion Regions

                Utils.Log("Submittar ändringar till databas.", System.Diagnostics.EventLogEntryType.Information, 126);

                fiberDb.SubmitChanges();    // Sparar till databasen.

                response.NewVersionNumber = map.Ver;

                // Rensar cachen för Regionsvyn (TotalMap.aspx), så att den får ladda upp ny fräsch information.
                HttpContext.Current.Cache.Remove("CachedTotalMap_" + map.MapType.Municipality.Code);
                HttpContext.Current.Cache.Remove("CachedTotalMap_null"); // Hela Sverige.

                Utils.Log("Ny version av karta sparad " + (publish ? "och publicerad " : string.Empty) + "(MapTypeId=" + map.MapTypeId + ", Version=" + map.Ver + " för användare=" + HttpContext.Current.User.Identity.Name + ").", System.Diagnostics.EventLogEntryType.Information, 125);

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

            return response;
        }

        /// <summary>
        /// Metod som används för att rapportera fel på ett fibernätverk.
        /// </summary>
        /// <param name="report">Felrapport</param>
        public Response ReportIncident(IncidentReport report)
        {
            Utils.Log("ReportIncident anropad för användare=" + HttpContext.Current.User.Identity.Name + ".", System.Diagnostics.EventLogEntryType.Information, 127);
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

                var existingMap = (from m in fiberDb.Maps where (m.MapTypeId == report.MapTypeId && m.Ver == report.Ver) select m).SingleOrDefault();

                if (existingMap == null)
                {
                    Utils.Log("Misslyckades med att rapportera incident, kunde inte hitta karta med MapTypeId=" + report.MapTypeId + ", Version=" + report.Ver + " för användare=" + HttpContext.Current.User.Identity.Name, System.Diagnostics.EventLogEntryType.Warning, 109);
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

                if (report.Position == null || string.IsNullOrEmpty(report.Position.Lat) || string.IsNullOrEmpty(report.Position.Lng))
                {
                    return new Response() { ErrorCode = ErrorCode.MissingInformation, ErrorMessage = "Position måste anges." };
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
                    Latitude = double.Parse(report.Position.Lat, CultureInfo.InvariantCulture.NumberFormat),
                    Longitude = double.Parse(report.Position.Lng, CultureInfo.InvariantCulture.NumberFormat),
                    Estate = report.Estate,
                    Description = report.Description
                };

                fiberDb.IncidentReports.InsertOnSubmit(incidentReport);

                fiberDb.SubmitChanges();    // Sparar till databasen.

                #region SendMail

                var body = new StringBuilder("<html><head><title>Felrapport</title></head><body><h1>Felrapport</h1>");
                body.Append(user.Name + " har rapporterat följande fel på fibernätverk <a href=\"" + ConfigurationManager.AppSettings["ServerAdress"] + "/admin/MapAdmin.aspx?mid=" + existingMap.MapTypeId + "&ver=" + existingMap.Ver + "\">\"" + existingMap.MapType.Title + "\"</a>:</p>");
                body.Append("Felrapport skapad: " + incidentReport.Created + "<br/>");
                body.Append("Till serviceföretag: " + existingMap.MapType.ServiceCompany.Name + "<br/>");
                body.Append("Position(WGS84) latitud: <strong>" + report.Position.Lat + "</strong> longitud: <strong>" + report.Position.Lng + "</strong><br/>");
                body.Append("Fastighet: <strong>" + incidentReport.Estate + "</strong><br/>");
                body.Append("Felbeskrivning: " + incidentReport.Description + "<br/>");

                using (var mail = new MailMessage()
                {
                    From = new MailAddress("noreply@fiberkartan.se", "FiberKartan"),
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
                    Utils.Log("Misslyckades med att rapportera incident med MapTypeId=" + report.MapTypeId + ", Version=" + report.Ver + " för användare=" + HttpContext.Current.User.Identity.Name + " med webbläsare: " + browserCapabilities.Type + ", " + browserCapabilities.Browser + ", " + browserCapabilities.Version + ", " + browserCapabilities.Platform + ". Error=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 125);

                }
                catch (Exception)
                {
                    // Ignorera.
                }

                throw;
            }

            return response;
        }

        /// <summary>
        /// Metod som returnerar beskrivningen för en markör.
        /// </summary>
        /// <param name="id">Markörens id</param>
        /// <returns>Markörens beskrivning</returns>
        public MarkerDescription MarkerDescription(string id)
        {
            var fiberDb = new FiberDataContext();

            var marker = (from m in fiberDb.Markers where (m.Id == int.Parse(id)) select m).SingleOrDefault();

            if (marker != null)
            {
                return new MarkerDescription { Desc = marker.Description ?? string.Empty };
            }

            return new MarkerDescription { Desc = string.Empty };
        }

        /// <summary>
        /// Metod som returnerar beskrivningen för en Linje.
        /// </summary>
        /// <param name="id">Linjens id</param>
        /// <returns>Linjens beskrivning</returns>
        public LineDescription LineDescription(string id)
        {
            var fiberDb = new FiberDataContext();

            var line = (from l in fiberDb.Lines where (l.Id == int.Parse(id)) select l).SingleOrDefault();

            if (line != null)
            {
                return new LineDescription { Desc = line.Description ?? string.Empty };
            }

            return new LineDescription { Desc = string.Empty };
        }

        /// <summary>
        /// Metod som returnerar beskrivningen för ett område.
        /// </summary>
        /// <param name="id">Områdets id</param>
        /// <returns>Områdets beskrivning</returns>
        public RegionDescription RegionDescription(string id)
        {
            var fiberDb = new FiberDataContext();

            var region = (from r in fiberDb.Regions where (r.Id == int.Parse(id)) select r).SingleOrDefault();

            if (region != null)
            {
                return new RegionDescription { Desc = region.Description ?? string.Empty };
            }

            return new RegionDescription { Desc = string.Empty };
        }

        /// <summary>
        /// Metod som används för att kontinuerligt anropa servern för att på så sätt påvisa att klienten ännu är ansluten.
        /// </summary>
        /// <returns>Ett dymmy-svar</returns>
        public PingResponse Ping()
        {
            try
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
            }

            return new PingResponse()
            {
                Message = "Pong"
            };
        }
    }
}
