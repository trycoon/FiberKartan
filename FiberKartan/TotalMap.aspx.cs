using System;
using System.Collections;
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
using FiberKartan.Kml;
using FiberKartan.MapEntities;
using Newtonsoft.Json;
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
    /// Översiktskarta för hela regionen.
    /// Henrik Östman, 2012-01-31.
    /// </summary>
    public partial class TotalMap : System.Web.UI.Page
    {
        // Mitten på Sverige.
        private const double DefaultLatitude = 60.12816100000001d;
        private const double DefaultLongitude = 18.6435010d;

        /// <summary>
        /// Metod som exekveras vid sidladdning.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            LoadAndRenderMapContent();
        }

        /// <summary>
        /// Hämtar upp kartinformation från databas och renderar kartan.
        /// </summary>
        private void LoadAndRenderMapContent()
        {
            var municipalityCode = Request["code"];  // Kommunkod

            var cachedMap = (CachedTotalMap)HttpContext.Current.Cache.Get("CachedTotalMap_" + municipalityCode);

            if (cachedMap == null)
            {
                var mapContent = new SimpleMapContent()
                {
                    DefaultLatitude = DefaultLatitude,
                    DefaultLongitude = DefaultLongitude,
                    DefaultZoom = 6.0,
                    Markers = new List<MapEntities.SimpleMarker>(),
                    Cables = new List<MapEntities.SimpleCable>()
                };

                cachedMap = new CachedTotalMap() { MapContent = mapContent, LastUpdateTime = DateTime.MinValue, PageTitle = "FiberKartan - Hela Sverige" };

                var fiberDb = new FiberDataContext();

                // Hämtar den senaste versioner av alla kartor som är markerade som publika och som tillåts att visas i en regionskarta.
                // Om municipalityCode är null så får man hela riket, annars så får man bara de som tillhör en viss kommun.
                var mapsToLoad = (from m in fiberDb.GetRegionMaps(municipalityCode) select m).ToList();

                if (mapsToLoad == null || mapsToLoad.Count == 0)
                {
                    this.ClientScript.RegisterStartupScript(typeof(Page), "mapWelcomeMessage", "var welcomeMessage='Kartan kunde inte hittas.'; ", true);
                }
                else
                {
                    var municipality = fiberDb.Municipalities.Where(mp => mp.Code == municipalityCode).FirstOrDefault();
                    if (municipality != null)
                    {
                        cachedMap.PageTitle = "FiberKartan - " + municipality.Name + " kommun";

                        var latitude = DefaultLatitude;
                        double.TryParse(municipality.CenterLatitude, NumberStyles.Any, CultureInfo.GetCultureInfo("en"), out latitude);
                        mapContent.DefaultLatitude = latitude;

                        var longitude = DefaultLongitude;
                        double.TryParse(municipality.CenterLongitude, NumberStyles.Any, CultureInfo.GetCultureInfo("en"), out longitude);
                        mapContent.DefaultLongitude = longitude;

                        mapContent.DefaultZoom = 9.0; // Eftersom vi hittat kommunen så kan vi kosta på oss ett mer inzoomat läge.
                    }

                    // Laddar upp de olika typer av markörer som finns.
                    mapContent.MarkerTypes = new List<MapEntities.MarkerType>();
                    var iconFolder = ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"];

                    // Hämtar upp typer av markörer, endast ett fåtal visas på Regionskartan.
                    var markerTypes = from mt in fiberDb.MarkerTypes
                                      where (
                                          mt.Name == MapEntityName.HouseYes ||
                                          mt.Name == MapEntityName.FiberNode ||
                                          mt.Name == MapEntityName.RoadCrossing_Existing ||
                                          mt.Name == MapEntityName.RoadCrossing_ToBeMade
                                          )
                                      select mt;
                    foreach (var dbMarkerType in markerTypes)
                    {
                        mapContent.MarkerTypes.Add(new MapEntities.MarkerType() { Id = dbMarkerType.Id, Name = dbMarkerType.Name, Description = dbMarkerType.Description, Icon = iconFolder + dbMarkerType.DestIcon });
                    }

                    foreach (var mapToLoad in mapsToLoad)
                    {
                        var map = fiberDb.Maps.Where(m => m.MapTypeId == mapToLoad.Id && m.Ver == mapToLoad.Ver).SingleOrDefault();
                        if (map != null)    // Ifall något fel har uppstått och en ny karta har skapats, men ingen första kartversion har skapats. Vi hoppar i så fall bara över kartan.
                        {
                            if (map.Created > cachedMap.LastUpdateTime)
                            {
                                cachedMap.LastUpdateTime = map.Created;
                            }
                            #region Skapa komplext objekt med alla markörer, fibersträckor, områden, m.m.

                            // Hämtar upp markörer, endast ett fåtal typer visas på Regionskartan.
                            var markerList = (from m in map.Markers
                                              where (
                                                  m.MarkerType.Name == MapEntityName.HouseYes ||
                                                  m.MarkerType.Name == MapEntityName.FiberNode ||
                                                  m.MarkerType.Name == MapEntityName.RoadCrossing_Existing ||
                                                  m.MarkerType.Name == MapEntityName.RoadCrossing_ToBeMade
                                              )
                                              select m).ToList();

                            foreach (var dbMarker in markerList)
                            {
                                mapContent.Markers.Add(new MapEntities.SimpleMarker() { Lat = Convert.ToSingle(dbMarker.Latitude), Long = Convert.ToSingle(dbMarker.Longitude), TypeId = dbMarker.MarkerTypeId });
                            }

                            foreach (var dbLine in map.Lines)
                            {
                                mapContent.Cables.Add(new MapEntities.SimpleCable() { LineColor = dbLine.LineColor, Width = dbLine.Width, Coordinates = dbLine.Coordinates });
                            }
                        }
                        #endregion Skapa komplext objekt med alla markörer, fibersträckor, områden, m.m.
                    }

                    mapContent.Created = cachedMap.LastUpdateTime.ToString();

                    // Kolla en gång till innan vi lägger in den i cachen, om steget ovanför tog lång tid att exekvera så kan någon annan tråd ha hunnit före sedan den första if-satsen.
                    if (HttpContext.Current.Cache.Get("CachedTotalMap_" + municipalityCode) == null)
                    {
                        HttpContext.Current.Cache.Insert("CachedTotalMap_" + municipalityCode, cachedMap, null, DateTime.Now.AddDays(1), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);
                    }
                }
            }

            // Dölj paletten utifrån önskemål från querystringen.
            mainPalette.Visible = !(Request["palette"] == "false");

            Page.Title = cachedMap.PageTitle;
            NrHouses.Text = cachedMap.MapContent.Markers.Count(x => x.TypeId == (int)MapEntityEnum.HouseYes).ToString();

            // Omvandlar information till ett JSON-objekt som renderas ut på sidan, så att kartinnehållet kan processas på klientsidan.
            this.ClientScript.RegisterStartupScript(typeof(Page), "mapContent", "fk.mapContent=" + JsonConvert.SerializeObject(cachedMap.MapContent) + "; ", true);
            this.ClientScript.RegisterStartupScript(typeof(Page), "serverRoot", "fk.serverRoot='" + ConfigurationManager.AppSettings.Get("ServerAdress") + "'; ", true);

            // Prevent caching.
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            Response.Cache.SetExpires(DateTime.Now.Subtract(TimeSpan.FromHours(10)));
        }
    }

    public class CachedTotalMap
    {
        public SimpleMapContent MapContent { get; set; }
        public DateTime LastUpdateTime { get; set; }
        public string PageTitle { get; set; }
    }
}