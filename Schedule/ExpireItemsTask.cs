using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DotNetNuclear.Modules.RestaurantMenuMVC.Components;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Scheduling;

namespace DotNetNuclear.Modules.RestaurantMenuMVC.Schedule
{
    public class ExpireItemsTask : SchedulerClient
    {
        public ExpireItemsTask(ScheduleHistoryItem oSchedHistoryItem)
            : base()
        {
            this.ScheduleHistoryItem = oSchedHistoryItem;
        }
        
        public override void DoWork()
        {
            var menuDataRepository = new MenuItemRepository();
            try
            {
                //--start the processing
                this.Progressing();
                this.ScheduleHistoryItem.AddLogNote("Started.");

                PortalController pCtrl = new PortalController();
                //--perform task for each portal.
                ModuleController mCtrl = new ModuleController();
                foreach (PortalInfo p in pCtrl.GetPortals())
                {
                    //--Get a list of all ScheduleTaskModule instances
                    var schedModules = mCtrl.GetModulesByDefinition(p.PortalID, "Restaurant Menu (MVC)");
                    foreach (var m in schedModules)
                    {
                        int moduleid = ((ModuleInfo)m).ModuleID;
                        var menuItems = menuDataRepository.GetItems(moduleid);
                        var expiredItems = menuItems.Where(i => i.ExpirationDate > (DateTime)System.Data.SqlTypes.SqlDateTime.MinValue && i.ExpirationDate < DateTime.Now).ToList();
                        expiredItems.ForEach(
                            x => menuDataRepository.DeleteItem(x)
                        );
                    }
                    this.ScheduleHistoryItem.AddLogNote(String.Format("Processed complete for Portal {0}.", p.PortalID));
                }

                //--finished processing
                this.ScheduleHistoryItem.AddLogNote("Finished.");
                this.ScheduleHistoryItem.Succeeded = true;
            }
            catch (Exception ex)
            {
                this.ScheduleHistoryItem.AddLogNote("Failed. " + ex.ToString());
                this.ScheduleHistoryItem.Succeeded = false;
                this.Errored(ref ex);
            }        
        }
    }
}