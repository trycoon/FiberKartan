<%@ Page Title="Fiber admin - Lista användare" Language="C#" MasterPageFile="~/Admin/AdminMasterPage.Master"
    AutoEventWireup="true" CodeBehind="ListUsers.aspx.cs" Inherits="FiberKartan.Admin.ListUsers" %>

<asp:Content ID="head" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="content" ContentPlaceHolderID="ContentPlaceHolder" runat="server">
    <asp:GridView ID="UsersGridView" runat="server" AutoGenerateColumns="False" Width="100%"
        BackColor="White" BorderColor="#DEDFDE" BorderStyle="Solid" BorderWidth="1px"
        CellPadding="4" DataSourceID="SqlDataSource1" ForeColor="Black" GridLines="Vertical"
        EmptyDataText="Inga användare finns" ShowFooter="True" AllowPaging="True" PageSize="30"
        AllowSorting="True" DataKeyNames="Id">
        <AlternatingRowStyle BackColor="White" />
        <PagerSettings Mode="NumericFirstLast" FirstPageText="Första" LastPageText="Sista"
            NextPageText="Nästa" PreviousPageText="Föregående" PageButtonCount="10" />
        <PagerStyle CssClass="gvPagerCss" />
        <Columns>
            <asp:BoundField DataField="Id" HeaderText="Id" SortExpression="Id" ReadOnly="True"
                Visible="False" />
            <asp:BoundField DataField="Name" HeaderText="Namn" SortExpression="Name" ReadOnly="True"/>
            <asp:BoundField DataField="Username" HeaderText="Användarnamn" ReadOnly="True" SortExpression="Username">
                <ItemStyle Wrap="False" />
            </asp:BoundField>
            <asp:TemplateField HeaderText="Spärrad" SortExpression="IsDeleted" ItemStyle-Width="60px" ItemStyle-HorizontalAlign="Center">
                <ItemTemplate><%# (Boolean.Parse(Eval("IsDeleted").ToString())) ? "X" : "" %></ItemTemplate>
            </asp:TemplateField>
            <asp:BoundField DataField="Description" HeaderText="Förening/företag" SortExpression="Description" />
            <asp:BoundField DataField="Created" HeaderText="Skapad" ReadOnly="True" SortExpression="Created" ItemStyle-HorizontalAlign="Center">
                <ItemStyle Wrap="False" />
            </asp:BoundField>
            <asp:BoundField DataField="LastActivity" HeaderText="Senast ansluten" ReadOnly="True" SortExpression="LastActivity" ItemStyle-HorizontalAlign="Center">
                <ItemStyle Wrap="False" />
            </asp:BoundField>
            <asp:BoundField DataField="Online" HeaderText="Ansluten" SortExpression="Online">
                <ItemStyle Wrap="False" />
            </asp:BoundField>
            <asp:TemplateField ItemStyle-Width="70px">
                <ItemTemplate>
                    <asp:HyperLink ID="editUserButton" runat="server" CssClass="button" NavigateUrl='<%# "EditUser.aspx?uid=" + Eval("Id") %>'
                        ToolTip="Redigera användare" Text="Redigera" />
                </ItemTemplate>
            </asp:TemplateField>            
        </Columns>
        <FooterStyle BackColor="#CCCC99" />
        <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="White" />
        <PagerStyle BackColor="#F7F7DE" ForeColor="Black" HorizontalAlign="Right" />
        <RowStyle BackColor="#F7F7DE" />
        <SelectedRowStyle BackColor="#CE5D5A" Font-Bold="True" ForeColor="White" />
        <SortedAscendingCellStyle BackColor="#FBFBF2" />
        <SortedAscendingHeaderStyle BackColor="#848384" />
        <SortedDescendingCellStyle BackColor="#EAEAD3" />
        <SortedDescendingHeaderStyle BackColor="#575357" />
    </asp:GridView>
    <asp:HyperLink ID="BackButton" ClientIDMode="Static" runat="server" CssClass="button" Text="Tillbaka" />
    <asp:HyperLink ID="newUserButton" runat="server" CssClass="button green" NavigateUrl='EditUser.aspx'
        ToolTip="Skapa användare" Text="Ny användare" />
    <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:FiberDBConnectionString %>"
        SelectCommand="SELECT Id, Name, Username, IsDeleted, Description, Created, LastActivity, coalesce((SELECT 'Aktiv' WHERE DATEADD(ss, 15, LastActivity) >= GETDATE()), '') AS Online FROM [User] ORDER BY Description ASC">
    </asp:SqlDataSource>
</asp:Content>
<asp:Content ID="footer" ContentPlaceHolderID="footer" runat="server">
</asp:Content>
