<%@ Page Language="C#" AutoEventWireup="True" MasterPageFile="~/Detector.Master" CodeBehind="Default.aspx.cs" Inherits="Detector.Tablet.Default" EnableSessionState="True" EnableViewState="false" %>
<%@ Register Src="~/DeviceProperties.ascx" TagPrefix="uc" TagName="DeviceProperties" %>

<asp:Content runat="server" ID="Head" ContentPlaceHolderID="head">
    <title>Tablet</title>
</asp:Content>

<asp:Content runat="server" ID="Body" ContentPlaceHolderID="body">
    <div style="text-align: center;">
        <div style="margin: 0px auto; display: inline;">
            <img src="<% =ResolveClientUrl("~/Tablet.png") %>" alt="Tablet" style="vertical-align: middle;"/>
            <span style="vertical-align: middle; font-size: 2em; font-weight: bold;">Tablet</span>
        </div>
    </div>
    <uc:DeviceProperties runat="server" ID="Device" />
</asp:Content>
