using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FiberKartan.MapEntities;
using System.Configuration;
using Newtonsoft.Json;
using FiberKartan.Kml;
using System.Globalization;

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
    /// Felrapporteringskarta för fibernätverk.
    /// Henrik Östman, 2013-09-22.
    /// </summary>
    public partial class IncidentReportMap : System.Web.UI.Page
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

            var accessRights = Utils.GetMapAccessRights(mapId);

            FiberKartan.Map map = null;
            if (mapVersion > 0)
            {
                map = (from m in fiberDb.Maps where (m.MapTypeId == mapId && m.Ver == mapVersion) select m).FirstOrDefault();
            }
            else
            {
                map = (from m in fiberDb.Maps.OrderByDescending(m => m.Ver) where m.MapTypeId == mapId select m).FirstOrDefault();
            }

            if (map != null && accessRights.HasFlag(MapAccessRights.Write) && map.MapType.ServiceCompanyId.HasValue)
            {
                mapContent.MapTypeId = map.MapTypeId;
                mapContent.MapVer = map.Ver;
                mapContent.MapName = map.MapType.Title;
                mapContent.Created = map.Created.ToString();
                mapContent.Views = map.Views;

                #region Skapa komplext objekt med alla markörer, fibersträckor, områden, m.m.

                if (!string.IsNullOrEmpty(map.MapType.Municipality.CenterLatitude) && !string.IsNullOrEmpty(map.MapType.Municipality.CenterLongitude))
                {
                    mapContent.DefaultLatitude = double.Parse(map.MapType.Municipality.CenterLatitude, NumberStyles.Float, CultureInfo.InvariantCulture);
                    mapContent.DefaultLongitude = double.Parse(map.MapType.Municipality.CenterLongitude, NumberStyles.Float, CultureInfo.InvariantCulture);
                }

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

                mapContent.Cables = new List<MapEntities.Cable>();
                foreach (var dbLine in map.Lines)
                {
                    mapContent.Cables.Add(new MapEntities.Cable() { Id = dbLine.Id, Uid = dbLine.Uid, Name = dbLine.Name, LineColor = dbLine.LineColor, Width = dbLine.Width, Coordinates = dbLine.Coordinates, Type = dbLine.Type });
                }

                #endregion Skapa komplext objekt med alla markörer, fibersträckor, områden, m.m.

                Page.Title = "Incidentrapportering - " + map.MapType.Title;
                Page.MetaDescription = "FiberKartan-Incidentrapportering - " + map.MapType.Title;
                Page.MetaKeywords = "fiberkarta,bredband,sockenmodell,byanät,nätverk,fibernät,projekteringsverktyg";

                // Omvandlar information till ett JSON-objekt som renderas ut på sidan, så att kartinnehållet kan processas på klientsidan.
                this.ClientScript.RegisterStartupScript(typeof(Page), "mapContent", "fk.mapContent=" + JsonConvert.SerializeObject(mapContent) + "; ", true);
                this.ClientScript.RegisterStartupScript(typeof(Page), "serverRoot", "fk.serverRoot='" + ConfigurationManager.AppSettings.Get("ServerAdress") + "'; ", true);
                this.ClientScript.RegisterStartupScript(typeof(Page), "serviceProvider", "fk.serviceProvider='" + map.MapType.ServiceCompany.Name + "'; ", true);
   
                Response.Cache.SetLastModified(map.Created);
                Response.Cache.SetETag(map.MapTypeId + "_" + map.Ver);
            }
        }
    }
}