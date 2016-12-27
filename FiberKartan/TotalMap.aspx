<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="TotalMap.aspx.cs" Inherits="FiberKartan.TotalMap" %>

<!doctype html>
<!--[if lt IE 7]> <html class="no-js lt-ie9 lt-ie8 lt-ie7" lang="sv"> <![endif]-->
<!--[if IE 7]>    <html class="no-js lt-ie9 lt-ie8" lang="sv"> <![endif]-->
<!--[if IE 8]>    <html class="no-js lt-ie9" lang="sv"> <![endif]-->
<!--[if gt IE 8]><!--><html class="no-js" lang="sv"><!--<![endif]-->
    <head runat="server">
        <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1" >
        <meta charset="utf-8" >        
        <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no" >
	    <meta name="author" content="&copy;Liquidbytes.se, Henrik Östman" >
        <meta name="description" content="FiberKartan - Projekteringsverktyg för socken och byanät" >
        <meta name="keywords" content="fiberkarta,bredband,sockenmodell,byanät,nätverk,fibernät,projekteringsverktyg" >
        <%: System.Web.Optimization.Styles.Render("~/inc/css/userCss") %>
        <link rel="Stylesheet" href="/inc/css/map_print.css?ver=1.1" media="print" />
        <script src="http://maps.google.com/maps/api/js?v=3&libraries=geometry"></script>
        <script>var fk = fk || {};</script>
        <!-- Google Analytics -->
        <script>
            window.ga=window.ga||function(){(ga.q=ga.q||[]).push(arguments)};ga.l=+new Date;
            ga('create', 'UA-33911019-1', 'auto');
            ga('send', 'pageview');
        </script>
        <script async src='https://www.google-analytics.com/analytics.js'></script>
        <!-- End Google Analytics -->
    </head>
    <body>
        <form id="mapForm" runat="server"  enableviewstate="false" onsubmit="return false;">
            <div id="map_canvas" style="width:100%; height:100%"></div>        
            <div id="mainPalette" class="palette" runat="server">
                <div class="paletteheader" title="Dra här för att flytta paletten.">Regional karta</div>
                <asp:Panel id="connectionStatisticsPanel" runat="server"> 
                    Fastigheter:&nbsp;<strong><asp:Literal ID="NrHouses" runat="server"></asp:Literal></strong>&nbsp;st                    
                </asp:Panel>
                <asp:Panel id="totalDigLengthStatisticsPanel" runat="server"> 
                    Uppskattad schaktl&auml;ngd: <strong><span id="totalDigLength"></span></strong>&nbsp;meter                    
                </asp:Panel>
                
                <fieldset>
                    <legend>Visa</legend>
                    <div>
                        <input id="show_houses" name="show_houses" type="checkbox" value="show_houses" /><label for="show_houses">Fastigheter</label>
                    </div>
                    <div>
                        <input id="show_crossings" name="show_crossings" type="checkbox" value="show_crossings" /><label for="show_crossings">Väg/kanal-undergångar</label>
                    </div>
                    <div>
                        <span id="myCurrentPosition"><input id="myCurrentPositionCheckbox" type="checkbox" value="myCurrentPositionCheckbox" /><label for="myCurrentPositionCheckbox">Min nuvarande position (kräver GPS)</label></span>
                    </div>
                </fieldset>
                <div class="viewSettingsBox">
                    Anpassa f&ouml;nster efter
                    <select id="viewSettings" name="viewSettings">
                        <option selected="selected" value="screen" >Bildskärm</option>
                        <option value="a0" >A0-papper</option>
                        <option value="a1" >A1-papper</option>
                        <option value="a2" >A2-papper</option>
                        <option value="a3" >A3-papper</option>
                        <option value="a4" >A4-papper</option>
                    </select>
                    <div>
                        <label>Liggande<input type="checkbox" id="viewSettingsHorizontal" /></label>
                    </div>
                </div>
                <input id="print_map" type="button" value="Skriv ut" onclick="window.print()" />
                <a href="#" id="mapInfoIcon" class="paletteInfoIcon" title="Kartinformation"><div></div></a>
                <a href="#" id="resetMapButton" class="paletteInfoIcon" title="Återställer kartvyn till ursprungsläget."><div></div></a>
                <a href="#" id="snapshotButton" class="paletteInfoIcon" title="Skapar ett e-postmeddelade med en länk till nuvarande kartvy." target="_blank"><div></div></a> 
            </div>
        </form>
        <%: System.Web.Optimization.Scripts.Render("~/inc/js/regionJs") %>
    </body>
</html>
