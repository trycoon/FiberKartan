using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using FiberKartan;
using System.Configuration;
using System.Web.Security;

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
namespace FiberKartan.admin
{
    //http://www.asp.net/web-forms/tutorials/data-access/editing,-inserting,-and-deleting-data/adding-client-side-confirmation-when-deleting-cs

    public partial class ShowMaps : System.Web.UI.Page
    {
        private FiberDataContext fiberDb;
        private User user;

        protected void Page_Load(object sender, EventArgs e)
        {
            ((Literal)Master.FindControl("PageTitle")).Text = "Kartor";

            fiberDb = new FiberDataContext();
            user = (from u in fiberDb.Users where (u.Username == HttpContext.Current.User.Identity.Name) select u).First();
        }

        protected void SqlDataSource_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
        {            
            e.Command.Parameters["@userId"].Value = user.Id;
        }

        protected string ServerAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["ServerAdress"];
            }
        }

        protected void MapGridView_ItemCommand(object sender, GridViewCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "ShowVersion":
                    Response.Redirect("ShowMapVersions.aspx?mid=" + e.CommandArgument);
                    break;
                case "SubscribeMapChanges":
                    SubscribeMapChanges(int.Parse((string)e.CommandArgument));
                    DataBind();
                    break;
                case "UnSubscribeMapChanges":
                    UnSubscribeMapChanges(int.Parse((string)e.CommandArgument));
                    DataBind();
                    break;
                default:
                    break;

            }
        }

        private void SubscribeMapChanges(int mapTypeId)
        {
            var mapType = fiberDb.MapTypeAccessRights.Where(mt => mt.MapTypeId == mapTypeId && mt.UserId == user.Id).First();
            mapType.EmailSubscribeChanges = true;
            fiberDb.SubmitChanges();
        }

        private void UnSubscribeMapChanges(int mapTypeId)
        {
            var mapType = fiberDb.MapTypeAccessRights.Where(mt => mt.MapTypeId == mapTypeId && mt.UserId == user.Id).First();
            mapType.EmailSubscribeChanges = false;
            fiberDb.SubmitChanges();
        }
    }
}