<%@ Page Language="C#" AutoEventWireup="True" Inherits="Detector.M" Codebehind="M.aspx.cs" %>

<html xmlns="http://www.w3.org/1999/xhtml" >
<head>
<title>Mobile page</title>
</head>
<body>
    <form id="form1" runat="server" style="text-align:center">
        <asp:Label ID="isMobile" Runat="server" Font-Size="Large" Text="Mobile Device" style="text-align:center"/>
        <asp:Image ID="image1" Runat="server" ImageUrl="~/PDA.gif" AlternateText="Mobile Device"/>
        <asp:Label ID="LabelManufacturer" Runat="server" Font-Size="Medium"  style="text-align:center"/>
        <asp:Label ID="LabelModel" Runat="server" Font-Size="Medium" style="text-align:center"/>
        <asp:Label ID="LabelUserAgent" Runat="server" Font-Size="Medium" style="text-align:center"/>
        <asp:LinkButton ID="LinkDesktop" Runat="server" Text="Desktop Site" PostBackUrl="~/Default.aspx" style="text-align:center"/>
    </form>
</body>
</html>