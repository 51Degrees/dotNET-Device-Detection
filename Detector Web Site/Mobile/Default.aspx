<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Detector.Mobile.Default" EnableSessionState="False" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Mobile</title>
    <link runat="server" rel="stylesheet" type="text/css" href="~/default.css" />
    <link runat="server" rel="stylesheet" type="text/css" href="~/skin.css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Panel runat="server" HorizontalAlign="Center">
            <asp:Label Runat="server" Alignment="Center" Font-Size="Large" Font-Bold="true" Text="Mobile" />
        </asp:Panel>
        <asp:Panel runat="server" HorizontalAlign="Center">
            <asp:Image Runat="server" Alignment="Center" ImageUrl="~/Mobile.png" AlternateText="Mobile Device" />
        </asp:Panel>
        <asp:Panel runat="server" HorizontalAlign="Center">
            <asp:Label ID="LabelManufacturer" Runat="server" Alignment="Center"/>
        </asp:Panel>
        <asp:Panel runat="server" HorizontalAlign="Center">
            <asp:Label ID="LabelModel" Runat="server" Alignment="Center"/>
        </asp:Panel>
        <asp:Panel runat="server" HorizontalAlign="Center">
            <asp:Label ID="LabelUserAgent" Runat="server" Alignment="Center"/>
        </asp:Panel>
        <asp:Panel runat="server" HorizontalAlign="Center">
            <asp:HyperLink ID="LinkDesktop" Runat="server" Text="Desktop Site" NavigateUrl="~/Default.aspx" />
        </asp:Panel>
    </div>
    </form>
</body>
</html>
