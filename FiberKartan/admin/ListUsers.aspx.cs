using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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
    public partial class ListUsers : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ((Literal)Master.FindControl("PageTitle")).Text = "Lista användare";

            var fiberDb = new FiberDataContext();

            if (!Request.IsAuthenticated)
            {
                Response.Redirect("ShowMaps.aspx");
            }

            var user = (from u in fiberDb.Users where u.Username == HttpContext.Current.User.Identity.Name select u).FirstOrDefault();

            if (!user.IsAdmin)
            {
                Response.Redirect("ShowMaps.aspx");
            }

            if (!IsPostBack)
            {
                BackButton.NavigateUrl = Request.QueryString["ReturnUrl"];
                if (string.IsNullOrEmpty(Request.QueryString["ReturnUrl"]) && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
                {
                    BackButton.NavigateUrl = Request.UrlReferrer.AbsoluteUri;
                }
            }
        }
    }
}