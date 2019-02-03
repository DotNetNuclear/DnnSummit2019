using System;
using System.Linq;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Portals;
using DotNetNuclear.Modules.RestaurantMenuMVC.Components;

namespace DotNetNuclear.Modules.RestaurantMenuMVC.UrlProvider
{
    public class MenuUrlProvider : ExtensionUrlProvider
    {
        private readonly Regex _detailsUrlPatternRegex;

        public MenuUrlProvider()
        {
            _detailsUrlPatternRegex = new Regex("/itemId/([0-9]*)/controller/Menu/action/Detail");
        }
 
        #region ExtensionUrlProvider Methods

        public override bool AlwaysUsesDnnPagePath(int portalId)
        {
            return true;
        }

        public override string ChangeFriendlyUrl(TabInfo tab, string friendlyUrlPath, FriendlyUrlOptions options, string cultureCode, ref string endingPageName, out bool useDnnPagePath,
                                                 ref List<string> messages)
        {
            string newUrlPath = friendlyUrlPath;
            useDnnPagePath = AlwaysUsesDnnPagePath(tab.PortalID);
            if (messages == null) messages = new List<string>();

            if (MenuTabId == tab.TabID)
            {
                // Match only path that starts with /menu/xxxx.  If there are parameters like "/ctl/PostEdit/"
                // before the string, then don't replace the url because it interferes with editing
                var match = _detailsUrlPatternRegex.Match(friendlyUrlPath);
                if (match.Success && match.Groups.Count >= 1 && !string.IsNullOrEmpty(match.Groups[1].Value))
                {
                    int itemId = 0;
                    Int32.TryParse(match.Groups[1].Value, out itemId);
                    if (itemId > 0)
                    {
                        var menuRepo = new MenuItemRepository();
                        var moduleItem = menuRepo.GetItem(itemId, MenuModuleId);
                        if (moduleItem != null && !String.IsNullOrEmpty(moduleItem.UrlSlug))
                        {
                            newUrlPath = "/" + moduleItem.UrlSlug;
                            endingPageName = "";
                        }
                    }
                }
            }

            return newUrlPath;
        }

        public override string TransformFriendlyUrlToQueryString(string[] urlParms, int tabId, int portalId, FriendlyUrlOptions options, string cultureCode,
                                                                 PortalAliasInfo portalAlias, ref List<string> messages, out int status, out string location)
        {
            //initialize results and output variables
            string result = ""; status = 200; //OK 
            location = null; //no redirect location
            if (messages == null) messages = new List<string>();
            
            try
            {
                // Rewrite the url if we are on the landing page from settings
                var pathSlug = string.Empty;
                if (tabId > 0 && (MenuTabId == tabId))
                {
                    // handle logoff case
                    int ctlIdx = Array.IndexOf(urlParms, "ctl");
                    if (ctlIdx > -1 && ((ctlIdx + 1) < urlParms.Length))
                    {
                        var ctlCmd = urlParms[ctlIdx + 1].ToLower();
                        if (ctlCmd == "logoff")
                        {
                            var pInfo = PortalController.Instance.GetPortal(portalId);
                            result = $"tabid={pInfo.LoginTabId}&ctl=logoff";
                            return result;
                        }
                        else if (ctlCmd == "edit")
                        {
                            return result;
                        }
                    } 

                    // Check if the slug after the page name matches to an item
                    pathSlug = urlParms[0].ToLower();
                    if (!string.IsNullOrEmpty(pathSlug) && MenuModuleId > 0)
                    {
                        var menuRepo = new MenuItemRepository();
                        var htSlugs = menuRepo.GetUrlSlugTable(MenuModuleId);
                        if (htSlugs.ContainsKey(pathSlug))
                        {
                            result = $"tabid={tabId}&controller=Menu&action=Detail&moduleId={MenuModuleId}&itemId={htSlugs[pathSlug]}";
                        }
                        else
                        {
                            result = "";
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            return result;
        }

        public override bool CheckForRedirect(int tabId, int portalid, string httpAlias, Uri requestUri, NameValueCollection queryStringCol, FriendlyUrlOptions options, out string redirectLocation,
                                              ref List<string> messages)
        {
            redirectLocation = String.Empty;
            return false;            
        }

        public override Dictionary<string, string> GetProviderPortalSettings()
        {
            return null;
        }

        #endregion

        #region Internal Properties

        internal int MenuTabId
        {
            get
            {
                return GetSafeInt(ProviderSettingsBase.MenuDetailPage, -1);
            }
        }

        internal int MenuModuleId
        {
            get
            {
                return GetSafeInt(ProviderSettingsBase.MenuDetailModule, -1);
            }
        }

        #endregion

        #region Private methods

        private int GetSafeInt(string settingName, int defaultValue)
        {
            int result = defaultValue;
            string raw = null;
            if (ProviderConfig.Settings.ContainsKey(settingName))
                raw = this.ProviderConfig.Settings[settingName];
            if (string.IsNullOrEmpty(raw) == false) int.TryParse(raw, out result);
            return result;
        }

        #endregion

    }

}
