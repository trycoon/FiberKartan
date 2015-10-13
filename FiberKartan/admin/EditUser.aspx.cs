using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Net.Mail;
using System.Configuration;
using System.Text;
using System.Net.Mime;
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
    public partial class EditUser : System.Web.UI.Page
    {
        private FiberDataContext fiberDb;
        private int editUserId;
        private User currentUser;

        protected void Page_Load(object sender, EventArgs e)
        {
            fiberDb = new FiberDataContext();
            editUserId = string.IsNullOrEmpty(Request.QueryString["uid"]) ? 0 : int.Parse(Request.QueryString["uid"]);

            BackButton.NavigateUrl = Request.QueryString["ReturnUrl"];
            if (string.IsNullOrEmpty(Request.QueryString["ReturnUrl"]) && Request.UrlReferrer != null && Request.UrlReferrer.ToString().Length > 0)
            {
                BackButton.NavigateUrl = Request.UrlReferrer.AbsoluteUri;
            }

            // Den inloggade användaren
            currentUser = (from u in fiberDb.Users where (u.Username == HttpContext.Current.User.Identity.Name) select u).First();

            if (!IsPostBack)
            {
                if (editUserId > 0)
                {
                    // Man får bara redigera sig själv, såvidare man inte är adminanvändare de får redigera alla.
                    if (currentUser.Id == editUserId || currentUser.IsAdmin)
                    {
                        // Användaren som skall redigeras
                        var editUser = (from u in fiberDb.Users where u.Id == editUserId select u).FirstOrDefault();
                        if (editUser == null)
                        {
                            Response.Redirect("ShowMaps.aspx");
                        }

                        ((Literal)Master.FindControl("PageTitle")).Text = "Redigerar " + editUser.Name;

                        Name.Text = editUser.Name;
                        Username.Text = editUser.Username;
                        Description.Text = editUser.Description;

                        if (currentUser.IsAdmin)
                        {
                            AdminPanel.Visible = true;
                            CreatedDate.Text = editUser.Created.ToString();
                            LastLoggedOn.Text = editUser.LastLoggedOn.ToString();
                            LastActivity.Text = editUser.LastActivity.ToString();
                            IsDisabled.Checked = editUser.IsDeleted;

                            if (currentUser.Id == editUserId)
                            {
                                IsDisabled.Visible = false; // Du får inte spärra dig själv som administratör, risk att du låser dig ute från systemet.
                            }
                        }
                    }
                    else
                    {
                        Response.Redirect("ShowMaps.aspx");
                    }
                }
                else
                {
                    // Bara adminanvändare får lägga till nya användare
                    if (!currentUser.IsAdmin)
                    {
                        Response.Redirect("ShowMaps.aspx");
                    }

                    ((Literal)Master.FindControl("PageTitle")).Text = "Ny användare";
                }
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (!Request.IsAuthenticated)
            {
                Response.Redirect("Logon.aspx");
            }

            ResultBox.Text = string.Empty;
            ResultBox.Visible = false;

            Name.Text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Name.Text.Trim());
            Username.Text = Username.Text.Trim().ToLower();
            Description.Text = Description.Text.Trim();

            try
            {
                // Man får bara redigera sig själv, såvidare man inte är adminanvändare de får redigera alla, de får även lägga till nya användare.
                if (currentUser.Id == editUserId || currentUser.IsAdmin)
                {
                    if (string.IsNullOrEmpty(Name.Text) || !Name.Text.Contains(' ') || Name.Text.Length < 5)
                    {
                        ResultBox.Text = "Ett namn måste anges.";
                        ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
                        Name.Focus();
                    }
                    else if (Name.Text.Length > 100)
                    {
                        ResultBox.Text = "En namn får innehålla max 100 tecken.";
                        ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
                        Name.Focus();
                    }
                    else if (string.IsNullOrEmpty(Username.Text))
                    {
                        ResultBox.Text = "Ett användarnamn måste anges.";
                        ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
                        Username.Focus();
                    }
                    else if (Username.Text.Length > 50)
                    {
                        ResultBox.Text = "Ett användarnamn får innehålla max 50 tecken.";
                        ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
                        Username.Focus();
                    }
                    else if (!new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*", RegexOptions.IgnoreCase).IsMatch(Username.Text))
                    {
                        ResultBox.Text = "Användarnamnet är ogiltigt formaterat, kontrollera att det är en giltig e-postadress.";
                        ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
                        Username.Focus();
                    }
                    else if (string.IsNullOrEmpty(Description.Text) || Description.Text.Length < 3)
                    {
                        ResultBox.Text = "En beskrivning av användaren måste anges. T.ex. vilken fiberförening eller företag denna repressenterar.";
                        ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
                        Description.Focus();
                    }
                    else if (Description.Text.Length > 1000)
                    {
                        ResultBox.Text = "En beskrivning får innehålla max 1000 tecken.";
                        ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
                        Description.Focus();
                    }
                    else
                    {
                        var existingUser = fiberDb.Users.Where(u => u.Username == Username.Text).FirstOrDefault();

                        if (existingUser != null)
                        {
                            if (existingUser.Id != editUserId)
                            {
                                ResultBox.Text = "Användarnamnet är upptaget, vad god ange ett annat.";
                                ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
                                Username.Focus();
                            }
                            else
                            {
                                // Redigera en befintlig användare
                                existingUser.Name = Name.Text;
                                existingUser.Username = Username.Text;
                                existingUser.Description = Description.Text;

                                if (AdminPanel.Visible)
                                {
                                    existingUser.IsDeleted = IsDisabled.Checked;
                                }

                                fiberDb.SubmitChanges();

                                ResultBox.Text = "Ändringar sparade.";
                                ResultBox.BoxTheme = ResultBox.ThemeChoices.InformationTheme;
                            }
                        }
                        else
                        {
                            // Lägg till ny användare
                            var newUser = new User()
                            {
                                Name = Name.Text,
                                Username = Username.Text,
                                Description = Description.Text,
                                Created = DateTime.Now
                            };

                            fiberDb.Users.InsertOnSubmit(newUser);
                            fiberDb.SubmitChanges();

                            // Skicka ett välkomstmail den nyskapta användaren.
                            var body = new StringBuilder("<html><head><title>Välkommen till FiberKartan</title></head><body><h1>Välkommen till FiberKartan " + newUser.Name + "!</h1>");
                            body.Append("<p>Du kan nu logga in på följande adress, <a href=\"http://fiberkartan.se/admin\">http://fiberkartan.se/admin</a>, för att skapa en ny karta till er förening. Användarnamnet är din privata e-postadress och lösenordet får du själv sätta vid första inloggning. Kontot är privat, ifall det finns fler personer i er arbetsgrupp/styrelse som behöver kunna ändra på kartan så kan du <a href=\"http://fiberkartan.se/admin/manual/index.htm#share_map\">\"Dela\"</a> kartan med dessa. När du loggat in så finns det i sidfoten en länk till en manual, annars får du gärna återkomma om du har några frågor.</p>");
                            body.Append("<p>Använd med fördel webbläsarna <a href=\"http://www.google.com/intl/sv/chrome/browser/\">Google Chrome</a> eller Mozilla Firefox för dessa fungerar bäst!</p>");
                            body.Append("<p>Med Vänliga Hälsningar " + currentUser.Name + "</p>");
                            body.Append("</body></html>");

                            using (var mail = new MailMessage()
                            {
                                From = new MailAddress("noreply@fiberkartan.se", "FiberKartan"),
                                Sender = new MailAddress(currentUser.Username, currentUser.Name),
                                ReplyTo = new MailAddress(currentUser.Username, currentUser.Name),
                                Subject = "Välkommen till FiberKartan",
                                IsBodyHtml = true,
                                DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess,
                            })
                            {
                                mail.To.Add(new MailAddress(newUser.Username));

                                // HTML-innehåll måste kodas så här
                                mail.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(body.ToString(), Encoding.UTF8, MediaTypeNames.Text.Html));

                                try
                                {
                                    using (var SMTPServer = new SmtpClient())
                                    {
                                        SMTPServer.Send(mail);

                                        Utils.Log("E-post till ny användare(id=" + newUser.Id + ") i systemet har nu skickats till \"" + newUser.Username + "\" av användare(id=" + currentUser.Id + ", namn=" + currentUser.Name + ").", System.Diagnostics.EventLogEntryType.Information, 1);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Utils.Log("Misslyckades med att skicka ett välkomstmail till den nyskapade användaren(id=" + newUser.Id + ", mailadress=" + newUser.Username + "). Errormsg=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 152);
                                }
                            }

                            ResultBox.Text = "Användare skapad.";
                            ResultBox.BoxTheme = ResultBox.ThemeChoices.InformationTheme;

                            // Tömmer fälten så att man kan mata in en ny användare.
                            Name.Text = string.Empty;
                            Username.Text = string.Empty;
                            Description.Text = string.Empty;
                            Name.Focus();
                        }
                    }
                }
                else
                {
                    Response.Redirect("ShowMaps.aspx");
                }
            }
            catch (Exception exception)
            {
                ResultBox.Text = "Ett fel uppstod vid sparande av redigerad användare.";
                ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
                Utils.Log("Ett fel uppstod vid sparande av redigerad användare(id=" + editUserId + ") för användare=" + HttpContext.Current.User.Identity.Name + "." + " Errormsg=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 108);
            }
        }

        protected void ResetPasswordButton_Click(object sender, EventArgs e)
        {
            if (!Request.IsAuthenticated)
            {
                Response.Redirect("Logon.aspx");
            }

            if (!currentUser.IsAdmin)
            {
                Response.Redirect("ShowMaps.aspx");
            }

            var editUser = (from u in fiberDb.Users where u.Id == editUserId select u).FirstOrDefault();
            if (editUser != null)
            {
                editUser.Password = null;

                fiberDb.SubmitChanges();

                ResultBox.Text = "Lösenordet är nollställt.";
                ResultBox.BoxTheme = ResultBox.ThemeChoices.InformationTheme;
            }
        }
    }
}