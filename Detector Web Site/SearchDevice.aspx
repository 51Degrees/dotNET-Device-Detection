<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="SearchDevice.aspx.cs" Inherits="Detector.SearchDevice" %>

<%@ Register Assembly="FiftyOne.Foundation" Namespace="FiftyOne.Foundation.UI.Web"
    TagPrefix="fiftyOne" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <fiftyOne:DeviceFinder runat="server" ID="finder" />
    </div>
    </form>
</body>
</html>
