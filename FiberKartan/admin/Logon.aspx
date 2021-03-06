﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Logon.aspx.cs" Inherits="FiberKartan.Admin.Logon" %>

<!DOCTYPE html>
<html xml:lang="sv" xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <meta http-equiv="X-UA-Compatible" content="IE=edge,chrome=1" >
        <title>Fiber admin - Logga in</title>
        <meta name="viewport" content="width=device-width" >
        <meta name="author" content="&copy;Liquidbytes.se, Henrik Östman" >
	    <meta http-equiv="content-type" content="text/html;charset=UTF-8" >
        <link rel="Stylesheet" href="/inc/css/base.css?ver=1.8" media="screen" />
        <link rel="Stylesheet" href="/inc/css/admin.css?ver=2.3" />
        <!-- Google Analytics -->
        <script>
            window.ga=window.ga||function(){(ga.q=ga.q||[]).push(arguments)};ga.l=+new Date;
            ga('create', 'UA-33911019-1', 'auto');
            ga('send', 'pageview');
        </script>
        <script async src='https://www.google-analytics.com/analytics.js'></script>
        <!-- End Google Analytics -->
    </head>
    <body  class="dark">
        <form id="loginForm" runat="server">
            <asp:Panel id="loginBox" runat="server">
                <h1><asp:Literal ID="title" runat="server" Text="Ange inloggningsuppgifter" /></h1>
                <p>
                    <label ID="usernameLabel" class="block" runat="server" for="username">E-postadress</label>
                    <asp:TextBox ID="username" runat="server" MaxLength="50" type="email" />
                </p>
                <p>
                    <label ID="passwordLabel" class="block" runat="server" for="password">Lösenord</label>
                    <asp:TextBox ID="password" runat="server" TextMode="Password" MaxLength="100" />
                </p>
                <p ID="repeatPasswordSection" runat="server" visible="false">
                    <label ID="passwordLabel2" class="block" runat="server" for="passwordLabel2">Upprepa lösenord</label>
                    <asp:TextBox ID="password2" runat="server" TextMode="Password" MaxLength="100" />
                </p>
                <fk:ResultBox ID="ResultBox" runat="server" />
                <asp:Button ID="loginButton" runat="server" Text="Logga in" onclick="loginButton_Click" />
                <asp:CheckBox ID="rememberMeCheckBox" runat="server" Text="Kom ihåg mig" />
                <asp:Button ID="savePasswordButton" runat="server" Text="Spara" Visible="false" onclick="savePasswordButton_Click" />
            </asp:Panel>
            <asp:Panel id="newUserBox" runat="server" Visible="false">
                <h1>Skapa konto</h1>
                <p>
                    Välkommen till FiberKartan, innan du kan börja arbeta med kartorna så måste vi skapa ett konto till dig.
                    Fyll i nedanstående uppgifter för att skapa ett konto.
                </p>
                <p>
                    <label for="name" class="block">Namn&nbsp;(förnamn efternamn)</label>
                    <asp:TextBox ID="name" runat="server" MaxLength="100" CssClass="inputField" ToolTip='T.ex. "Göran Svensson"' />
                </p>
                <p>
                    <label for="description" class="block">Beskrivning&nbsp;(ange namn på fiberförening eller företag)</label>
                    <asp:TextBox ID="description" runat="server" MaxLength="200" CssClass="inputField" ToolTip='T.ex. "Vallstena fiber" eller "Tottes Gräv Ab"' />
                </p>
                <p>
                    <label for="newUserPassword" class="block">Önskat lösenord</label>
                    <asp:TextBox ID="newUserPassword" runat="server" TextMode="Password" MaxLength="100" />                    
                </p>
                <p>
                    <label for="newUserPassword2" class="block">Upprepa lösenord</label>
                    <asp:TextBox ID="newUserPassword2" runat="server" TextMode="Password" MaxLength="100" />
                </p>
                <fk:ResultBox ID="NewUserResultBox" runat="server" />
                <div class="center">
                    <asp:Button ID="createUserButton" runat="server" Text="Skapa konto" onclick="createUserButton_Click" />
                </div>
            </asp:Panel>
        </form>
        <script src="/inc/js/jquery.min.js?ver=1.1"></script>
        <script src="/inc/js/jquery-ui.min.js?ver=1.1"></script>
        <script src="/inc/js/base.js?ver=1.5"></script>
        <script>
            $(document).ready(function() {
                $('#loginButton').click(function(e) {
                    showLoader("Loggar in");
                });
            });
	    </script>
    </body>
</html>
