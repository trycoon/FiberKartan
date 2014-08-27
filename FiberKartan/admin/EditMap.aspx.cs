using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using FiberKartan;
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
namespace FiberKartan.Admin
{
    public partial class EditMap : System.Web.UI.Page
    {
        private FiberDataContext fiberDb;
        private int mapTypeId;

        protected void Page_Load(object sender, EventArgs e)
        {
            fiberDb = new FiberDataContext();
            mapTypeId = string.IsNullOrEmpty(Request.QueryString["mid"]) ? 0 : int.Parse(Request.QueryString["mid"]);

            if (!IsPostBack)
            {
                var user = (from u in fiberDb.Users where (u.Username == HttpContext.Current.User.Identity.Name) select u).First();

                if (user.IsAdmin)
                {
                    var serviceCompanies = from sc in fiberDb.ServiceCompanies orderby sc.Name select sc;
                    var serviceCompaniesList = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("0", "Ingen") };
                    foreach (var company in serviceCompanies)
                    {
                        serviceCompaniesList.Add(new KeyValuePair<string, string>(company.Id.ToString(), company.Name));
                    }
                    ServiceCompany.DataSource = serviceCompaniesList;
                    ServiceCompany.DataValueField = "Key";
                    ServiceCompany.DataTextField = "Value";
                    ServiceCompany.DataBind();

                    ServiceCompanyOption.Visible = true;
                }

                Municipality.DataSource = from mp in fiberDb.Municipalities orderby mp.Name select mp;
                Municipality.DataValueField = "Code";
                Municipality.DataTextField = "Name";
                Municipality.DataBind();

                if (mapTypeId > 0)  // Om befintlig karta.
                {
                    ((Literal)Master.FindControl("PageTitle")).Text = "Redigera karta";
                    FiberKartan.MapType mapType = (from mt in fiberDb.MapTypes where mt.Id == mapTypeId select mt).FirstOrDefault();

                    // Om kartan inte finns eller om man inte har skrivrättigheter 
                    if (mapType == null || !Request.IsAuthenticated || !Utils.GetMapAccessRights().HasFlag(MapAccessRights.Write))
                    {
                        Response.Redirect("ShowMaps.aspx", false);
                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }
                    
                    ServiceCompany.SelectedValue = mapType.ServiceCompanyId.ToString();

                    Municipality.SelectedValue = mapType.MunicipalityCode;
                    var viewSettings = ((MapViewSettings)mapType.ViewSettings);

                    MapTitle.Text = mapType.Title;
                    WelcomeMessage.Text = mapType.WelcomeMessage;

                    PublicVisible.Checked = viewSettings.HasFlag(MapViewSettings.PublicVisible);
                    ShowPalette.Checked = viewSettings.HasFlag(MapViewSettings.ShowPalette);
                    ShowConnectionStatistics.Checked = viewSettings.HasFlag(MapViewSettings.ShowConnectionStatistics);
                    ShowTotalDigLengthStatistics.Checked = viewSettings.HasFlag(MapViewSettings.ShowTotalDigLengthStatistics);
                    AllowViewAggregatedMaps.Checked = viewSettings.HasFlag(MapViewSettings.AllowViewAggregatedMaps);
                    OnlyShowYesHouses.Checked = viewSettings.HasFlag(MapViewSettings.OnlyShowHouseYesOnPublicMap);

                    // Om man sedan tidigare har laddat upp en fil med fastighetsgränser till denna karta, så visar vi en knapp för att ta bort denna uppladdade fil.
                    if (mapType.MapFiles.Count() > 0)
                    {
                        DeletePropertyBoundariesButton.Visible = true;
                    }

                    // Man får bara radera kartor om man har fullständiga rättigheter.
                    if (Utils.GetMapAccessRights(mapTypeId).HasFlag(MapAccessRights.FullAccess))
                    {
                        DeleteButton.Visible = true;
                    }
                }
                else
                {
                    ((Literal)Master.FindControl("PageTitle")).Text = "Skapa ny karta";
                    DeleteButton.Visible = false;   // Det skall inte gå att radera en ny karta man försöker skapa.
                    SaveButton.Text = "Skapa";
                }
            }
        }

        protected void SaveButton_Click(object sender, EventArgs e)
        {
            if (mapTypeId > 0)  // Om befintlig karta.
            {
                try
                {
                    FiberKartan.MapType mapType = (from mt in fiberDb.MapTypes where mt.Id == mapTypeId select mt).FirstOrDefault();

                    // Om kartan inte finns eller om man inte har skrivrättigheter 
                    if (mapType == null || !Request.IsAuthenticated || !Utils.GetMapAccessRights().HasFlag(MapAccessRights.Write))
                    {
                        Response.Redirect("ShowMaps.aspx", false);
                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }

                    if (!string.IsNullOrEmpty(MapTitle.Text))
                        MapTitle.Text = MapTitle.Text.Trim();
                    if (string.IsNullOrEmpty(MapTitle.Text))
                        throw new ApplicationException("En titel för kartan måste anges.");
                    if (MapTitle.Text.Length > 255)
                        throw new ApplicationException("En titel får innehålla max 255 tecken.");
                    mapType.Title = MapTitle.Text;

                    if (!string.IsNullOrEmpty(WelcomeMessage.Text))
                        WelcomeMessage.Text = WelcomeMessage.Text.Trim();
                    mapType.WelcomeMessage = WelcomeMessage.Text;

                    // Sparar ner serviceleverantör.
                    if (ServiceCompany.Visible)
                    {
                        if (ServiceCompany.SelectedValue == "0")
                        {
                            mapType.ServiceCompanyId = null;
                        }
                        else
                        {
                            mapType.ServiceCompanyId = int.Parse(ServiceCompany.SelectedValue);
                        }
                    }
                    
                    // Sparar ner kommun.
                    mapType.MunicipalityCode = Municipality.SelectedValue;

                    var viewSettings = MapViewSettings.None;

                    if (PublicVisible.Checked)
                        viewSettings |= MapViewSettings.PublicVisible;

                    if (ShowPalette.Checked)
                        viewSettings |= MapViewSettings.ShowPalette;

                    if (ShowConnectionStatistics.Checked)
                        viewSettings |= MapViewSettings.ShowConnectionStatistics;

                    if (ShowTotalDigLengthStatistics.Checked)
                        viewSettings |= MapViewSettings.ShowTotalDigLengthStatistics;

                    if (AllowViewAggregatedMaps.Checked)
                        viewSettings |= MapViewSettings.AllowViewAggregatedMaps;

                    if (OnlyShowYesHouses.Checked)
                        viewSettings |= MapViewSettings.OnlyShowHouseYesOnPublicMap;

                    mapType.ViewSettings = (int)viewSettings;

                    Utils.Log("Sparar kartinställningar(mapTypeId=" + mapTypeId + ", title=" + mapType.Title + ") för användare=" + HttpContext.Current.User.Identity.Name + ".", System.Diagnostics.EventLogEntryType.Information, 107);

                    fiberDb.SubmitChanges();

                    Response.Redirect("ShowMaps.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                catch (ApplicationException exception)
                {
                    ResultBox.Text = exception.Message;
                    ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
                    Utils.Log("Ett fel uppstod vid sparande av kartinställningar(mapTypeId=" + mapTypeId + ", title=" + MapTitle.Text + ") för användare=" + HttpContext.Current.User.Identity.Name + "." + " Errormsg=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Information, 108);
                }
                catch (Exception exception)
                {
                    ResultBox.Text = exception.Message;
                    ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;
                    Utils.Log("Ett fel uppstod vid sparande av kartinställningar(mapTypeId=" + mapTypeId + ", title=" + MapTitle.Text + ") för användare=" + HttpContext.Current.User.Identity.Name + "." + " Errormsg=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 108);
                }
            }
            else
            {
                // Skapa en ny karta.
                try
                {
                    if (!Request.IsAuthenticated)
                    {
                        Response.Redirect("Logon.aspx", false);
                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }

                    var user = (from u in fiberDb.Users where u.Username == HttpContext.Current.User.Identity.Name select u).FirstOrDefault();

                    if (user == null)
                    {
                        Response.Redirect("Logon.aspx", false);
                        Context.ApplicationInstance.CompleteRequest();
                        return;
                    }

                    FiberKartan.MapType mapType = new MapType();
                    mapType.CreatorId = user.Id;
                    mapType.User = user;

                    if (!string.IsNullOrEmpty(MapTitle.Text))
                        MapTitle.Text = MapTitle.Text.Trim();
                    if (string.IsNullOrEmpty(MapTitle.Text))
                        throw new ApplicationException("En titel för kartan måste anges.");
                    if (MapTitle.Text.Length > 255)
                        throw new ApplicationException("En titel får innehålla max 255 tecken.");
                    mapType.Title = MapTitle.Text;

                    if (!string.IsNullOrEmpty(WelcomeMessage.Text))
                    {
                        WelcomeMessage.Text = WelcomeMessage.Text.Trim();
                    }

                    mapType.WelcomeMessage = WelcomeMessage.Text;

                    // Sparar ner serviceleverantör.
                    if (ServiceCompany.Visible)
                    {
                        if (ServiceCompany.SelectedValue == "0")
                        {
                            mapType.ServiceCompanyId = null;
                        }
                        else
                        {
                            mapType.ServiceCompanyId = int.Parse(ServiceCompany.SelectedValue);
                        }
                    }

                    // Sparar ner kommun.
                    mapType.MunicipalityCode = Municipality.SelectedValue;

                    var viewSettings = MapViewSettings.None;

                    if (PublicVisible.Checked)
                        viewSettings |= MapViewSettings.PublicVisible;

                    if (ShowPalette.Checked)
                        viewSettings |= MapViewSettings.ShowPalette;

                    if (ShowConnectionStatistics.Checked)
                        viewSettings |= MapViewSettings.ShowConnectionStatistics;

                    if (ShowTotalDigLengthStatistics.Checked)
                        viewSettings |= MapViewSettings.ShowTotalDigLengthStatistics;

                    if (AllowViewAggregatedMaps.Checked)
                        viewSettings |= MapViewSettings.AllowViewAggregatedMaps;

                    if (OnlyShowYesHouses.Checked)
                        viewSettings |= MapViewSettings.OnlyShowHouseYesOnPublicMap;

                    mapType.ViewSettings = (int)viewSettings;

                    // Skapare av kartan skall ha fullständiga rättigheter.
                    var mapAcessRight = MapAccessRights.None;
                    mapAcessRight |= MapAccessRights.Read;
                    mapAcessRight |= MapAccessRights.Export;
                    mapAcessRight |= MapAccessRights.Write;
                    mapAcessRight |= MapAccessRights.FullAccess;
                    var mapTypeAccessRight = new FiberKartan.MapTypeAccessRight()
                    {
                        MapTypeId = mapType.Id,
                        UserId = user.Id,
                        AccessRight = (int)mapAcessRight
                    };
                    mapTypeAccessRight.MapType = mapType;
                    mapTypeAccessRight.MapTypeId = mapType.Id;

                    Utils.Log("Sparar kartinställningar(mapTypeId=" + mapTypeId + ", title=" + mapType.Title + ") för användare=" + HttpContext.Current.User.Identity.Name + ".", System.Diagnostics.EventLogEntryType.Information, 113);

                    fiberDb.SubmitChanges();

                    Response.Redirect("ShowMaps.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                catch (Exception exception)
                {
                    ResultBox.Text = exception.Message;
                    ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;

                    Utils.Log("Ett fel uppstod vid sparande av kartinställningar(mapTypeId=" + mapTypeId + ", title=" + MapTitle.Text + ") för användare=" + HttpContext.Current.User.Identity.Name + "." + " Errormsg=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 114);
                }
            }
        }

        protected void DeleteButton_Click(object sender, EventArgs e)
        {
            if (!Request.IsAuthenticated)
            {
                Response.Redirect("Logon.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            FiberKartan.MapType mapType = (from mt in fiberDb.MapTypes where mt.Id == mapTypeId select mt).FirstOrDefault();

            if (mapType == null || !Utils.GetMapAccessRights(mapTypeId).HasFlag(MapAccessRights.FullAccess))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                Response.StatusDescription = "Forbidden";
                Response.Write("Du saknar rättigheter för att få radera kartan.");
                Response.End();
            }

            try
            {
                Utils.Log("Raderar existerande karta(mapTypeId=" + mapType.Id + ", title=" + mapType.Title + ") by user=" + HttpContext.Current.User.Identity.Name, System.Diagnostics.EventLogEntryType.Information, 120);

                fiberDb.DeleteMapType(mapType.Id);

                fiberDb.SubmitChanges();

                Response.Redirect("ShowMaps.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception exception)
            {
                ResultBox.Text = exception.Message;
                ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;

                Utils.Log("Ett fel uppstod vid raderande av existerande karta(mapTypeId=" + mapType.Id + ", title=" + mapType.Title + "). Errormsg=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 123);
            }
        }

        protected void DeletePropertyBoundariesButton_Click(object sender, EventArgs e)
        {
            if (!Request.IsAuthenticated)
            {
                Response.Redirect("Logon.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            FiberKartan.MapType mapType = (from mt in fiberDb.MapTypes where mt.Id == mapTypeId select mt).FirstOrDefault();

            if (mapType == null || !Utils.GetMapAccessRights(mapTypeId).HasFlag(MapAccessRights.Write))
            {
                Response.StatusCode = (int)HttpStatusCode.Forbidden;
                Response.StatusDescription = "Forbidden";
                Response.Write("Du saknar rättigheter för att få ta bort fastighetsgränser från kartan.");
                Response.End();
            }

            try
            {
                Utils.Log("Tar bort fastighetsgränser från existerande karta(mapTypeId=" + mapType.Id + ", title=" + mapType.Title + ") by user=" + HttpContext.Current.User.Identity.Name, System.Diagnostics.EventLogEntryType.Information, 120);

                fiberDb.MapFiles.DeleteAllOnSubmit(mapType.MapFiles);
                
                fiberDb.SubmitChanges();

                Response.Redirect("ShowMaps.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (Exception exception)
            {
                ResultBox.Text = exception.Message;
                ResultBox.BoxTheme = ResultBox.ThemeChoices.ErrorTheme;

                Utils.Log("Ett fel uppstod vid borttagning av fastighetsgränser från existerande karta(mapTypeId=" + mapType.Id + ", title=" + mapType.Title + "). Errormsg=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 123);
            }
        }
    }
}