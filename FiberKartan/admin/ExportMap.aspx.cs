using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FiberKartan.Kml;
using MightyLittleGeodesy.Positions;
using OfficeOpenXml;
using OfficeOpenXml.Style;

/*
The zlib/libpng License
Copyright (c) 2012 Henrik Östman

This software is provided 'as-is', without any express or implied warranty. In no event will the authors be held liable for any damages arising from the use of this software.
Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/
namespace FiberKartan.Admin
{
    public partial class ExportMap : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void ExportButton_Click(object sender, EventArgs e)
        {
            var mapTypeId = int.Parse(Request.QueryString["mid"]);
            var mapVersion = int.Parse(Request.QueryString["ver"]);

            MapEntityEnum exportOptions = MapEntityEnum.Unknown;

            if (HouseYesBox.Checked)
                exportOptions |= MapEntityEnum.HouseYes;
            if (HouseMaybeBox.Checked)
                exportOptions |= MapEntityEnum.HouseMaybe;
            if (HouseNoBox.Checked)
                exportOptions |= MapEntityEnum.HouseNo;
            if (HouseNotContactedBox.Checked)
                exportOptions |= MapEntityEnum.HouseNotContacted;
            if (FiberNodeBox.Checked)
                exportOptions |= MapEntityEnum.FiberNode;
            if (FiberBoxBox.Checked)
                exportOptions |= MapEntityEnum.FiberBox;
            if (FiberCableBox.Checked)
                exportOptions |= MapEntityEnum.FiberCable;
            if (RoadCrossing_ExistingBox.Checked)
                exportOptions |= MapEntityEnum.RoadCrossing_Existing;
            if (RoadCrossing_ToBeMadeBox.Checked)
                exportOptions |= MapEntityEnum.RoadCrossing_ToBeMade;
            if (FornlamningBox.Checked)
                exportOptions |= MapEntityEnum.Fornlamning;
            if (ObserveBox.Checked)
                exportOptions |= MapEntityEnum.Observe;
            if (NoteBox.Checked)
                exportOptions |= MapEntityEnum.Note;
            if (RegionBox.Checked)
                exportOptions |= MapEntityEnum.Region;

            ExportMapInformation(mapTypeId, mapVersion, exportFile.SelectedValue, exportOptions);
        }

        /// <summary>
        /// Exporterar vald information till en fil som användaren ombeds att ladda ner.
        /// </summary>
        /// <param name="mapTypeId">Vilken karta som skall exporteras</param>
        /// <param name="mapVersion">Vilken version av kartan som skall exporteras</param>
        /// <param name="fileType">Filtyp man vill exportera till</param>
        /// <param name="exportOptions">Vilken information som skall exporteras</param>
        private void ExportMapInformation(int mapTypeId, int mapVersion, string fileType, MapEntityEnum exportOptions)
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

            if (map == null || !Utils.GetMapAccessRights(mapTypeId).HasFlag(MapAccessRights.Export))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                Response.StatusDescription = "Forbidden";
                Response.Write("Du saknar rättigheter för att få exportera kartan.");
                Response.End();
            }

            if (!string.IsNullOrEmpty(fileType))
            {
                if (fileType.Trim().ToLower() == "kml")
                {
                    #region KML

                    Utils.Log("Creating KML-file for download. MapTypeId=" + map.MapTypeId + ", Ver=" + map.Ver, System.Diagnostics.EventLogEntryType.Information, 104);

                    Response.ContentType = "application/vnd.google-earth.kml+xml";
                    Response.AddHeader("Content-Disposition", "attachment; filename=fiber_map" + map.MapTypeId + "_v" + map.Ver + ".kml");

                    var kmlString = KmlImportExport.ExportMapToKmlString(map.MapTypeId, map.Ver, exportOptions);

                    Response.Write(kmlString);
                    Utils.Log("Done writing KML-file to stream.", System.Diagnostics.EventLogEntryType.Information, 105);
                    Response.End();

                    #endregion KML
                }
                else if (fileType.Trim().ToLower() == "excel")
                {
                    #region Excel

                    // http://epplus.codeplex.com/wikipage?title=WebapplicationExample
                    // http://epplus.codeplex.com/wikipage?title=FAQ&referringTitle=Documentation

                    Utils.Log("Creating Excel-file for download. MapTypeId=" + map.MapTypeId + ", Ver=" + map.Ver, System.Diagnostics.EventLogEntryType.Information, 106);
                    using (ExcelPackage pck = new ExcelPackage())
                    {
                        //
                        // Se även ExportMapToKmlString() i KmlImportExport.cs för exportering till Kml.
                        //
                        #region Interested_Markers

                        if (exportOptions.HasFlag(MapEntityEnum.HouseYes))
                        {
                            var houseYes = (from m in map.Markers where m.MarkerType.Name == MapEntityName.HouseYes select m);
                            AddExcelWorkSheet(pck, "Skall anslutas", houseYes, MapEntityEnum.HouseYes);
                        }
                        if (exportOptions.HasFlag(MapEntityEnum.HouseMaybe))
                        {
                            var houseMaybe = (from m in map.Markers where m.MarkerType.Name == MapEntityName.HouseMaybe select m);
                            AddExcelWorkSheet(pck, "Kanske skall anslutas", houseMaybe, MapEntityEnum.HouseMaybe);
                        }

                        #endregion Interested_Markers

                        #region Not_Interested_Markers

                        if (exportOptions.HasFlag(MapEntityEnum.HouseNo))
                        {
                            var noHouse = (from m in map.Markers where m.MarkerType.Name == MapEntityName.HouseNo select m);
                            AddExcelWorkSheet(pck, "Vill inte ha", noHouse, MapEntityEnum.HouseNo);
                        }
                        if (exportOptions.HasFlag(MapEntityEnum.HouseNotContacted))
                        {
                            var notContactedHouse = (from m in map.Markers where m.MarkerType.Name == MapEntityName.HouseNotContacted select m);
                            AddExcelWorkSheet(pck, "Ej svarat", notContactedHouse, MapEntityEnum.HouseNotContacted);
                        }

                        #endregion Not_Interested_Markers

                        #region Nodes

                        if (exportOptions.HasFlag(MapEntityEnum.FiberNode))
                        {
                            var nodes = (from m in map.Markers where m.MarkerType.Name == MapEntityName.FiberNode select m);
                            AddExcelWorkSheet(pck, "Noder", nodes, MapEntityEnum.FiberNode);
                        }

                        #endregion Nodes

                        #region Fiberboxes

                        if (exportOptions.HasFlag(MapEntityEnum.FiberBox))
                        {
                            var boxes = (from m in map.Markers where m.MarkerType.Name == MapEntityName.FiberBox select m);
                            AddExcelWorkSheet(pck, "Kopplingsskåp", boxes, MapEntityEnum.FiberBox);
                        }

                        #endregion Fiberboxes

                        #region FiberCable

                        if (exportOptions.HasFlag(MapEntityEnum.FiberCable))
                        {
                            var cables = (from l in map.Lines select l);
                            AddExcelWorkSheet(pck, "Schaktsträcka", cables);
                        }

                        #endregion FiberCable

                        #region RoadCrossings

                        if (exportOptions.HasFlag(MapEntityEnum.RoadCrossing_Existing))
                        {
                            var roadCrossings = (from m in map.Markers where m.MarkerType.Name == MapEntityName.RoadCrossing_Existing select m);
                            AddExcelWorkSheet(pck, "Befintliga väggenomgångar", roadCrossings, MapEntityEnum.RoadCrossing_Existing);
                        }
                        if (exportOptions.HasFlag(MapEntityEnum.RoadCrossing_ToBeMade))
                        {
                            var roadCrossings = (from m in map.Markers where m.MarkerType.Name == MapEntityName.RoadCrossing_ToBeMade select m);
                            AddExcelWorkSheet(pck, "Väggenomgångar som måste göras", roadCrossings, MapEntityEnum.RoadCrossing_ToBeMade);
                        }

                        #endregion RoadCrossings

                        #region Fornlämningar

                        if (exportOptions.HasFlag(MapEntityEnum.Fornlamning))
                        {
                            var fornlamningar = (from m in map.Markers where m.MarkerType.Name == MapEntityName.Fornlamning select m);
                            AddExcelWorkSheet(pck, "Fornlämningar", fornlamningar, MapEntityEnum.Fornlamning);
                        }

                        #endregion Fornlämningar

                        #region Observe

                        if (exportOptions.HasFlag(MapEntityEnum.Observe))
                        {
                            var observes = (from m in map.Markers where m.MarkerType.Name == MapEntityName.Observe select m);
                            AddExcelWorkSheet(pck, "Känsliga platser", observes, MapEntityEnum.Observe);
                        }

                        #endregion Observe

                        #region Note

                        if (exportOptions.HasFlag(MapEntityEnum.Note))
                        {
                            var notes = (from m in map.Markers where m.MarkerType.Name == MapEntityName.Note select m);
                            AddExcelWorkSheet(pck, "Textrutor", notes, MapEntityEnum.Note);
                        }

                        #endregion Note

                        #region Region

                        if (exportOptions.HasFlag(MapEntityEnum.Region))
                        {
                            var regions = (from r in map.Regions select r);
                            AddExcelWorkSheet(pck, "Områden", regions);
                        }

                        #endregion Region

                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("content-disposition", "attachment;  filename=fiber_" + map.MapTypeId + "_v" + map.Ver + ".xlsx");
                        Response.BinaryWrite(pck.GetAsByteArray());

                        Utils.Log("Done writing Excel-file to stream.", System.Diagnostics.EventLogEntryType.Information, 119);
                        Response.End();
                    }

                    #endregion Excel
                }
            }
        }

        /// <summary>
        /// Fyller en flik i Excel-arket med information
        /// </summary>
        /// <param name="pck">Referens till Excel-arket</param>
        /// <param name="name">Namn på den nya fliken</param>
        /// <param name="markers">Markörer som skall skrivas ut</param>
        /// <param name="entityType">Markörernas typ</param>
        private void AddExcelWorkSheet(ExcelPackage pck, string name, IEnumerable<FiberKartan.Marker> markers, MapEntityEnum entityType)
        {
            var sheet = pck.Workbook.Worksheets.Add(name);

            // Sätt standardjustering på kolumner.
            sheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(7).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(8).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(9).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(10).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            // Sätt rubrik på kolumner.
            sheet.Cells[1, 1].Value = "Id";
            sheet.Cells[1, 2].Value = "Namn";
            sheet.Cells[1, 3].Value = "Beskrivning";
            sheet.Cells[1, 4].Value = "Latitud(WGS84, Google Earth)";
            sheet.Cells[1, 5].Value = "Longitud(WGS84, Google Earth)";
            sheet.Cells[1, 6].Value = "Latitud(SWEREF 99 TM, Lantmäteriet)";
            sheet.Cells[1, 7].Value = "Longitud(SWEREF 99 TM, Lantmäteriet)";
            sheet.Cells[1, 8].Value = "RT90 X";
            sheet.Cells[1, 9].Value = "RT90 Y";
            sheet.Cells[1, 10].Value = "Direktlänk";


            //Sätt stil på rubriker.
            using (var rng = sheet.Cells["A1:J1"])
            {
                rng.Style.Font.Bold = true;
                rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));  //Set color to dark blue
                rng.Style.Font.Color.SetColor(Color.White);
                rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            // Fastigheter har en extra kolumn för bindning till koppingsskåp.
            if (entityType == MapEntityEnum.HouseYes || entityType == MapEntityEnum.HouseMaybe || entityType == MapEntityEnum.HouseNotContacted || entityType == MapEntityEnum.HouseNo)
            {
                sheet.Cells[1, 11].Value = "Tillhör kopplingsskåp";

                //Sätt stil på rubriker.
                using (var rng = sheet.Cells["K1"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));  //Set color to dark blue
                    rng.Style.Font.Color.SetColor(Color.White);
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
            }

            // Om fastigheter som vill ha fiber eller är intresserade av fiber, rendera några extra kolumner med information om markörerna.
            if (entityType == MapEntityEnum.HouseYes || entityType == MapEntityEnum.HouseMaybe)
            {
                sheet.Cells[1, 12].Value = "Har betalat insats";
                sheet.Cells[1, 13].Value = "Avser flygelavtal";
                sheet.Cells[1, 14].Value = "Önskar grävning på fastighet";
                sheet.Cells[1, 15].Value = "Vilande abonnemang";

                //Sätt stil på rubriker.
                using (var rng = sheet.Cells["L1:O1"])
                {
                    rng.Style.Font.Bold = true;
                    rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                    rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));  //Set color to dark blue
                    rng.Style.Font.Color.SetColor(Color.White);
                    rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                }
            }

            var tmpRowIndex = 2;
            foreach (var marker in markers)
            {
                sheet.Cells[tmpRowIndex, 1].Value = marker.Id;

                // Vi lägger på prefixet "KS" för alla kopplingsskåp, annars kör vi direkt på namnet i databasen.
                sheet.Cells[tmpRowIndex, 2].Value = entityType == MapEntityEnum.FiberBox ? ("KS" + marker.Name) : marker.Name;

                sheet.Cells[tmpRowIndex, 3].Value = marker.Description;

                sheet.Cells[tmpRowIndex, 4].Value = marker.Latitude;
                sheet.Cells[tmpRowIndex, 5].Value = marker.Longitude;
                var wgs84Pos = new WGS84Position(marker.Latitude, marker.Longitude);
                var rtPos = new RT90Position(wgs84Pos, RT90Position.RT90Projection.rt90_2_5_gon_v);
                var sweref99 = new SWEREF99Position(wgs84Pos, SWEREF99Position.SWEREFProjection.sweref_99_tm);
                // För mer information, se här: http://www.lantmateriet.se/Kartor-och-geografisk-information/GPS-och-geodetisk-matning/Referenssystem/Tvadimensionella-system/SWEREF-99-projektioner/
                // och här http://www.lantmateriet.se/Global/Kartor%20och%20geografisk%20information/GPS%20och%20m%C3%A4tning/Referenssystem/2D-system/kontrollpunkter_sweref99tm.pdf

                sheet.Cells[tmpRowIndex, 6].Value = String.Format("{0:0000000}", sweref99.Latitude);
                sheet.Cells[tmpRowIndex, 7].Value = String.Format("{0:000000}", sweref99.Longitude);
                sheet.Cells[tmpRowIndex, 8].Value = rtPos.Latitude;
                sheet.Cells[tmpRowIndex, 9].Value = rtPos.Longitude;
                // Skriver ut en direktlänk till Google Maps som visar markörens position.
                sheet.Cells[tmpRowIndex, 10].Value = "http://maps.google.com/maps/api/staticmap?size=800x600&maptype=hybrid&sensor=false&markers=label:P|" + marker.Latitude.ToString(CultureInfo.InvariantCulture.NumberFormat) + "," + marker.Longitude.ToString(CultureInfo.InvariantCulture.NumberFormat);

                if (entityType == MapEntityEnum.HouseYes || entityType == MapEntityEnum.HouseMaybe || entityType == MapEntityEnum.HouseNotContacted || entityType == MapEntityEnum.HouseNo)
                {
                    var optionalInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(marker.OptionalInfo);
                    sheet.Cells[tmpRowIndex, 11].Value = optionalInfo.KS;
                }

                if (entityType == MapEntityEnum.HouseYes || entityType == MapEntityEnum.HouseMaybe)
                {
                    var markerSettingsEnum = (MapEntities.Marker.MARKER_SETTINGS)marker.Settings;
                    if (markerSettingsEnum.HasFlag(MapEntities.Marker.MARKER_SETTINGS.payedStake))
                    {
                        sheet.Cells[tmpRowIndex, 12].Value = "X";
                    }
                    if (markerSettingsEnum.HasFlag(MapEntities.Marker.MARKER_SETTINGS.extraHouse))
                    {
                        sheet.Cells[tmpRowIndex, 13].Value = "X";
                    }
                    if (markerSettingsEnum.HasFlag(MapEntities.Marker.MARKER_SETTINGS.wantDigHelp))
                    {
                        sheet.Cells[tmpRowIndex, 14].Value = "X";
                    }
                    if (markerSettingsEnum.HasFlag(MapEntities.Marker.MARKER_SETTINGS.noISPsubscription))
                    {
                        sheet.Cells[tmpRowIndex, 15].Value = "X";
                    }
                }
                tmpRowIndex++;
            }

            // Anpassa columnbredd efter innehållet.
            sheet.Column(1).AutoFit();
            sheet.Column(2).AutoFit();
            sheet.Column(3).AutoFit();
            sheet.Column(4).AutoFit();
            sheet.Column(5).AutoFit();
            sheet.Column(6).AutoFit();
            sheet.Column(7).AutoFit();
            sheet.Column(8).AutoFit();
            sheet.Column(9).AutoFit();
            sheet.Column(10).AutoFit();

            if (entityType == MapEntityEnum.HouseYes || entityType == MapEntityEnum.HouseMaybe || entityType == MapEntityEnum.HouseNotContacted || entityType == MapEntityEnum.HouseNo)
            {
                sheet.Column(11).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Column(11).AutoFit();
            }

            if (entityType == MapEntityEnum.HouseYes || entityType == MapEntityEnum.HouseMaybe)
            {
                sheet.Column(12).Style.Font.Bold = true;
                sheet.Column(12).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Column(12).AutoFit();

                sheet.Column(13).Style.Font.Bold = true;
                sheet.Column(13).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Column(13).AutoFit();

                sheet.Column(14).Style.Font.Bold = true;
                sheet.Column(14).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Column(14).AutoFit();

                sheet.Column(15).Style.Font.Bold = true;
                sheet.Column(15).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Column(15).AutoFit();
            }
        }

        private void AddExcelWorkSheet(ExcelPackage pck, string name, IEnumerable<FiberKartan.Line> lines)
        {
            var sheet = pck.Workbook.Worksheets.Add(name);

            // Sätt standardjustering på kolumner.
            sheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(5).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(6).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            // Sätt rubrik på kolumner.
            sheet.Cells[1, 1].Value = "Id";
            sheet.Cells[1, 2].Value = "Namn";
            sheet.Cells[1, 3].Value = "Beskrivning";
            sheet.Cells[1, 4].Value = "Bredd";
            sheet.Cells[1, 5].Value = "Längd(meter)";
            sheet.Cells[1, 6].Value = "Direktlänk";
            sheet.Cells[1, 7].Value = "CSV";

            //Sätt stil på rubriker.
            using (var rng = sheet.Cells["A1:G1"])
            {
                rng.Style.Font.Bold = true;
                rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));  //Set color to dark blue
                rng.Style.Font.Color.SetColor(Color.White);
                rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            var tmpRowIndex = 2;
            foreach (var line in lines)
            {
                sheet.Cells[tmpRowIndex, 1].Value = line.Id;
                sheet.Cells[tmpRowIndex, 2].Value = line.Name;
                sheet.Cells[tmpRowIndex, 3].Value = line.Description;
                sheet.Cells[tmpRowIndex, 4].Value = line.Width;

                #region CalculateLineLength
                // Parsar alla koordinater till Vectors.
                var coordinates = new List<SharpKml.Base.Vector>();
                string[] coordinatePair = null;
                var coordinatesArray = line.Coordinates.Split('|');
                foreach (var coordinate in coordinatesArray)
                {
                    coordinatePair = coordinate.Split(':');
                    coordinates.Add(new SharpKml.Base.Vector(double.Parse(coordinatePair[0], NumberStyles.Float, CultureInfo.InvariantCulture), double.Parse(coordinatePair[1], NumberStyles.Float, CultureInfo.InvariantCulture), 0));
                }

                var distance = 0.0d;
                var previousPoint = coordinates.ElementAt(0);
                SharpKml.Base.Vector currentPoint = null;

                // Traverserar alla punkter på linjen och summerar avstånden för att få linjens totala längd.
                var enumerator = coordinates.GetEnumerator();
                while(enumerator.MoveNext())
                {
                    // Hoppar över första punkten, den finns redan i previousPoint.
                    if (previousPoint != enumerator.Current)
                    {
                        currentPoint = enumerator.Current;
                        distance += SharpKml.Base.MathHelpers.Distance(previousPoint, currentPoint);
                        previousPoint = currentPoint;
                    }
                }

                sheet.Cells[tmpRowIndex, 5].Value = Math.Ceiling(distance);
                #endregion CalculateLineLength

                // Skriver ut en direktlänk till Google Maps som visar linjens sträckning.
                var googleMapsPathCoordinates = line.Coordinates;
                sheet.Cells[tmpRowIndex, 6].Value = "http://maps.google.com/maps/api/staticmap?size=800x600&maptype=hybrid&sensor=false&path=color:0x0000ff|weight:5|" + googleMapsPathCoordinates.Replace(':', ',');

                var CsvCoordinates = line.Coordinates;
                sheet.Cells[tmpRowIndex, 7].Value = CsvCoordinates.Replace(':', ',').Replace('|', ' ');    // Kommaseparerad lista, ett kommatecken skiljer longitud och latitud, ett mellanslag skiljer koordinatpunkterna åt.

                tmpRowIndex++;
            }

            // Anpassa columnbredd efter innehållet.
            sheet.Column(1).AutoFit();
            sheet.Column(2).AutoFit();
            sheet.Column(3).AutoFit();
            sheet.Column(4).AutoFit();
            sheet.Column(5).AutoFit();
            sheet.Column(6).AutoFit();
            sheet.Column(7).AutoFit();
        }

        private void AddExcelWorkSheet(ExcelPackage pck, string name, IEnumerable<FiberKartan.Region> regions)
        {
            var sheet = pck.Workbook.Worksheets.Add(name);

            // Sätt standardjustering på kolumner.
            sheet.Column(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(2).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(3).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            sheet.Column(4).Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

            // Sätt rubrik på kolumner.
            sheet.Cells[1, 1].Value = "Id";
            sheet.Cells[1, 2].Value = "Namn";
            sheet.Cells[1, 3].Value = "Beskrivning";
            sheet.Cells[1, 4].Value = "CSV";

            //Sätt stil på rubriker.
            using (var rng = sheet.Cells["A1:D1"])
            {
                rng.Style.Font.Bold = true;
                rng.Style.Fill.PatternType = ExcelFillStyle.Solid;                      //Set Pattern for the background to Solid
                rng.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(79, 129, 189));  //Set color to dark blue
                rng.Style.Font.Color.SetColor(Color.White);
                rng.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            }

            var tmpRowIndex = 2;
            foreach (var region in regions)
            {
                sheet.Cells[tmpRowIndex, 1].Value = region.Id;
                sheet.Cells[tmpRowIndex, 2].Value = region.Name;
                sheet.Cells[tmpRowIndex, 3].Value = region.Description;
                sheet.Cells[tmpRowIndex, 4].Value = region.Coordinates.Replace(':', ',').Replace('|', ' ');    // Kommaseparerad lista, ett kommatecken skiljer longitud och latitud, ett mellanslag skiljer koordinatpunkterna åt.

                tmpRowIndex++;
            }

            // Anpassa columnbredd efter innehållet.
            sheet.Column(1).AutoFit();
            sheet.Column(2).AutoFit();
            sheet.Column(3).AutoFit();
            sheet.Column(4).AutoFit();
        }
    }
}