<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="FiberKartan.Default" %>

<!doctype html>
<!--[if lt IE 7]> <html class="no-js lt-ie9 lt-ie8 lt-ie7" lang="sv"> <![endif]-->
<!--[if IE 7]>    <html class="no-js lt-ie9 lt-ie8" lang="sv"> <![endif]-->
<!--[if IE 8]>    <html class="no-js lt-ie9" lang="sv"> <![endif]-->
<!--[if gt IE 8]><!--><html class="no-js" lang="sv"><!--<![endif]-->
    <head runat="server">
        <meta http-equiv="Content-type" content="text/html; charset=utf-8"/>        
        <meta name="viewport" content="width=device-width" />
	    <meta name="author" content="&copy;Liquidbytes.se, Henrik Östman" />
        <meta name="description" content="Projekteringsverktyg för fiber till socken och byanät" />
        <meta name="keywords" content="fiber,karta,bredband,sockenmodell,byanät,nätverk,fibernät,projekteringsverktyg,gis" />
        <title>FiberKartan - Projekteringsverktyg för fiber till socken och byanät</title>
        <link rel="stylesheet" href="/inc/css/firstpage.css?ver=1.0" media="screen" />
        <link rel="stylesheet" href="/inc/js/fancybox/jquery.fancybox.css?ver=1.0" media="screen" />
        <!-- Google Analytics -->
        <script>
            window.ga=window.ga||function(){(ga.q=ga.q||[]).push(arguments)};ga.l=+new Date;
            ga('create', 'UA-33911019-1', 'auto');
            ga('send', 'pageview');
        </script>
        <script async src='https://www.google-analytics.com/analytics.js'></script>
        <!-- End Google Analytics -->
    </head>
    <body class="dark">
        <form id="default" runat="server" onsubmit="return false;" enableviewstate="false">
            <article>
                <header>
                    <h1 class="text-center">FiberKartan - Projekteringsverktyg för fiber till socken och byanät</h1>
                </header>
                <p>
                    Verktyget är framtaget för fiberföreningar som följer socken- eller byanätsmodellen och som på ett enkelt sätt vill projektera för hur fiberkabel skall dras i området och vilka fastigheter som skall anslutas till fibernätverket.
                    FiberKartan består av en publik karta som man kan länka till ifrån föreningens hemsida och som pressenterar projekteringen på ett överskådligt sätt för intressenter och besökare. FiberKartan består även av en administrativ del avsedd för föreningens styrelse och föreningens entreprenörer där mer detaljerad information och verktyg för att rita och ändra på kartorna finns.
                    FiberKartan bygger på Google Maps, men utan många av dess brister och med ett utseende som är anpassat för fiberprojektering. <strong>Verktyget är GRATIS för fiberföreningar att använda.</strong> Är ni intresserade av att använda verktyget, kontakta oss på följande <a href="mailto:joachim.pettersson@brsnetworks.se?subject=Fiberkartan" target="_blank">e-postadress</a>.
                </p>
                <div class="image">
                    <a rel="group1" href="/inc/img/firstpage/ViewAdminMap.jpg" target="_blank" title="Ritläge"><img src="/inc/img/firstpage/tn_ViewAdminMap.jpg" alt="Ritläge" /></a>
                </div>
                <div class="text-center">
                    <a href="/region">Publika kartor</a>
                </div>
                <footer>
                    <div class="text-center">&copy;Liquidbytes.se, Henrik Östman</div>
                </footer>
            </article>
        </form>
        <script src="/inc/js/jquery.min.js?ver=1.1"></script>
        <script src="/inc/js/fancybox/jquery.fancybox.pack.js?ver=1.0"></script>
        <script>
            $(document).ready(function() {
                $("a.image").fancybox();
            });
        </script>
    </body>
</html>
