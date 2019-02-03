using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Social;
using DotNetNuclear.Modules.RestaurantMenuMVC.Components;
using System.Web.Script.Serialization;

namespace DotNetNuclear.Modules.RestaurantMenuMVC.Services.Controllers
{
    public class ItemNotificationController : DnnApiController
    {
        public class NotificationDto
        {
            public int NotificationId { get; set; }
        }

		[ValidateAntiForgeryToken]
        [DnnAuthorize]
        [ActionName("like")]
        [HttpPost]
		public HttpResponseMessage Like(NotificationDto postData)
		{
			bool rc = UpdateItemPopularityFromNotification(postData, 1);
			return rc ? Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" }) : 
				 Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification action");
		}

		[ValidateAntiForgeryToken]
        [DnnAuthorize]
        [ActionName("dislike")]
        [HttpPost]
		public HttpResponseMessage Dislike(NotificationDto postData)
		{
			bool rc = UpdateItemPopularityFromNotification(postData, -1);
			return rc ? Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" }) : 
				 Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification action");
		}

		private bool UpdateItemPopularityFromNotification(NotificationDto notificationDto, int increment)
		{
			bool success = false;
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			IMenuItemRepository menuDataRepo = new MenuItemRepository();
			try
			{
				var notify = NotificationsController.Instance.GetNotification(notificationDto.NotificationId);
				var itemKey = serializer.Deserialize<ItemContextKey>(notify.Context);
				if (itemKey != null)
				{
					// Update item popularity
					var notifiedItem = menuDataRepo.GetItem(itemKey.MenuItemId, itemKey.ModuleId);
					if (notifiedItem != null)
					{
						notifiedItem.Popularity = notifiedItem.Popularity + increment;
						menuDataRepo.UpdateItem(notifiedItem);
					}
					success = true;
					NotificationsController.Instance.DeleteNotification(notificationDto.NotificationId);
				}
			}
			catch (Exception ex)
			{
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
			}
			return success;
		}
    }
}