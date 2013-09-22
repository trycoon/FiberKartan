using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

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
        }
    }
}