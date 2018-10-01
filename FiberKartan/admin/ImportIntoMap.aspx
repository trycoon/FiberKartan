<%@ Page Title="Fiber admin - Importera till karta" Language="C#" MasterPageFile="~/Admin/AdminMasterPage.Master"
    AutoEventWireup="true" CodeBehind="ImportIntoMap.aspx.cs" Inherits="FiberKartan.Admin.ImportIntoMap" %>

<asp:Content ID="head" ContentPlaceHolderID="head" runat="server">
</asp:Content>
<asp:Content ID="content" ContentPlaceHolderID="ContentPlaceHolder" runat="server">
    <div id="mapsImport">
        <fieldset>
            <legend>Information att importera</legend>
            <asp:RadioButton ID="markerAndLineImportSelectButton" GroupName="inputTypeRadioButton"
                Text="Fastigheter, grävsträckor, och områden" Checked="true" runat="server" AutoPostBack="true"
                OnCheckedChanged="InputTypeChanged" />
            <asp:RadioButton ID="propertyBoundariesImportSelectButton" GroupName="inputTypeRadioButton"
                Text="Fastighetsgränser" runat="server" AutoPostBack="true" OnCheckedChanged="InputTypeChanged" />
        </fieldset>
        <asp:Panel ID="ImportMarkerAndLineMapPanel" runat="server">
            <fieldset>
                <legend>Källa</legend>
                <div class="fileUpload">
                    <p class="center">
                        Välj den <strong>KML-fil</strong> som innehåller fastigheter, grävsträckor, och områden som skall
                        importeras.
                    </p>
                    <p class="dropZoneText center" style="display: none;">
                        Bläddra fram filen eller dra och släpp den här!</p>
                    <div class="fileupload-button">
                        <span class="button">Välj fil</span>
                        <asp:FileUpload ID="MarkerAndLineMapFileUpload" runat="server" ClientIDMode="Static" onchange="this.form.submit();" />
                    </div>
                </div>
            </fieldset>
        </asp:Panel>
        <asp:Panel ID="ImportPropertyBoundariesMapPanel" runat="server" Visible="false">
            <fieldset>
                <legend>Källa</legend>
                <div class="fileUpload">
                    <p class="center">
                        Välj den KML/KMZ-fil som innehåller fastighetsgränser och fastighetsbeteckningar som
                        skall importeras.
                    </p>
                    <p class="dropZoneText center" style="display: none;">
                        Bläddra fram filen eller dra och släpp den här!</p>
                    <div class="fileupload-button">
                        <span class="button">Välj fil</span>
                        <asp:FileUpload ID="PropertyBoundariesFileUpload" runat="server" ClientIDMode="Static" onchange="this.form.submit();" />
                    </div>
                </div>
            </fieldset>
        </asp:Panel>
        <asp:Panel ID="MarkerAndLineMapImportSelectionPanel" runat="server" Visible="false">
            <fieldset>
                <legend>Välj information att importera</legend>
                <p>Följande markörer hittades:</p>
                <asp:Repeater ID="markerTypes" runat="server" OnItemDataBound="MarkerTypesListBound" >
                    <HeaderTemplate>
                        <table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><%# Eval("NrFound")%> st</td>
                            <td><asp:Image ID="MarkerImage" runat="server" Width="32" Height="32" /></td>
                            <td>Tolka dessa som</td>
                            <td>
                                <asp:DropDownList ID="markerSelection" runat="server">
                                    <asp:ListItem Text="- Importera inte markörer -" Value="DontImport"></asp:ListItem>
                                    <asp:ListItem Text="Fastighet som vill ha fiber" Value="HouseYes"></asp:ListItem>
                                    <asp:ListItem Text="Fastighet som ännu inte har bestämt sig" Value="HouseMaybe"></asp:ListItem>
                                    <asp:ListItem Text="Fastighet som inte är intresserad av fiber" Value="HouseNo"></asp:ListItem>
                                    <asp:ListItem Text="Fastighet som ännu inte har kontaktats" Value="HouseNotContacted"></asp:ListItem>
                                    <asp:ListItem Text="Fibernod/överlämningspunkt" Value="FiberNode"></asp:ListItem>
                                    <asp:ListItem Text="Kopplingsskåp/kopplingsbrunn" Value="FiberBox"></asp:ListItem>
                                    <asp:ListItem Text="Befintlig väg/kanal-undergång" Value="RoadCrossing_Existing"></asp:ListItem>
                                    <asp:ListItem Text="Väg/kanal-undergång som måste göras" Value="RoadCrossing_ToBeMade"></asp:ListItem>
                                    <asp:ListItem Text="Fornlämning" Value="Fornlamning"></asp:ListItem>
                                    <asp:ListItem Text="Känslig plats(dränering, avlopp, elkabel, m.m.)" Value="Observe"></asp:ListItem>
                                    <asp:ListItem Text="Textruta" Value="Note"></asp:ListItem>
                                    <asp:ListItem Text="Okänd markör" Value="Unknown"></asp:ListItem>
                                </asp:DropDownList>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>                        
                        </table>
                        <asp:Panel ID="EmptyMarkerListLabel"
                             runat="server"
                             Visible="<%# bool.Parse((markerTypes.Items.Count == 0).ToString()) %>">
                             <p class="center redText">Inga markörer hittades.</p>
                        </asp:Panel>
                    </FooterTemplate>
                </asp:Repeater>
                <hr />
                <p>Följande linjer hittades:</p>
                <asp:Repeater ID="lineTypes" runat="server" OnItemDataBound="LineTypesListBound">
                    <HeaderTemplate>
                        <table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><%# Eval("NrFound")%> st</td>
                            <td><span>linjer.</span></td>
                            <td>Tolka dessa som</td>
                            <td>
                                <asp:DropDownList ID="linesSelection" runat="server">
                                    <asp:ListItem Text="- Importera inte linjer -" Value="DontImport"></asp:ListItem>
                                    <asp:ListItem Text="Grävsträckor" Value="0"></asp:ListItem>                                   
                                </asp:DropDownList>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>                        
                        </table>
                        <asp:Panel ID="EmptyLineListLabel"
                             runat="server"
                             Visible="<%# bool.Parse((lineTypes.Items.Count == 0).ToString()) %>">
                             <p class="center redText">Inga linjer hittades.</p>
                        </asp:Panel>
                    </FooterTemplate>
                </asp:Repeater>
                <hr />
                <p>Följande ytor hittades:</p>
                <asp:Repeater ID="polygonTypes" runat="server" OnItemDataBound="PolygonListBound" >
                    <HeaderTemplate>
                        <table>
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr>
                            <td><%# Eval("NrFound")%> st</td>
                            <td><span>ytor.</span></td>
                            <td>Tolka dessa som</td>
                            <td>
                                <asp:DropDownList ID="polygonSelection" runat="server">
                                    <asp:ListItem Text="- Importera inte ytor -" Value="DontImport"></asp:ListItem>
                                    <asp:ListItem Text="Områden" Value="0"></asp:ListItem>                                   
                                </asp:DropDownList>
                            </td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>                        
                        </table>
                        <asp:Panel ID="EmptyPolygonListLabel"
                             runat="server"
                             Visible="<%# bool.Parse((polygonTypes.Items.Count == 0).ToString()) %>">
                            <p class="center redText">Inga ytor hittades.</p>
                        </asp:Panel>
                    </FooterTemplate>
                </asp:Repeater>
            </fieldset>
            <fieldset>
                <legend>Sammanfoga</legend>
                <p>
                    Välj om den importerade informationen skall sammanfogas med en eventuell tidigare
                    version av kartan för att bilda en ny sammanfogad version.</p>
                <span>Sammanfoga med:
                    <asp:DropDownList ID="mergeVersionDropdown" runat="server"></asp:DropDownList>&nbsp;<asp:CheckBox ID="includeOldLinesCheckbox" runat="server" Checked="true" Enabled="false" Text="Inkludera gamla linjer" ToolTip="Inkludera linjer från den gamla kartan i sammanslagningen" />&nbsp;<asp:CheckBox ID="includeOldMarkersCheckbox" runat="server" Enabled="false" Checked="true" Text="Inkludera gamla markörer" ToolTip="Inkludera markörer från den gamla kartan i sammanslagningen" />
                </span>
            </fieldset>
        </asp:Panel>
        <asp:Panel ID="PropertyBoundariesImportSelectionPanel" runat="server" Visible="false">
        </asp:Panel>

        <asp:HiddenField ID="FilenameHiddenfield" runat="server" />
        <asp:Button ID="ImportButton" runat="server" CssClass="big red importButton" 
            Text="Importera" Visible="false" ClientIDMode="Static" onclick="ImportButton_Click" />

        <fk:ResultBox ID="ResultBox" runat="server" />
        <div class="marginTop10px">
            <a href="ShowMaps.aspx" class="button">Tillbaka</a>
        </div>
    </div>
</asp:Content>
<asp:Content ID="footer" ContentPlaceHolderID="footer" runat="server">
    <script src="/inc/js/importMap.js?ver=1.5"></script>
</asp:Content>
