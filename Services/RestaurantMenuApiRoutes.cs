using System;
using DotNetNuke.Web.Api;
using DotNetNuclear.Modules.RestaurantMenuMVC.Components;

namespace DotNetNuclear.Modules.RestaurantMenuMVC.Services
{
    public class RestaurantMenuApiRoutes : IServiceRouteMapper
    {
        public void RegisterRoutes(IMapRoute routeManager)
        {
            routeManager.MapHttpRoute("DotNetNuclear.RestaurantMenu.Mvc", "default", "{controller}/{action}",
                    new[] { "DotNetNuclear.Modules.RestaurantMenuMVC.Services.Controllers" });

        }
    }
}