/*
' Copyright (c) 2019 DotNetNuclear.com
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using DotNetNuclear.Modules.RestaurantMenuMVC.Models;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Search.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotNetNuclear.Modules.RestaurantMenuMVC.Components
{

    public class FeatureController: IPortable
    {
        public const string BASEMODULEPATH = @"/DesktopModules/MVC/DotNetNuclear/RestaurantMenu";
        public const string DESKTOPMODULE_FRIENDLYNAME = "Restaurant Menu (MVC)";

        #region IPortable Implementation

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ExportModule implements the IPortable ExportModule Interface
        /// </summary>
        /// <param name="ModuleID">The Id of the module to be exported</param>
        /// -----------------------------------------------------------------------------
        public string ExportModule(int ModuleID)
        {
            string result = string.Empty;
            IMenuItemRepository menuDataRepo = new MenuItemRepository();
            try
            {
                // Get the enumerable list of module data items
                var items = menuDataRepo.GetItems(ModuleID);
                PortableItems root = new PortableItems(ModuleID)
                {
                    ModuleData = items.Cast<MenuItem>().ToList()
                };
                result = root.Serialize();
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
            return result;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// ImportModule implements the IPortable ImportModule Interface
        /// </summary>
        /// <param name="ModuleID">The Id of the module to be imported</param>
        /// <param name="Content">The content to be imported</param>
        /// <param name="Version">The version of the module to be imported</param>
        /// <param name="UserId">The Id of the user performing the import</param>
        /// -----------------------------------------------------------------------------
        public void ImportModule(int ModuleID, string Content, string Version, int UserID)
        {
            IMenuItemRepository menuDataRepo = new MenuItemRepository();
            try
            {
                // Import module content as new items
                PortableItems root = new PortableItems(ModuleID);
                root.Deserialize(Content);
                foreach (IMenuItem i in root.ModuleData)
                {
                    // Overwrite last modified Id and module Id and Add item to db
                    i.ModuleId = ModuleID;
                    i.ModifiedByUserId = UserID;
                    i.DateModified = DateTime.Now;
                    menuDataRepo.CreateItem(i);
                }

                // Import module settings
                var settings = new Models.Settings();
                foreach (DictionaryEntry setting in root.ModSettings)
                {
                    ModuleController.Instance.UpdateModuleSetting(ModuleID, setting.Key.ToString(), setting.Value.ToString());
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        #endregion
    }

}
