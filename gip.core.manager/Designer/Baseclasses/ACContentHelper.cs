using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.manager
{
    public static class ACContentHelper
    {
        public static IEnumerable<IACObject> GetACContentList(this IACObject acValue)
        {
            List<IACObject> acContentList = new List<IACObject>();

            ACContentHelper.FillACContentList(ref acContentList, acValue);

            return acContentList;
        }

        private static void FillACContentList(ref List<IACObject> acContentList, IACObject acValue)
        {
            if (acValue.ACContentList != null)
            {
                foreach (var acValueItem in acValue.ACContentList)
                {
                    acContentList.Add(acValueItem);
                    if (acValueItem is IACContainer)
                    {
                        FillACContentList(ref acContentList, acValueItem);
                    }
                }
            }
        }

        public static IACObject GetACValue(this IACObject acValue, Type t, int maxLevel = 1)
        {
            var tt = acValue.GetType();

            if (t.IsInterface)
            {
                if (tt.GetInterface(t.Name, false) != null)
                    return acValue;
            }
            else if (t.IsClass)
            {
                if (tt == t)
                {
                    return acValue;
                }
                else
                {
                    while (tt.BaseType != null)
                    {
                        if (tt.BaseType == t)
                            return acValue;
                        tt = tt.BaseType;
                    }
                }
            }

            if (maxLevel == 0)
                return null;

            return GetACValueRekursive(acValue, t, 1, maxLevel);
        }

        private static IACObject GetACValueRekursive(IACObject acValue, Type t, int currentLevel, int maxLevel)
        {

            // Ist "instance" selbst der "t"
            IEnumerable<IACObject> acContentList = GetACContentList(acValue);

            foreach (var contentACValue in acContentList)
            {
                if (contentACValue == null)
                    continue;
                var tt = contentACValue.GetType();

                if (contentACValue is PWOfflineNodeMethod)
                {
                    var tt2 = (contentACValue as PWOfflineNodeMethod).ContentACClassWF.GetType();
                    if (tt2.GetInterface(t.Name, false) != null)
                        return (contentACValue as PWOfflineNodeMethod).ContentACClassWF;
                    
                }

                if (t.IsInterface)
                {
                    if (tt.GetInterface(t.Name, false) != null)
                        return contentACValue;
                }
                else if (t.IsClass)
                {
                    if (tt == t)
                    {
                        return contentACValue;
                    }
                    else
                    {
                        while (tt.BaseType != null)
                        {
                            if (tt.BaseType == t)
                                return contentACValue;
                            tt = tt.BaseType;
                        }
                    }
                    
                }

                if (contentACValue is IACContainer)
                {
                    object value = (contentACValue as IACContainer).Value;
                    if (value != null && t.IsAssignableFrom(value.GetType()))
                    {
                        return (contentACValue as IACContainer).Value as IACObject;
                    }
                }
            }


            if (currentLevel < maxLevel)
            {
                foreach (var contentACValue in acContentList)
                {
                    var result = GetACValueRekursive(contentACValue, t, currentLevel + 1, maxLevel);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        public static IACObject GetFirstInnerValue(this IACObject instance)
        {
            if (instance.ACContentList == null || !instance.ACContentList.Any())
                return null;
            return instance.ACContentList.First() as IACObject;
        }
    }
}
