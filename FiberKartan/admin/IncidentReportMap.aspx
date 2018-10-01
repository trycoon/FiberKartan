<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="IncidentReportMap.aspx.cs" Inherits="FiberKartan.Admin.IncidentReportMap" %>

<!doctype html>
<!--[if lt IE 7]> <html class="no-js lt-ie9 lt-ie8 lt-ie7" lang="sv"> <![endif]-->
<!--[if IE 7]>    <html class="no-js lt-ie9 lt-ie8" lang="sv"> <![endif]-->
<!--[if IE 8]>    <html class="no-js lt-ie9" lang="sv"> <![endif]-->
<!--[if gt IE 8]><!-->
<html class="no-js" lang="sv">
<!--<![endif]-->
    <head runat="server">
        <meta http-equiv="Content-type" content="text/html; charset=utf-8"/>        
        <meta name="viewport" content="width=device-width, initial-scale=1.0, user-scalable=no" >
	    <meta name="author" content="&copy;Liquidbytes.se, Henrik Östman" >
        <meta name="description" content="FiberKartan - Projekteringsverktyg för socken och byanät" >
        <meta name="keywords" content="fiberkarta,bredband,sockenmodell,byanät,nätverk,fibernät,projekteringsverktyg" >
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
        <form id="mapForm" runat="server" enableviewstate="false" onsubmit="return false;">
            <div id="map_canvas" style="width: 100%; height: 100%"></div>
            <div id="buttonBar" class="incidentButtonBar center alpha60" runat="server" visible="true">
                <a href="ShowMaps.aspx" class="button">Tillbaka</a>
            </div>
        </form>
        <script src="/inc/js/tiny_mce/tiny_mce.js?ver=1.3"></script>
        <%: System.Web.Optimization.Scripts.Render("~/inc/js/incidentReportJs") %>
        <%: System.Web.Optimization.Scripts.Render("~/inc/views") %>
        <script async defer src="https://maps.googleapis.com/maps/api/js?v=weekly&libraries=geometry,drawing&key=AIzaSyD_YkeofIsttEGex9GJfRzoI9FGxVKWt7M&callback=initMap"></script>
    </body>
</html>
