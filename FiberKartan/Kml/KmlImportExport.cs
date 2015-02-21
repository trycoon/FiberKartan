using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using FiberKartan;
using FiberKartan.Kml;
using SharpKml.Base;
using SharpKml.Dom;
using SharpKml.Engine;

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
namespace FiberKartan.Kml
{
    /// <summary>
    /// Klass som hanterar importering och exportering av KML-information.
    /// </summary>
    public static class KmlImportExport
    {
        /// <summary>
        /// Metoden analyserar en KML-sträng och ser hur många och vad för markörer och andra objekt som finns på kartan.
        /// Detta så att man kan besluta vad man vill importera till kartan.
        /// </summary>
        /// <param name="kmlString">KML-dokument i formen av en sträng</param>
        /// <returns>Lista på funna objekt</returns>
        public static AnalyzedKml AnalyzeKml(string kmlString)
        {
            if (string.IsNullOrEmpty(kmlString))
            {
                throw new ApplicationException("Filen innehåller ingen information.");
            }

            var fiberDb = new FiberDataContext();
            var analyzedKml = new AnalyzedKml();

            var parser = new Parser();
            parser.ParseString(kmlString, false);

            // Skapar en dictionary med styleId som nyckel och Href som värde, skall användas som lookup-tabell för ikoner sedan.
            var tempStyles = parser.Root.Flatten().OfType<SharpKml.Dom.Style>().Where(s => s.Icon != null);
            var styleLookup = new Dictionary<string, string>();
            var key = string.Empty;

            foreach (var style in tempStyles)
            {
                key = "#" + style.Id;
                if (style.Id != null && !styleLookup.ContainsKey(key))
                {
                    styleLookup.Add(key, style.Icon.Icon.Href.OriginalString);
                }
            }

            // Itererar igenom alla StyleMaps i dokumentet och lägger in deras style i styleLookup också.
            var styleMaps = parser.Root.Flatten().OfType<SharpKml.Dom.StyleMapCollection>();
            foreach (var styleMap in styleMaps)
            {
                styleLookup.Add("#" + styleMap.Id, styleLookup[styleMap.First().StyleUrl.OriginalString]);
            }

            var unkownMarkerType = fiberDb.MarkerTypes.Where(mt => mt.Name == "Unknown").Single();

            var markerTypeCount = new Dictionary<string, int>();
            foreach (var placemark in parser.Root.Flatten().OfType<SharpKml.Dom.Placemark>().Where(p => p.Geometry.GetType() == typeof(SharpKml.Dom.Point)))
            {
                string href = null;

                if (placemark.StyleUrl != null)
                {
                    href = styleLookup[placemark.StyleUrl.OriginalString];
                }
                else
                {
                    // För placemarks som innehåller en IconStyle istället för styleUrl. 
                    if (placemark.StyleSelector != null)
                    {
                        var style = placemark.StyleSelector.Flatten().OfType<SharpKml.Dom.Style>().Where(p => p.Icon != null).FirstOrDefault();
                        if (style != null)
                        {
                            href = style.Icon.Icon.Href.OriginalString;
                        }
                        else
                        {
                            // Om ingen style finns satt på markörerna, sätt dom som okända.
                            href = ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"] + unkownMarkerType.DestIcon;
                        }
                    }
                    else
                    {
                        // Om ingen style finns satt på markörerna, sätt dom som okända.
                        href = ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"] + unkownMarkerType.DestIcon;
                    }
                }

                // Räkna ut hur många markörer av varje sort som finns.
                if (markerTypeCount.ContainsKey(href))
                {
                    markerTypeCount[href]++;
                }
                else
                {
                    markerTypeCount.Add(href, 1);
                }
            }

            foreach (var markerType in markerTypeCount)
            {
                var lastSlashIndex = markerType.Key.LastIndexOf('/');
                var sourceImage = markerType.Key.Substring(lastSlashIndex + 1, markerType.Key.Length - lastSlashIndex - 1);    // Hämtar ut t.ex. "green.png" ur "http://maps.gstatic.com/intl/sv_se/mapfiles/ms/micons/green.png".
                var foundMarkerType = fiberDb.MarkerTypes.Where(mt => mt.SourceIcon.Contains(sourceImage)).FirstOrDefault();

                analyzedKml.FoundMarkers.Add(new FoundMarker()
                {
                    MarkerHref = markerType.Key,
                    NrFound = markerType.Value,
                    SuggestedMarkerTranslation = foundMarkerType != null ? foundMarkerType.Name : null
                });
            }

            // Denna struktur gör det möjligt att stödja olika slags linjer i framtiden.
            var lineCount = parser.Root.Flatten().OfType<SharpKml.Dom.LineString>().Count();
            if (lineCount > 0)
            {
                analyzedKml.FoundLines.Add(new FoundLine()
                    {
                        NrFound = lineCount,
                        SuggestedLineTranslation = 0
                    });
            }

            // Denna struktur gör det möjligt att stödja olika slags polygoner i framtiden.
            var polygonCount = parser.Root.Flatten().OfType<SharpKml.Dom.Polygon>().Count();
            if (polygonCount > 0)
            {
                analyzedKml.FoundPolygons.Add(new FoundPolygon()
                {
                    NrFound = polygonCount,
                    SuggestedPolygonTranslation = 0
                });
            }

            return analyzedKml;
        }

        /// <summary>
        /// Importerar karta med fastigheter, linjer och områden.
        /// </summary>
        /// <param name="kmlString">KML-document att importera.</param>
        /// <param name="mapTypeId">Vilke karta vi skall importera till.</param>
        /// <param name="markerTranslations">En översättningstabell som anger hur användaren vill att respektive markör skall tolkas.</param>
        /// <param name="lineTranslations">En översättningstabell som anger hur användaren vill att respektive linje skall tolkas.</param>
        /// <param name="polygonTranslations">En översättningstabell som anger hur användaren vill att respektive polygon skall tolkas.</param>
        /// <param name="mapVersion">Vilken version av kartan vi skall slås samman med, 0=ny version.</param>
        /// <param name="includeOldLines">Inkludera linjer från den gamla kartan i sammanslagningen.</param>
        /// <param name="includeOldMarkers">Inkludera markörer från den gamla kartan i sammanslagningen.</param>
        /// <returns>Nytt versionsnummer.</returns>
        public static int ImportKmlToMap(string kmlString, int mapTypeId, Dictionary<string, string> markerTranslations, Dictionary<string, string> lineTranslations, Dictionary<string, string> polygonTranslations, int mapVersion = 0, bool includeOldLines = true, bool includeOldMarkers = true)
        {
            var newMapVersion = 0;

            if (string.IsNullOrEmpty(kmlString) || mapTypeId < 1)
            {
                throw new ApplicationException("Uppladdad karta är tom!");
            }

            var fiberDb = new FiberDataContext();
            var mapType = fiberDb.MapTypes.Where(m => m.Id == mapTypeId).SingleOrDefault();

            if (mapType == null) throw new ApplicationException("Map with mapTypeId=" + mapTypeId + " not found.");

            var lastMap = mapType.Maps.OrderByDescending(x => x.Ver).FirstOrDefault();

            try
            {
                var hash = BitConverter.ToString(new SHA1Managed().ComputeHash(Encoding.UTF8.GetBytes(kmlString))).Replace("-", "");
                var map = new FiberKartan.Map();
                map.Created = DateTime.Now;
                map.MapType = mapType;

                map.KML_Hash = hash;
                map.SourceKML = kmlString;

                map.Layers = "{}"; //TODO: Fixa denna när vi skall ta stöd för flera lager.

                // Påvisa vem som skapade denna nya version.
                var user = (from u in fiberDb.Users where (u.Username == HttpContext.Current.User.Identity.Name) select u).FirstOrDefault();
                map.User = user;
                map.CreatorId = user.Id;

                var mapContent = ParseKmlString(kmlString, markerTranslations, lineTranslations, polygonTranslations);

                if (lastMap == null)
                {
                    // Det finns ingen tidigare karta.
                    map.Ver = 1;
                    map.PreviousVer = 0;
                }
                else
                {
                    map.Ver = lastMap.Ver + 1;
                }

                // Ifall vi skall utgå ifrån en befintlig kartversion.
                if (mapVersion > 0)
                {
                    // Merga importerad KML med specificerad kartversion.
                    var mergeMap = mapType.Maps.Where(x => x.Ver == mapVersion).FirstOrDefault();
                    if (mergeMap == null) throw new ApplicationException("Merge with map not possible, map with version=" + mapVersion + " not found.");

                    map.PreviousVer = mergeMap.Ver;

                    // Kopiera markörer från gamla kartan till denna nya.
                    if (includeOldMarkers)
                    {
                        foreach (var existingMarker in mergeMap.Markers)
                        {
                            map.Markers.Add(new FiberKartan.Marker()
                            {
                                MarkerType = existingMarker.MarkerType,
                                Uid = existingMarker.Uid,
                                Name = existingMarker.Name,
                                Description = existingMarker.Description,
                                Latitude = existingMarker.Latitude,
                                Longitude = existingMarker.Longitude,
                                Settings = existingMarker.Settings,
                                OptionalInfo = existingMarker.OptionalInfo
                            });
                        }
                    }

                    // Kopiera linjer från gamla kartan till denna nya.
                    if (includeOldLines)
                    {
                        foreach (var existingLine in mergeMap.Lines)
                        {
                            map.Lines.Add(new FiberKartan.Line()
                            {
                                Uid = existingLine.Uid,
                                Name = existingLine.Name,
                                Description = existingLine.Description,
                                LineColor = existingLine.LineColor,
                                Width = existingLine.Width,
                                Type = existingLine.Type,
                                Coordinates = existingLine.Coordinates
                            });
                        }
                    }

                    // Kopiera områden från gamla kartan till denna nya.
                    foreach (var existingRegion in mergeMap.Regions)
                    {
                        map.Regions.Add(new FiberKartan.Region()
                        {
                            Uid = existingRegion.Uid,
                            Name = existingRegion.Name,
                            Description = existingRegion.Description,
                            LineColor = existingRegion.LineColor,
                            FillColor = existingRegion.FillColor,
                            Coordinates = existingRegion.Coordinates
                        });
                    }
                }
                else if (lastMap != null) // Ifall vi vill utgå ifrån den senaste versionen. Obs! att en helt ny karta inte har någon tidigare version.
                {
                    // Skapa en ny kartversion med den importerade filen.
                    map.PreviousVer = lastMap.Ver;
                }

                mapType.Maps.Add(map);

                Utils.Log("Sparar ner " + mapContent.Markers.Count + " st markörer till databasen.", System.Diagnostics.EventLogEntryType.Information, 126);
                foreach (var kmlMarker in mapContent.Markers)
                {
                    var marker = new FiberKartan.Marker()
                        {
                            MapTypeId = map.MapTypeId,
                            Name = Utils.RemoveInvalidXmlChars(kmlMarker.Name, true),
                            MarkerTypeId = kmlMarker.MarkerTypeId,
                            Description = Utils.RemoveInvalidXmlChars(kmlMarker.Description, true),
                            Latitude = kmlMarker.Point.Lat,
                            Longitude = kmlMarker.Point.Long,
                            Settings = kmlMarker.Settings,
                            OptionalInfo = kmlMarker.OptionalInfo
                        };

                    map.Markers.Add(marker);
                }

                Utils.Log("Sparar ner " + mapContent.Lines.Count + " st linjer till databasen.", System.Diagnostics.EventLogEntryType.Information, 126);
                foreach (var kmlLine in mapContent.Lines)
                {
                    var line = new FiberKartan.Line()
                    {
                        MapTypeId = map.MapTypeId,
                        Name = Utils.RemoveInvalidXmlChars(kmlLine.Name, true),
                        Description = Utils.RemoveInvalidXmlChars(kmlLine.Description, true),
                        LineColor = Utils.KmlColor2HtmlColor(kmlLine.Color),
                        Width = kmlLine.Width,
                        Type = kmlLine.Type
                    };

                    for (var i = 0; i < kmlLine.Coordinates.Count; i++)
                    {
                        line.Coordinates += (kmlLine.Coordinates[i].Lat.ToString().Replace(",", ".") + ":" + kmlLine.Coordinates[i].Long.ToString().Replace(",", ".")) + (i < kmlLine.Coordinates.Count - 1 ? "|" : string.Empty);
                    }

                    map.Lines.Add(line);
                }

                Utils.Log("Sparar ner " + mapContent.Polygons.Count + " st polygoner till databasen.", System.Diagnostics.EventLogEntryType.Information, 126);
                foreach (var kmlPolygon in mapContent.Polygons)
                {
                    var region = new FiberKartan.Region()
                    {
                        MapTypeId = map.MapTypeId,
                        Name = Utils.RemoveInvalidXmlChars(kmlPolygon.Name, true),
                        Description = Utils.RemoveInvalidXmlChars(kmlPolygon.Description, true),
                        FillColor = Utils.KmlColor2HtmlColor(kmlPolygon.FillColor),
                        LineColor = Utils.KmlColor2HtmlColor(kmlPolygon.LineColor)
                    };

                    for (var i = 0; i < kmlPolygon.Coordinates.Count; i++)
                    {
                        region.Coordinates += (kmlPolygon.Coordinates[i].Lat.ToString().Replace(",", ".") + ":" + kmlPolygon.Coordinates[i].Long.ToString().Replace(",", ".")) + (i < kmlPolygon.Coordinates.Count - 1 ? "|" : string.Empty);
                    }

                    map.Regions.Add(region);
                }

                fiberDb.SubmitChanges();

                newMapVersion = map.Ver;
            }
            catch (Exception ex)
            {
                User user = null;
                try
                {
                    user = (from u in fiberDb.Users where (u.Username == HttpContext.Current.User.Identity.Name) select u).FirstOrDefault();
                    var errorToLog = new ImportError()
                    {
                        UserId = user.Id,
                        MapTypeId = mapTypeId,
                        MergeVersion = mapVersion,
                        Date = DateTime.Now,
                        KML = kmlString,
                        ErrorMessage = ex.Message,
                        StackTrace = ex.StackTrace
                    };
                    fiberDb.ImportErrors.InsertOnSubmit(errorToLog);
                    fiberDb.SubmitChanges();
                }
                catch (Exception exception)
                {
                    Utils.Log("Failed to log import-error to database. Errormsg=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 150);
                }

                try
                {
                    using (var mail = new MailMessage(
                            "noreply@fiberkartan.se",
                            ConfigurationManager.AppSettings.Get("adminMail"),
                            "Misslyckad import av karta",
                            "För användare " + user.Username + ", MaptypeId=" + mapTypeId + ", Errormsg=" + ex.Message + ", Stacktrace=" + ex.StackTrace
                        ))
                    {
                        using (var SMTPServer = new SmtpClient())
                        {
                            SMTPServer.Send(mail);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Utils.Log("Failed to send mail of import-error. Errormsg=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 151);
                }

                throw ex;
            }

            return newMapVersion;
        }

        /// <summary>
        /// Parsar en sträng som repressenterar ett KML-dokument och returnerar typade objekt.
        /// </summary>
        /// <param name="rawXml">KML-sträng</param>
        /// <param name="markerTranslations">En översättningstabell som anger hur användaren vill att respektive markör skall tolkas.</param>
        /// <param name="lineTranslations">En översättningstabell som anger hur användaren vill att respektive linje skall tolkas.</param>
        /// <param name="polygonTranslations">En översättningstabell som anger hur användaren vill att respektive polygon skall tolkas.</param>
        /// <returns>Typad KML-objekt</returns>
        private static Kml ParseKmlString(string rawXml, Dictionary<string, string> markerTranslations, Dictionary<string, string> lineTranslations, Dictionary<string, string> polygonTranslations)
        {
            var fiberDb = new FiberDataContext();
            var nonDigitRegexp = new Regex("[^0-9]", RegexOptions.Compiled);
            var kml = new Kml();

            var parser = new Parser();
            parser.ParseString(rawXml, false);

            // Skapar en dictionary med styleId som nyckel och Href som värde, skall användas som lookup-tabell för ikoner sedan.
            #region createMarkerStyleLookup
            Utils.Log("Start creating stylelookup.", System.Diagnostics.EventLogEntryType.Information, 126);

            var markerStyleLookup = new Dictionary<string, MarkerStyleLookup>();

            var tmpIcons = parser.Root.Flatten().OfType<SharpKml.Dom.Style>().Where(s => s.Icon != null);
            foreach (var tmpIcon in tmpIcons)
            {
                var lastSlashIndex = tmpIcon.Icon.Icon.Href.OriginalString.LastIndexOf('/');
                var sourceImage = tmpIcon.Icon.Icon.Href.OriginalString.Substring(lastSlashIndex + 1, tmpIcon.Icon.Icon.Href.OriginalString.Length - lastSlashIndex - 1);    // Hämtar ut t.ex. "green.png" ur "http://maps.gstatic.com/intl/sv_se/mapfiles/ms/micons/green.png".
                if (!markerStyleLookup.ContainsKey("#" + tmpIcon.Id))
                {
                    markerStyleLookup.Add("#" + tmpIcon.Id, new MarkerStyleLookup { Href = tmpIcon.Icon.Icon.Href.OriginalString, ImageName = sourceImage });
                }
            }

            // Itererar igenom alla StyleMaps i dokumentet och lägger in deras style i styleLookup också.
            var styleMaps = parser.Root.Flatten().OfType<SharpKml.Dom.StyleMapCollection>();
            foreach (var styleMap in styleMaps)
            {
                var targetMarkerStyle = markerStyleLookup[styleMap.First().StyleUrl.OriginalString];
                markerStyleLookup.Add("#" + styleMap.Id, new MarkerStyleLookup { Href = targetMarkerStyle.Href, ImageName = targetMarkerStyle.ImageName });
            }

            Utils.Log("Done creating stylelookup.", System.Diagnostics.EventLogEntryType.Information, 126);
            #endregion createMarkerStyleLookup

            #region parseMarkers
            Utils.Log("Start parsing markers.", System.Diagnostics.EventLogEntryType.Information, 126);

            // Förladdar Okänd-markören, markörer som inte har någon style får denna.
            var unkownMarkerType = fiberDb.MarkerTypes.Where(mt => mt.Name == "Unknown").Single();
            var unkownMarkerStyle = new MarkerStyleLookup { Href = ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"] + unkownMarkerType.DestIcon, ImageName = unkownMarkerType.Name };

            var tmpMarkerTypeLookup = new Dictionary<string, MarkerType>();
            // Loopa igenom alla markörer(point) som finns i filen.
            foreach (var placemark in parser.Root.Flatten().OfType<SharpKml.Dom.Placemark>().Where(p => p.Geometry.GetType() == typeof(SharpKml.Dom.Point)))
            {
                string styleHref = null;

                if (placemark.StyleUrl != null && !string.IsNullOrEmpty(placemark.StyleUrl.OriginalString))
                {
                    styleHref = markerStyleLookup[placemark.StyleUrl.OriginalString].Href;
                }
                else
                {
                    // För placemarks som innehåller en IconStyle istället för styleUrl. 
                    if (placemark.StyleSelector != null)
                    {
                        var style = placemark.StyleSelector.Flatten().OfType<SharpKml.Dom.Style>().Where(p => p.Icon != null).FirstOrDefault();
                        if (style != null)
                        {
                            styleHref = style.Icon.Icon.Href.OriginalString;
                        }
                        else
                        {
                            // Om ingen style finns satt på markörerna, sätt dom som okända.
                            styleHref = ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"] + unkownMarkerType.DestIcon;
                        }
                    }
                    else
                    {
                        // Om ingen style finns satt på markörerna, sätt dom som okända.
                        styleHref = ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"] + unkownMarkerType.DestIcon;
                    }
                }

                // Hämtar vilken markörtyp vi valt att ikonen skall repressentera.
                var markerTypeName = markerTranslations[styleHref];

                if (!string.IsNullOrEmpty(markerTypeName) && markerTypeName != "DontImport")
                {
                    MarkerType markerType = null;
                    // Hämta upp markertype från databasen bara en gång i loopen.
                    if (!tmpMarkerTypeLookup.TryGetValue(markerTypeName, out markerType))
                    {
                        markerType = fiberDb.MarkerTypes.Where(mt => mt.Name == markerTypeName).FirstOrDefault();
                        tmpMarkerTypeLookup.Add(markerTypeName, markerType);
                    }

                    if (markerType != null)
                    {
                        var kmlMarker = new KmlMarker()
                        {
                            Id = placemark.Id,
                            Name = placemark.Name,
                            Description = placemark.Description == null ? string.Empty : placemark.Description.Text,
                            MarkerTypeId = markerType.Id,
                            Point = new KmlCoordinate() { Lat = ((Point)placemark.Geometry).Coordinate.Latitude, Long = ((Point)placemark.Geometry).Coordinate.Longitude }
                        };
                        // Läser in extra information, som kan finnas tillgänglig om KML-filen exporterats från denna applikation.
                        if (placemark.ExtendedData != null)
                        {
                            try
                            {
                                #region MarkerSettingsAndInfo
                                var extendedData = placemark.ExtendedData.Data.Where(s => s.Name == "settings").FirstOrDefault();
                                if (extendedData != null)
                                {
                                    int value = 0;
                                    int.TryParse(extendedData.Value, out value);
                                    kmlMarker.Settings = value;
                                }

                                extendedData = placemark.ExtendedData.Data.Where(s => s.Name == "optionalInfo").FirstOrDefault();
                                if (extendedData != null)
                                {
                                    kmlMarker.OptionalInfo = extendedData.Value;
                                }
                                else
                                {
                                    kmlMarker.OptionalInfo = "{}";
                                }
                                #endregion MarkerSettingsAndInfo

                                #region parseAddressPoint
                                // Lantmäteriets kartor har adresspunkter för varje fastighet, vi hämtar ut nyttig information här.
                                var addressPoint = placemark.ExtendedData.SchemaData.Where(s => s.SchemaUrl.OriginalString == "#S_Adresspunkter_IISSSSDSSSS").FirstOrDefault();
                                if (addressPoint != null)
                                {
                                    var propertyNr = addressPoint.SimpleData.Where(sd => sd.Name == "FNR").FirstOrDefault();
                                    var adress = addressPoint.SimpleData.Where(sd => sd.Name == "ADRESS").FirstOrDefault();
                                    var city = addressPoint.SimpleData.Where(sd => sd.Name == "POSTORT").FirstOrDefault();
                                    var property = addressPoint.SimpleData.Where(sd => sd.Name == "FASTIGHET").FirstOrDefault();

                                    var tableDescription = new StringBuilder("<table border='0'>");
                                    if (propertyNr != null)
                                    {
                                        tableDescription.Append("<tr><td><b>FNR</b></td><td>" + propertyNr.Text + "</td></tr>");
                                    }
                                    if (adress != null)
                                    {
                                        tableDescription.Append("<tr><td><b>ADRESS</b></td><td>" + adress.Text + "</td></tr>");
                                    }
                                    if (city != null)
                                    {
                                        tableDescription.Append("<tr><td><b>POSTORT</b></td><td>" + city.Text + "</td></tr>");
                                    }
                                    if (property != null)
                                    {
                                        tableDescription.Append("<tr><td><b>FASTIGHET</b></td><td>" + property.Text + "</td></tr>");
                                        // Om markören inte redan har ett namn så sätter vi fastighetsbeteckningen som namn.
                                        if (string.IsNullOrEmpty(kmlMarker.Name))
                                        {
                                            kmlMarker.Name = property.Text;
                                        }
                                    }
                                    tableDescription.Append("</table>");

                                    kmlMarker.Description = string.IsNullOrEmpty(kmlMarker.Description) ? tableDescription.ToString() : ("<br/>" + tableDescription.ToString());
                                }
                                #endregion parseAddressPoint
                            }
                            catch (Exception exception)
                            {
                                Utils.Log("Misslyckades med att tolka en markörs ExtendedData, hoppar över informationen. Message=" + exception.Message + ", stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Warning, 126);
                            }
                        }

                        // För kopplingsskåp filtrerar vi bort alla tecken som inte är siffror, för namnet på ett kopplingsskåp måste vara ett tal.
                        if (markerType.Name == MapEntityName.FiberBox)
                        {
                            kmlMarker.Name = nonDigitRegexp.Replace(kmlMarker.Name, "");
                        }

                        kml.Markers.Add(kmlMarker);
                    }
                }
            }

            Utils.Log("Done parsing markers.", System.Diagnostics.EventLogEntryType.Information, 126);
            #endregion parseMarkers

            #region parseLines
            Utils.Log("Start parsing lines.", System.Diagnostics.EventLogEntryType.Information, 126);

            // Just nu finns bara stöd för grävsträckor, så antingen importeras alla linjer som grävsträck eller också importeras inga linjer alls.
            string lineType = null;
            lineTranslations.TryGetValue("0", out lineType);

            //TODO: Om vi skall stödja olika typer av linjer i framtiden så måste denna if-sats flyttas in i for-loopen istället.
            if (lineType == "0")
            {
                // Loopa igenom alla linjer som finns i filen.
                foreach (var lineString in parser.Root.Flatten().OfType<LineString>())
                {
                    // Ta bara med linjer som har fler koordinater än en(ja, man kan få linjer med färre koordinater om man tar bort en linje i Google Earth så bara en punkt blir kvar).
                    if (lineString.Coordinates.Count > 1)
                    {
                        if (lineString.Parent is SharpKml.Dom.MultipleGeometry)
                        {
                            throw new ApplicationException("Applikationen stöder inte filer som innehåller MultipleGeometry-element.");
                        }

                        var kmlLine = new KmlLineString()
                        {
                            Id = lineString.Id,
                            Name = ((SharpKml.Dom.Placemark)lineString.Parent).Name,
                            Description = ((SharpKml.Dom.Placemark)lineString.Parent).Description == null ? string.Empty : ((SharpKml.Dom.Placemark)lineString.Parent).Description.Text,
                            Color = "73ff0000", //TODO: Borde tas bort, Type skall räcka.
                            Width = 4,  //TODO: Borde tas bort, Type skall räcka.
                            Type = int.Parse(lineType)
                        };

                        foreach (var coordinate in lineString.Coordinates)
                        {
                            kmlLine.Coordinates.Add(new KmlCoordinate() { Lat = coordinate.Latitude, Long = coordinate.Longitude });
                        }

                        kml.Lines.Add(kmlLine);
                    }
                }
            }

            Utils.Log("Done parsing lines.", System.Diagnostics.EventLogEntryType.Information, 126);
            #endregion parseLines

            #region parsePolygons
            Utils.Log("Start parsing polygons.", System.Diagnostics.EventLogEntryType.Information, 126);

            // Just nu finns bara stöd för områden, så antingen importeras alla polygoner som områden eller också importeras inga polygoner alls.
            string polygonType = null;
            polygonTranslations.TryGetValue("0", out polygonType);

            //TODO: Om vi skall stödja olika typer av polygoner i framtiden så måste denna if-sats flyttas in i for-loopen istället.
            if (polygonType == "0")
            {
                // Loopa igenom alla polygoner som finns i filen.
                foreach (var polygon in parser.Root.Flatten().OfType<Polygon>())
                {
                    // Ta bara med områden som har fler koordinater än två, minst tre koordinater krävs för att bilda en yta.
                    if (polygon.OuterBoundary.LinearRing.Coordinates.Count > 2)
                    {
                        var kmlPolygon = new KmlPolygon()
                        {
                            Id = polygon.Id,
                            Name = ((SharpKml.Dom.Placemark)polygon.Parent).Name,
                            Description = ((SharpKml.Dom.Placemark)polygon.Parent).Description == null ? string.Empty : ((SharpKml.Dom.Placemark)polygon.Parent).Description.Text,
                            FillColor = "73ff9999",
                            LineColor = "80000000"
                        };

                        foreach (var coordinate in polygon.OuterBoundary.LinearRing.Coordinates)
                        {
                            kmlPolygon.Coordinates.Add(new KmlCoordinate() { Lat = coordinate.Latitude, Long = coordinate.Longitude });
                        }

                        kml.Polygons.Add(kmlPolygon);
                    }
                }
            }

            Utils.Log("Done parsing polygons.", System.Diagnostics.EventLogEntryType.Information, 126);
            #endregion parsePolygons

            return kml;
        }


        /// <summary>
        /// Importerar karta med fastighetsgränser.
        /// </summary>
        /// <param name="file">KML eller KMZ-fil att importera.</param>
        /// <param name="mapTypeId">Vilken karta vi skall importera till.</param>
        public static void ImportPropertyBoundaries(HttpPostedFile file, int mapTypeId)
        {
            if (file == null || mapTypeId < 1)
            {
                throw new ApplicationException("Ingen fil eller karta vald!");
            }

            var fiberDb = new FiberDataContext();
            var mapType = fiberDb.MapTypes.Where(m => m.Id == mapTypeId).SingleOrDefault();

            if (mapType == null) throw new ApplicationException("Map with mapTypeId=" + mapTypeId + " not found.");

            // Påvisa vem som laddade upp denna kartfil.
            var user = (from u in fiberDb.Users where (u.Username == HttpContext.Current.User.Identity.Name) select u).FirstOrDefault();

            try
            {
                KmlFile kmlFile = null;

                using (var fileStream = file.InputStream)
                {
                    if (fileStream == null || fileStream.Length < 1)
                    {
                        Utils.Log("Tom fil laddades upp. Filename=\"" + file.FileName + "\", ContentLength=" + file.ContentLength + ", ContentType=\"" + file.ContentType + "\".", System.Diagnostics.EventLogEntryType.Warning, 105);
                        throw new ApplicationException("Filen är tom, inget kan importeras.");
                    }

                    if (file.ContentType.ToLower() == "application/vnd.google-earth.kml+xml" || file.ContentType.ToLower() == "application/octet-stream")
                    {
                        kmlFile = KmlFile.Load(file.InputStream);
                    }
                    else if (file.ContentType.ToLower() == "application/vnd.google-earth.kmz")
                    {
                        kmlFile = KmlFile.LoadFromKmz(KmzFile.Open(file.InputStream));
                    } 
                    else
                    {
                        Utils.Log("Fil för fastighetsgräns som laddades upp stöds inte av systemet. Användare=" + HttpContext.Current.User.Identity.Name + ", Filnamn=\"" + file.FileName + "\", ContentLength=" + file.ContentLength + ", ContentType=\"" + file.ContentType + "\".", System.Diagnostics.EventLogEntryType.Warning, 105);
                        throw new ApplicationException("Denna filtyp stöds inte.");
                    }
                }

                var destinationKml = new SharpKml.Dom.Kml();
                var rootDocument = new Document() { Name = "Fastighetsgränser " + mapType.Title };
                destinationKml.Feature = rootDocument;

                var linesDocument = new Document() { Name = "Gränser" };
                rootDocument.AddFeature(linesDocument);

                var markersDocument = new Document() { Name = "Fastighetsnamn" };
                rootDocument.AddFeature(markersDocument);

                // Vi använder bara en Style för alla gränslinjer, eftersom de ju är av samma typ. På så sätt blir filen rätt mycket mindre.
                linesDocument.AddStyle(new Style()
                {
                    Id = "pb",
                    Line = new LineStyle()
                    {
                        Color = new Color32(115, 255, 0, 255),
                        Width = 2
                    }
                });

                markersDocument.AddStyle(new Style()
                {
                    Id = "pn",
                    Icon = new IconStyle()
                    {
                        Icon = new IconStyle.IconLink(
                            new Uri("http://maps.google.com/mapfiles/kml/shapes/placemark_circle.png")
                        )
                    }
                });

                // Loopa igenom alla linjer som finns i filen.
                foreach (var lineString in kmlFile.Root.Flatten().OfType<LineString>())
                {
                    // Ta bara med linjer som har fler koordinater än en(ja, man kan få linjer med färre koordinater om man tar bort en linje i Google Earth så bara en punkt blir kvar).
                    if (lineString.Coordinates.Count > 1)
                    {
                        var newPlacemark = new Placemark()
                        {
                            StyleUrl = new Uri("#pb", UriKind.Relative),
                            Geometry = lineString
                        };

                        linesDocument.AddFeature(newPlacemark);
                    }
                }

                foreach (var placemark in kmlFile.Root.Flatten().OfType<SharpKml.Dom.Placemark>().Where(p => p.Geometry.GetType() == typeof(SharpKml.Dom.Point)))
                {
                    var newPlacemark = new Placemark()
                    {
                        Name = placemark.Name,
                        Geometry = placemark.Geometry,
                        StyleUrl = new Uri("#pn", UriKind.Relative),
                    };

                    if (placemark.ExtendedData != null)
                    {
                        newPlacemark.ExtendedData = placemark.ExtendedData;
                    }

                    markersDocument.AddFeature(newPlacemark);
                }

                // Tar bort redan uppladdad karta, man kan bara ha en fastighetskarta per karta.
                fiberDb.MapFiles.DeleteAllOnSubmit(mapType.MapFiles);

                var destinationKmlFile = KmlFile.Create(destinationKml, false);

                using (var stream = new MemoryStream())
                {
                    using (var kmzFile = KmzFile.Create(destinationKmlFile))
                    {
                        kmzFile.Save(stream);
                    }

                    stream.Position = 0;

                    mapType.MapFiles.Add(new MapFile()
                       {
                           Id = Guid.NewGuid(),
                           Created = DateTime.Now,
                           MapTypeId = mapTypeId,
                           CreatorId = user.Id,
                           MapData = new System.Data.Linq.Binary(stream.ToArray())
                       });
                }

                Utils.Log("Sparar ner kartfil med fastighetsgränser till databasen.", System.Diagnostics.EventLogEntryType.Information, 126);

                fiberDb.SubmitChanges();
            }
            catch (Exception ex)
            {
                try
                {
                    using (var mail = new MailMessage(
                            "noreply@fiberkartan.se",
                            ConfigurationManager.AppSettings.Get("adminMail"),
                            "Misslyckad import av karta med fastighetsgränser",
                            "För användare " + user.Username + ", MaptypeId=" + mapTypeId + ", Errormsg=" + ex.Message + ", Stacktrace=" + ex.StackTrace
                        ))
                    {
                        using (var SMTPServer = new SmtpClient())
                        {
                            SMTPServer.Send(mail);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Utils.Log("Failed to send mail of import-error. Errormsg=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 151);
                }

                throw ex;
            }
        }

        /// <summary>
        /// Exporterar en karta till en KML-sträng.
        /// </summary>
        /// <param name="mapTypeId">Id på karta som skall exporteras</param>
        /// <param name="mapVersion">Version av kartan som skall exporteras, 0 = senaste version.</param>
        /// <param name="exportOptions">Vilken information som skall exporteras</param>
        /// <returns>Kartan serialiserad till en KML-sträng</returns>
        public static string ExportMapToKmlString(int mapTypeId, int mapVersion, MapEntityEnum exportOptions)
        {
            var fiberDb = new FiberDataContext();

            FiberKartan.Map map = null;
            if (mapVersion > 0)
            {
                // Hämtar en specifik version av kartan.
                map = (from m in fiberDb.Maps where (m.MapTypeId == mapTypeId && m.Ver == mapVersion) select m).FirstOrDefault();
            }
            else
            {
                // Hämtar den senaste versionen av kartan.
                map = (from m in fiberDb.Maps.OrderByDescending(m => m.Ver) where m.MapTypeId == mapTypeId select m).FirstOrDefault();
            }

            if (map == null)
            {
                throw new ApplicationException("Could not export map to KML, map with ID=" + mapTypeId + " not found.");
            }

            var kml = new SharpKml.Dom.Kml();
            var rootDocument = new Document();
            rootDocument.Name = map.MapType.Title + " - " + DateTime.Now.ToString();

            if (exportOptions.HasFlag(MapEntityEnum.HouseYes))
            {
                WriteHouseYesMarkers(rootDocument, map);
            }
            if (exportOptions.HasFlag(MapEntityEnum.HouseMaybe))
            {
                WriteHouseMaybeMarkers(rootDocument, map);
            }
            if (exportOptions.HasFlag(MapEntityEnum.HouseNotContacted))
            {
                WriteHouseNotContactedMarkers(rootDocument, map);
            }
            if (exportOptions.HasFlag(MapEntityEnum.HouseNo))
            {
                WriteHouseNoMarkers(rootDocument, map);
            }

            if (exportOptions.HasFlag(MapEntityEnum.FiberNode))
            {
                WriteFiberNodeMarkers(rootDocument, map);
            }
            if (exportOptions.HasFlag(MapEntityEnum.FiberBox))
            {
                WriteFiberBoxMarkers(rootDocument, map);
            }
            if (exportOptions.HasFlag(MapEntityEnum.RoadCrossing_Existing))
            {
                WriteRoadCrossingExistingMarkers(rootDocument, map);
            }
            if (exportOptions.HasFlag(MapEntityEnum.RoadCrossing_ToBeMade))
            {
                WriteRoadCrossingToBeMadeMarkers(rootDocument, map);
            }
            if (exportOptions.HasFlag(MapEntityEnum.Fornlamning))
            {
                WriteFornlamningMarkers(rootDocument, map);
            }
            if (exportOptions.HasFlag(MapEntityEnum.Observe))
            {
                WriteObserveMarkers(rootDocument, map);
            }
            if (exportOptions.HasFlag(MapEntityEnum.Note))
            {
                WriteNoteMarkers(rootDocument, map);
            }

            if (exportOptions.HasFlag(MapEntityEnum.FiberCable))
            {
                WriteFiberCables(rootDocument, map);
            }
            if (exportOptions.HasFlag(MapEntityEnum.Region))
            {
                WriteRegions(rootDocument, map);
            }

            kml.Feature = rootDocument;
            var serializer = new Serializer();
            serializer.Serialize(kml);

            return serializer.Xml;
        }

        private static void WriteHouseYesMarkers(Document rootDocument, Map map)
        {
            var houseYes = (from m in map.Markers where m.MarkerType.Name == MapEntityName.HouseYes select m);

            if (houseYes.Count() > 0)
            {
                var document = new Document() { Name = houseYes.First().MarkerType.Description };
                // Vi använder bara en Style för alla markörer, eftersom de ju är av samma typ. På så sätt blir KML-filen rätt mycket mindre.
                document.AddStyle(new Style()
                {
                    Id = "houseYes",
                    Icon = new IconStyle()
                    {
                        Icon = new IconStyle.IconLink(
                            new Uri(ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"] + houseYes.First().MarkerType.DestIcon)
                        )
                    }
                });

                foreach (var house in houseYes)
                {
                    var placemark = new Placemark()
                    {
                        Id = "FK_M" + house.Uid,
                        Name = house.Name,
                        Description = new Description()
                        {
                            Text = house.Description
                        },
                        Geometry = new Point()
                        {
                            Coordinate = new Vector(house.Latitude, house.Longitude, 0)
                        },
                        StyleUrl = new Uri("#houseYes", UriKind.Relative),
                        ExtendedData = new ExtendedData()
                    };
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "markerType",
                        Value = house.MarkerType.Name
                    });
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "settings",
                        Value = house.Settings.ToString()
                    });
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "optionalInfo",
                        Value = house.OptionalInfo
                    });

                    document.AddFeature(placemark);
                }

                rootDocument.AddFeature(document);
            }
        }

        private static void WriteHouseMaybeMarkers(Document rootDocument, Map map)
        {
            var houseMaybe = (from m in map.Markers where m.MarkerType.Name == MapEntityName.HouseMaybe select m);

            if (houseMaybe.Count() > 0)
            {
                var document = new Document() { Name = houseMaybe.First().MarkerType.Description };
                // Vi använder bara en Style för alla markörer, eftersom de ju är av samma typ. På så sätt blir KML-filen rätt mycket mindre.
                document.AddStyle(new Style()
                {
                    Id = "houseMaybe",
                    Icon = new IconStyle()
                    {
                        Icon = new IconStyle.IconLink(
                            new Uri(ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"] + houseMaybe.First().MarkerType.DestIcon)
                        )
                    }
                });

                foreach (var house in houseMaybe)
                {
                    var placemark = new Placemark()
                    {
                        Id = "FK_M" + house.Uid,
                        Name = house.Name,
                        Description = new Description()
                        {
                            Text = house.Description
                        },
                        Geometry = new Point()
                        {
                            Coordinate = new Vector(house.Latitude, house.Longitude, 0)
                        },
                        StyleUrl = new Uri("#houseMaybe", UriKind.Relative),
                        ExtendedData = new ExtendedData()
                    };
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "markerType",
                        Value = house.MarkerType.Name
                    });
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "settings",
                        Value = house.Settings.ToString()
                    });
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "optionalInfo",
                        Value = house.OptionalInfo
                    });

                    document.AddFeature(placemark);
                }

                rootDocument.AddFeature(document);
            }
        }

        private static void WriteHouseNotContactedMarkers(Document rootDocument, Map map)
        {
            var houseNotContacted = (from m in map.Markers where m.MarkerType.Name == MapEntityName.HouseNotContacted select m);

            if (houseNotContacted.Count() > 0)
            {
                var document = new Document() { Name = houseNotContacted.First().MarkerType.Description };
                // Vi använder bara en Style för alla markörer, eftersom de ju är av samma typ. På så sätt blir KML-filen rätt mycket mindre.
                document.AddStyle(new Style()
                {
                    Id = "houseNotContacted",
                    Icon = new IconStyle()
                    {
                        Icon = new IconStyle.IconLink(
                            new Uri(ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"] + houseNotContacted.First().MarkerType.DestIcon)
                        )
                    }
                });

                foreach (var house in houseNotContacted)
                {
                    var placemark = new Placemark()
                    {
                        Id = "FK_M" + house.Uid,
                        Name = house.Name,
                        Description = new Description()
                        {
                            Text = house.Description
                        },
                        Geometry = new Point()
                        {
                            Coordinate = new Vector(house.Latitude, house.Longitude, 0)
                        },
                        StyleUrl = new Uri("#houseNotContacted", UriKind.Relative),
                        ExtendedData = new ExtendedData()
                    };
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "markerType",
                        Value = house.MarkerType.Name
                    });
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "settings",
                        Value = house.Settings.ToString()
                    });
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "optionalInfo",
                        Value = house.OptionalInfo
                    });

                    document.AddFeature(placemark);
                }

                rootDocument.AddFeature(document);
            }
        }

        private static void WriteHouseNoMarkers(Document rootDocument, Map map)
        {
            var houseNo = (from m in map.Markers where m.MarkerType.Name == MapEntityName.HouseNo select m);

            if (houseNo.Count() > 0)
            {
                var document = new Document() { Name = houseNo.First().MarkerType.Description };
                // Vi använder bara en Style för alla markörer, eftersom de ju är av samma typ. På så sätt blir KML-filen rätt mycket mindre.
                document.AddStyle(new Style()
                {
                    Id = "houseNo",
                    Icon = new IconStyle()
                    {
                        Icon = new IconStyle.IconLink(
                            new Uri(ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"] + houseNo.First().MarkerType.DestIcon)
                        )
                    }
                });

                foreach (var house in houseNo)
                {
                    var placemark = new Placemark()
                    {
                        Id = "FK_M" + house.Uid,
                        Name = house.Name,
                        Description = new Description()
                        {
                            Text = house.Description
                        },
                        Geometry = new Point()
                        {
                            Coordinate = new Vector(house.Latitude, house.Longitude, 0)
                        },
                        StyleUrl = new Uri("#houseNo", UriKind.Relative),
                        ExtendedData = new ExtendedData()
                    };
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "markerType",
                        Value = house.MarkerType.Name
                    });
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "settings",
                        Value = house.Settings.ToString()
                    });
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "optionalInfo",
                        Value = house.OptionalInfo
                    });

                    document.AddFeature(placemark);
                }

                rootDocument.AddFeature(document);
            }
        }

        private static void WriteFiberNodeMarkers(Document rootDocument, Map map)
        {
            var fiberNodes = (from m in map.Markers where m.MarkerType.Name == MapEntityName.FiberNode select m);

            if (fiberNodes.Count() > 0)
            {
                var document = new Document() { Name = fiberNodes.First().MarkerType.Description };
                // Vi använder bara en Style för alla markörer, eftersom de ju är av samma typ. På så sätt blir KML-filen rätt mycket mindre.
                document.AddStyle(new Style()
                {
                    Id = "fiberNode",
                    Icon = new IconStyle()
                    {
                        Icon = new IconStyle.IconLink(
                            new Uri(ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"] + fiberNodes.First().MarkerType.DestIcon)
                        )
                    }
                });

                foreach (var fiberNode in fiberNodes)
                {
                    var placemark = new Placemark()
                    {
                        Id = "FK_M" + fiberNode.Uid,
                        Name = fiberNode.Name,
                        Description = new Description()
                        {
                            Text = fiberNode.Description
                        },
                        Geometry = new Point()
                        {
                            Coordinate = new Vector(fiberNode.Latitude, fiberNode.Longitude, 0)
                        },
                        StyleUrl = new Uri("#fiberNode", UriKind.Relative),
                        ExtendedData = new ExtendedData()
                    };
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "markerType",
                        Value = fiberNode.MarkerType.Name
                    });

                    document.AddFeature(placemark);
                }

                rootDocument.AddFeature(document);
            }
        }

        private static void WriteFiberBoxMarkers(Document rootDocument, Map map)
        {
            var fiberBoxes = (from m in map.Markers where m.MarkerType.Name == MapEntityName.FiberBox select m);

            if (fiberBoxes.Count() > 0)
            {
                var document = new Document() { Name = fiberBoxes.First().MarkerType.Description };
                // Vi använder bara en Style för alla markörer, eftersom de ju är av samma typ. På så sätt blir KML-filen rätt mycket mindre.
                document.AddStyle(new Style()
                {
                    Id = "fiberBox",
                    Icon = new IconStyle()
                    {
                        Icon = new IconStyle.IconLink(
                            new Uri(ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"] + fiberBoxes.First().MarkerType.DestIcon)
                        )
                    }
                });

                foreach (var fiberBox in fiberBoxes)
                {
                    var placemark = new Placemark()
                    {
                        Id = "FK_M" + fiberBox.Uid,
                        Name = "KS" + fiberBox.Name,
                        Description = new Description()
                        {
                            Text = fiberBox.Description
                        },
                        Geometry = new Point()
                        {
                            Coordinate = new Vector(fiberBox.Latitude, fiberBox.Longitude, 0)
                        },
                        StyleUrl = new Uri("#fiberBox", UriKind.Relative),
                        ExtendedData = new ExtendedData()
                    };
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "markerType",
                        Value = fiberBox.MarkerType.Name
                    });

                    document.AddFeature(placemark);
                }

                rootDocument.AddFeature(document);
            }
        }

        private static void WriteRoadCrossingExistingMarkers(Document rootDocument, Map map)
        {
            var roadCrossings = (from m in map.Markers where m.MarkerType.Name == MapEntityName.RoadCrossing_Existing select m);

            if (roadCrossings.Count() > 0)
            {
                var document = new Document() { Name = roadCrossings.First().MarkerType.Description };
                // Vi använder bara en Style för alla markörer, eftersom de ju är av samma typ. På så sätt blir KML-filen rätt mycket mindre.
                document.AddStyle(new Style()
                {
                    Id = "roadCrossingExisting",
                    Icon = new IconStyle()
                    {
                        Icon = new IconStyle.IconLink(
                            new Uri(ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"] + roadCrossings.First().MarkerType.DestIcon)
                        )
                    }
                });

                foreach (var crossing in roadCrossings)
                {
                    var placemark = new Placemark()
                    {
                        Id = "FK_M" + crossing.Uid,
                        Name = crossing.Name,
                        Description = new Description()
                        {
                            Text = crossing.Description
                        },
                        Geometry = new Point()
                        {
                            Coordinate = new Vector(crossing.Latitude, crossing.Longitude, 0)
                        },
                        StyleUrl = new Uri("#roadCrossingExisting", UriKind.Relative),
                        ExtendedData = new ExtendedData()
                    };
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "markerType",
                        Value = crossing.MarkerType.Name
                    });

                    document.AddFeature(placemark);
                }

                rootDocument.AddFeature(document);
            }
        }

        private static void WriteRoadCrossingToBeMadeMarkers(Document rootDocument, Map map)
        {
            var roadCrossings = (from m in map.Markers where m.MarkerType.Name == MapEntityName.RoadCrossing_ToBeMade select m);

            if (roadCrossings.Count() > 0)
            {
                var document = new Document() { Name = roadCrossings.First().MarkerType.Description };
                // Vi använder bara en Style för alla markörer, eftersom de ju är av samma typ. På så sätt blir KML-filen rätt mycket mindre.
                document.AddStyle(new Style()
                {
                    Id = "roadCrossingToBeMade",
                    Icon = new IconStyle()
                    {
                        Icon = new IconStyle.IconLink(
                            new Uri(ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"] + roadCrossings.First().MarkerType.DestIcon)
                        )
                    }
                });

                foreach (var crossing in roadCrossings)
                {
                    var placemark = new Placemark()
                    {
                        Id = "FK_M" + crossing.Uid,
                        Name = crossing.Name,
                        Description = new Description()
                        {
                            Text = crossing.Description
                        },
                        Geometry = new Point()
                        {
                            Coordinate = new Vector(crossing.Latitude, crossing.Longitude, 0)
                        },
                        StyleUrl = new Uri("#roadCrossingToBeMade", UriKind.Relative),
                        ExtendedData = new ExtendedData()
                    };
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "markerType",
                        Value = crossing.MarkerType.Name
                    });

                    document.AddFeature(placemark);
                }

                rootDocument.AddFeature(document);
            }
        }

        private static void WriteFornlamningMarkers(Document rootDocument, Map map)
        {
            var fornlamningar = (from m in map.Markers where m.MarkerType.Name == MapEntityName.Fornlamning select m);

            if (fornlamningar.Count() > 0)
            {
                var document = new Document() { Name = fornlamningar.First().MarkerType.Description };
                // Vi använder bara en Style för alla markörer, eftersom de ju är av samma typ. På så sätt blir KML-filen rätt mycket mindre.
                document.AddStyle(new Style()
                {
                    Id = "fornlamning",
                    Icon = new IconStyle()
                    {
                        Icon = new IconStyle.IconLink(
                            new Uri(ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"] + fornlamningar.First().MarkerType.DestIcon)
                        )
                    }
                });

                foreach (var fornlamning in fornlamningar)
                {
                    var placemark = new Placemark()
                    {
                        Id = "FK_M" + fornlamning.Uid,
                        Name = fornlamning.Name,
                        Description = new Description()
                        {
                            Text = fornlamning.Description
                        },
                        Geometry = new Point()
                        {
                            Coordinate = new Vector(fornlamning.Latitude, fornlamning.Longitude, 0)
                        },
                        StyleUrl = new Uri("#fornlamning", UriKind.Relative),
                        ExtendedData = new ExtendedData()
                    };
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "markerType",
                        Value = fornlamning.MarkerType.Name
                    });

                    document.AddFeature(placemark);
                }

                rootDocument.AddFeature(document);
            }
        }

        private static void WriteObserveMarkers(Document rootDocument, Map map)
        {
            var observes = (from m in map.Markers where m.MarkerType.Name == MapEntityName.Observe select m);

            if (observes.Count() > 0)
            {
                var document = new Document() { Name = observes.First().MarkerType.Description };
                // Vi använder bara en Style för alla markörer, eftersom de ju är av samma typ. På så sätt blir KML-filen rätt mycket mindre.
                document.AddStyle(new Style()
                {
                    Id = "observe",
                    Icon = new IconStyle()
                    {
                        Icon = new IconStyle.IconLink(
                            new Uri(ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"] + observes.First().MarkerType.DestIcon)
                        )
                    }
                });

                foreach (var observe in observes)
                {
                    var placemark = new Placemark()
                    {
                        Id = "FK_M" + observe.Uid,
                        Name = observe.Name,
                        Description = new Description()
                        {
                            Text = observe.Description
                        },
                        Geometry = new Point()
                        {
                            Coordinate = new Vector(observe.Latitude, observe.Longitude, 0)
                        },
                        StyleUrl = new Uri("#observe", UriKind.Relative),
                        ExtendedData = new ExtendedData()
                    };
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "markerType",
                        Value = observe.MarkerType.Name
                    });

                    document.AddFeature(placemark);
                }

                rootDocument.AddFeature(document);
            }
        }

        private static void WriteNoteMarkers(Document rootDocument, Map map)
        {
            var notes = (from m in map.Markers where m.MarkerType.Name == MapEntityName.Note select m);

            if (notes.Count() > 0)
            {
                var document = new Document() { Name = notes.First().MarkerType.Description };
                // Vi använder bara en Style för alla markörer, eftersom de ju är av samma typ. På så sätt blir KML-filen rätt mycket mindre.
                document.AddStyle(new Style()
                {
                    Id = "note",
                    Icon = new IconStyle()
                    {
                        Icon = new IconStyle.IconLink(
                            new Uri(ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"] + notes.First().MarkerType.DestIcon)
                        )
                    }
                });

                foreach (var note in notes)
                {
                    var placemark = new Placemark()
                    {
                        Id = "FK_M" + note.Uid,
                        Name = note.Name,
                        Description = new Description()
                        {
                            Text = note.Description
                        },
                        Geometry = new Point()
                        {
                            Coordinate = new Vector(note.Latitude, note.Longitude, 0)
                        },
                        StyleUrl = new Uri("#note", UriKind.Relative),
                        ExtendedData = new ExtendedData()
                    };
                    placemark.ExtendedData.AddData(new Data()
                    {
                        Name = "markerType",
                        Value = note.MarkerType.Name
                    });

                    document.AddFeature(placemark);
                }

                rootDocument.AddFeature(document);
            }
        }

        private static void WriteFiberCables(Document rootDocument, Map map)
        {
            var lines = (from l in map.Lines select l);

            if (lines.Count() > 0)
            {
                var document = new Document() { Name = "Schakt" };
                // Vi använder bara en Style för alla linjer, eftersom de ju är av samma typ. På så sätt blir KML-filen rätt mycket mindre.
                document.AddStyle(new Style()
                {
                    Id = "fiberDig",
                    Line = new LineStyle()
                    {
                        Color = new Color32(115, 255, 0, 0),
                        Width = 4
                    }
                });

                foreach (var line in lines)
                {
                    var coordinates = new CoordinateCollection();
                    string[] coordinatePair = null;
                    var coordinatesArray = line.Coordinates.Split('|');
                    foreach (var coordinate in coordinatesArray)
                    {
                        coordinatePair = coordinate.Split(':');
                        coordinates.Add(new Vector(double.Parse(coordinatePair[0], NumberStyles.Float, CultureInfo.InvariantCulture), double.Parse(coordinatePair[1], NumberStyles.Float, CultureInfo.InvariantCulture), 0));
                    }

                    var lineString = new LineString()
                    {
                        Extrude = true,
                        Tessellate = true,
                        Coordinates = coordinates
                    };

                    var placemark = new Placemark()
                    {
                        Id = "FK_L" + line.Uid,
                        Name = line.Name,
                        Description = new Description()
                        {
                            Text = line.Description
                        },
                        StyleUrl = new Uri("#fiberDig", UriKind.Relative),
                        Geometry = lineString
                    };

                    document.AddFeature(placemark);
                }

                rootDocument.AddFeature(document);
            }
        }

        private static void WriteRegions(Document rootDocument, Map map)
        {
            var regions = (from r in map.Regions select r);

            if (regions.Count() > 0)
            {
                var document = new Document() { Name = "Områden" };
                // Vi använder bara en Style för alla polygoner, eftersom de ju är av samma typ. På så sätt blir KML-filen rätt mycket mindre.
                document.AddStyle(new Style()
                {
                    Id = "region",
                    Polygon = new PolygonStyle()
                    {
                        Color = new Color32(115, 255, 153, 153),
                        Fill = true,
                        Outline = true
                    },
                    Line = new LineStyle()
                    {
                        Color = new Color32(128, 0, 0, 0),
                        Width = 2
                    }
                });

                foreach (var region in regions)
                {
                    var coordinates = new CoordinateCollection();
                    string[] coordinatePair = null;
                    var coordinatesArray = region.Coordinates.Split('|');
                    foreach (var coordinate in coordinatesArray)
                    {
                        coordinatePair = coordinate.Split(':');
                        coordinates.Add(new Vector(double.Parse(coordinatePair[0], NumberStyles.Float, CultureInfo.InvariantCulture), double.Parse(coordinatePair[1], NumberStyles.Float, CultureInfo.InvariantCulture), 0));
                    }

                    var polygon = new Polygon()
                    {
                        OuterBoundary = new OuterBoundary()
                        {
                            LinearRing = new LinearRing()
                            {
                                Tessellate = true,
                                Coordinates = coordinates
                            }
                        }
                    };

                    var placemark = new Placemark()
                    {
                        Id = "FK_R" + region.Uid,
                        Name = region.Name,
                        Description = new Description()
                        {
                            Text = region.Description
                        },
                        StyleUrl = new Uri("#region", UriKind.Relative),
                        Geometry = polygon
                    };

                    document.AddFeature(placemark);
                }

                rootDocument.AddFeature(document);
            }
        }
    }

    /// <summary>
    /// Delar som en karta kan bestå av (olika typer av markörer, områden, schaktsträcka, osv..)
    /// </summary>
    [Flags]
    public enum MapEntityEnum : int
    {
        Unknown = 0,
        HouseYes = 1,
        HouseMaybe = 2,
        HouseNo = 4,
        HouseNotContacted = 8,
        FiberNode = 16,
        FiberBox = 32,
        RoadCrossing_Existing = 64,
        RoadCrossing_ToBeMade = 128,
        Fornlamning = 256,
        Observe = 512,
        FiberCable = 1024,
        Region = 2048,
        Note = 4096
    }

    /// <summary>
    /// Delar som en karta kan bestå av (olika typer av markörer, områden, schaktsträcka, osv..)
    /// </summary>
    public sealed class MapEntityName
    {
        public static readonly string Unknown = "Unknown";
        public static readonly string HouseYes = "HouseYes";
        public static readonly string HouseMaybe = "HouseMaybe";
        public static readonly string HouseNo = "HouseNo";
        public static readonly string HouseNoFiber = "HouseNoFiber";
        public static readonly string HouseNotContacted = "HouseNotContacted";
        public static readonly string FiberNode = "FiberNode";
        public static readonly string FiberBox = "FiberBox";
        public static readonly string RoadCrossing_Existing = "RoadCrossing_Existing";
        public static readonly string RoadCrossing_ToBeMade = "RoadCrossing_ToBeMade";
        public static readonly string Fornlamning = "Fornlamning";
        public static readonly string Observe = "Observe";
        public static readonly string FiberCable = "FiberCable";
        public static readonly string Region = "Region";
        public static readonly string Note = "Note";
    }

    public sealed class MarkerStyleLookup
    {
        public string Href { get; set; }
        public string ImageName { get; set; }
    }
}