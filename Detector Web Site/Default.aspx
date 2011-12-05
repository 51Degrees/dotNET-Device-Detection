<%@ Page Language="C#" AutoEventWireup="True" CodeBehind="Default.aspx.cs" Inherits="Detector._Default" EnableSessionState="True" EnableViewState="false" %>
<%@ Register Src="~/DeviceProperties.ascx" TagPrefix="uc" TagName="DeviceProperties" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Desktop / Laptop</title>
    <link runat="server" rel="stylesheet" type="text/css" href="~/default.css" />
    <link runat="server" rel="stylesheet" type="text/css" href="~/skin.css" />
</head>
<body>
    <form runat="server" class="normal">
    <asp:Panel runat="server" HorizontalAlign="Center">
        <asp:Label runat="server" Text="Desktop / Laptop" Font-Size="Large" Font-Bold="true" />
    </asp:Panel>
    <asp:Panel runat="server" HorizontalAlign="Center">
        <asp:Image runat="server" ImageUrl="~/Desktop.png" AlternateText="Desktop / Laptop" ImageAlign="Middle" />
    </asp:Panel>
    <asp:Panel runat="server" HorizontalAlign="Center">
        <asp:HyperLink runat="server" NavigateUrl="~/Mobile/Default.aspx" Text="Mobile Site" />
    </asp:Panel>
    <asp:Panel runat="server" HorizontalAlign="Center">
        <asp:HyperLink runat="server" NavigateUrl="~/Tablet/Default.aspx" Text="Tablet Site" />
    </asp:Panel>
    <asp:Panel runat="server" HorizontalAlign="Center">
        <asp:HyperLink runat="server" NavigateUrl="~/CheckUA.aspx" Text="Check a UserAgent" />
    </asp:Panel>
    <hr />
    <uc:DeviceProperties runat="server" ID="PropertiesDevice" />    
    </form>
</body>
</html>
