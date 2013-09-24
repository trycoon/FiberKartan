﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="IncidentReportMap.aspx.cs" Inherits="FiberKartan.Admin.IncidentReportMap" %>

<!doctype html>
<!--[if lt IE 7]> <html class="no-js lt-ie9 lt-ie8 lt-ie7" lang="sv"> <![endif]-->
<!--[if IE 7]>    <html class="no-js lt-ie9 lt-ie8" lang="sv"> <![endif]-->
<!--[if IE 8]>    <html class="no-js lt-ie9" lang="sv"> <![endif]-->
<!--[if gt IE 8]><!-->
<html class="no-js" lang="sv">
<!--<![endif]-->
    <head runat="server">
        <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1" >
        <meta charset="utf-8" >        
        <meta name="viewport" content="width=device-width" >
	    <meta name="author" content="&copy;Liquidbytes.se, Henrik Östman" >
        <meta name="description" content="FiberKartan - Projekteringsverktyg för socken och byanät" >
        <meta name="keywords" content="fiberkarta,bredband,sockenmodell,byanät,nätverk,fibernät,projekteringsverktyg" >
        <%= WebExtensions.CombresLink("userCss")%>
        <link rel="Stylesheet" type="text/css" href="/inc/css/map_print.css?ver=1.1" media="print" />
        <script type="text/javascript" src="http://maps.google.com/maps/api/js?v=3&sensor=false&libraries=geometry,drawing"></script>
    </head>
    <body>
        <form id="mapForm" runat="server" enableviewstate="false" onsubmit="return false;">
            <div id="map_canvas" style="width: 100%; height: 100%">
            </div>
        </form>
        <script src="/inc/js/tiny_mce/tiny_mce.js?ver=1.3"></script>
        <%= WebExtensions.CombresLink("incidentReportJs")%>
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
        </script>
    </body>
</html>