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
using FiberKartan.Kml;
using FiberKartan.MapEntities;

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
    /// <summary>
    /// Kartvy för en fiberförening.
    /// Henrik Östman, 2011-06-27.
    /// </summary>
    public partial class MapAdmin : System.Web.UI.Page
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

            // Sätter länk tillbaka till listan över versioner.
            backButton.NavigateUrl = "ShowMapVersions.aspx?mid=" + mapId;

            LoadAndRenderMapContent();
        }

        /// <summary>
        /// Hämtar upp kartinformation från databas och renderar kartan.
        /// </summary>
        private void LoadAndRenderMapContent()
        {
            mapContent = new MapContent();
            fiberDb = new FiberDataContext();

            var accessRights = Utils.GetMapAccessRights(mapId);
            if (accessRights.HasFlag(MapAccessRights.Write))
            {
                saveButton.Visible = true;
                markerTypes.Visible = true;
                mapContent.Settings.HasWritePrivileges = true;
            } else
            {
                saveButton.Visible = false;
                markerTypes.Visible = false;
            }

            FiberKartan.Map map = null;
            if (mapVersion > 0)
            {
                map = (from m in fiberDb.Maps where (m.MapTypeId == mapId && m.Ver == mapVersion) select m).FirstOrDefault();
            }
            else
            {
                map = (from m in fiberDb.Maps.OrderByDescending(m => m.Ver) where m.MapTypeId == mapId select m).FirstOrDefault();
            }

            if (map != null && accessRights.HasFlag(MapAccessRights.Read))
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

                #region Skapa komplext objekt med alla markörer, fibersträckor, områden, m.m.

                mapContent.MarkerTypes = new List<MapEntities.MarkerType>();
                var markerTypes = fiberDb.MarkerTypes;
                var iconFolder = ConfigurationManager.AppSettings["ServerAdress"] + ConfigurationManager.AppSettings["IconFolder"];
                foreach (var dbMarkerType in markerTypes)
                {
                    mapContent.MarkerTypes.Add(new MapEntities.MarkerType() { Id = dbMarkerType.Id, Name = dbMarkerType.Name, Description = dbMarkerType.Description, Icon = iconFolder + dbMarkerType.DestIcon });
                }

                mapContent.Markers = new List<MapEntities.Marker>();
                foreach (var dbMarker in map.Markers)
                {
                    mapContent.Markers.Add(new MapEntities.Marker() { Id = dbMarker.Id, Uid = dbMarker.Uid, Name = dbMarker.Name, Lat = Convert.ToSingle(dbMarker.Latitude), Long = Convert.ToSingle(dbMarker.Longitude), TypeId = dbMarker.MarkerTypeId, Settings = dbMarker.Settings, OptionalInfo = JsonConvert.DeserializeObject<OptionalInfo>(dbMarker.OptionalInfo) });
                }

                mapContent.Regions = new List<MapEntities.Region>();
                foreach (var dbRegion in map.Regions)
                {
                    mapContent.Regions.Add(new MapEntities.Region() { Id = dbRegion.Id, Uid = dbRegion.Uid, Name = dbRegion.Name, BorderColor = dbRegion.LineColor, FillColor = dbRegion.FillColor, Coordinates = dbRegion.Coordinates });
                }

                mapContent.Cables = new List<MapEntities.Cable>();
                foreach (var dbLine in map.Lines)
                {
                    mapContent.Cables.Add(new MapEntities.Cable() { Id = dbLine.Id, Uid = dbLine.Uid, Name = dbLine.Name, LineColor = dbLine.LineColor, Width = dbLine.Width, Coordinates = dbLine.Coordinates, Type = dbLine.Type });
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
                Page.MetaDescription = "Fiberkartan" + " - " + map.MapType.Title;
                Page.MetaKeywords = "fiberkarta,bredband,sockenmodell,byanät,nätverk,fibernät,projekteringsverktyg";

                // Omvandlar information till ett JSON-objekt som renderas ut på sidan, så att kartinnehållet kan processas på klientsidan.
                this.ClientScript.RegisterStartupScript(typeof(Page), "mapContent", "var mapContent=" + JsonConvert.SerializeObject(mapContent) + "; ", true);
                this.ClientScript.RegisterStartupScript(typeof(Page), "serverRoot", "var serverRoot='" + ConfigurationManager.AppSettings.Get("ServerAdress") + "'", true);

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