<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LogTable.aspx.cs" Inherits="Detector.Table" ValidateRequest="false" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Log</title>
    <link id="Link1" runat="server" rel="stylesheet" type="text/css" href="~/default.css" />
    <link id="Link2" runat="server" rel="stylesheet" type="text/css" href="~/skin.css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <h4>51Degrees.mobi Foundation Log</h4>
        This page demonstrates how to view the Azure log table. To compile insert 'AZURE' as compilation symbol in the build tab of 
        the Detector and Foundation project properties.
        <br />
        <table>
            <tr>
                <td>
                    <asp:TextBox ID="OutBox" runat="server" Height="432px" TextMode="MultiLine" Width="633px"></asp:TextBox>
                </td>
                <td>
                    <asp:Button ID="DelAllBut" runat="server" OnClick="DelAllBut_Click" Text="Delete All" />
                </td>
            </tr>
        </table>
    </div>
    </form>
</body>
</html>
