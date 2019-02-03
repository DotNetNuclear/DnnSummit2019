/*
' Copyright (c) 2019 DotNetNuclear
' http://www.dotnetnuclear.com
' All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using DotNetNuclear.Modules.RestaurantMenuMVC.Models;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Social.Notifications;

namespace DotNetNuclear.Modules.RestaurantMenuMVC.Components
{
    public class Notifications : ServiceLocator<INotifications, Notifications>
    {
        #region Overrides of ServiceLocator<INotifications,Notifications>

        protected override Func<INotifications> GetFactory()
        {
            return () => new NotificationsImpl();
        }

        #endregion
    }

    public interface INotifications
    {
        /// <summary>
        /// This will create a notification type associated w/ the module and also handle the actions that must be associated with it.
        /// </summary>
        void AddNotificationTypes();

        /// <summary>
        /// </summary>
        void SendItemAddedNotification(MenuItem newItem);
    }

    public class NotificationsImpl : INotifications
    {
        private ILog _logger;

        public NotificationsImpl()
        {
            _logger = LoggerSource.Instance.GetLogger(typeof(NotificationsImpl));
        }

        #region Install Methods


        public void AddNotificationTypes()
        {
            var desktopModuleId = 0;
            var actions = new List<NotificationTypeAction>();
            try
            {
                desktopModuleId = DesktopModuleController.GetDesktopModuleByFriendlyName(FeatureController.DESKTOPMODULE_FRIENDLYNAME).DesktopModuleID;
            }
            catch
            {
                _logger.Error("An instance of the restaurant module does not exist for notifications");
            }

            var objNotificationType = new NotificationType
            {
                Name = FeatureController.NOTIFICATION_ITEMADDED,
                Description = "Restaurant Menu Item Added",
                DesktopModuleId = desktopModuleId
            };

            if (NotificationsController.Instance.GetNotificationType(objNotificationType.Name) == null)
            {
                var objAction = new NotificationTypeAction
                {
                    /* Resolving the names of these resource keys is currently a bug with MVC modules, DNN is expecting the resx
                     * file to be in path "/DesktopModules/DotNetNuclear/RestaurantMenu/App_LocalResources/SharedResources.resx"
                     * but for MVC modules, the path is: "/DesktopModules/MVC/DotNetNuclear/RestaurantMenu/App_LocalResources/SharedResources.resx"
                     * therefore, we either need to fix the bug in the DNN library, or deploy the resource file in the place it's looking for
                    */
                    NameResourceKey = "Dislike",
                    DescriptionResourceKey = "DislikeItem_Desc",
                    APICall = "DesktopModules/DotNetNuclear.RestaurantMenu.Mvc/API/ItemNotification/dislike",
                    Order = 2
                };
                actions.Add(objAction);
                objAction = new NotificationTypeAction
                {
                    NameResourceKey = "Like",
                    DescriptionResourceKey = "LikeItem_Desc",
                    APICall = "DesktopModules/DotNetNuclear.RestaurantMenu.Mvc/API/ItemNotification/like",
                    Order = 1
                };
                actions.Add(objAction);

                NotificationsController.Instance.CreateNotificationType(objNotificationType);
                NotificationsController.Instance.SetNotificationTypeActions(actions, objNotificationType.NotificationTypeId);
            }
        }

        #endregion

        /// <summary>
        /// This will create an item added notification.
        /// </summary>
        public void SendItemAddedNotification(MenuItem menuItem)
        {
            // Get the notification type; if it doesn't exist, create it
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            ModuleController mCtrl = new ModuleController();
            var itemAddedNType = NotificationsController.Instance.GetNotificationType(FeatureController.NOTIFICATION_ITEMADDED);
            if (itemAddedNType == null)
            {
                AddNotificationTypes();
                itemAddedNType = NotificationsController.Instance.GetNotificationType(FeatureController.NOTIFICATION_ITEMADDED);
            }

            ModuleInfo itemModule = mCtrl.GetModule(menuItem.ModuleId);
            var itemTabInfo = TabController.Instance.GetTab(itemModule.TabID, itemModule.PortalID);

            string alertBody = $"A new menu item has been added to the '{itemTabInfo.Title}' menu on {menuItem.DateAdded}." + 
                               $"<br/>Item Name: '{menuItem.Name}' <br/>Description: '{menuItem.Desc}'";
            if (itemAddedNType != null)
            {
                Notification msg = new Notification
                {
                    NotificationTypeID = itemAddedNType.NotificationTypeId,
                    To = "Registered Users",
                    SenderUserID = menuItem.AddedByUserId,
                    Subject = "A new menu item has been added.",
                    Body = alertBody,
                    ExpirationDate = DateTime.MaxValue,
                    IncludeDismissAction = false,
                    Context = serializer.Serialize(new ItemContextKey { MenuItemId = menuItem.MenuItemId, ModuleId = menuItem.ModuleId })
                };

                List<RoleInfo> sendUsers = new List<RoleInfo>();
                var roleRegUsers = RoleController.Instance.GetRoleByName(itemModule.PortalID, "Registered Users");
                sendUsers.Add(roleRegUsers);

                NotificationsController.Instance.SendNotification(msg, itemModule.PortalID, sendUsers, null);
            }
        }

    }

    public class ItemContextKey
    {
        public int MenuItemId { get; set; }
        public int ModuleId { get; set; }
    }

}