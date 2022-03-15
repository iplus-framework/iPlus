using System.Collections.Generic;
using System.Linq;


namespace gip.core.datamodel
{
    public class ScheduledOrderManager<T> where T : IScheduledOrder
    {
        public static void MoveUp(IScheduledOrder[] items)
        {
            Dictionary<int, IScheduledOrder> dict = items.ToDictionary(key => key.ScheduledOrder ?? 0, val => val);
            IScheduledOrder firstItem = null;
            IScheduledOrder secondItem = null;
            for (int i = 0; i < dict.Count(); i++)
            {
                firstItem = null;
                secondItem = null;
                if (dict.Keys.Contains(i))
                    firstItem = dict[i];
                if (dict.Keys.Contains(i + 1))
                    secondItem = dict[i + 1];

                if (firstItem != null && secondItem != null)
                {
                    if (!firstItem.IsSelected && secondItem.IsSelected)
                    {
                        dict[i] = secondItem;
                        dict[i + 1] = firstItem;
                    }
                }
            }
            WriteValues(dict);
        }

        public static void MoveDown(IScheduledOrder[] items)
        {
            Dictionary<int, IScheduledOrder> dict = items.ToDictionary(key => key.ScheduledOrder ?? 0, val => val);
            IScheduledOrder firstItem = null;
            IScheduledOrder secondItem = null;
            for (int i = dict.Count(); i > -1; i--)
            {
                firstItem = null;
                secondItem = null;
                if (dict.Keys.Contains(i))
                    firstItem = dict[i];
                if (dict.Keys.Contains(i - 1))
                    secondItem = dict[i - 1];

                if (firstItem != null && secondItem != null)
                {
                    if (!firstItem.IsSelected && secondItem.IsSelected)
                    {
                        dict[i] = secondItem;
                        dict[i - 1] = firstItem;
                    }
                }
            }
            WriteValues(dict);
        }

        private static void WriteValues(Dictionary<int, IScheduledOrder> dict)
        {
            foreach (KeyValuePair<int, IScheduledOrder> kvp in dict)
                kvp.Value.ScheduledOrder = kvp.Key;
        }
    }
}
