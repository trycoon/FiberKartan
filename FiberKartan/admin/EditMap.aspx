<%@ Page Title="Fiber admin - Redigera karta" Language="C#" MasterPageFile="~/Admin/AdminMasterPage.Master"
    AutoEventWireup="true" CodeBehind="EditMap.aspx.cs" Inherits="FiberKartan.Admin.EditMap"
    ValidateRequest="false" %>

<asp:Content ID="head" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="content" ContentPlaceHolderID="ContentPlaceHolder" runat="server">
    <div id="editMapForm">
        <asp:Panel ID="editMapPanel" runat="server">
            <label for="MapTitle">
                Namn / Sidtitel</label><asp:TextBox ID="MapTitle" ToolTip="Namn på kartan, visas även som sidtitel på webbläsarens fönster."
                    MaxLength="255" autocomplete="off" TextMode="SingleLine" runat="server"></asp:TextBox><br />
            <br />
            <label for="WelcomeMessage" title="Frivilligt välkomstmeddelande. Visas i ett fönster när man besöker kartan.">
                Välkomstmeddelande</label><asp:TextBox ID="WelcomeMessage" TextMode="MultiLine" runat="server"></asp:TextBox><br />
            <label for="Municipality">
                Kommun</label><asp:DropDownList ID="Municipality" ClientIDMode="Static" runat="server" ToolTip="Vilken kommun tillhör denna karta" CssClass="marginLeft10px">
                </asp:DropDownList>
            <span ID="ServiceCompanyOption" runat="server" Visible="false" class="marginLeft10px">
                <label for="ServiceCompany">
                    Serviceleverantör</label><asp:DropDownList ID="ServiceCompany" ClientIDMode="Static" runat="server" ToolTip="Företag som sköter service på nätverket" CssClass="marginLeft10px">
                    </asp:DropDownList>
            </span>
            <fieldset class="marginTop10px">
                <legend>Inställningar</legend>
                <input id="PublicVisible" type="checkbox" value="PublicVisible" checked="checked"
                    runat="server" /><label for="PublicVisible" title="Om kartan skall vara synlig för publika besökare, annars syns den bara i admin vyn.">Publikt
                        synlig</label><br />
                <input id="ShowPalette" type="checkbox" value="ShowPalette" checked="checked" runat="server" /><label
                    for="ShowPalette" title="Om paletten, där bl.a anslutningsgrad och olika visningsalternativ listas, skall visas(publika vyn).">Visa
                    "paletten"</label><br />
                <input id="ShowConnectionStatistics" type="checkbox" value="ShowConnectionStatistics"
                    checked="checked" runat="server" /><label for="ShowConnectionStatistics" title="Visar anslutningsgraden och antal fastigheter i paletten(publika vyn)">Visa
                        anslutningsgraden</label><br />
                <input id="ShowTotalDigLengthStatistics" type="checkbox" value="ShowTotalDigLengthStatistics"
                    checked="checked" runat="server" /><label for="ShowTotalDigLengthStatistics" title="Visar den uppskattade schaktlängden i paletten(publika vyn)">Visa
                        uppskattad schaktlängd</label><br />
                <input id="AllowViewAggregatedMaps" type="checkbox" value="AllowViewAggregatedMaps"
                    runat="server" /><label for="AllowViewAggregatedMaps" title="Tillåt innehållet på kartan att visas i regions vyn (en karta som visar flera kartor sammanslagna).">Tillåt
                        att karta visas på regionskarta</label><br />
                <input id="OnlyShowYesHouses" type="checkbox" value="OnlyShowYesHouses" runat="server" /><label
                    for="OnlyShowYesHouses" title="Visar bara markörer för de som vill ha fiber.">Visa
                    bara fastigheter som skall ha fiber</label><br />
            </fieldset>
        </asp:Panel>
        <fk:ResultBox ID="ResultBox" runat="server" />
        <div class="marginTop10px">
            <a href="ShowMaps.aspx" class="button">Tillbaka</a>
            <asp:Button ID="SaveButton" runat="server" Text="Spara" OnClick="SaveButton_Click" ClientIDMode="Static" CssClass="green" />
            <asp:Button ID="DeletePropertyBoundariesButton" runat="server" Text="Ta bort fastighetsgränser" ClientIDMode="Static"
                OnClick="DeletePropertyBoundariesButton_Click" Visible="false" CssClass="red right" />
            <asp:Button ID="DeleteButton" runat="server" Text="Radera" OnClick="DeleteButton_Click" ClientIDMode="Static"
                Visible="false" CssClass="red right" />
        </div>
    </div>
</asp:Content>
<asp:Content ID="footer" ContentPlaceHolderID="footer" runat="server">
    <script type="text/javascript" src="/inc/js/tiny_mce/tiny_mce.js?ver=1.3"></script>
    <script type="text/javascript">
        $(function () {
            tinyMCE.init({
                // General options
                mode: "textareas",
                language: "sv",
                theme: "advanced",
                entity_encoding: "raw",
                plugins: "autolink,lists,style,advlink,inlinepopups,noneditable,wordcount",

                // Theme options
                theme_advanced_buttons1: "bold,italic,underline,strikethrough,formatselect,bullist,numlist,|,undo,redo,|,link,unlink",
                theme_advanced_buttons2: "",
                theme_advanced_buttons3: "",
                theme_advanced_buttons4: "",
                theme_advanced_toolbar_location: "top",
                theme_advanced_toolbar_align: "left",
                theme_advanced_statusbar_location: "bottom",
                theme_advanced_resizing: false,

                // Example content CSS (should be your site CSS)
                content_css: "/inc/css/base.css?ver=1.7"
            });

            if ($("#DeleteButton").length > 0) {
                $("#DeleteButton").click(function (e) {
                    if (confirm("Är du säker på att du vill radera kartan med alla dess versioner? Tryck Ok för att fortsätta med raderingen.")) {
                        showLoader('Raderar karta, vänligen vänta...');
                    } else {
                        e.preventDefault();
                    }
                });
            }

            if ($("#DeletePropertyBoundariesButton").length > 0) {
                $("#DeletePropertyBoundariesButton").click(function (e) {
                    if (!confirm("Är du säker på att du vill ta bort fastighetsgränser från kartan?")) {
                        e.preventDefault();
                    }
                });
            }
        });
    </script>
</asp:Content>
