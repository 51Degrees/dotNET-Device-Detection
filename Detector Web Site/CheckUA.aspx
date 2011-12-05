<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CheckUA.aspx.cs" Inherits="Detector.CheckUA" %>
<%@ Register Src="~/DeviceProperties.ascx" TagPrefix="uc" TagName="DeviceProperties" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Check a UserAgent String</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Panel runat="server" GroupingText="Test Data">
            <asp:Panel runat="server">
                <asp:Label runat="server" Text="Enter UserAgent string to check:" />
                <asp:TextBox runat="server" ID="TextBoxUA" Width="640" />
            </asp:Panel>
            <asp:Panel runat="server">
                <asp:Button runat="server" UseSubmitBehavior="true" Text="Submit" />
            </asp:Panel>
        </asp:Panel>
        <uc:DeviceProperties runat="server" ID="PropertiesDevice" />
    </div>
    </form>
</body>
</html>
