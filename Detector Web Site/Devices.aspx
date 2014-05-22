<%@ Page Title="" Language="C#" MasterPageFile="~/Detector.Master" AutoEventWireup="true" CodeBehind="Devices.aspx.cs" Inherits="Detector.Devices" %>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
<fiftyOne:LiteMessage runat="server" ID="Message" FooterEnabled="false" LogoEnabled="false" />
<fiftyOne:DeviceExplorer runat="server" ID="DeviceExplorer" CssClass="deviceExplorer" Navigation="true" ImagesEnabled="true" LogoEnabled="true" FooterEnabled="false"/>
</asp:Content>