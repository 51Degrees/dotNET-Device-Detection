<%@ Page Language="C#" AutoEventWireup="True" MasterPageFile="~/Detector.Master" CodeBehind="Default.aspx.cs" Inherits="Detector._Default" EnableSessionState="True" EnableViewState="false" %>
<%@ OutputCache VaryByParam="*" VaryByHeader="User-Agent" Duration="240" Location="Any" %>
<%@ Register Src="~/DeviceProperties.ascx" TagPrefix="uc" TagName="DeviceProperties" %>

<asp:Content runat="server" ID="Head" ContentPlaceHolderID="head">
    <title>Desktop / Laptop</title>
</asp:Content>

<asp:Content runat="server" ID="Body" ContentPlaceHolderID="body">
    <div style="text-align: center;">
        <div style="margin: 0px auto; display: inline;">
            <img src="<% =ResolveClientUrl("~/Desktop.png") %>" alt="Desktop / Laptop" style="vertical-align: middle;"/>
            <span style="vertical-align: middle; font-size: 2em; font-weight: bold;">Desktop / Laptop</span>
        </div>
    </div>
    <uc:DeviceProperties runat="server" ID="Device" />
</asp:Content>
