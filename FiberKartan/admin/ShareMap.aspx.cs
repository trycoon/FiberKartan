using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Text;
using System.Net.Mime;
using System.Configuration;

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
    public partial class ShareMap : System.Web.UI.Page
    {
        private FiberDataContext fiberDb;
        private int mapTypeId;

        protected void Page_Load(object sender, EventArgs e)
        {
            ((Literal)Master.FindControl("PageTitle")).Text = "Dela karta med andra";

            fiberDb = new FiberDataContext();
            mapTypeId = string.IsNullOrEmpty(Request.QueryString["mid"]) ? 0 : int.Parse(Request.QueryString["mid"]);
            if (!Utils.GetMapAccessRights(mapTypeId).HasFlag(MapAccessRights.FullAccess))
            {
                Response.Redirect("ShowMaps.aspx");
            }
        }

        protected void SqlDataSource_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
        {
            var user = (from u in fiberDb.Users where (u.Username == HttpContext.Current.User.Identity.Name) select u).First();
            e.Command.Parameters["@userId"].Value = user.Id;
        }

        protected void ShareMapGridView_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            ResultBox.Text = string.Empty;
            ResultBox.Visible = false;

            var mapAccessRight = fiberDb.MapTypeAccessRights.Where(ar => ar.MapTypeId == mapTypeId && ar.UserId == int.Parse((string)e.CommandArgument)).FirstOrDefault();

            if (e.CommandName == "SaveChanges")
            {
                var clickedRow = ((Button)e.CommandSource).NamingContainer as GridViewRow;
                mapAccessRight.AccessRight = int.Parse((clickedRow.FindControl("AccessRight") as DropDownList).SelectedItem.Value);
                fiberDb.SubmitChanges();

                Utils.Log("Användare id=" + mapAccessRight.UserId + ", namn=\"" + mapAccessRight.User.Name + "\" har blivit tilldelad rättighet=" + mapAccessRight.AccessRight + " till karta=" + mapAccessRight.MapTypeId + " av " + HttpContext.Current.User.Identity.Name + ".", System.Diagnostics.EventLogEntryType.SuccessAudit, 103);

                ResultBox.Text = "Ändringar sparade.";
                ResultBox.BoxTheme = ResultBox.ThemeChoices.InformationTheme;
            }
            else if (e.CommandName == "RemoveUserRight")
            {
                fiberDb.MapTypeAccessRights.DeleteOnSubmit(mapAccessRight);
                fiberDb.SubmitChanges();

                Utils.Log("Användare id=" + mapAccessRight.UserId + ", namn=\"" + mapAccessRight.User.Name + "\" har blivit fråntagen rättigheter till karta=" + mapAccessRight.MapTypeId + " av " + HttpContext.Current.User.Identity.Name + ".", System.Diagnostics.EventLogEntryType.SuccessAudit, 103);
            }

            ShareMapGridView.DataBind();
        }

        protected void NewAccessButton_Click(object sender, EventArgs e)
        {
            ResultBox.Text = string.Empty;
            ResultBox.Visible = false;

            if (string.IsNullOrEmpty(NewMapAccessEmail.Text))
            {
                ResultBox.Text = "Ange en e-postadress.";
                ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
            }
            else if (!new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.IgnoreCase).IsMatch(NewMapAccessEmail.Text))
            {
                ResultBox.Text = "E-postadress är felformaterad.";
                ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
            }
            else
            {
                var emailAdress = NewMapAccessEmail.Text.ToLower().Trim();

                var existingUser = fiberDb.Users.Where(u => u.Username == emailAdress).FirstOrDefault();

                if (existingUser != null)
                {
                    // Om en användare finns i systemet med angiven e-postadress, lägg till rättigheterna direkt.

                    if (fiberDb.MapTypeAccessRights.Where(ar => ar.MapTypeId == mapTypeId && ar.UserId == existingUser.Id).Count() > 0)
                    {
                        ResultBox.Text = "Användaren har redan tillgång till denna karta.";
                        ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
                    }
                    else
                    {
                        var accessRight = new MapTypeAccessRight() {
                            MapTypeId = mapTypeId,
                            UserId = existingUser.Id, 
                            AccessRight = int.Parse(NewAccessRight.SelectedValue)
                        };

                        fiberDb.MapTypeAccessRights.InsertOnSubmit(accessRight);
                        fiberDb.SubmitChanges();

                        Utils.Log("Användare id=" + existingUser.Id + ", namn=\"" + existingUser.Name + "\", username=\"" + existingUser.Username + "\" har blivit tilldelad rättighet=" + accessRight.AccessRight + " till karta=" + accessRight.MapTypeId + " av " + HttpContext.Current.User.Identity.Name + ".", System.Diagnostics.EventLogEntryType.SuccessAudit, 103);

                        #region SendMail

                        var currentUser = (from u in fiberDb.Users where (u.Username == HttpContext.Current.User.Identity.Name) select u).First();
                        var body = new StringBuilder("<html><head><title>Inbjudan till fiberkarta</title></head><body><h1>Inbjudan till fiberkarta</h1>");
                        body.Append("<p>" + currentUser.Name + " har inbjudit dig till att samarbeta kring fiberkartan \"").Append(accessRight.MapType.Title).Append("\", klicka på följande <a href=\"")
                            .Append(ConfigurationManager.AppSettings["ServerAdress"]).Append("/admin/MapAdmin.aspx?mid=").Append(accessRight.MapTypeId)
                            .Append("\">länk</a> för att studera kartan.</p>")
                            .Append("<p>Detta e-postmeddelande är automatgenererat av <a href=\"http://fiberkartan.se\">Fiberkartan.se</a>.</p>");
                        body.Append("</body></html>");

                        using (var mail = new MailMessage()
                        {
                            From = new MailAddress("noreply@fiberkartan.se", "FiberKartan"),
                            Sender = new MailAddress(currentUser.Username, currentUser.Name),
                            ReplyTo = new MailAddress(currentUser.Username, currentUser.Name),
                            Subject = "Inbjudan till fiberkarta",
                            IsBodyHtml = true,
                            DeliveryNotificationOptions = DeliveryNotificationOptions.Never
                        })
                        {
                            mail.To.Add(new MailAddress(emailAdress));

                            // HTML-innehåll måste kodas så här
                            mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(body.ToString(), Encoding.UTF8, MediaTypeNames.Text.Html));

                            try
                            {
                                using (var SMTPServer = new SmtpClient())
                                {
                                    SMTPServer.Send(mail);

                                    Utils.Log("Inbjudan till en befintlig person i systemet med e-postadress \"" + emailAdress + "\" för karta(id=" + mapTypeId + ") har skickats av användare(id=" + currentUser.Id + ", namn=" + currentUser.Name + ").", System.Diagnostics.EventLogEntryType.Information, 1);
                                }
                            }
                            catch (Exception exception)
                            {
                                Utils.Log("Misslyckades med att skicka inbjudan till person med e-postadress \"" + emailAdress + "\" för karta(id=" + mapTypeId + ") av användare(id=" + currentUser.Id + ", namn=" + currentUser.Name + "). Message=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 1);
                            }
                        }
                        #endregion SendMail

                        NewMapAccessEmail.Text = string.Empty;
                    }
                }
                else
                {
                    // Om ingen användare kan hittas så får vi lägga till en inbjudan i MapAccessInvitation-tabellen och skicka en e-postinbjudan.
                    // Om inbjudan accepteras så blir personen en ny användare i systemet.
                    
                    // Tar bort utstående inbjudningar till denna karta för denna användare.
                    var existingInvitationsByUser = fiberDb.MapAccessInvitations.Where(ar => ar.MapTypeId == mapTypeId && ar.Email == emailAdress);
                    foreach (var invitation in existingInvitationsByUser)
                    {
                        fiberDb.MapAccessInvitations.DeleteOnSubmit(invitation);
                    }

                    var currentUser = (from u in fiberDb.Users where (u.Username == HttpContext.Current.User.Identity.Name) select u).First();

                    var accessInvitation = new MapAccessInvitation()
                    {
                        MapTypeId = mapTypeId,
                        Email = emailAdress,
                        AccessRight = int.Parse(NewAccessRight.SelectedValue),
                        InvitationSentBy = currentUser.Id,
                        InvitationSent = DateTime.Now,
                        InvitationCode = Guid.NewGuid().ToShortString()
                    };

                    fiberDb.MapAccessInvitations.InsertOnSubmit(accessInvitation);
                    fiberDb.SubmitChanges();

                    #region SendMail

                    var body = new StringBuilder("<html><head><title>Inbjudan till fiberkarta</title></head><body><h1>Inbjudan till fiberkarta</h1>");
                    body.Append("<p>" + currentUser.Name + " har inbjudit dig till att samarbeta kring fiberkartan \"").Append(accessInvitation.MapType.Title).Append("\", klicka på följande <a href=\"")
                        .Append(ConfigurationManager.AppSettings["ServerAdress"]).Append("/admin/Logon.aspx?invitation=").Append(accessInvitation.InvitationCode)
                        .Append("\">länk</a> för att besvara inbjudan och skapa ett konto.</p>")
                        .Append("<p>Detta e-postmeddelande är automatgenererat av <a href=\"http://fiberkartan.se\">Fiberkartan.se</a>.</p>");
                    body.Append("</body></html>");

                    using (var mail = new MailMessage()
                    {
                        From = new MailAddress("noreply@fiberkartan.se", "FiberKartan"),
                        Sender = new MailAddress(currentUser.Username, currentUser.Name),
                        ReplyTo = new MailAddress(currentUser.Username, currentUser.Name),
                        Subject = "Inbjudan till fiberkarta",
                        IsBodyHtml = true,
                        DeliveryNotificationOptions = DeliveryNotificationOptions.Never
                    })
                    {
                        mail.To.Add(new MailAddress(emailAdress));

                        // HTML-innehåll måste kodas så här
                        mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(body.ToString(), Encoding.UTF8, MediaTypeNames.Text.Html));

                        try
                        {
                            using (var SMTPServer = new SmtpClient())
                            {
                                SMTPServer.Send(mail);

                                Utils.Log("Inbjudan för person(ej befintligt konto) med e-postadress \"" + emailAdress + "\" till karta(id=" + mapTypeId + ") har skickats av användare(id=" + currentUser.Id + ", namn=" + currentUser.Name + ").", System.Diagnostics.EventLogEntryType.Information, 1);
                            }
                        }
                        catch (Exception exception)
                        {
                            Utils.Log("Misslyckades med att skicka inbjudan till person med e-postadress \"" + emailAdress + "\" för karta(id=" + mapTypeId + ") av användare(id=" + currentUser.Id + ", namn=" + currentUser.Name + "). Message=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 1);
                        }
                    }
                    #endregion SendMail

                    ResultBox.Text = "Inbjudan skickad. Användaren kommer att dyka upp på listan när denna har accepterat inbjudan och skapat ett konto.";
                    ResultBox.BoxTheme = ResultBox.ThemeChoices.InformationTheme;

                    NewMapAccessEmail.Text = string.Empty;
                }
            }

            ShareMapGridView.DataBind();
        }
    }
}