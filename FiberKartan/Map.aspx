﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Map.aspx.cs" Inherits="FiberKartan.Map" %>

<!doctype html>
<!--[if lt IE 7]> <html class="no-js lt-ie9 lt-ie8 lt-ie7" lang="sv"> <![endif]-->
<!--[if IE 7]>    <html class="no-js lt-ie9 lt-ie8" lang="sv"> <![endif]-->
<!--[if IE 8]>    <html class="no-js lt-ie9" lang="sv"> <![endif]-->
<!--[if gt IE 8]><!--><html class="no-js" lang="sv"><!--<![endif]-->
    <head runat="server">
        <meta http-equiv="Content-type" content="text/html; charset=utf-8"/>        
        <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no" >
        <meta name="author" content="&copy;Liquidbytes.se, Henrik Östman" >
        <meta name="apple-mobile-web-app-capable" content="yes">
        <meta name="apple-mobile-web-app-status-bar-style" content="black">
        <%: System.Web.Optimization.Styles.Render("~/inc/css/userCss") %>
        <link rel="Stylesheet" href="/inc/css/map_print.css?ver=1.1" media="print" />
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
        <form id="mapForm" runat="server" onsubmit="return false;" enableviewstate="false">
            <div id="map_canvas" style="width:100%; height:100%"></div>        
            <div id="mainPalette" class="palette" runat="server" visible="false">
                <div class="paletteheader" title="Dra här för att flytta paletten.">Publik karta</div>
                <asp:Panel id="connectionStatisticsPanel" runat="server" visible="false">
                    <div>
                        Fastigheter som &auml;r intresserade:&nbsp;<strong><asp:Literal ID="NumberOfIntresstedLiteral" runat="server"></asp:Literal></strong>st
                    </div>
                    <div>
                        Fastigheter som inte &auml;r intresserade:&nbsp;<strong><asp:Literal ID="NumberOfNotIntresstedLiteral" runat="server"></asp:Literal></strong>st
                    </div>
                    <div>
                        Anslutningsgraden &auml;r&nbsp;<strong><asp:Literal ID="ConnectionRatioLiteral" runat="server"></asp:Literal>%</strong>
                    </div>
                </asp:Panel>
                <asp:Panel id="totalDigLengthStatisticsPanel" runat="server" visible="false"> 
                    Uppskattad schaktl&auml;ngd: <strong><span id="totalDigLength"></span></strong>&nbsp;meter                    
                </asp:Panel>

                <fieldset>
                    <legend>Visa</legend>
                        <div>
                            <input id="show_house_to_install" name="show_house_to_install" type="checkbox" value="show_house_to_install" checked="checked" /><label for="show_house_to_install">Fastigheter, JA eller KANSKE</label>
                        </div>
                        <div>
                            <input id="show_house_no_dice" name="show_house_no_dice" type="checkbox" value="show_house_no_dice" checked="checked" /><label for="show_house_no_dice">Fastigheter, NEJ eller Ej svarat</label>
                        </div>
                        <div>
                            <input id="show_network" name="show_network" type="checkbox" value="show_network" />
                            <label for="show_network">Fibern&auml;tverk</label>
                            <div id="subOption_fibernodes" class="subOption">
                                <input id="show_fibernodes" name="show_fibernodes" type="checkbox" value="show_fibernodes" />
                                <label for="show_fibernodes">Noder</label>
                            </div>
                            <div id="subOption_fiberboxes" class="subOption">
                                <input id="show_fiberboxes" name="show_fiberboxes" type="checkbox" value="show_fiberboxes" />
                                <label for="show_fiberboxes">Kopplingsskåp</label>
                            </div>
                        </div>
                        <div>
                            <input id="show_crossings" name="show_crossings" type="checkbox" value="show_crossings" /><label for="show_crossings">Väg/kanal-undergångar</label>
                        </div>
                        <div>
                            <input id="show_regions" name="show_regions" type="checkbox" value="show_regions" /><label for="show_regions">Omr&aring;den</label>
                        </div>
                        <div>
                            <span id="myCurrentPosition"><input id="myCurrentPositionCheckbox" name="myCurrentPositionCheckbox" type="checkbox" value="myCurrentPositionCheckbox" /><label for="myCurrentPositionCheckbox">Min nuvarande position (kräver GPS)</label></span>
                        </div>
                        <div id="propertyBoundaries">
                            <input id="show_propertyBoundaries" name="show_propertyBoundaries" type="checkbox" value="show_propertyBoundaries" />
                            <label for="show_propertyBoundaries">Visa fastighetsgränser</label>
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
        <%: System.Web.Optimization.Scripts.Render("~/inc/js/userJs") %>
        <script async defer src="https://maps.googleapis.com/maps/api/js?v=weekly&libraries=geometry&key=AIzaSyD_YkeofIsttEGex9GJfRzoI9FGxVKWt7M&callback=initMap"></script>
    </body>
</html>
