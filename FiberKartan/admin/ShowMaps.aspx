<%@ Page Title="Fiber admin - Visa kartor" Language="C#" MasterPageFile="~/Admin/AdminMasterPage.Master"
    AutoEventWireup="true" CodeBehind="ShowMaps.aspx.cs" Inherits="FiberKartan.admin.ShowMaps" %>

<%@ Import Namespace="FiberKartan" %>
<asp:Content ID="head" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="content" ContentPlaceHolderID="ContentPlaceHolder" runat="server">
    <div id="maps">
        <asp:GridView ID="MapGridView" runat="server" AutoGenerateColumns="False" Width="100%"
            BackColor="White" BorderColor="#DEDFDE" BorderStyle="Solid" BorderWidth="1px"
            CellPadding="4" DataKeyNames="Id" DataSourceID="SqlDataSource" AllowSorting="True"
            ForeColor="Black" GridLines="Vertical" EmptyDataText="Inga kartor finns" ShowFooter="True"
            OnRowCommand="MapGridView_ItemCommand">
            <AlternatingRowStyle BackColor="White" />
            <Columns>
                <asp:TemplateField ItemStyle-Width="80px" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <a class="directMapUrl" href='<%# "/" + Eval("Id")%>' title="Direktlänk">Länk</a>
                        <asp:ImageButton ID="SubscribeMapChanges" runat="server" CssClass="marginLeft5px"
                            ImageUrl="~/inc/img/email_add.png" ToolTip="Prenumerera på ändringar av kartan"
                            CommandName="SubscribeMapChanges" CommandArgument='<%# Eval("Id") %>' Visible='<%# !((bool)Eval("EmailSubscribeChanges")) %>' />
                        <asp:ImageButton ID="UnSubscribeMapChanges" runat="server" CssClass="marginLeft5px"
                            ImageUrl="~/inc/img/email_remove.png" ToolTip="Avsluta prenumeration på ändringar av kartan"
                            CommandName="UnSubscribeMapChanges" CommandArgument='<%# Eval("Id") %>' Visible='<%# ((bool)Eval("EmailSubscribeChanges")) %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Title" HeaderText="Namn/Sidtitel" SortExpression="Title">
                </asp:BoundField>
                <asp:BoundField DataField="Creator" HeaderText="Skapad av" SortExpression="Creator">
                    <ItemStyle Width="150px" />
                </asp:BoundField>
                <asp:TemplateField ItemStyle-Width="110px" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <asp:Button ID="ShowVersion" runat="server" Text="Visa versioner" ToolTip="Visa versioner av kartan"
                            CommandName="ShowVersion" CommandArgument='<%# Eval("Id") %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ItemStyle-Width="80px" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <asp:HyperLink ID="ImportMapLink" runat="server" CssClass="importMapButton button"
                            NavigateUrl='<%# "/admin/ImportIntoMap.aspx?mid=" + Eval("Id") %>' ToolTip="Importera information till karta"
                            Text="Importera" Visible='<%# ((MapAccessRights)Eval("AccessRight")).HasFlag(MapAccessRights.Write) %>' />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ItemStyle-Width="200px" ItemStyle-HorizontalAlign="Left">
                    <ItemTemplate>
                        <asp:HyperLink ID="EditMap" runat="server" CssClass="editMapButton button" NavigateUrl='<%# "/admin/EditMap.aspx?mid=" + Eval("Id") %>'
                            ToolTip="Redigera karta" Text="Redigera" Visible='<%# ((MapAccessRights)Eval("AccessRight")).HasFlag(MapAccessRights.Write) %>' />
                        <asp:HyperLink ID="ShareMap" runat="server" CssClass="marginLeft5px" NavigateUrl='<%# "/admin/ShareMap.aspx?mid=" + Eval("Id") %>'
                            ToolTip="Dela karta med andra" Text="Dela" Visible='<%# ((MapAccessRights)Eval("AccessRight")).HasFlag(MapAccessRights.FullAccess) %>' />
                        <asp:HyperLink ID="ReportIncident" runat="server" CssClass="marginLeft5px" NavigateUrl='<%# "/admin/IncidentReportMap.aspx?mid=" + Eval("Id") %>'
                            ToolTip="Rapportera fel på nätverk" Text="Rapportera fel" Visible='<%# (((MapAccessRights)Eval("AccessRight")).HasFlag(MapAccessRights.FullAccess) && (Eval("HasServiceCompany").Equals(1))) %>' />
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
        <asp:SqlDataSource ID="SqlDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:FiberDBConnectionString %>"
            SelectCommand="SELECT mt.[Id], mt.[Title], mt.[MapUrl], CASE WHEN mt.[ServiceCompanyId] is not null THEN 1 ELSE 0 END [HasServiceCompany], (SELECT u.[Name] FROM [User] u WHERE u.[Id] = mt.CreatorId) as Creator, mtar.[AccessRight], mtar.[EmailSubscribeChanges] FROM [MapTypeAccessRight] mtar INNER JOIN [MapType] mt ON mt.[Id]=mtar.[MapTypeId] WHERE (mtar.[UserId] = @userId) AND (mtar.[AccessRight] > 0) ORDER BY mt.[Title], mt.[Id]"
            OnSelecting="SqlDataSource_Selecting">
            <SelectParameters>
                <asp:Parameter Name="userId" Type="Int32" />
            </SelectParameters>
        </asp:SqlDataSource>
        <a id="newMapButton" class="button" href="/admin/EditMap.aspx" title="Skapa en ny tom karta">
            Ny karta</a>
    </div>
</asp:Content>
<asp:Content ID="footer" ContentPlaceHolderID="footer" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            $(document).on('click', '.directMapUrl', function (e) {
                e.preventDefault();
                var $dialog = $('<div></div>')
                .html('För att länka till denna karta, använd följande adress:<br/><br/><strong><%=ServerAddress%>' + $(this).attr("href") + '</strong><br/><br/>Adress som HTML länk:<br/><br/><strong>&lt;a href=\"<%=ServerAddress%>' + $(this).attr("href") + '\" target=\"_blank\"&gt;Karta&lt;/a&gt;</strong>')
                .dialog({
                    autoOpen: false,
                    title: $(this).attr("title"),
                    close: function () { $(this).remove() },
                    position: 'center',
                    width: 520,
                    modal: true,
                    resizable: false,
                    buttons: {
                        Ok: function () {
                            $(this).dialog("close");
                        }
                    }
                });
                $dialog.dialog('open');
            });
        });
    </script>
</asp:Content>
