using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Text;

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
    public partial class AdminMasterPage : System.Web.UI.MasterPage
    {
        private FiberDataContext fiberDb;
        private User currentUser;

        protected void Page_Load(object sender, EventArgs e)
        {
            fiberDb = new FiberDataContext();
            currentUser = (from u in fiberDb.Users where (u.Username == HttpContext.Current.User.Identity.Name) select u).First();

            if (!IsPostBack)
            {
                loggedOnName.Text = currentUser.Name;
                loggedOnName.NavigateUrl = "EditUser.aspx?uid=" + currentUser.Id;
                lastLoggedOn.Text = currentUser.LastLoggedOn.ToString();

                HandleNotifications();
            }

            if (currentUser.IsAdmin)
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

        /// <summary>
        /// Visar upp systemmeddelanden/notifieringar för användaren.
        /// </summary>
        private void HandleNotifications()
        {
            //  Id på det senaste visade meddelandet sparas i en kaka så att vi kan hålla reda på vilka meddelanden vi visat för användaren.
            try
            {
                var lastNotification = fiberDb.NotificationMessages.OrderByDescending(n => n.Id).FirstOrDefault();

                if (lastNotification != null)
                {
                    if (!currentUser.LastNotificationMessage.HasValue)
                    {
                        currentUser.LastNotificationMessage = 0;
                    }

                    if (currentUser.LastNotificationMessage.Value < lastNotification.Id)
                    {
                        var messages = fiberDb.NotificationMessages.Where(n => currentUser.LastNotificationMessage.Value < n.Id).OrderByDescending(n => n.Id);
                        
                        var msgHTML = new StringBuilder();
                        foreach (var msg in messages)
                        {
                            msgHTML.Append("<article>")
                                .Append("<header>")
                                    .Append("<h3>").Append(msg.Title).Append("</h3>")
                                    .Append("<p>Tid: ").Append(msg.Created).Append("</p>")
                                 .Append("</header>")
                                 .Append(msg.Body)
                           .Append("</article>")
                           .Append("<hr/>");
                        }

                        Page.ClientScript.RegisterStartupScript(typeof(Page), "message",
                           "var $dialog = $('<div></div>')" +
                           ".html('" + msgHTML + "')" +
                           ".dialog({" +
                                   "autoOpen: false," +
                                   "title: 'Meddelande'," +
                                   "close: function() { $(this).remove(); }," +
                                   "width: 500," +
                                   "height: 500," +
                                   "modal: true," +
                                   "buttons: {" +
                                       "Ok: function() {" +
                                           "$(this).dialog(\"close\");" +
                                       "}" +
                                   "}," +
                                   "dialogClass: 'buttons-centered'" +
                           "});" +
                           "$dialog.dialog('open');"
                        , true);

                        currentUser.LastNotificationMessage = lastNotification.Id;
                        fiberDb.SubmitChanges();
                    }
                }
            }
            catch (Exception exception)
            {
                Utils.Log("Misslyckades med att visa upp notifieringsmeddelande för användare. Felmeddelande=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 170);
            }
        }
    }
}