<%@ Control Language="C#" AutoEventWireup="True" CodeBehind="DeviceProperties.ascx.cs" Inherits="Detector.DeviceProperties" %>
<%@ Register Assembly="FiftyOne.Foundation" Namespace="FiftyOne.Foundation.UI.Web" TagPrefix="fiftyOne" %>

<div class="tabs">
    <asp:Button runat="server" ID="CurrentButton" CommandName="Tab" CommandArgument="CurrentView" OnCommand="TabChange" Text="Current Device" PostBackUrl="~/Default.aspx" />
    <asp:Button runat="server" ID="ExplorerButton" CommandName="Tab" CommandArgument="ExplorerView" OnCommand="TabChange" Text="Device Explorer" PostBackUrl="~/Default.aspx" />
    <asp:Button runat="server" ID="TopButton" CommandName="Tab" CommandArgument="TopView" OnCommand="TabChange" Text="Top Devices" PostBackUrl="~/Default.aspx" />
    <asp:Button runat="server" ID="DictionaryButton" CommandName="Tab" CommandArgument="DictionaryView" OnCommand="TabChange" Text="Dictionary" PostBackUrl="~/Default.aspx" />
    <asp:Button runat="server" ID="UserAgentTesterButton" CommandName="Tab" CommandArgument="UserAgentTesterView" OnCommand="TabChange" Text="Test UserAgent" PostBackUrl="~/Default.aspx" />
    <asp:Button runat="server" ID="RedirectButton" CommandName="Tab" CommandArgument="RedirectView" OnCommand="TabChange" Text="Redirect" PostBackUrl="~/Default.aspx" />
    <asp:Button runat="server" ID="DetectionButton" CommandName="Tab" CommandArgument="DetectionView" OnCommand="TabChange" Text="Detection" PostBackUrl="~/Default.aspx"/>
    <asp:Button runat="server" ID="StandardPropertiesButton" CommandName="Tab" CommandArgument="StandardPropertiesView" OnCommand="TabChange" Text="Standard Properties" PostBackUrl="~/Default.aspx" />
</div>

<div class="border">
<asp:MultiView runat="server" ID="Tabs" ActiveViewIndex="0">
    <asp:View runat="server" ID="CurrentView">
        <fiftyOne:DeviceExplorer runat="server" ID="Current" Navigation="false" CmsCssClass="cms" />
    </asp:View>
    <asp:View runat="server" ID="ExplorerView">
        <fiftyOne:LiteMessage runat="server" ID="Message1" FooterEnabled="false" LogoEnabled="false" />
        <fiftyOne:DeviceExplorer runat="server" ID="Explorer" Navigation="true" CmsCssClass="cms" />
    </asp:View>
    <asp:View runat="server" ID="TopView">
        <fiftyOne:TopDevices runat="server" ID="TopDevices" />
    </asp:View>
    <asp:View runat="server" ID="DictionaryView">
        <fiftyOne:PropertyDictionary runat="server" ID="Dictionary" CssClass="propertyDictionary" CmsCssClass="cms" />
    </asp:View>
    <asp:View runat="server" ID="UserAgentTesterView">
        <fiftyOne:UserAgentTester runat="server" ID="UserAgentTester" />        
    </asp:View>
    <asp:View runat="server" ID="RedirectView">
        <fiftyOne:Redirect runat="server" ID="Redirect" />        
    </asp:View>
    <asp:View runat="server" ID="DetectionView" >
        <fiftyOne:Detection runat="server" ID="Detection" />
    </asp:View>
    <asp:View runat="server" ID="StandardPropertiesView">
        <p>This page provides the values .NET will report for properties of the HttpBrowserCapabilities class. Many of these properties are no longer relevent but are provided here for convenience.</p>
        <p>ActiveXControls <% =Request.Browser.ActiveXControls %></p>
        <p>Adapters <% =Request.Browser.Adapters %></p>
        <p>AOL <% =Request.Browser.AOL %></p>
        <p>BackgroundSounds <% =Request.Browser.BackgroundSounds %></p>
        <p>Beta <% =Request.Browser.Beta %></p>
        <p>Browser <% =Request.Browser.Browser %></p>
        <p>Browsers <% =Request.Browser.Browsers %></p>
        <p>CanCombineFormsInDeck <% =Request.Browser.CanCombineFormsInDeck %></p>
        <p>CanInitiateVoiceCall <% =Request.Browser.CanInitiateVoiceCall %></p>
        <p>CanRenderAfterInputOrSelectElement <% =Request.Browser.CanRenderAfterInputOrSelectElement %></p>
        <p>CanRenderEmptySelects <% =Request.Browser.CanRenderEmptySelects %></p>
        <p>CanRenderInputAndSelectElementsTogether <% =Request.Browser.CanRenderInputAndSelectElementsTogether %></p>
        <p>CanRenderMixedSelects <% =Request.Browser.CanRenderMixedSelects %></p>
        <p>CanRenderOneventAndPrevElementsTogether <% =Request.Browser.CanRenderOneventAndPrevElementsTogether %></p>
        <p>CanRenderPostBackCards <% =Request.Browser.CanRenderPostBackCards %></p>
        <p>CanRenderSetvarZeroWithMultiSelectionList <% =Request.Browser.CanRenderSetvarZeroWithMultiSelectionList %></p>
        <p>CanSendMail <% =Request.Browser.CanSendMail %></p>
        <p>Capabilities <% =Request.Browser.Capabilities %></p>
        <p>CDF <% =Request.Browser.CDF %></p>
        <p>ClrVersion <% =Request.Browser.ClrVersion %></p>
        <p>Cookies <% =Request.Browser.Cookies %></p>
        <p>Crawler <% =Request.Browser.Crawler %></p>
        <p>DefaultSubmitButtonLimit <% =Request.Browser.DefaultSubmitButtonLimit %></p>
        <p>EcmaScriptVersion <% =Request.Browser.EcmaScriptVersion %></p>
        <p>Frames <% =Request.Browser.Frames %></p>
        <p>GatewayMajorVersion <% =Request.Browser.GatewayMajorVersion %></p>
        <p>GatewayMinorVersion <% =Request.Browser.GatewayMinorVersion %></p>
        <p>GatewayVersion <% =Request.Browser.GatewayVersion %></p>
        <p>HasBackButton <% =Request.Browser.HasBackButton %></p>
        <p>HidesRightAlignedMultiselectScrollbars <% =Request.Browser.HidesRightAlignedMultiselectScrollbars %></p>
        <p>HtmlTextWriter <% =Request.Browser.HtmlTextWriter %></p>
        <p>Id <% =Request.Browser.Id %></p>
        <p>InputType <% =Request.Browser.InputType %></p>
        <p>IsColor <% =Request.Browser.IsColor %></p>
        <p>IsMobileDevice <% =Request.Browser.IsMobileDevice %></p>
        <p>JavaApplets <% =Request.Browser.JavaApplets %></p>
        <p>JavaScript <% =Request.Browser.JavaScript %></p>
        <p>JScriptVersion <% =Request.Browser.JScriptVersion %></p>
        <p>MajorVersion <% =Request.Browser.MajorVersion %></p>
        <p>MaximumHrefLength <% =Request.Browser.MaximumHrefLength %></p>
        <p>MaximumRenderedPageSize <% =Request.Browser.MaximumRenderedPageSize %></p>
        <p>MaximumSoftkeyLabelLength <% =Request.Browser.MaximumSoftkeyLabelLength %></p>
        <p>MinorVersion <% =Request.Browser.MinorVersion %></p>
        <p>MinorVersionString <% =Request.Browser.MinorVersionString %></p>
        <p>MobileDeviceManufacturer <% =Request.Browser.MobileDeviceManufacturer %></p>
        <p>MobileDeviceModel <% =Request.Browser.MobileDeviceModel %></p>
        <p>MSDomVersion <% =Request.Browser.MSDomVersion %></p>
        <p>NumberOfSoftkeys <% =Request.Browser.NumberOfSoftkeys %></p>
        <p>Platform <% =Request.Browser.Platform %></p>
        <p>PreferredImageMime <% =Request.Browser.PreferredImageMime %></p>
        <p>PreferredRenderingMime <% =Request.Browser.PreferredRenderingMime %></p>
        <p>PreferredRenderingType <% =Request.Browser.PreferredRenderingType %></p>
        <p>PreferredRequestEncoding <% =Request.Browser.PreferredRequestEncoding %></p>
        <p>PreferredResponseEncoding <% =Request.Browser.PreferredResponseEncoding %></p>
        <p>RendersBreakBeforeWmlSelectAndInput <% =Request.Browser.RendersBreakBeforeWmlSelectAndInput %></p>
        <p>RendersBreaksAfterHtmlLists <% =Request.Browser.RendersBreaksAfterHtmlLists %></p>
        <p>RendersBreaksAfterWmlAnchor <% =Request.Browser.RendersBreaksAfterWmlAnchor %></p>
        <p>RendersBreaksAfterWmlInput <% =Request.Browser.RendersBreaksAfterWmlInput %></p>
        <p>RendersWmlDoAcceptsInline <% =Request.Browser.RendersWmlDoAcceptsInline %></p>
        <p>RendersWmlSelectsAsMenuCards <% =Request.Browser.RendersWmlSelectsAsMenuCards %></p>
        <p>RequiredMetaTagNameValue <% =Request.Browser.RequiredMetaTagNameValue %></p>
        <p>RequiresAttributeColonSubstitution <% =Request.Browser.RequiresAttributeColonSubstitution %></p>
        <p>RequiresContentTypeMetaTag <% =Request.Browser.RequiresContentTypeMetaTag %></p>
        <p>RequiresControlStateInSession <% =Request.Browser.RequiresControlStateInSession %></p>
        <p>RequiresDBCSCharacter <% =Request.Browser.RequiresDBCSCharacter %></p>
        <p>RequiresHtmlAdaptiveErrorReporting <% =Request.Browser.RequiresHtmlAdaptiveErrorReporting %></p>
        <p>RequiresLeadingPageBreak <% =Request.Browser.RequiresLeadingPageBreak %></p>
        <p>RequiresNoBreakInFormatting <% =Request.Browser.RequiresNoBreakInFormatting %></p>
        <p>RequiresOutputOptimization <% =Request.Browser.RequiresOutputOptimization %></p>
        <p>RequiresPhoneNumbersAsPlainText <% =Request.Browser.RequiresPhoneNumbersAsPlainText %></p>
        <p>RequiresSpecialViewStateEncoding <% =Request.Browser.RequiresSpecialViewStateEncoding %></p>
        <p>RequiresUniqueFilePathSuffix <% =Request.Browser.RequiresUniqueFilePathSuffix %></p>
        <p>RequiresUniqueHtmlCheckboxNames <% =Request.Browser.RequiresUniqueHtmlCheckboxNames %></p>
        <p>RequiresUniqueHtmlInputNames <% =Request.Browser.RequiresUniqueHtmlInputNames %></p>
        <p>RequiresUrlEncodedPostfieldValues <% =Request.Browser.RequiresUrlEncodedPostfieldValues %></p>
        <p>ScreenBitDepth <% =Request.Browser.ScreenBitDepth %></p>
        <p>ScreenCharactersHeight <% =Request.Browser.ScreenCharactersHeight %></p>
        <p>ScreenCharactersWidth <% =Request.Browser.ScreenCharactersWidth %></p>
        <p>ScreenPixelsHeight <% =Request.Browser.ScreenPixelsHeight %></p>
        <p>ScreenPixelsWidth <% =Request.Browser.ScreenPixelsWidth %></p>
        <p>SupportsAccesskeyAttribute <% =Request.Browser.SupportsAccesskeyAttribute %></p>
        <p>SupportsBodyColor <% =Request.Browser.SupportsBodyColor %></p>
        <p>SupportsBold <% =Request.Browser.SupportsBold %></p>
        <p>SupportsCacheControlMetaTag <% =Request.Browser.SupportsCacheControlMetaTag %></p>
        <p>SupportsCallback <% =Request.Browser.SupportsCallback %></p>
        <p>SupportsCss <% =Request.Browser.SupportsCss %></p>
        <p>SupportsDivAlign <% =Request.Browser.SupportsDivAlign %></p>
        <p>SupportsDivNoWrap <% =Request.Browser.SupportsDivNoWrap %></p>
        <p>SupportsEmptyStringInCookieValue <% =Request.Browser.SupportsEmptyStringInCookieValue %></p>
        <p>SupportsFontColor <% =Request.Browser.SupportsFontColor %></p>
        <p>SupportsFontName <% =Request.Browser.SupportsFontName %></p>
        <p>SupportsFontSize <% =Request.Browser.SupportsFontSize %></p>
        <p>SupportsImageSubmit <% =Request.Browser.SupportsImageSubmit %></p>
        <p>SupportsIModeSymbols <% =Request.Browser.SupportsIModeSymbols %></p>
        <p>SupportsInputIStyle <% =Request.Browser.SupportsInputIStyle %></p>
        <p>SupportsInputMode <% =Request.Browser.SupportsInputMode %></p>
        <p>SupportsItalic <% =Request.Browser.SupportsItalic %></p>
        <p>SupportsJPhoneMultiMediaAttributes <% =Request.Browser.SupportsJPhoneMultiMediaAttributes %></p>
        <p>SupportsJPhoneSymbols <% =Request.Browser.SupportsJPhoneSymbols %></p>
        <p>SupportsQueryStringInFormAction <% =Request.Browser.SupportsQueryStringInFormAction %></p>
        <p>SupportsRedirectWithCookie <% =Request.Browser.SupportsRedirectWithCookie %></p>
        <p>SupportsSelectMultiple <% =Request.Browser.SupportsSelectMultiple %></p>
        <p>SupportsUncheck <% =Request.Browser.SupportsUncheck %></p>
        <p>SupportsXmlHttp <% =Request.Browser.SupportsXmlHttp %></p>
        <p>Tables <% =Request.Browser.Tables %></p>
        <p>TagWriter <% =Request.Browser.TagWriter %></p>
        <p>Type <% =Request.Browser.Type %></p>
        <p>UseOptimizedCacheKey <% =Request.Browser.UseOptimizedCacheKey %></p>
        <p>VBScript <% =Request.Browser.VBScript %></p>
        <p>Version <% =Request.Browser.Version %></p>
        <p>W3CDomVersion <% =Request.Browser.W3CDomVersion %></p>
        <p>Win16 <% =Request.Browser.Win16 %></p>
        <p>Win32 <% =Request.Browser.Win32 %></p>
    </asp:View>
</asp:MultiView>
</div>