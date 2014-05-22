<%@ Page Language="C#" AutoEventWireup="True" MasterPageFile="~/Detector.Master" CodeBehind="Network.aspx.cs" Inherits="Detector.Network" EnableSessionState="True" EnableViewState="false" %>

<asp:Content runat="server" ID="Body" ContentPlaceHolderID="body">
        <% if (Request.Browser["JavascriptBandwidth"] != null)
           { %>
            <div style="clear: both">
                <p>Last Response Time: <% =String.Format("{0:ss\\.fff}s", Context.Items["51D_LastResponseTime"])%></p>
                <p>Last Completion Time: <% =String.Format("{0:ss\\.fff}s", Context.Items["51D_LastCompletionTime"])%></p>
                <p>Average Response Time: <% =String.Format("{0:ss\\.fff}s", Context.Items["51D_AverageResponseTime"])%></p>
                <p>Average Completion Time: <% =String.Format("{0:ss\\.fff}s", Context.Items["51D_AverageCompletionTime"])%></p>
                <p>Average Bandwidth: <% =String.Format("{0:n0}bps", Context.Items["51D_AverageBandwidth"])%></p>
            </div>
        <% } %>
</asp:Content>
