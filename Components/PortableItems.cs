using DotNetNuke.Entities.Modules;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DotNetNuclear.Modules.RestaurantMenuMVC.Components
{ 
    [XmlRoot("RestaurantMenuModule")]
    public class PortableItems
    {
        [XmlElement]
        public List<RestaurantMenuMVC.Models.MenuItem> ModuleData;

        [XmlElement]
        public List<DictionaryEntry> ModSettings;

        #region Constructors

        public PortableItems() { }

        public PortableItems(int moduleId)
        {
            ModuleData = new List<RestaurantMenuMVC.Models.MenuItem>();
            ModSettings = new List<DictionaryEntry>();

            ModuleInfo currentMod = ModuleController.Instance.GetModule(moduleId, DotNetNuke.Common.Utilities.Null.NullInteger, false);
            if (currentMod != null && currentMod.ModuleSettings != null)
            {
                ModSettings = currentMod.ModuleSettings.Cast<DictionaryEntry>().ToList();
            }
        }

        #endregion

        #region Serialization methods

        public string Serialize()
        {
            XmlSerializer xmlSerializer = new XmlSerializer(this.GetType());
            using (StringWriter textWriter = new StringWriter())
            {
                xmlSerializer.Serialize(textWriter, this);
                return textWriter.ToString();
            }
        }
        public void Deserialize(string fromSerialize)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(this.GetType());
                using (TextReader reader = new StringReader(fromSerialize))
                {
                    var result = (PortableItems)serializer.Deserialize(reader);
                    this.ModuleData = result.ModuleData;
                    this.ModSettings = result.ModSettings;
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