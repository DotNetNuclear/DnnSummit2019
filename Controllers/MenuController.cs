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

using System;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Web.Mvc;
using DotNetNuke.Instrumentation;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuclear.Modules.RestaurantMenuMVC.Components;
using DotNetNuclear.Modules.RestaurantMenuMVC.Models;
using DotNetNuke.Services.Social.Notifications;
using System.Web.Script.Serialization;

namespace DotNetNuclear.Modules.RestaurantMenuMVC.Controllers
{

    [DnnHandleError]
    public class MenuController : DnnController
    {
        private IMenuItemRepository _menuDataRepository;
        private int _moduleId = 0;
        private ILog _log;

        protected ILog Log
        {
            get { return _log ?? (_log = LoggerSource.Instance.GetLogger(this.GetType())); }
        }

        public int CurrentModuleId
        {
            get { return ModuleContext?.ModuleId ?? _moduleId; }
            set { _moduleId = value; }
        }


        public MenuController()
        {
            _menuDataRepository = new MenuItemRepository();
        }

        /// <summary>
        /// Testable Controller constructor
        /// </summary>
        /// <param name="dalRepository"></param>
        /// <param name="moduleid"></param>
        public MenuController(IMenuItemRepository dalRepository, int moduleid)
        {
            _menuDataRepository = dalRepository;
            CurrentModuleId = moduleid;
        }

        [ModuleAction(ControlKey = "Edit", TitleKey = "AddItem")]
        public ActionResult Index()
        {
            string culSetting = "en-US";
            var items = _menuDataRepository.GetItems(CurrentModuleId);
            if (ModuleContext.Settings.ContainsKey("RestaurantMenu_CurrencyCulture"))
            {
                if (!String.IsNullOrEmpty(ModuleContext.Settings["RestaurantMenu_CurrencyCulture"].ToString()))
                    culSetting = ModuleContext.Settings["RestaurantMenu_CurrencyCulture"].ToString();
            }
            ViewBag.Culture = new CultureInfo(culSetting);

            return View(items);
        }

        public ActionResult Delete(int itemId)
        {
            _menuDataRepository.DeleteItem(itemId, CurrentModuleId);
            return RedirectToDefaultRoute();
        }

        public JsonResult Upload()
        {
            string imageUrl = string.Empty;
            string imgPath = Server.MapPath("~/Portals/0/Restaurant/");
            if (!Directory.Exists(imgPath))
            {
                Directory.CreateDirectory(imgPath);
            }

            foreach (string s in Request.Files)
            {
                var file = Request.Files[s];
                if (file.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(file.FileName);
                    var path = Path.Combine(imgPath, fileName);
                    file.SaveAs(path);
                    imageUrl = string.Format("/Portals/0/Restaurant/{0}", fileName);
                }
            }

            return Json(new { img = imageUrl, thumb = imageUrl }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Edit(int itemId = -1)
        {
            IMenuItem item = new MenuItem();
            try
            {
                item.ModuleId = CurrentModuleId;

                if (itemId > 0)
                {
                    item = _menuDataRepository.GetItem(itemId, CurrentModuleId);
                }

                if (String.IsNullOrEmpty(item.ImageUrl))
                {
                    try
                    {
                        item.ImageUrl = FeatureController.BASEMODULEPATH + "/Resources/images/noimage.png";
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("An error occurred in the Edit Action. Exception: {0}", ex);
            }

            return View(item);
        }

        [HttpPost]
        [DotNetNuke.Web.Mvc.Framework.ActionFilters.ValidateAntiForgeryToken]
        public ActionResult Edit(MenuItem item)
        {
            if (ModelState.IsValid)
            {  
                try
                {
                    if (item.MenuItemId <= 0)
                    {
                        item.AddedByUserId = User.UserID;
                        item.DateAdded = DateTime.UtcNow;
                        item.ModifiedByUserId = User.UserID;
                        item.DateModified = DateTime.UtcNow;

                        _menuDataRepository.CreateItem(item);
                    }
                    else
                    {
                        var existingItem = _menuDataRepository.GetItem(item.MenuItemId, item.ModuleId);
                        existingItem.ModifiedByUserId = User.UserID;
                        existingItem.DateModified = DateTime.Now;
                        existingItem.IsDailySpecial = item.IsDailySpecial;
                        existingItem.IsVegetarian = item.IsVegetarian;
                        existingItem.ImageUrl = item.ImageUrl;
                        existingItem.Price = item.Price;
                        existingItem.Name = item.Name;
                        existingItem.Desc = item.Desc;

                        _menuDataRepository.UpdateItem(existingItem);
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("An error occurred in saving the menu item. Exception: {0}", ex);
                }
                return RedirectToDefaultRoute();
            }

            return View(item);
        }
    }

    public class NotificationDto
    {
        public int NotificationId { get; set; }
    }
}
