<%@ Page Language="C#" AutoEventWireup="True" Inherits="Detector.M" Codebehind="M.aspx.cs" %>
<%@ Register TagPrefix="mobile" Namespace="System.Web.UI.MobileControls" Assembly="System.Web.Mobile" %>

<html xmlns="http://www.w3.org/1999/xhtml" >
<body>
    <mobile:Form runat="server" Title="Mobile Device">
        <mobile:Label Runat="server" Alignment="Center" Font-Size="Large" Text="Mobile Device"/>
        <mobile:Image Runat="server" Alignment="Center" ImageUrl="~/PDA.gif" AlternateText="Mobile Device"/>
        <mobile:Label ID="LabelManufacturer" Runat="server" Alignment="Center" Font-Size="Normal"/>
        <mobile:Label ID="LabelModel" Runat="server" Alignment="Center" Font-Size="Normal"/>
        <mobile:Label ID="LabelUserAgent" Runat="server" Alignment="Center" Font-Size="Normal"/>
        <mobile:Link ID="LinkDesktop" Runat="server" Text="Desktop Site" NavigateUrl="~/Default.aspx" Alignment="Center"/>
    </mobile:Form>
</body>
</html>