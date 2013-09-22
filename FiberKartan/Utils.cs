using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Diagnostics;
using FiberKartan;
using System.Text;
using System.Net.Mail;
using System.Configuration;
using System.Net.Mime;
using System.Threading;

/*
The zlib/libpng License
Copyright (c) 2012 Henrik Östman

This software is provided 'as-is', without any express or implied warranty. In no event will the authors be held liable for any damages arising from the use of this software.
Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/
namespace FiberKartan
{
    public sealed class Utils
    {
        private static readonly string LogSource = "FiberKartan";

        public static void Log(string strEvent, EventLogEntryType type, int eventId)
        {
            if (!System.Diagnostics.EventLog.SourceExists(LogSource))
                System.Diagnostics.EventLog.CreateEventSource(LogSource, "Application");

            EventLog MyEventLog = new EventLog();
            MyEventLog.Source = LogSource;
            MyEventLog.WriteEntry(strEvent, type, eventId);
        }

        public static string KmlColor2HtmlColor(string kmlColor)
        {
            //http://code.google.com/intl/sv-SE/apis/kml/documentation/kmlreference.html#colorstyle
            //Format: aabbggrr

            if (string.IsNullOrEmpty(kmlColor))
                return string.Empty;

            if (kmlColor.Length != 8)
                return kmlColor;

            var arr = kmlColor.Remove(0, 2).ToCharArray();
            Array.Reverse(arr);

            return new string(arr);
        }

        public static string HtmlColor2KmlColor(string htmlColor, string alpha = "00")
        {
            //Format: rrggbb

            if (string.IsNullOrEmpty(htmlColor))
                return string.Empty;

            if (htmlColor.Length != 6)
                throw new ApplicationException("Wrong formated HtmlColor(" + htmlColor + ")");

            var arr = htmlColor.ToCharArray();
            Array.Reverse(arr);

            return alpha + new string(arr);
        }

        /// <summary>
        /// Metoden används för att returnera vilken tillträdesnivå en användare har till en karta.
        /// </summary>
        /// <param name="mapTypeId">Id på karta som man vill granska</param>
        /// <returns>Vilken tillträdesnivå man har till kartan</returns>
        public static MapAccessRights GetMapAccessRights(int mapTypeId)
        {
            if (mapTypeId < 1)
            {
                return MapAccessRights.None;
            }

            // Kolla om vi är inloggade.
            if (!HttpContext.Current.User.Identity.IsAuthenticated)
            {
                return MapAccessRights.None;
            }

            var fiberDb = new FiberDataContext();
            var user = fiberDb.Users.Where(u => u.Username == HttpContext.Current.User.Identity.Name).SingleOrDefault();
            if (user == null)
            {
                return MapAccessRights.None;
            }

            var access = fiberDb.MapTypeAccessRights.Where(mtar => mtar.MapTypeId == mapTypeId && mtar.UserId == user.Id).SingleOrDefault();

            if (access == null)
            {
                return MapAccessRights.None;
            }

            return (MapAccessRights)access.AccessRight;
        }

        /// <summary>
        /// Metoden används för att returnera vilken tillträdesnivå en användare har till en karta. Metoden förutsätter att parametern "mid" finns på querystringen, om inte, använd den överlagrade metoden som tar en mapTypeId-parameter.
        /// </summary>
        /// <returns>Vilken tillträdesnivå man har till kartan</returns>
        public static MapAccessRights GetMapAccessRights()
        {
            int mapTypeId = 0;
            if (!int.TryParse(HttpContext.Current.Request.QueryString["mid"], out mapTypeId))
                return MapAccessRights.None;

            return Utils.GetMapAccessRights(mapTypeId);
        }

        /// <summary>
        /// Skicka ett mail till alla som prenumererar på förändringar av kartan.
        /// </summary>
        public static void NotifyEmailSubscribers(int mapTypeId, int version)
        {
            // Kör detta i en egen tråd så att det inte påverkar sparning- och importeringstiden ifall det tar lång tid att bygga ihop mailet.
            var sendThread = new Thread(() =>
            {
                try
                {
                    var fiberDb = new FiberDataContext();

                    var notifyUsers = fiberDb.MapTypeAccessRights.Where(ma => ma.MapTypeId == mapTypeId && ma.AccessRight > 0 && ma.EmailSubscribeChanges == true).Select(u => u.User);

                    if (notifyUsers != null && notifyUsers.Count() > 0)
                    {
                        var newMap = fiberDb.Maps.Where(mt => mt.MapTypeId == mapTypeId && mt.Ver == version).First();
                        var oldMap = fiberDb.Maps.Where(mt => mt.MapTypeId == mapTypeId && mt.Ver == newMap.PreviousVer).First();
                        var creator = fiberDb.Users.Where(u => u.Id == newMap.CreatorId).First();

                        var body = new StringBuilder("<html><head><title>Uppdaterad fiberkarta</title></head><body><h1>Uppdaterad fiberkarta</h1>");
                        body.Append(creator.Name + " har uppdaterat fiberkartan <a href=\"" + ConfigurationManager.AppSettings["ServerAdress"] + "/admin/MapAdmin.aspx?mid=" + newMap.MapTypeId + "&ver=" + newMap.Ver + "\">\"" + newMap.MapType.Title + "\"</a> den " + newMap.Created + ".</p>");
                        body.Append("<h2>Följande ändringar har gjorts:</h2>");

                        #region CheckMarkers
                        var newMapMarkerUids = newMap.Markers.Select(m => m.Uid);
                        var oldMapMarkerUids = oldMap.Markers.Select(m => m.Uid);

                        var newMarkers = newMapMarkerUids.Except(oldMapMarkerUids);
                        if (newMarkers != null && newMarkers.Count() > 0)
                        {
                            body.Append("<h3>Lagt till markörer</h3><ol>");

                            foreach (var newMarker in newMarkers)
                            {
                                var marker = newMap.Markers.Where(m => m.Uid == newMarker).First();
                                var markerUrl = new StringBuilder().Append(ConfigurationManager.AppSettings["ServerAdress"]).Append("/admin/MapAdmin.aspx?mid=").Append(newMap.MapTypeId).Append("&ver=").Append(newMap.Ver).Append("&houseyes=true&houseno=true&network=true&crossings=true").Append("&markerId=").Append(marker.Id).ToString();
                                body.Append("<li><a href=\"" + markerUrl + "\">").Append(string.IsNullOrEmpty(marker.Name) ? "[Inget namn]" : marker.Name).Append("</a> - ").Append(marker.MarkerType.Description).Append("</li>");
                            }

                            body.Append("</ol>");
                        }

                        var removedMarkers = oldMapMarkerUids.Except(newMapMarkerUids);
                        if (removedMarkers != null && removedMarkers.Count() > 0)
                        {
                            body.Append("<h3>Tagit bort markörer</h3><ol>");

                            foreach (var removedMarker in removedMarkers)
                            {
                                var marker = oldMap.Markers.Where(m => m.Uid == removedMarker).First();
                                var markerUrl = new StringBuilder().Append(ConfigurationManager.AppSettings["ServerAdress"]).Append("/admin/MapAdmin.aspx?mid=").Append(oldMap.MapTypeId).Append("&ver=").Append(oldMap.Ver).Append("&houseyes=true&houseno=true&network=true&crossings=true").Append("&markerId=").Append(marker.Id).ToString();
                                body.Append("<li><a href=\"" + markerUrl + "\">").Append(string.IsNullOrEmpty(marker.Name) ? "[Inget namn]" : marker.Name).Append("</a> - ").Append(marker.MarkerType.Description).Append("</li>");
                            }

                            body.Append("</ol>");
                        }

                        var stillExistingMarkers = newMapMarkerUids.Intersect(oldMapMarkerUids);
                        if (stillExistingMarkers != null && stillExistingMarkers.Count() > 0)
                        {
                            var isHeadlineSet = false;
                            var foundMarkerChanged = false;

                            foreach (var stillExistingMarker in stillExistingMarkers)
                            {
                                var oldMarker = oldMap.Markers.Where(m => m.Uid == stillExistingMarker).First();
                                var newMarker = newMap.Markers.Where(m => m.Uid == stillExistingMarker).First();

                                if (newMarker.MarkerTypeId != oldMarker.MarkerTypeId)
                                {
                                    if (!isHeadlineSet)
                                    {
                                        // Sätter rubrik i mailet först när vi vet att det finns minst en markör som bytt typ.
                                        body.Append("<h3>Bytt typ på markörer</h3><ol>");
                                        isHeadlineSet = true;
                                        foundMarkerChanged = true;
                                    }

                                    var markerUrl = new StringBuilder().Append(ConfigurationManager.AppSettings["ServerAdress"]).Append("/admin/MapAdmin.aspx?mid=").Append(newMap.MapTypeId).Append("&ver=").Append(newMap.Ver).Append("&houseyes=true&houseno=true&network=true&crossings=true").Append("&markerId=").Append(newMarker.Id).ToString();
                                    body.Append("<li><a href=\"" + markerUrl + "\">").Append(string.IsNullOrEmpty(newMarker.Name) ? "[Inget namn]" : newMarker.Name).Append("</a> - har bytt typ från \"").Append(oldMarker.MarkerType.Description).Append("\" till \"").Append(newMarker.MarkerType.Description).Append("\"</li>");
                                }
                            }

                            if (foundMarkerChanged)
                            {
                                body.Append("</ol>");
                            }
                        }
                        #endregion CheckMarkers

                        #region CheckLines
                        var newMapLinesUids = newMap.Lines.Select(l => l.Uid);
                        var oldMapLinesUids = oldMap.Lines.Select(l => l.Uid);

                        var newLines = newMapLinesUids.Except(oldMapLinesUids);
                        if (newLines != null && newLines.Count() > 0)
                        {
                            body.Append("<h3>Lagt till linjer</h3><ol>");

                            foreach (var newLine in newLines)
                            {
                                var line = newMap.Lines.Where(l => l.Uid == newLine).First();
                                var lineUrl = new StringBuilder().Append(ConfigurationManager.AppSettings["ServerAdress"]).Append("/admin/MapAdmin.aspx?mid=").Append(newMap.MapTypeId).Append("&ver=").Append(newMap.Ver).Append("&houseyes=false&houseno=false&network=true&crossings=false").Append("&lineId=").Append(line.Id).ToString();
                                body.Append("<li><a href=\"" + lineUrl + "\">").Append(string.IsNullOrEmpty(line.Name) ? "[Inget namn]" : line.Name).Append("</a> - Schaktsträcka").Append("</li>");
                            }

                            body.Append("</ol>");
                        }

                        var removedLines = oldMapLinesUids.Except(newMapLinesUids);
                        if (removedLines != null && removedLines.Count() > 0)
                        {
                            body.Append("<h3>Tagit bort linjer</h3><ol>");

                            foreach (var removedLine in removedLines)
                            {
                                var line = oldMap.Lines.Where(l => l.Uid == removedLine).First();
                                var lineUrl = new StringBuilder().Append(ConfigurationManager.AppSettings["ServerAdress"]).Append("/admin/MapAdmin.aspx?mid=").Append(oldMap.MapTypeId).Append("&ver=").Append(oldMap.Ver).Append("&houseyes=false&houseno=false&network=true&crossings=false").Append("&lineId=").Append(line.Id).ToString();
                                body.Append("<li><a href=\"" + lineUrl + "\">").Append(string.IsNullOrEmpty(line.Name) ? "[Inget namn]" : line.Name).Append("</a> - Schaktsträcka").Append("</li>");
                            }

                            body.Append("</ol>");
                        }
                        #endregion CheckLines

                        body.Append("<p>Observera att listan ovan bara ger en grov bild av de ändringar som kan ha gjorts på kartan, ett flertal mindre ändringar kanske inte detekteras av denna funktion!</p>");
                        body.Append("<br/><p>Detta e-post meddelande är automatgenererat av <a href=\"http://fiberkartan.se\">Fiberkartan.se</a> och kan inte svaras på - Om du inte längre önskar få e-post angående uppdateringar av denna karta, logga in på <a href=\"").Append(ConfigurationManager.AppSettings["ServerAdress"]).Append("/admin/ShowMaps.aspx\">Fiberkartan.se</a> och klicka på \"kuvertet\" för denna karta för att avsluta prenumerationen.</p></body></html>");

                        using (var mail = new MailMessage()
                        {
                            From = new MailAddress("noreply@fiberkartan.se", "FiberKartan"),
                            Subject = "Uppdaterad fiberkarta",
                            IsBodyHtml = true,
                            DeliveryNotificationOptions = DeliveryNotificationOptions.Never
                        })
                        {
                            // HTML-innehåll måste kodas så här
                            mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(body.ToString(), Encoding.UTF8, MediaTypeNames.Text.Html));

                            // Bygg mottagarlista.
                            foreach (var user in notifyUsers)
                            {
                                mail.Bcc.Add(new MailAddress(user.Username));
                            }

                            using (var SMTPServer = new SmtpClient())
                            {
                                SMTPServer.Send(mail);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Utils.Log("Misslyckades med att skicka notifieringsmail angående uppdaterad karta(mapTypeId=" + mapTypeId + "). Errormsg=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 159);
                }
            });

            sendThread.Priority = ThreadPriority.BelowNormal; // Konkurrera inte om resurser om det finns viktigare saker att göra.
            sendThread.Start();
        }
    }

    /// <summary>
    /// Extension-method för att få en kortare och "URL-friendly" GUID.
    /// </summary>
    public static class GuidExtensions
    {
        /// <summary>Get a 22-character, case-sensitive GUID as a string.</summary>
        //http://web.archive.org/web/20100408172352/http://prettycode.org/2009/11/12/short-guid/            
        public static string ToShortString(this Guid guid)
        {
            return Convert.ToBase64String(guid.ToByteArray())
                .Substring(0, 22)
                .Replace("/", "_")
                .Replace("+", "-");
        }
    }

    /// <summary>
    /// Vilka tillträdesnivåer som finns för en karta.
    /// </summary>
    [Flags]
    public enum MapAccessRights : int
    {
        None = 0,
        Read = 1,   // Kan granska en karta och alla dess versioner.
        Export = 2, // Kan exportera en karta och alla dess versioner.
        Write = 4,  // Kan modifiera en karta och skapa nya versioner.
        FullAccess = 8  // Fulla rättigheter, kan även bjuda in och ge rättigheter till andra användare.
    }

    /// <summary>
    /// Inställningar för en karta, sparas i databasen som en integer. OBS! Att flera flaggor kan vara aktiva samtidigt!
    /// </summary>
    [Flags]
    public enum MapViewSettings : int
    {
        None = 0,
        PublicVisible = 1,
        ShowPalette = 2,
        ShowConnectionStatistics = 4,
        ShowTotalDigLengthStatistics = 8,
        AllowViewAggregatedMaps = 16,
        OnlyShowHouseYesOnPublicMap = 32
    }
}