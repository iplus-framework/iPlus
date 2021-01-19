using System.Collections.Generic;
using System.Linq;

namespace gip.core.datamodel
{
    /// <summary>
    /// Set sequence fields to be ordinal numbers without gaps
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SequenceManager<T> where T : ISequence
    {
        public static void Order(IEnumerable<T> list)
        {
            if (list != null && list.Any())
            {
                list = list.OrderBy(x => x.Sequence);
                int i = 0;
                list.ToList().ForEach(x => x.Sequence = ++i);
            }
        }


        public static void Order(ref List<T> list)
        {
            if (list != null && list.Any())
            {
                list = list.OrderBy(x => x.Sequence).ToList();
                int i = 0;
                list.ForEach(x => x.Sequence = ++i);
            }
        }
    }
}
