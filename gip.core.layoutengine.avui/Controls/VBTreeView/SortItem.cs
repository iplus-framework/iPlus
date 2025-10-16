using gip.core.datamodel;
using System.Linq;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents the class for SortItem.
    /// </summary>
    internal class SortItem
    {
        /// <summary>
        /// Creates a new instance of Sortitem.
        /// </summary>
        /// <param name="item">The item parameter.</param>
        /// <param name="property">The name of property.</param>
        public SortItem(IACObject item, string property)
        {
            Item = item;

            object actItem = item;

            string[] dataPath = property.Split('.');
            // Wenn es Properties in untergeordneten Objekten sind, dann sind diese erst zu ermitteln
            for (int i = 0; i < dataPath.Length - 1; i++)
            {
                string path = dataPath[i];
                actItem = actItem.GetType().GetProperty(path).GetValue(actItem, null);
            }

            Property = (string)actItem.GetType().GetProperty(dataPath.Last()).GetValue(actItem, null);
        }

        /// <summary>
        /// Gets or sets the Item.
        /// </summary>
        public IACObject Item
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of property.
        /// </summary>
        public string Property
        {
            get;
            set;
        }

    }
}
