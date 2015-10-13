using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FiberKartan;
using System.Configuration;
using System.Web.Security;
using FiberKartan.Database;
using FiberKartan.Database.Internal;

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
namespace FiberKartan.Admin
{
    public partial class ShowMapVersions : System.Web.UI.Page
    {
        private MapType mapType;
        private FiberDataContext fiberDb;
        private User user;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Utils.GetMapAccessRights() == MapAccessRights.None)
            {
                Response.Redirect("ShowMaps.aspx");
            }

            fiberDb = new FiberDataContext();
            user = (from u in fiberDb.Users where (u.Username == HttpContext.Current.User.Identity.Name) select u).First();

            mapType = fiberDb.MapTypes.Where(mt => mt.Id == int.Parse(Request.QueryString["mid"])).SingleOrDefault();

            // Om kartan inte finns, skicka tillbaka användaren.
            if (mapType == null)
            {
                Response.Redirect("ShowMaps.aspx");
            }

            // Om man skapat en ny kartan, men inte valt att importera information till denna så skapar vi en ny tom version av denna vid första besöket på denna sida.
            // Om vi inte gör det så kan man inte "Visa" kartan och därifrån lägga till markörer osv. Detta gäller såklart bara om vi har skrivrättigheter, annars står det bara att det inte finns några versioner av denna karta.
            if (mapType.Maps.Count == 0)
            {
                if (Utils.GetMapAccessRights().HasFlag(MapAccessRights.Write))
                {
                    mapType.Maps.Add(new FiberKartan.Map()
                    {
                        Created = DateTime.Now,
                        MapType = mapType,
                        Ver = 1,
                        PreviousVer = 0,
                        SourceKML = string.Empty,
                        KML_Hash = string.Empty,
                        Layers = "{}",
                        User = user,
                        CreatorId = user.Id
                    });

                    fiberDb.SubmitChanges();
                }
            }

            ((Literal)Master.FindControl("PageTitle")).Text = "Versioner av " + mapType.Title;
        }

        protected string ServerAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["ServerAdress"];
            }
        }

        protected void SqlDataSource_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
        {
            e.Command.Parameters["@userId"].Value = user.Id;
        }

        protected string UrlToLastMapVersion()
        {
            var mapTypeId = int.Parse(Request.QueryString["mid"]);
            var lastVersion = fiberDb.Maps.Where(m => m.MapTypeId == mapTypeId).OrderByDescending(m => m.Ver).FirstOrDefault();

            if (lastVersion == null)
                return "#";
            else
                return "/admin/MapAdmin.aspx?mid=" + mapTypeId + "&ver=" + lastVersion.Ver;
        }
    }
}