<%@ Page Title="" Language="C#" MasterPageFile="~/Detector.Master" AutoEventWireup="true" CodeBehind="Gallery.aspx.cs" Inherits="Detector.Gallery" %>
<%@ Import Namespace="System.IO" %>
<asp:Content ID="Content2" ContentPlaceHolderID="body" runat="server">
<asp:DataList runat="server" ID="Images" RepeatColumns="3" RepeatDirection="Horizontal" RepeatLayout="Table" CssClass="gallery">
    <ItemStyle Width="33.3%" />
    <ItemTemplate>
        <a href='<%# String.Format("GalleryImage.aspx?Image={0}", ResolveUrl(Path.Combine("~/Gallery", Eval("name").ToString()))) %>' style="max-width: 200px">
            <% if (Request.Browser["JavascriptImageOptimiser"] is string)
                { %>
                <img src='<%# ResolveUrl("~/Empty.gif") %>' data-src='<%# ResolveUrl(Path.Combine("~/Gallery", Eval("name").ToString())) %>?w=auto' />
            <% }
                else
                { %>
                <img src='<%# ResolveUrl(Path.Combine("~/Gallery", Eval("name").ToString())) %>?w=200' />
            <% } %>
        </a>
    </ItemTemplate>
</asp:DataList>
</asp:Content>
