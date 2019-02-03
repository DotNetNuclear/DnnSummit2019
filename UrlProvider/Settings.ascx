<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Settings.ascx.cs" Inherits="DotNetNuclear.Modules.RestaurantMenuMVC.UrlProvider.Settings" %>
<%@ Register TagName="label" TagPrefix="dnn" Src="~/controls/labelcontrol.ascx" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke" Namespace="DotNetNuke.UI.WebControls" %>
<%@ Register TagPrefix="dnn" Assembly="DotNetNuke.Web" Namespace="DotNetNuke.Web.UI.WebControls" %>

<div class="dnnPanel">
    <h2 id="menuSitePanel-UrlSettings" class="dnnFormSectionHead"><a href="#" class="dnnSectionExpanded"><%=LocalizeString("UrlProviderSettings")%></a></h2>
    <fieldset>
        <div class="dnnFormItem">
            <dnn:label id="lblMenuDetailPage" runat="server" controlname="cboMenuPage" ResourceKey="lblMenuDetailPage" />
            <dnn:DnnPageDropDownList ID="cboMenuPage" runat="server" />
        </div>
    </fieldset>
</div>