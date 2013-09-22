<%@ Page Title="Fiber admin - Redigera användare" Language="C#" MasterPageFile="~/Admin/AdminMasterPage.Master"
    AutoEventWireup="true" CodeBehind="EditUser.aspx.cs" Inherits="FiberKartan.Admin.EditUser" %>

<asp:Content ID="head" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="content" ContentPlaceHolderID="ContentPlaceHolder" runat="server">
    <asp:Panel ID="editUserPanel" runat="server">
        <label for="Name" class="block">
            Namn (förnamn efternamn)</label>
        <asp:TextBox ID="Name" ToolTip="För- och efternamn på användaren." MaxLength="255"
            Width="100%" TextMode="SingleLine" runat="server"></asp:TextBox>
        <label for="Username" class="block">
            Användarnamn (e-postadress)</label>
        <asp:TextBox ID="Username" ToolTip="Användarnamn (e-postadress)" MaxLength="50" Width="100%"
            TextMode="SingleLine" runat="server"></asp:TextBox>
        <label for="Description" class="block">
            Fiberförening eller företagsnamn</label>
        <asp:TextBox ID="Description" MaxLength="200" Width="100%" TextMode="SingleLine"
            runat="server"></asp:TextBox>
        <asp:Panel ID="AdminPanel" class="whiteText marginTop10px" Visible="false" runat="server">
            <fieldset>
                <legend>Adminpanel</legend>
                <asp:CheckBox ID="IsDisabled" Text="Spärrad" runat="server"></asp:CheckBox>
                <p>
                    Konto skapat:
                    <asp:Literal ID="CreatedDate" runat="server"></asp:Literal></p>
                <p>
                    Senast inloggad:
                    <asp:Literal ID="LastLoggedOn" runat="server"></asp:Literal></p>
                <p>
                    Senast aktiv:
                    <asp:Literal ID="LastActivity" runat="server"></asp:Literal></p>
                <asp:HyperLink ID="ListUsersButton" runat="server" NavigateUrl="ListUsers.aspx" CssClass="button" Text="Lista användare"
                    ToolTip="Lista alla användare i systemet" />
                <asp:Button ID="ResetPasswordButton" runat="server" ClientIDMode="Static" Text="Nollställ lösenord"
                    ToolTip="Nollställer användarens lösenord, vid nästa inloggning ombeds dom att sätta ett nytt"
                    OnClick="ResetPasswordButton_Click" />
            </fieldset>
        </asp:Panel>
    </asp:Panel>
    <fk:ResultBox ID="ResultBox" runat="server" />
    <div class="marginTop10px">
        <asp:HyperLink ID="BackButton" ClientIDMode="Static" runat="server" NavigateUrl="ShowMaps.aspx"
            CssClass="button" Text="Tillbaka" /><asp:Button ID="SaveButton" runat="server" Text="Spara"
                OnClick="SaveButton_Click" CssClass="green" />
    </div>
    <script>
        $('#BackButton').click(function (e) {
            e.preventDefault();
            history.back();
        });
    </script>
</asp:Content>
