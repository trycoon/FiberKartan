<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="ResultBox.ascx.cs" Inherits="FiberKartan.ResultBox" %>
<asp:Panel runat="server" ID="msgTable" BackColor="#fff4f1" BorderColor="Red" BorderWidth="1" Style="margin: 5px 0px 5px 0px;">
    <div style="vertical-align: middle; margin:10px;">
        <asp:Label runat="server" ID="lblError" ForeColor="Red" Font-Bold="true" Text="" />
    </div>
    <script>
        setTimeout(function() {
            if ($('#ResultBox_msgTable').length > 0) {
                $('#ResultBox_msgTable').hide('slow');
            }
            if ($('#ContentPlaceHolder_ResultBox_msgTable').length > 0) {
                $('#ContentPlaceHolder_ResultBox_msgTable').hide('slow');
            }            
        }, 5000);
    </script>
</asp:Panel>
