<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExportMap.aspx.cs" Inherits="FiberKartan.Admin.ExportMap" %>

<!DOCTYPE html>
<html lang="sv" class="dialog">
    <head id="Head1" runat="server">
        <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1" >
        <title>Fiber admin - Exportera karta</title>
        <meta name="viewport" content="width=device-width" >
        <meta name="author" content="&copy;Liquidbytes.se, Henrik Östman" >
	    <meta http-equiv="content-type" content="text/html;charset=UTF-8" >
        <link rel="Stylesheet" type="text/css" href="/inc/css/base.css?ver=1.7" media="screen" />
        <link rel="Stylesheet" type="text/css" href="/inc/css/admin.css?ver=2.2" media="screen" />
    </head>
    <body>
        <form id="exportForm" runat="server">
            <fieldset>
                <legend>Fastigheter</legend>
                <input id="HouseYesBox" type="checkbox" value="HouseYesBox" checked="checked" runat="server" /><label for="HouseYesBox">Ja</label><br />
                <input id="HouseMaybeBox" type="checkbox" value="HouseMaybeBox" checked="checked" runat="server" /><label for="HouseMaybeBox">Kanske</label><br />
                <input id="HouseNoBox" type="checkbox" value="HouseNoBox" checked="checked" runat="server" /><label for="HouseNoBox">Nej</label><br />
                <input id="HouseNotContactedBox" type="checkbox" value="HouseNotContactedBox" checked="checked" runat="server" /><label for="HouseNotContactedBox">Ej svarat</label>
            </fieldset>
            <fieldset>
                <legend>Fibernätverk</legend>                
                <input id="FiberNodeBox" type="checkbox" value="FiberNodeBox" checked="checked" runat="server" /><label for="FiberNodeBox">Fibernoder</label><br />
                <input id="FiberBoxBox" type="checkbox" value="FiberBoxBox" checked="checked" runat="server" /><label for="FiberBoxBox">Kopplingsskåp</label><br />
                <input id="FiberCableBox" type="checkbox" value="FiberCableBox" checked="checked" runat="server" /><label for="FiberCableBox">Schaktsträckor</label><br />
            </fieldset>
            <fieldset>
                <legend>Övrigt</legend>               
                <input id="RoadCrossing_ExistingBox" type="checkbox" value="RoadCrossing_ExistingBox" checked="checked" runat="server" /><label for="RoadCrossing_ExistingBox">Befintliga väggenomgångar</label><br />
                <input id="RoadCrossing_ToBeMadeBox" type="checkbox" value="RoadCrossing_ToBeMadeBox" checked="checked" runat="server" /><label for="RoadCrossing_ToBeMadeBox">Väggenomgångar som skall skapas</label><br />
                <input id="FornlamningBox" type="checkbox" value="FornlamningBox" checked="checked" runat="server" /><label for="FornlamningBox">Fornlämningar</label><br />
                <input id="ObserveBox" type="checkbox" value="ObserveBox" checked="checked" runat="server" /><label for="ObserveBox">Känsliga platser</label><br />
                <input id="NoteBox" type="checkbox" value="NoteBox" checked="checked" runat="server" /><label for="NoteBox">Textrutor</label><br />
                <input id="RegionBox" type="checkbox" value="RegionBox" checked="checked" runat="server" /><label for="RegionBox">Områden</label><br />
            </fieldset>
            <br />
            <fieldset>
                <legend>Till format</legend>
                <asp:RadioButtonList id="exportFile" runat="server">
                    <asp:ListItem Selected="true" Value="kml">KML (Google Earth)</asp:ListItem>
                    <asp:ListItem Value="excel">MS Excel</asp:ListItem>
                </asp:RadioButtonList>
            </fieldset>
            <br />
            <asp:Button ID="ExportButton" runat="server" Text="Exportera" CssClass="button" onclick="ExportButton_Click" />
        </form>
        <script type="text/javascript" src="/inc/js/jquery.min.js?ver=1.1"></script>
        <script type="text/javascript" src="/inc/js/base.js?ver=1.4"></script>
        <script type="text/javascript">
            var _gaq = _gaq || [];
            _gaq.push(['_setAccount', 'UA-33911019-1']);
            _gaq.push(['_setDomainName', 'fiberkartan.se']);
            _gaq.push(['_gat._anonymizeIp']);
            _gaq.push(['_trackPageview']);

            (function () {
                var ga = document.createElement('script'); ga.type = 'text/javascript'; ga.async = true;
                ga.src = ('https:' == document.location.protocol ? 'https://ssl' : 'http://www') + '.google-analytics.com/ga.js';
                var s = document.getElementsByTagName('script')[0]; s.parentNode.insertBefore(ga, s);
            })();

            // Startar ping-funktion.
            setInterval(function () {
                $.get("../REST/FKService.svc/Ping");
            }, 5000);
	    </script>
    </body>
</html>