<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="Default.aspx.cs" Inherits="Detector.Tablet.Default" EnableSessionState="True" EnableViewState="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Tablet</title>
    <link runat="server" rel="stylesheet" type="text/css" href="~/default.css" />
    <link runat="server" rel="stylesheet" type="text/css" href="~/skin.css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Panel runat="server" HorizontalAlign="Center">
            <asp:Label runat="server" Text="Tablet" Font-Size="Large" Font-Bold="true" />
        </asp:Panel>
        <asp:Panel runat="server" HorizontalAlign="Center">
            <asp:Image Runat="server" Alignment="Center" ImageUrl="~/Tablet.png" AlternateText="Tablet Device" />
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
