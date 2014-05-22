<%@ Page Language="C#" AutoEventWireup="True" MasterPageFile="~/Detector.Master" CodeBehind="Redirect.aspx.cs" Inherits="Detector.Redirect" EnableSessionState="True" EnableViewState="false" %>

<asp:Content runat="server" ID="Body" ContentPlaceHolderID="body">
    <fiftyOne:Redirect runat="server" ID="Settings" FooterEnabled="false" />
</asp:Content>
