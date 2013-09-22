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
The zlib/libpng License
Copyright (c) 2012 Henrik Östman

This software is provided 'as-is', without any express or implied warranty. In no event will the authors be held liable for any damages arising from the use of this software.
Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
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