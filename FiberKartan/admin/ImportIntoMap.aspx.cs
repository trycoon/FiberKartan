using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using FiberKartan.Kml;
using System.Text;

/*
The zlib/libpng License
Copyright (c) 2012 Henrik Östman

This software is provided 'as-is', without any express or implied warranty. In no event will the authors be held liable for any damages arising from the use of this software.
Permission is granted to anyone to use this software for any purpose, including commercial applications, and to alter it and redistribute it freely, subject to the following restrictions:

1. The origin of this software must not be misrepresented; you must not claim that you wrote the original software. If you use this software in a product, an acknowledgment in the product documentation would be appreciated but is not required.
2. Altered source versions must be plainly marked as such, and must not be misrepresented as being the original software.
3. This notice may not be removed or altered from any source distribution.
*/
namespace FiberKartan.Admin
{
    public partial class ImportIntoMap : System.Web.UI.Page
    {
        private int mapTypeId;
        private FiberDataContext fiberDb;

        protected void Page_Load(object sender, EventArgs e)
        {
            ((Literal)Master.FindControl("PageTitle")).Text = "Importera till karta";

            fiberDb = new FiberDataContext();

            if (!int.TryParse(Request.QueryString["mid"], out mapTypeId))
            {
                Response.Redirect("ShowMaps.aspx");
            }

            if (!Request.IsAuthenticated || !Utils.GetMapAccessRights().HasFlag(MapAccessRights.Write))
            {
                Response.Redirect("ShowMaps.aspx");
            }

            if (IsPostBack)
            {
                ImportButton.Visible = false;

                if (MarkerAndLineMapFileUpload.HasFile || PropertyBoundariesFileUpload.HasFile)
                {
                    ImportPropertyBoundariesMapPanel.Visible = false;
                    MarkerAndLineMapImportSelectionPanel.Visible = false;

                    if (markerAndLineImportSelectButton.Checked)
                    {
                        MarkerAndLineMapImportSelectionPanel.Visible = true;
                        UploadedMarkerAndLineMapFile();
                    }
                    else if (propertyBoundariesImportSelectButton.Checked)
                    {
                        ImportPropertyBoundariesMapPanel.Visible = true;
                        UploadedPropertyBoundariesMapFile();
                    }
                }
            }           
        }

        protected void InputTypeChanged(object sender, System.EventArgs e)
        {
            ImportMarkerAndLineMapPanel.Visible = false;
            ImportPropertyBoundariesMapPanel.Visible = false;
            MarkerAndLineMapImportSelectionPanel.Visible = false;
            PropertyBoundariesImportSelectionPanel.Visible = false;

            if (markerAndLineImportSelectButton.Checked)
            {
                ImportMarkerAndLineMapPanel.Visible = true;
            }
            else
            {
                ImportPropertyBoundariesMapPanel.Visible = true;
            }
        }

        #region ImportMarkerAndLineMapFile
        /// <summary>
        /// Denna metod anropas då vi vill importera en karta med fastigheter, linjer och områden.
        /// </summary>
        private void UploadedMarkerAndLineMapFile()
        {
            AnalyzedKml analyzedKml = null;
            try
            {
                var file = MarkerAndLineMapFileUpload.PostedFile;

                Utils.Log("Laddar upp och analyserar fil. Filnamn=\"" + file.FileName + "\", ContentLength=" + file.ContentLength + ", ContentType=\"" + file.ContentType + "\".", System.Diagnostics.EventLogEntryType.Information, 103);

                string dataString = null;

                using (var fileStream = file.InputStream)
                {
                    if (fileStream == null || fileStream.Length < 1)
                    {
                        Utils.Log("Tom fil laddades upp. Filename=\"" + file.FileName + "\", ContentLength=" + file.ContentLength + ", ContentType=\"" + file.ContentType + "\".", System.Diagnostics.EventLogEntryType.Warning, 105);
                        throw new ApplicationException("Filen är tom, inget kan importeras.");
                    }

                    if (file.ContentType.ToLower() != "application/vnd.google-earth.kml+xml")
                    {
                        Utils.Log("Fil som laddades upp stöds inte av systemet. Filnamn=\"" + file.FileName + "\", ContentLength=" + file.ContentLength + ", ContentType=\"" + file.ContentType + "\".", System.Diagnostics.EventLogEntryType.Warning, 105);
                        throw new ApplicationException("Denna filtyp stöds inte.");
                    }

                    using (var reader = new StreamReader(fileStream, Encoding.UTF8))
                    {
                        dataString = reader.ReadToEnd();
                    }
                }

                analyzedKml = KmlImportExport.AnalyzeKml(dataString);

                // Remove old cached file, if any.
                if (!string.IsNullOrEmpty(FilenameHiddenfield.Value))
                {
                    HttpContext.Current.Cache.Remove(HttpContext.Current.User.Identity.Name + "#" + FilenameHiddenfield.Value);
                }

                // Save filename for later use (so that we can recreate the cachekey).
                FilenameHiddenfield.Value = file.FileName;

                // Cache file so that we could import it later.
                HttpContext.Current.Cache.Insert(HttpContext.Current.User.Identity.Name + "#" + file.FileName, dataString, null, DateTime.Now.AddMinutes(30), System.Web.Caching.Cache.NoSlidingExpiration, System.Web.Caching.CacheItemPriority.Default, null);

                Utils.Log("Fil med filnamn=\"" + file.FileName + "\" laddades upp korrekt, inväntar nu användarens importeringsval.", System.Diagnostics.EventLogEntryType.Information, 103);

                markerTypes.DataSource = analyzedKml.FoundMarkers;
                markerTypes.DataBind();

                lineTypes.DataSource = analyzedKml.FoundLines;
                lineTypes.DataBind();

                polygonTypes.DataSource = analyzedKml.FoundPolygons;
                polygonTypes.DataBind();

                // Ladda dropdown med lista över föregående versioner av kartan.
                var mapVersions = from m in fiberDb.Maps.Where(m => m.MapTypeId == mapTypeId) select m;
                var mapVersionsList = new List<KeyValuePair<string, string>>() { new KeyValuePair<string, string>("0", "Ingen") };
                foreach (var mapVersion in mapVersions)
                {
                    mapVersionsList.Add(new KeyValuePair<string, string>(mapVersion.Ver.ToString(), "ver #" + mapVersion.Ver + " (" + mapVersion.Created + ")"));
                }

                mergeVersionDropdown.DataSource = mapVersionsList;
                mergeVersionDropdown.DataTextField = "Value";
                mergeVersionDropdown.DataValueField = "Key";
                mergeVersionDropdown.DataBind();

                // Visa bara importera-knappen om det finns nått att importera.
                if (analyzedKml.FoundMarkers.Count > 0 || analyzedKml.FoundLines.Count > 0 || analyzedKml.FoundPolygons.Count > 0)
                {
                    ImportButton.Visible = true;
                }
            }
            catch (Exception exception)
            {
                Utils.Log("Misslyckades med att ladda upp och analysera fil. Felmeddelande=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 159);
                this.ClientScript.RegisterStartupScript(typeof(Page), "alertError",
                   "var $dialog = $('<div></div>')" +
                   ".html('Misslyckades med att ladda upp och analysera fil, detta kan t.ex. bero på att filen innehåller felaktig information eller information som denna applikation ännu inte stödjer. Skicka ett <u><a href=\"mailto:trycoon@gmail.com?subject=Misslyckad importering&body=" + HttpUtility.HtmlEncode("Misslyckades med att importera fil i FiberKartan. MapTypeId=" + mapTypeId + ", tidpunkt=" + DateTime.Now) + ". Med Vänliga Hälsningar, \" target=\"blank\">e-brev</a></u> så att vi kan felsöka och eventuellt åtgärda problemet.')" +
                   ".dialog({" +
                           "autoOpen: false," +
                           "title: 'Importering misslyckades'," +
                           "close: function () { $(this).remove(); }," +
                           "position: 'center'," +
                           "width: 400," +
                           "height: 220," +
                           "modal: true," +
                           "resizable: false," +
                           "buttons: {" +
                               "Ok: function () {" +
                                   "$(this).dialog(\"close\");" +
                               "}" +
                           "}" +
                   "});" +
                   "$dialog.dialog('open');"
                   , true);
            }
        }

        /// <summary>
        /// Anropas för varje markörtyp som hittats och som skall renderas ut i listan.
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        protected void MarkerTypesListBound(Object Sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                ((Image)e.Item.FindControl("MarkerImage")).ImageUrl = ((FoundMarker)e.Item.DataItem).MarkerHref;

                // Sätter förslagen markörtyp i dropdown.
                if (!string.IsNullOrEmpty(((FoundMarker)e.Item.DataItem).SuggestedMarkerTranslation))
                {
                    ((DropDownList)e.Item.FindControl("markerSelection")).SelectedValue = ((FoundMarker)e.Item.DataItem).SuggestedMarkerTranslation;
                }
            }
        }

        /// <summary>
        /// Anropas för varje linjetyp som hittats och som skall renderas ut i listan.
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        protected void LineTypesListBound(Object Sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                // Sätter förslagen linjetyp i dropdown.                
                ((DropDownList)e.Item.FindControl("linesSelection")).SelectedValue = ((FoundLine)e.Item.DataItem).SuggestedLineTranslation.ToString();
            }
        }

        /// <summary>
        /// Anropas för varje polygontyp som hittats och som skall renderas ut i listan.
        /// </summary>
        /// <param name="Sender"></param>
        /// <param name="e"></param>
        protected void PolygonListBound(Object Sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                // Sätter förslagen polygontyp i dropdown.                
                ((DropDownList)e.Item.FindControl("polygonSelection")).SelectedValue = ((FoundPolygon)e.Item.DataItem).SuggestedPolygonTranslation.ToString();
            }
        }
        #endregion ImportMarkerAndLineMapFile

        #region ImportPropertyBoundariesMapFile
        /// <summary>
        /// Denna metod anropas då man vill ladda upp en fil med fastighetsgränser.
        /// </summary>
        private void UploadedPropertyBoundariesMapFile()
        {
            try
            {
                var file = PropertyBoundariesFileUpload.PostedFile;

                Utils.Log("Laddar upp kartfil med fastighetsgränser för karta, mapTypeId=" + mapTypeId + ". Filnamn=\"" + file.FileName + "\", ContentLength=" + file.ContentLength + ", ContentType=\"" + file.ContentType + "\".", System.Diagnostics.EventLogEntryType.Information, 103);

                KmlImportExport.ImportPropertyBoundaries(file, mapTypeId);

                Utils.Log("Uppladdningen av kartfil med fastighetsgränser lyckades.", System.Diagnostics.EventLogEntryType.Information, 111);

                // Visa ruta om att importeringen lyckades och skicka sedan användaren vidare till sidan med kartversioner.
                this.ClientScript.RegisterStartupScript(typeof(Page), "alertSuccessAndRedirect",
                    "var $dialog = $('<div></div>')" +
                    ".html('Importeringen lyckades.')" +
                    ".dialog({" +
                            "autoOpen: false," +
                            "title: 'Importering'," +
                            "close: function () { $(this).remove(); window.location.href='ShowMapVersions.aspx?mid=" + mapTypeId + "'; }," +
                            "position: 'center'," +
                            "width: 400," +
                            "modal: true," +
                            "resizable: false," +
                            "buttons: {" +
                                "Ok: function () {" +
                                    "$(this).dialog(\"close\"); window.location.href='ShowMapVersions.aspx?mid=" + mapTypeId + "';" +
                                "}" +
                            "}" +
                    "});" +
                    "$dialog.dialog('open');"
                    , true);
            }
            catch (Exception exception)
            {
                Utils.Log("Misslyckades med att importera karta med fastighetsgränser. MapTypeID=" + mapTypeId + ", Felmeddelande=" + exception.Message + ", Stacktrace=" + exception.StackTrace, System.Diagnostics.EventLogEntryType.Error, 104);

                this.ClientScript.RegisterStartupScript(typeof(Page), "alertError",
                    "var $dialog = $('<div></div>')" +
                    ".html('Ett eller flera fel uppstod vid importering av kartan med fastighetsgränser, detta kan t.ex. bero på att filen innehåller felaktig information eller information som denna applikation ännu inte stödjer. Skicka ett <u><a href=\"mailto:trycoon@gmail.com?subject=Misslyckad importering&body=" + HttpUtility.HtmlEncode("Misslyckades med att importera fil i FiberKartan. MapTypeId=" + mapTypeId + ", tidpunkt=" + DateTime.Now) + ". Med Vänliga Hälsningar, \" target=\"blank\">e-brev</a></u> så att vi kan felsöka och eventuellt åtgärda problemet.')" +
                    ".dialog({" +
                            "autoOpen: false," +
                            "title: 'Importering misslyckades'," +
                            "close: function () { $(this).remove(); }," +
                            "position: 'center'," +
                            "width: 400," +
                            "height: 220," +
                            "modal: true," +
                            "resizable: false," +
                            "buttons: {" +
                                "Ok: function () {" +
                                    "$(this).dialog(\"close\");" +
                                "}" +
                            "}" +
                    "});" +
                    "$dialog.dialog('open');"
                    , true);
            }
        }
        #endregion ImportPropertyBoundariesMapFile

        /// <summary>
        /// Anropas när vi trycker på importera-knappen.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ImportButton_Click(object sender, EventArgs e)
        {
            var mergeVersion = int.Parse(mergeVersionDropdown.SelectedValue ?? "0");
            var mapFilename = FilenameHiddenfield.Value;

            Utils.Log("Importing file. Filename=\"" + mapFilename + "\", MapTypeID=" + mapTypeId + ", MergeVersion=" + mergeVersion, System.Diagnostics.EventLogEntryType.Information, 104);

            try
            {
                var markerTranslations = new Dictionary<string, string>();
                foreach (Control control in markerTypes.Items) {
                    var markerImage = control.FindControl("MarkerImage") as Image;
                    var markerTranslationChoice = control.FindControl("markerSelection") as DropDownList;

                    if (markerImage != null)
                    {
                        markerTranslations.Add(markerImage.ImageUrl, markerTranslationChoice.SelectedValue);
                    }
                }

                // Endast ett värde stöds idag, men vi gör så här för att vara framtidskompatibla ändå.
                var lineTranslations = new Dictionary<string, string>();
                foreach (Control control in lineTypes.Items)
                {
                    var lineTranslationChoice = control.FindControl("linesSelection") as DropDownList;
                    lineTranslations.Add("0", lineTranslationChoice.SelectedValue);
                }

                // Endast ett värde stöds idag, men vi gör så här för att vara framtidskompatibla ändå.
                var polygonTranslations = new Dictionary<string, string>();
                foreach (Control control in polygonTypes.Items)
                {
                    var polygonTranslationChoice = control.FindControl("polygonSelection") as DropDownList;
                    polygonTranslations.Add("0", polygonTranslationChoice.SelectedValue);
                }

                // Hämta upp fildata som skickades in för analys i föregående steg från cachen, nu skall denna information importeras.
                var kmlString = HttpContext.Current.Cache.Get(HttpContext.Current.User.Identity.Name + "#" + mapFilename) as String;

                var newMapVersion = KmlImportExport.ImportKmlToMap(kmlString, mapTypeId, markerTranslations, lineTranslations, polygonTranslations, mergeVersion, includeOldLinesCheckbox.Checked, includeOldMarkersCheckbox.Checked);

                Utils.Log("Done importing file, filename=\"" + mapFilename + "\". New versionnumber=" + newMapVersion, System.Diagnostics.EventLogEntryType.Information, 111);

                // Visa ruta om att importeringen lyckades och skicka sedan användaren vidare till sidan med kartversioner.
                this.ClientScript.RegisterStartupScript(typeof(Page), "alertSuccessAndRedirect",
                    "var $dialog = $('<div></div>')" +
                    ".html('Importeringen lyckades.')" +
                    ".dialog({" +
                            "autoOpen: false," +
                            "title: 'Importering'," +
                            "close: function () { $(this).remove(); window.location.href='ShowMapVersions.aspx?mid=" + mapTypeId + "'; }," +
                            "position: 'center'," +
                            "width: 400," +
                            "modal: true," +
                            "resizable: false," +
                            "buttons: {" +
                                "Ok: function () {" +
                                    "$(this).dialog(\"close\"); window.location.href='ShowMapVersions.aspx?mid=" + mapTypeId + "';" +
                                "}" +
                            "}" +
                    "});" +
                    "$dialog.dialog('open');"
                    , true);

                Utils.NotifyEmailSubscribers(mapTypeId, newMapVersion);
            }
            catch (Exception)
            {
                Utils.Log("Misslyckades med att importera karta. Filename=\"" + mapFilename + "\", MapTypeID=" + mapTypeId + ", MergeVersion=" + mergeVersion, System.Diagnostics.EventLogEntryType.Error, 104);

                this.ClientScript.RegisterStartupScript(typeof(Page), "alertError",
                    "var $dialog = $('<div></div>')" +
                    ".html('Ett eller flera fel uppstod vid importering av kartan, detta kan t.ex. bero på att filen innehåller felaktig information eller information som denna applikation ännu inte stödjer. Skicka ett <u><a href=\"mailto:trycoon@gmail.com?subject=Misslyckad importering&body=" + HttpUtility.HtmlEncode("Misslyckades med att importera fil i FiberKartan. Filnamn=" + mapFilename + ", mapTypeId=" + mapTypeId + ", tidpunkt=" + DateTime.Now) + ". Med Vänliga Hälsningar, \" target=\"blank\">e-brev</a></u> så att vi kan felsöka och eventuellt åtgärda problemet.')" +
                    ".dialog({" +
                            "autoOpen: false," +
                            "title: 'Importering misslyckades'," +
                            "close: function () { $(this).remove(); }," +
                            "position: 'center'," +
                            "width: 400," +
                            "height: 220," +
                            "modal: true," +
                            "resizable: false," +
                            "buttons: {" +
                                "Ok: function () {" +
                                    "$(this).dialog(\"close\");" +
                                "}" +
                            "}" +
                    "});" +
                    "$dialog.dialog('open');"
                    , true);
            }
        }
    }
}