using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using FiberKartan;
using FiberKartan.Admin.Security;
using System.Net.Mail;
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
    public partial class Logon : System.Web.UI.Page
    {
        private FiberDataContext fiberDb;
        private MapAccessInvitation invitation;

        protected void Page_Load(object sender, EventArgs e)
        {
            fiberDb = new FiberDataContext();

            // Kolla om sidan laddas med en Invitation-kod, i så fall skall vi visa upp ett formulär för att skapa ny användare.
            if (!string.IsNullOrEmpty(Request.QueryString["invitation"]))
            {
                invitation = fiberDb.MapAccessInvitations.Where(ar => ar.InvitationCode == Request.QueryString["invitation"]).FirstOrDefault();

                if (invitation == null)
                {
                    // Ogilltig eller gäller inte längre.
                    Utils.Log("Ogiltigt inbjudningskod \"" + Request.QueryString["invitation"] + "\" från ip-adress \"" + Request.ServerVariables["REMOTE_ADDR"].ToString() + "\". Visar inloggningsbox istället.", System.Diagnostics.EventLogEntryType.Information, 1);
                }
                else
                {
                    // Här kollar vi att det inte finns en användare redan med samma e-postadress som i inbjudan, dom kan ju ha kommit till efter inbjudan skickades ut och i så fall kan vi inte skapa en ny användare.
                    var existingUser = fiberDb.Users.Where(u => u.Username == invitation.Email).FirstOrDefault();
                    if (existingUser == null)
                    {
                        loginBox.Visible = false;
                        newUserBox.Visible = true;
                    }
                }
            }

            // Om man redan är inloggad så skall man inte kunna hamna på denna sida.
            // PostBack kollar vi bara för att man måste sätta ett lösenord om man inte har ett.
            else if (!this.IsPostBack && HttpContext.Current.User.Identity.IsAuthenticated)
            {
                Response.Redirect("ShowMaps.aspx");
            }
        }

        protected void loginButton_Click(object sender, EventArgs e)
        {
            ResultBox.Text = string.Empty;
            ResultBox.Visible = false;
            var provider = (AdminMemberProvider)Membership.Provider;

            if (provider.ValidateUser(username.Text, password.Text))
            {
                var dbUser = fiberDb.Users.Where(u => u.Id == provider.User.UserId).Single();
                dbUser.LastLoggedOn = DateTime.Now; // Uppdaterar tiden.
                fiberDb.SubmitChanges();

                // Om man klickat i "Kom ihåg mig" så förblir användaren inloggad upp till ett halvår, annars så blir man det bara i fem timmar såvidare man inte stänger ner fönstret.
                if (rememberMeCheckBox.Checked)
                {
                    var ticket = new FormsAuthenticationTicket(1, provider.User.UserName, DateTime.Now, DateTime.Now.AddDays(183), true, string.Empty);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
                    cookie.Expires = DateTime.Now.AddDays(183);
                    Response.Cookies.Add(cookie);
                }
                else
                {
                    var ticket = new FormsAuthenticationTicket(1, provider.User.UserName, DateTime.Now, DateTime.Now.AddHours(5), false, string.Empty);
                    var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
                    cookie.Expires = DateTime.Now.AddHours(5);
                    Response.Cookies.Add(cookie);
                }

                Utils.Log("Användare id=" + dbUser.Id + ", username=\"" + dbUser.Username + "\" loggade in från ip-adress \"" + Request.ServerVariables["REMOTE_ADDR"].ToString() + "\".", System.Diagnostics.EventLogEntryType.SuccessAudit, 102);

                // Om man inte har ett lösenord satt, uppmana användaren att sätta ett lösenord.
                if (string.IsNullOrEmpty(dbUser.Password))
                {
                    ResultBox.Text = string.Empty;
                    ResultBox.Visible = false;

                    loginButton.Visible = false;
                    rememberMeCheckBox.Visible = false;
                    usernameLabel.Visible = false;
                    username.Visible = false;

                    title.Text = "Skapa lösenord";
                    passwordLabel.InnerText = "Ange önskat lösenord";
                    repeatPasswordSection.Visible = true;
                    savePasswordButton.Visible = true;
                }
                else
                {
                    if (string.IsNullOrEmpty(Request.QueryString["ReturnUrl"]))
                    {
                        Response.Redirect("ShowMaps.aspx");
                    }
                    else
                    {
                        Response.Redirect(Request.QueryString["ReturnUrl"]);
                    }
                }
            }
            else
            {
                ResultBox.Text = "Felaktiga inloggningsuppgifter, var god försök igen.";
                ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
                Utils.Log("Misslyckad inloggning för användare \"" + username.Text + "\" från ip-adress \"" + Request.ServerVariables["REMOTE_ADDR"].ToString() + "\".", System.Diagnostics.EventLogEntryType.FailureAudit, 102);

                Thread.Sleep(100);  // Fördröjning så att man inte kan bygga ett program som söker efter lösenord.
            }
        }

        protected void savePasswordButton_Click(object sender, EventArgs e)
        {
            password.Text = password.Text.Trim();
            password2.Text = password2.Text.Trim();

            if (string.IsNullOrEmpty(password.Text) || string.IsNullOrEmpty(password2.Text))
            {
                ResultBox.Text = "Du måste ange ett önskat lösenord och sedan återupprepa lösenordet.";
                ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
            }
            else if (password.Text.Length < 5)
            {
                ResultBox.Text = "Angivet lösenord är för kort, var god ange ett längre och säkrare lösenord.";
                ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
            }
            else if (password.Text.Length > 50)
            {
                ResultBox.Text = "Angivet lösenord är för långt, var god ange ett kortare lösenord.";
                ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
            }
            else if (password.Text != password2.Text)
            {
                ResultBox.Text = "Lösenorden överensstämmer inte, var god återupprepa lösenordet korrekt.";
                ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
            }
            else
            {
                var provider = (AdminMemberProvider)Membership.Provider;
                var dbUser = fiberDb.Users.Where(u => u.Id == provider.User.UserId).Single();
                dbUser.Password = AdminMemberProvider.GeneratePasswordHash(dbUser.Username, password.Text);
                fiberDb.SubmitChanges();

                if (string.IsNullOrEmpty(Request.QueryString["ReturnUrl"]))
                {
                    Response.Redirect("ShowMaps.aspx");
                }
                else
                {
                    Response.Redirect(Request.QueryString["ReturnUrl"]);
                }
            }
        }

        protected void createUserButton_Click(object sender, EventArgs e)
        {
            NewUserResultBox.Text = string.Empty;
            NewUserResultBox.Visible = false;

            name.Text = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(name.Text.ToLower().Trim());
            description.Text = description.Text.Trim();
            newUserPassword.Text = newUserPassword.Text.Trim();
            newUserPassword2.Text = newUserPassword2.Text.Trim();

            if (!name.Text.Contains(' ') || name.Text.Length < 5) // Minsta tänkbara längd på namn? Två bokstäver till förnamn + mellanslag + två bokstäver för efternamn.
            {
                NewUserResultBox.Text = "Du måste ange ditt för- och efternamn.";
                NewUserResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
            }
            else if (name.Text.Contains("@"))   // En del verkar vilja ange e-postadressen som namn.
            {
                NewUserResultBox.Text = "Ditt namn innehåller ett ogiltigt tecken.";
                NewUserResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
            }
            else if (description.Text.Length < 5) // Minsta tänkbara längd på namn för fiberförening eller företag.
            {
                NewUserResultBox.Text = "Du måste ange namnet på din fiberförening eller företag.";
                NewUserResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
            }
            else if (string.IsNullOrEmpty(newUserPassword.Text) || string.IsNullOrEmpty(newUserPassword2.Text))
            {
                NewUserResultBox.Text = "Du måste ange ett önskat lösenord och sedan återupprepa lösenordet.";
                NewUserResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
            }
            else if (newUserPassword.Text.Length < 5)
            {
                NewUserResultBox.Text = "Angivet lösenord är för kort, var god ange ett längre och säkrare lösenord.";
                NewUserResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
            }
            else if (newUserPassword.Text.Length > 50)
            {
                NewUserResultBox.Text = "Angivet lösenord är för långt, var god ange ett kortare lösenord.";
                NewUserResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
            }
            else if (newUserPassword.Text != newUserPassword2.Text)
            {
                NewUserResultBox.Text = "Lösenorden överensstämmer inte, var god återupprepa lösenordet korrekt.";
                NewUserResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
            }
            else
            {
                // Nu har vi tagit oss igenom kontrollen, skapa konto, tilldela rättigheter och ta bort inbjudan.
                var newUser = new User()
                {
                    Created = DateTime.Now,
                    Name = name.Text,
                    Description = description.Text,
                    Password = AdminMemberProvider.GeneratePasswordHash(invitation.Email.Trim().ToLower(), newUserPassword.Text),
                    Username = invitation.Email.Trim().ToLower(),
                    LastLoggedOn = DateTime.Now
                };
                fiberDb.Users.InsertOnSubmit(newUser);

                var newMapAccessRight = new MapTypeAccessRight
                {
                    User = newUser,
                    MapTypeId = invitation.MapTypeId,
                    AccessRight = invitation.AccessRight
                };
                fiberDb.MapTypeAccessRights.InsertOnSubmit(newMapAccessRight);

                // Inbjudan är förbrukad.
                fiberDb.MapAccessInvitations.DeleteOnSubmit(invitation);

                fiberDb.SubmitChanges();

                // Skapa inloggnings-kaka.
                var ticket = new FormsAuthenticationTicket(1, newUser.Username, DateTime.Now, DateTime.Now.AddHours(5), false, string.Empty);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, FormsAuthentication.Encrypt(ticket));
                cookie.Expires = DateTime.Now.AddHours(5);
                Response.Cookies.Add(cookie);

                Utils.Log("Ny användare skapad! Id=" + newUser.Id + ", namn=\"" + newUser.Name + "\", username=\"" + newUser.Username + "\" med rättighet=" + newMapAccessRight.AccessRight + " till karta=" + newMapAccessRight.MapTypeId + ". Skapad från ip-adress \"" + Request.ServerVariables["REMOTE_ADDR"].ToString() + "\".", System.Diagnostics.EventLogEntryType.SuccessAudit, 103);

                #region SendMail
                // Skicka ett mail till admin för systemet så att dom vet att vi har fått en ny användare.
                try
                {
                    using (var mail = new MailMessage(
                            "noreply@fiberkartan.se",
                            ConfigurationManager.AppSettings.Get("adminMail"),
                            "Ny användare skapad i FiberKartan",
                            "Ny användare har skapats. Id=" + newUser.Id + ", namn=\"" + newUser.Name + "\", username=\"" + newUser.Username + "\" med rättighet=" + newMapAccessRight.AccessRight + " till karta=" + newMapAccessRight.MapType.Title + ". Skapad från ip-adress \"" + Request.ServerVariables["REMOTE_ADDR"].ToString() + "\"."
                        ))
                    {
                        using (var SMTPServer = new SmtpClient())
                        {
                            SMTPServer.Send(mail);
                        }
                    }
                }
                catch (Exception exception)
                {
                    Utils.Log("Misslyckades med att skicka mail angående skapandet av ny användare(id=" + newUser.Id+ "). Errormsg=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 152);
                }

                // Skicka ett bekräftelse mail till personen som skickade inbjudan, så att dom vet att mottagaren nu har registrerat ett konto och kan arbeta med kartan.
                try
                {
                    var invitationSentBy = fiberDb.Users.Where(u => u.Id == invitation.InvitationSentBy).FirstOrDefault();

                    if (invitationSentBy != null)
                    {
                        using (var mail = new MailMessage(
                                "noreply@fiberkartan.se",
                                invitationSentBy.Username,
                                "Användare har accepterat din inbjudan till att samarbeta kring karta.",
                                "Användaren " + newUser.Name + " med e-postadress " + newUser.Username + " har accepterat din inbjudan till att samarbeta kring kartan \"" + newMapAccessRight.MapType.Title + "\"."
                            ))
                        {
                            using (var SMTPServer = new SmtpClient())
                            {
                                SMTPServer.Send(mail);
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    Utils.Log("Misslyckades med att skicka bekräftelsemail till inbjudaren angående att användaren(id=" + newUser.Id + ") accepterat inbjudan. Errormsg=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 152);
                }
                #endregion SendMail


                if (string.IsNullOrEmpty(Request.QueryString["ReturnUrl"]))
                {
                    Response.Redirect("ShowMaps.aspx");
                }
                else
                {
                    Response.Redirect(Request.QueryString["ReturnUrl"]);
                }
            }
        }
    }
}