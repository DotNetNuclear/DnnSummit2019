/*
' Copyright (c) 2018 Epic Games, Inc.
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using System;
using System.Linq;
using System.Collections.Generic;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Urls;
using DotNetNuke.Entities.Modules;
using DotNetNuclear.Modules.RestaurantMenuMVC.Components;

namespace DotNetNuclear.Modules.RestaurantMenuMVC.UrlProvider
{
    public partial class Settings : ProviderSettingsBase, IExtensionUrlProviderSettingsControl
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            LocalResourceFile = "~/DesktopModules/MVC/DotNetNuclear/RestaurantMenu/UrlProvider/App_LocalResources/Settings.ascx.resx";
        }

        public void LoadSettings()
        {
            if (!IsPostBack)
            {
                if (Provider != null)
                {
                    cboMenuPage.SelectedPage = GetSafeTab(ProviderSettingsBase.MenuDetailPage, null);
                }
                else
                {
                    throw new ArgumentNullException("ExtensionUrlProviderInfo is null on LoadSettings()");
                }
            }
        }

        public Dictionary<string, string> SaveSettings()
        {
            var settings = new Dictionary<string, string>();

            if (cboMenuPage.SelectedPage != null)
            {
                var tabId = cboMenuPage.SelectedPage.TabID;
                settings.Add(ProviderSettingsBase.MenuDetailPage, tabId.ToString());

                if (tabId > 0)
                {
                    try
                    {
                        var portalId = DotNetNuke.Entities.Portals.PortalController.Instance.GetCurrentPortalSettings().PortalId;
                        var menuTab = TabController.Instance.GetTab(tabId, portalId);
                        var menuModule = menuTab.Modules.Cast<ModuleInfo>().FirstOrDefault(m => m.ModuleDefinition.DefinitionName == FeatureController.DESKTOPMODULE_FRIENDLYNAME);
                        if (menuModule != null)
                        {
                            settings.Add(ProviderSettingsBase.MenuDetailModule, menuModule.ModuleID.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                    }
                }

            }

            return settings;
        }
    }

}
