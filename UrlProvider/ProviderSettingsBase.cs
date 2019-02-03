using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Framework.Providers;

namespace DotNetNuclear.Modules.RestaurantMenuMVC.UrlProvider
{
    public class ProviderSettingsBase : PortalModuleBase
    {
        public ExtensionUrlProviderInfo Provider { get; set; }
        public const string MenuDetailPage = "MenuDetailPage";
        public const string MenuDetailModule = "MenuDetailModule";

        #region private helper methods

        protected TabInfo GetSafeTab(string settingName, TabInfo defaultValue)
        {
            TabInfo result = defaultValue;
            int tabId = GetSafeInt(settingName, -1);
            if (tabId > 0)
            {
                TabController tc = new TabController();
                result = tc.GetTab(tabId, this.PortalId, false);

            }
            return result;
        }

        protected int GetSafeInt(string settingName, int defaultValue)
        {
            int result = defaultValue;
            string raw = null;
            if (Provider != null && Provider.Settings != null && Provider.Settings.ContainsKey(settingName))
            {
                raw = Provider.Settings[settingName];
            }
            if (string.IsNullOrEmpty(raw) == false) int.TryParse(raw, out result);
            return result;
        }

        #endregion
    }
}