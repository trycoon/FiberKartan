using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;

namespace FiberKartan.Admin
{
    public partial class AdminMasterPage : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var fiberDb = new FiberDataContext();
            var user = (from u in fiberDb.Users where (u.Username == HttpContext.Current.User.Identity.Name) select u).First();

            if (!IsPostBack)
            {
                loggedOnName.Text = user.Name;
                loggedOnName.NavigateUrl = "EditUser.aspx?uid=" + user.Id;
                lastLoggedOn.Text = user.LastLoggedOn.ToString();
            }

            if (user.IsAdmin)
            {
                ListUsersButton.Visible = true;
                ListUsersButton.NavigateUrl = "ListUsers.aspx?ReturnUrl=" + Request.Url.AbsoluteUri;
            }
        }

        protected void logoutButton_Click(object sender, EventArgs e)
        {
            FormsAuthentication.SignOut();
            Response.Redirect("Logon.aspx");
        }
    }
}