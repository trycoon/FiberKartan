<%@ Page Title="Fiber admin - Visa kartversioner" Language="C#" MasterPageFile="~/Admin/AdminMasterPage.Master"
    AutoEventWireup="true" CodeBehind="ShowMapVersions.aspx.cs" Inherits="FiberKartan.Admin.ShowMapVersions" %>

<%@ Import Namespace="FiberKartan" %>
<asp:Content ID="head" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="content" ContentPlaceHolderID="ContentPlaceHolder" runat="server">
    <div id="mapVersions">
        <asp:GridView ID="MapVersionsGridView" runat="server" AutoGenerateColumns="False"
            Width="100%" BackColor="White" BorderColor="#DEDFDE" BorderStyle="Solid" BorderWidth="1px"
            CellPadding="4" DataSourceID="SqlDataSource1" ForeColor="Black" GridLines="Vertical"
            EmptyDataText="Inga versioner finns" ShowFooter="True" AllowPaging="True" PageSize="30"
            AllowSorting="True" DataKeyNames="MapTypeId,Ver">
            <AlternatingRowStyle BackColor="White" />
            <PagerSettings Mode="NumericFirstLast" FirstPageText="Första" LastPageText="Sista"
                NextPageText="Nästa" PreviousPageText="Föregående" PageButtonCount="10" />
            <PagerStyle CssClass="gvPagerCss" />
            <Columns>
                <asp:TemplateField ItemStyle-Width="50px" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate>
                        <a class="directMapUrl" href='<%# "/" + Eval("MapTypeId") + "/" + Eval("Ver")%>'
                            title="Direktlänk">Länk</a>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="MapTypeId" HeaderText="MapTypeId" SortExpression="MapTypeId"
                    ReadOnly="True" Visible="False" />
                <asp:BoundField DataField="Ver" HeaderText="Version" SortExpression="Ver" ReadOnly="True"
                    ItemStyle-Width="44px" />
                <asp:BoundField DataField="Created" HeaderText="Skapad" ReadOnly="True" SortExpression="Created"
                    ItemStyle-Width="114px">
                    <ItemStyle Wrap="False" />
                </asp:BoundField>
                <asp:TemplateField HeaderText="Publicerad" SortExpression="Published" ItemStyle-Width="90px" ItemStyle-HorizontalAlign="Center">
                    <ItemTemplate><%# String.IsNullOrEmpty(Eval("Published").ToString()) ? "" : "X" %></ItemTemplate>
                </asp:TemplateField>
                <asp:BoundField DataField="Creator" HeaderText="Skapad av" SortExpression="Creator" />
                <asp:BoundField DataField="Views" HeaderText="Antal visningar" ReadOnly="True" SortExpression="Views"
                    ItemStyle-Width="58px" />
                <asp:TemplateField ItemStyle-Width="60px" ItemStyle-HorizontalAlign="Center">
                    <HeaderTemplate>
                        <a id="showLastMapVersionLink" class="button" href='<%=UrlToLastMapVersion() %>'
                            title="Visa senaste version av kartan.">Senaste</a>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <asp:HyperLink ID="ShowMapLink" runat="server" CssClass="button" NavigateUrl='<%# "/admin/MapAdmin.aspx?mid=" + Eval("MapTypeId") + "&ver=" + Eval("Ver")%>'
                            ToolTip="Visa version av kartan." Text="Visa" />
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField ItemStyle-Width="70px">
                    <ItemTemplate>
                        <asp:HyperLink ID="exportButton" runat="server" CssClass="button exportButton" NavigateUrl='<%# "/admin/ExportMap.aspx?mid=" + Eval("MapTypeId") + "&ver=" + Eval("Ver")%>'
                            Visible='<%# ((MapAccessRights)Eval("AccessRight")).HasFlag(MapAccessRights.Export) %>'
                            ToolTip="Exportera karta" Text="Exportera" />
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
        <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:FiberDBConnectionString %>"
            SelectCommand="SELECT m.[MapTypeId], m.[Ver], m.[Created], m.[Views], (SELECT u.[Name] FROM [User] u WHERE u.[Id] = m.[CreatorId]) as Creator, m.[Published], (SELECT mtar.[AccessRight] FROM [MapTypeAccessRight] mtar INNER JOIN [MapType] mt ON mt.[Id]=mtar.[MapTypeId] WHERE mt.[Id] = m.[MapTypeId] AND mtar.[UserId] = @userId) as AccessRight FROM [Map] m WHERE (m.[MapTypeId] = @MapTypeId) ORDER BY m.[Ver] DESC"
            OnSelecting="SqlDataSource_Selecting">
            <SelectParameters>
                <asp:QueryStringParameter Name="MapTypeId" QueryStringField="mid" Type="Int32" />
                <asp:Parameter Name="userId" Type="Int32" />
            </SelectParameters>
        </asp:SqlDataSource>
        <a href="ShowMaps.aspx" class="button">Tillbaka</a>
    </div>
</asp:Content>
<asp:Content ID="footer" ContentPlaceHolderID="footer" runat="server">
    <script type="text/javascript">
        $(document).ready(function () {
            $(document).on('click', '.exportButton', function (e) {
                e.preventDefault();
                var page = $(this).attr("href")
                var pagetitle = $(this).attr("title")
                var $dialog = $('<div></div>')
                    .html('<iframe style="border: 0px; " src="' + page + '" width="100%" height="100%"></iframe>')
                    .dialog({
                        autoOpen: false,
                        title: pagetitle,
                        close: function () { $(this).remove() },
                        width: 485,
                        height: 560,
                        modal: true,
                        resizable: false,
                        dialogClass: 'buttons-centered',
                        open: function (event, ui) {
                            $('.ui-dialog-content').css('overflow', 'hidden');
                        }
                    });
                $dialog.dialog('open');
            });
            $(document).on('click', '.directMapUrl', function (e) {
                e.preventDefault();
                var $dialog = $('<div></div>')
                    .html('För att länka till denna kartversion, använd följande adress:<br/><br/><strong><%=ServerAddress%>' + $(this).attr("href") + '</strong>')
                    .dialog({
                        autoOpen: false,
                        title: $(this).attr("title"),
                        close: function () { $(this).remove() },
                        width: 500,
                        modal: true,
                        resizable: false,
                        dialogClass: 'buttons-centered',
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
