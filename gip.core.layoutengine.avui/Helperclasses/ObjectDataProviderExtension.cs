using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    public static class ObjectDataProviderExtension
    {
        public static ObjectDataProvider CreateObjectDataProvider(string acSource, object dsSource, string dsPath)
        {
            ObjectDataProvider odp = new ObjectDataProvider();
            var methodParams = ACUrlHelper.ExtractParamsForObjectDataProvider(acSource);
            if (methodParams != null)
            {
                foreach (var param in methodParams)
                {
                    odp.MethodParameters.Add(param);
                }
            }
            if (dsSource != null)
                odp.ObjectInstance = dsSource;
            if (!string.IsNullOrEmpty(dsPath))
                odp.MethodName = dsPath.Substring(1);  // "!" bei Methodenname entfernen
            return odp;
        }
    }
}