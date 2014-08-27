using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using FiberKartan;
using Newtonsoft.Json;
using System.Data.Linq;
using FiberKartan.Kml;
using FiberKartan.MapEntities;
using System.Globalization;

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
namespace FiberKartan
{
    /// <summary>
    /// Kartvy för en fiberförening.
    /// Henrik Östman, 2011-06-27.
    /// </summary>
    public partial class Map : System.Web.UI.Page
    {
        private int mapId;
        private int mapVersion;
        private FiberDataContext fiberDb;
        private MapContent mapContent;

        /// <summary>
        /// Metod som exekveras vid sidladdning.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            mapId = 1;  // Om inget annat anges.
            int.TryParse(Request["mid"], out mapId);
            int.TryParse(Request["ver"], out mapVersion);

            LoadAndRenderMapContent();
        }

        /// <summary>
        /// Hämtar upp kartinformation från databas och renderar kartan.
        /// </summary>
        private void LoadAndRenderMapContent()
        {
            mapContent = new MapContent();
            fiberDb = new FiberDataContext();

            FiberKartan.Map map = null;
            if (mapVersion > 0)
            {
                map = (from m in fiberDb.Maps where (m.MapTypeId == mapId && m.Ver == mapVersion) select m).FirstOrDefault();
            }
            else
            {
                map = (from m in fiberDb.Maps.OrderByDescending(m => m.Ver) where m.MapTypeId == mapId select m).FirstOrDefault();
            }

            if (map != null && ((MapViewSettings)map.MapType.ViewSettings).HasFlag(MapViewSettings.PublicVisible))
            {
                mapContent.MapTypeId = map.MapTypeId;
                mapContent.MapVer = map.Ver;
                mapContent.MapName = map.MapType.Title;
                mapContent.Created = map.Created.ToString();
                mapContent.Views = map.Views;

                var hasPropertyBoundaries = map.MapType.MapFiles.Any();
                if (hasPropertyBoundaries)
                {
                    mapContent.PropertyBoundariesFile = ConfigurationManager.AppSettings["ServerAdress"] + "/FileTransferHandler.ashx?map=" + map.MapType.MapFiles.First().Id;
                }

                // Visar olika mycket med information på kartan utifrån inställningarna på kartan som är lagrade i databasen.
                var viewSettings = ((MapViewSettings)map.MapType.ViewSettings);

                mainPalette.Visible = viewSettings.HasFlag(MapViewSettings.ShowPalette);
                connectionStatisticsPanel.Visible = viewSettings.HasFlag(MapViewSettings.ShowConnectionStatistics);
                totalDigLengthStatisticsPanel.Visible = viewSettings.HasFlag(MapViewSettings.ShowTotalDigLengthStatistics);


                map.Views++;    // Öka räknare och spara till databas.
                fiberDb.SubmitChanges();

                #region Skapa komplext objekt med alla markörer, fibersträckor, områden, m.m.

                if (!string.IsNullOrEmpty(map.MapType.Municipality.CenterLatitude) && !string.IsNullOrEmpty(map.MapType.Municipality.CenterLongitude))
                {
                    mapContent.DefaultLatitude = double.Parse(map.MapType.Municipality.CenterLatitude, NumberStyles.Float, CultureInfo.InvariantCulture);
                    mapContent.DefaultLongitude = double.Parse(map.MapType.Municipality.CenterLongitude, NumberStyles.Float, CultureInfo.InvariantCulture);
                }

                mapContent.MarkerTypes = new List<MapEntities.MarkerType>();
                var iconFolder = ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"];

                // Hämtar upp alla typer av markörer förutom "Observe" som bara skall visas på adminkartan.
                var markerTypes = from mt in fiberDb.MarkerTypes where (mt.Name != MapEntityName.Observe) select mt;
                foreach (var dbMarkerType in markerTypes)
                {
                    mapContent.MarkerTypes.Add(new MapEntities.MarkerType() { Id = dbMarkerType.Id, Name = dbMarkerType.Name, Description = dbMarkerType.Description, Icon = iconFolder + dbMarkerType.DestIcon });
                }

                #region FeatchAllMarkersToRender
                mapContent.Markers = new List<MapEntities.Marker>();

                if (viewSettings.HasFlag(MapViewSettings.OnlyShowHouseYesOnPublicMap))
                {
                    // Om vi valt att bara visa fastigheter som kommer att få fiber, hämta upp dessa samt andra markörer(kopplingsskåp, väggenomgångar, osv) som inte har med fastigheter att göra.
                    // Observera-markören visas inte heller på den publika kartan.
                    var markerList = (from m in map.Markers
                                      where (
                                          m.MarkerType.Name != MapEntityName.HouseNo &&
                                          m.MarkerType.Name != MapEntityName.HouseMaybe &&
                                          m.MarkerType.Name != MapEntityName.HouseNotContacted &&
                                          m.MarkerType.Name != MapEntityName.Observe
                                          )
                                      select m).ToList();
                    foreach (var dbMarker in markerList)
                    {
                        // Om markören är en Textmarkör så skall den bara visas om man valt att den skall få visas på den publika kartan.
                        if (dbMarker.MarkerType.Name == MapEntityName.Note)
                        {
                            if (!string.IsNullOrEmpty(dbMarker.OptionalInfo))
                            {
                                var OptionalInfo = JsonConvert.DeserializeObject<OptionalInfo>(dbMarker.OptionalInfo);

                                if (OptionalInfo.ShowPublic)
                                {
                                    mapContent.Markers.Add(new MapEntities.Marker() { Id = dbMarker.Id, Uid = dbMarker.Uid, Name = dbMarker.Name, Lat = Convert.ToSingle(dbMarker.Latitude), Long = Convert.ToSingle(dbMarker.Longitude), TypeId = dbMarker.MarkerTypeId, OptionalInfo = JsonConvert.DeserializeObject<OptionalInfo>(dbMarker.OptionalInfo) });
                                }
                            }
                        }
                        else
                        {
                            mapContent.Markers.Add(new MapEntities.Marker() { Id = dbMarker.Id, Uid = dbMarker.Uid, Name = dbMarker.Name, Lat = Convert.ToSingle(dbMarker.Latitude), Long = Convert.ToSingle(dbMarker.Longitude), TypeId = dbMarker.MarkerTypeId });
                        }
                    }
                }
                else
                {
                    // Hämta upp alla markörer förutom Observera, som inte skall visas på den publika kartan.
                    var markerList = (from m in map.Markers
                                      where (
                                          m.MarkerType.Name != MapEntityName.Observe
                                      )
                                      select m).ToList();

                    foreach (var dbMarker in markerList)
                    {
                        // Om markören är en Textmarkör så skall den bara visas om man valt att den skall få visas på den publika kartan.
                        if (dbMarker.MarkerType.Name == MapEntityName.Note)
                        {
                            if (!string.IsNullOrEmpty(dbMarker.OptionalInfo))
                            {
                                var OptionalInfo = JsonConvert.DeserializeObject<OptionalInfo>(dbMarker.OptionalInfo);

                                if (OptionalInfo.ShowPublic)
                                {
                                    mapContent.Markers.Add(new MapEntities.Marker() { Id = dbMarker.Id, Uid = dbMarker.Uid, Name = dbMarker.Name, Lat = Convert.ToSingle(dbMarker.Latitude), Long = Convert.ToSingle(dbMarker.Longitude), TypeId = dbMarker.MarkerTypeId, OptionalInfo = JsonConvert.DeserializeObject<OptionalInfo>(dbMarker.OptionalInfo) });
                                }
                            }
                        }
                        else
                        {
                            mapContent.Markers.Add(new MapEntities.Marker() { Id = dbMarker.Id, Uid = dbMarker.Uid, Name = dbMarker.Name, Lat = Convert.ToSingle(dbMarker.Latitude), Long = Convert.ToSingle(dbMarker.Longitude), TypeId = dbMarker.MarkerTypeId });
                        }
                    }
                }
                #endregion FeatchAllMarkersToRender

                mapContent.Regions = new List<MapEntities.Region>();
                foreach (var dbRegion in map.Regions)
                {
                    mapContent.Regions.Add(new MapEntities.Region() { Id = dbRegion.Id, Uid = dbRegion.Uid, Name = dbRegion.Name, BorderColor = dbRegion.LineColor, FillColor = dbRegion.FillColor, Coordinates = dbRegion.Coordinates });
                }

                mapContent.Cables = new List<MapEntities.Cable>();
                foreach (var dbLine in map.Lines)
                {
                    mapContent.Cables.Add(new MapEntities.Cable() { Id = dbLine.Id, Uid = dbLine.Uid, Name = dbLine.Name, LineColor = dbLine.LineColor, Width = dbLine.Width, Coordinates = dbLine.Coordinates });
                }

                mapContent.Settings.ShowTotalDigLength = viewSettings.HasFlag(MapViewSettings.ShowTotalDigLengthStatistics);
                #endregion Skapa komplext objekt med alla markörer, fibersträckor, områden, m.m.

                #region Statistik

                var numberOfIntressted = map.Markers.Count(x => x.MarkerType.Name == MapEntityName.HouseYes);
                var numberOfNotIntressted = map.Markers.Count(x => x.MarkerType.Name == MapEntityName.HouseMaybe || x.MarkerType.Name == MapEntityName.HouseNo || x.MarkerType.Name == MapEntityName.HouseNotContacted);
                NumberOfIntresstedLiteral.Text = numberOfIntressted.ToString();
                NumberOfNotIntresstedLiteral.Text = numberOfNotIntressted.ToString();

                if (numberOfNotIntressted == 0)
                    ConnectionRatioLiteral.Text = "100";
                else if (numberOfIntressted == 0)
                    ConnectionRatioLiteral.Text = "0";
                else
                    ConnectionRatioLiteral.Text = ((int)Math.Ceiling((double)numberOfIntressted / (numberOfIntressted + numberOfNotIntressted) * 100)).ToString();
                #endregion Statistik

                Page.Title = map.MapType.Title;
                Page.MetaDescription = "Fiberkartan - " + map.MapType.Title;
                Page.MetaKeywords = "fiberkarta,bredband,sockenmodell,byanät,nätverk,fibernät,projekteringsverktyg";

                // Välkomsttexten som skall visas när man laddar kartan. Kan vara en tom sträng om det inte finns någon.
                this.ClientScript.RegisterStartupScript(typeof(Page), "mapWelcomeMessage", "fk.welcomeMessage='" + (string.IsNullOrEmpty(map.MapType.WelcomeMessage) ? string.Empty : HttpUtility.JavaScriptStringEncode(map.MapType.WelcomeMessage)) + "'; ", true);
                // Omvandlar information till ett JSON-objekt som renderas ut på sidan, så att kartinnehållet kan processas på klientsidan.
                this.ClientScript.RegisterStartupScript(typeof(Page), "mapContent", "fk.mapContent=" + JsonConvert.SerializeObject(mapContent) + "; ", true);
                this.ClientScript.RegisterStartupScript(typeof(Page), "serverRoot", "fk.serverRoot='" + ConfigurationManager.AppSettings.Get("ServerAdress") + "'; ", true);

                Response.Cache.SetLastModified(map.Created);
                Response.Cache.SetETag(map.MapTypeId + "_" + map.Ver);
            }
            else
            {
                this.ClientScript.RegisterStartupScript(typeof(Page), "mapWelcomeMessage", "var welcomeMessage='Kartan kunde inte hittas.'; ", true);
            }
        }
    }
}