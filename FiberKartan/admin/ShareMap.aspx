<%@ Page Title="Fiber admin - Dela karta med andra" Language="C#" MasterPageFile="~/Admin/AdminMasterPage.Master"
    AutoEventWireup="true" CodeBehind="ShareMap.aspx.cs" Inherits="FiberKartan.Admin.ShareMap" %>

<asp:Content ID="head" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="content" ContentPlaceHolderID="ContentPlaceHolder" runat="server">
    <div id="shareMap">
        <asp:GridView ID="ShareMapGridView" runat="server" AutoGenerateColumns="False" Width="100%"
            BackColor="White" BorderColor="#DEDFDE" BorderStyle="Solid" BorderWidth="1px"
            CellPadding="4" DataKeyNames="UserId" DataSourceID="SqlDataSource" AllowSorting="False"
            ForeColor="Black" GridLines="Vertical" EmptyDataText="Kartan är inte delad med någon annan."
            OnRowCommand="ShareMapGridView_RowCommand">
            <AlternatingRowStyle BackColor="White" />
            <Columns>
                <asp:BoundField DataField="UserId" HeaderText="AnvändarId" SortExpression="AnvändarId"
                    ReadOnly="true" Visible="false"></asp:BoundField>
                <asp:BoundField DataField="Name" HeaderText="Namn" SortExpression="Name" ItemStyle-Width="20%">
                </asp:BoundField>
                <asp:BoundField DataField="Description" HeaderText="Beskrivning" SortExpression="Description"
                    ItemStyle-Width="30%"></asp:BoundField>
                <asp:TemplateField HeaderText="Rättighet" ItemStyle-Width="30%">
                    <ItemTemplate>
                        <asp:DropDownList ID="AccessRight" runat="server" SelectedValue='<%# Eval("AccessRight") %>'>
                            <asp:ListItem Value="1">1. Granska kartversioner</asp:ListItem>
                            <asp:ListItem Value="3">2. Exportera kartinformation (och ovan)</asp:ListItem>
                            <asp:ListItem Value="7">3. Ändra på karta (och ovan)</asp:ListItem>
                            <asp:ListItem Value="15">4. Fullständiga rättigheter</asp:ListItem>
                        </asp:DropDownList>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ItemStyle-Width="20%">
                    <ItemTemplate>
                        <asp:Button ID="SaveButton" runat="server" Text="Spara" CssClass="button" CommandName="SaveChanges"
                            CommandArgument='<%# Eval("UserId") %>' />
                        <asp:Button ID="RemoveButton" runat="server" Text="Ta bort" CssClass="button removeButton"
                            CommandName="RemoveUserRight" CommandArgument='<%# Eval("UserId") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="White" />
            <RowStyle BackColor="#F7F7DE" />
        </asp:GridView>
        <asp:SqlDataSource ID="SqlDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:FiberDBConnectionString %>"
            SelectCommand="SELECT u.[Id] as UserId, u.[Name] as Name, u.[Description] as Description, mtar.[AccessRight] as AccessRight FROM [MapTypeAccessRight] mtar INNER JOIN [User] u ON u.[Id ]= mtar.[UserId] WHERE mtar.[MapTypeId] = @MapTypeId AND mtar.[UserId] <> @userId"
            OnSelecting="SqlDataSource_Selecting">
            <SelectParameters>
                <asp:QueryStringParameter Name="MapTypeId" QueryStringField="mid" Type="Int32" />
                <asp:Parameter Name="userId" Type="Int32" />
            </SelectParameters>
        </asp:SqlDataSource>
        <div id="newAccessRight">
            <fieldset>
                <legend>Lägg till användare</legend>
                <label id="UsernameLabel" runat="server" for="NewMapAccessEmail">
                    E-postadress</label><br />
                <asp:TextBox ID="NewMapAccessEmail" runat="server" MaxLength="50" AutoCompleteType="Email"
                    type="email" />
                <asp:DropDownList ID="NewAccessRight" runat="server">
                    <asp:ListItem Value="1">1. Granska kartversioner</asp:ListItem>
                    <asp:ListItem Value="3">2. Exportera kartinformation (och ovan)</asp:ListItem>
                    <asp:ListItem Value="7">3. Ändra på karta (och ovan)</asp:ListItem>
                    <asp:ListItem Value="15">4. Fullständiga rättigheter</asp:ListItem>
                </asp:DropDownList>
                <asp:Button ID="NewAccessButton" runat="server" Text="Skicka inbjudan" CssClass="button"
                    OnClick="NewAccessButton_Click" />
            </fieldset>
        </div>
    </div>
    <fk:ResultBox ID="ResultBox" runat="server" />
    <div class="marginTop10px">
        <asp:HyperLink ID="BackButton" runat="server" NavigateUrl="ShowMaps.aspx" CssClass="button"
            Text="Tillbaka" />
    </div>
    <script type="text/javascript">
        if ($(".removeButton").length > 0) {
            $(".removeButton").click(function (e) {
                if (!confirm("Är du säker på att du vill ta bort rättigheterna för denna användare? Tryck Ok för att ta bort rättigheter.")) {
                    e.preventDefault();
                }
            });
        }
    </script>
</asp:Content>
