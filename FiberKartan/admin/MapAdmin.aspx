<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="MapAdmin.aspx.cs" Inherits="FiberKartan.Admin.MapAdmin" %>

<!doctype html>
<!-- paulirish.com/2008/conditional-stylesheets-vs-css-hacks-answer-neither/ -->
<!--[if lt IE 7]> <html class="no-js lt-ie9 lt-ie8 lt-ie7" lang="sv"> <![endif]-->
<!--[if IE 7]>    <html class="no-js lt-ie9 lt-ie8" lang="sv"> <![endif]-->
<!--[if IE 8]>    <html class="no-js lt-ie9" lang="sv"> <![endif]-->
<!--[if gt IE 8]><!--><html class="no-js" lang="sv"><!--<![endif]-->
    <head runat="server">
        <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1" >
        <meta charset="utf-8" >        
        <meta name="viewport" content="width=device-width" >
	    <meta name="author" content="&copy;Liquidbytes.se, Henrik Östman" >
        <%= WebExtensions.CombresLink("adminCss")%>
        <link rel="Stylesheet" type="text/css" href="/inc/css/map_print.css?ver=1.1" media="print" />        
        <script src="http://maps.google.com/maps/api/js?v=3&sensor=false&libraries=geometry,drawing"></script>
        <script>var fk = fk || {};</script>
    </head>
    <body>
        <form id="mapForm" runat="server" enableviewstate="false" onsubmit="return false;">
            <div id="map_canvas" style="width:100%; height:100%"></div>        
            <div id="mainPalette" class="palette" runat="server" visible="false">
                <div class="paletteheader admin" title="Dra här för att flytta paletten.">Administrativ karta</div>
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
                
                <div id="togglePanels">                    
                    <h3><a href="#">Visa</a></h3>
                    <div class="showPanel">
                        <div>
                            <input id="show_house_to_install" name="show_house_to_install" type="checkbox" value="show_house_to_install" checked="checked" />
                            <label for="show_house_to_install">Fastigheter, JA eller KANSKE</label>
                        </div>
                        <div>
                            <input id="show_house_no_dice" name="show_house_no_dice" type="checkbox" value="show_house_no_dice" checked="checked" />
                            <label for="show_house_no_dice">Fastigheter, NEJ eller Ej svarat</label>
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
                            <div id="subOption_fiberboxbindings" class="subSubOption">
                                <input id="show_fiberboxbindings" name="show_fiberboxbindings" type="checkbox" value="show_fiberboxbindings" disabled="disabled" title="Funktionen är tyvärr inaktiverad i väntan på att Google fixar en bugg i deras karttjänst." />
                                <label for="show_fiberboxbindings">Kopplingsskåpsnumrering</label>
                            </div>
                        </div>
                        <div>
                            <input id="show_crossings" name="show_crossings" type="checkbox" value="show_crossings" />
                            <label for="show_crossings">Väg/kanal-undergångar</label>
                        </div>
                        <div>
                            <input id="show_regions" name="show_regions" type="checkbox" value="show_regions" />
                            <label for="show_regions">Omr&aring;den</label>
                        </div>
                        <div id="myCurrentPosition">
                            <input id="myCurrentPositionCheckbox" name="myCurrentPositionCheckbox" type="checkbox" value="myCurrentPositionCheckbox" />
                            <label for="myCurrentPositionCheckbox">Min nuvarande position (kräver GPS)</label>
                        </div>
                        <div id="propertyBoundaries">
                            <input id="show_propertyBoundaries" name="show_propertyBoundaries" type="checkbox" value="show_propertyBoundaries" />
                            <label for="show_propertyBoundaries">Visa fastighetsgränser</label>
                        </div>
                    </div> 
                    <h3><a href="#">Markörer</a></h3>
                    <div id="markerTypes" class="hidden" runat="server">
                        <img title="Dra ut markör för att lägga till, fastighet som vill ha fiber." src="../inc/img/markers/HouseYes.png" data-markerType="HouseYes" />
                        <img title="Dra ut markör för att lägga till, fastighet som ännu inte har bestämt sig." src="../inc/img/markers/HouseNotDecided.png" data-markerType="HouseMaybe" />
                        <img title="Dra ut markör för att lägga till, fastighet som ännu inte har kontaktats." src="../inc/img/markers/HouseNotContacted.png" data-markerType="HouseNotContacted" />
                        <img title="Dra ut markör för att lägga till, fastighet som inte är intresserad av fiber." src="../inc/img/markers/HouseNo.png" data-markerType="HouseNo" />
                        <img title="Dra ut markör för att lägga till, fibernod/överlämningspunkt." src="../inc/img/markers/FiberNode.png" data-markerType="FiberNode" />
                        <img title="Dra ut markör för att lägga till, kopplingsskåp." src="../inc/img/markers/FiberBox.png" data-markerType="FiberBox" />
                        <img title="Dra ut markör för att lägga till, befintlig väg/kanal-undergång." src="../inc/img/markers/RoadCrossing_Existing.png" data-markerType="RoadCrossing_Existing" />
                        <img title="Dra ut markör för att lägga till, väg/kanal-undergång som måste göras." src="../inc/img/markers/RoadCrossing_ToBeMade.png" data-markerType="RoadCrossing_ToBeMade" />
                        <img title="Dra ut markör för att lägga till, fornlämning." src="../inc/img/markers/Fornlamning.png" data-markerType="Fornlamning" />
                        <img title="Dra ut markör för att lägga till, känslig plats(dränering, avlopp, elkabel, m.m.)." src="../inc/img/markers/Observe.png" data-markerType="Observe" />
                        <img title="Dra ut markör för att lägga till, info-ruta." src="../inc/img/markers/Note.png" data-markerType="Note" />
                    </div>
                    <h3><a href="#">Sök</a></h3>
                    <div class="searchPanel hidden">                        
                        <input id="searchBox" maxlength="100" /><input id="clearSearchBox" class="button" value="Rensa" />
                    </div>
                    <h3><a href="#">Utskrift</a></h3>
                    <div class="viewSettingsBox hidden">
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
                            <div id="print_map" class="button" onclick="window.print()" >Skriv ut</div>
                        </div>
                    </div>
                </div>                
                <div class="palette_footer">
                    <asp:HyperLink id="backButton" runat="server" CssClass="button" Text="Tillbaka" title="Tillbaka till versionsväljaren" />
                    <input id="saveButton" type="button" runat="server" value="Spara" title="Spara ändringar av kartan" class="button big red" />
                    <a href="#" id="mapInfoIcon" class="paletteInfoIcon" title="Kartinformation"><div></div></a>
                    <a href="#" id="resetMapButton" class="paletteInfoIcon" title="Återställer kartvyn till ursprungsläget."><div></div></a>
                    <a href="#" id="snapshotButton" class="paletteInfoIcon" title="Skapar ett e-postmeddelade med en länk till nuvarande kartvy." target="_blank"><div></div></a> 
                </div>                
            </div>
            <!-- Contextmenyer -->
            <div id="contextMenuLinePlaceholder"></div>
            <div id="contextMenuRegionPlaceholder"></div>
            <!-- Slut Contextmenyer -->
        </form>
        <script src="/inc/js/tiny_mce/tiny_mce.js?ver=1.3"></script>
        <script>
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
	    </script>
        <%= WebExtensions.CombresLink("adminJs")%>
    </body>
</html>
