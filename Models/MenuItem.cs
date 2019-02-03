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
using System.Web.Caching;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using DotNetNuke.ComponentModel.DataAnnotations;
using DotNetNuclear.Modules.RestaurantMenuMVC.Components;
using System.Xml.Serialization;

namespace DotNetNuclear.Modules.RestaurantMenuMVC.Models
{
    [TableName("DotNetNuclear_RestaurantMenuMVC_Item")]
    //setup the primary key for table
    [PrimaryKey("MenuItemId", AutoIncrement = true)]
    //configure caching using PetaPoco
    [Cacheable("RestaurantMenuMVC_Items", CacheItemPriority.Default, 20)]
    //scope the objects to the ModuleId of a module on a page (or copy of a module on a page)
    [Scope("ModuleId")]
    public class MenuItem : IMenuItem, IValidatableObject
    {
        IMenuItemRepository _menuItemLookupRepo;

        public MenuItem() { }

        public MenuItem(IMenuItemRepository argMenuItemRepo)
        {
            _menuItemLookupRepo = argMenuItemRepo;
        }

        [XmlAttribute]
        public int MenuItemId { get; set; }

        [XmlAttribute]
        public int ModuleId { get; set; }

        [Required]
        [XmlElement]
        public string Name { get; set; }

        [XmlElement]
        public string Desc { get; set; }

        [XmlElement]
        public string ImageUrl { get; set; }

        [XmlAttribute]
        public bool IsDailySpecial { get; set; }

        [XmlAttribute]
        public bool IsVegetarian { get; set; }

        [Range(0, double.MaxValue)]
        [XmlElement]
        public decimal Price { get; set; }

        [XmlElement]
        public int DisplayOrder { get; set; }

        [XmlElement]
        public int AddedByUserId { get; set; }

        [XmlIgnore]
        public int ModifiedByUserId { get; set; }

        [XmlElement]
        public DateTime DateAdded { get; set; }

        [XmlIgnore]
        public DateTime DateModified { get; set; }

        [XmlElement]
        public int Popularity { get; set; }

        [XmlElement]
        public DateTime ExpirationDate { get; set; }

        [Required]
        [XmlElement]
        public string UrlSlug { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (_menuItemLookupRepo == null)
            {
                _menuItemLookupRepo = new MenuItemRepository();
            }

            var items = _menuItemLookupRepo.GetItems(ModuleId);
            bool nameExistsOnOtherItem = items.Any(i => i.MenuItemId != MenuItemId &&
                                    i.Name.Equals(Name, StringComparison.CurrentCultureIgnoreCase));
            if (nameExistsOnOtherItem)
            {
                yield return new ValidationResult("An item with this name already exists.", new[] { nameof(Name) });
            }
        }
    }

}
