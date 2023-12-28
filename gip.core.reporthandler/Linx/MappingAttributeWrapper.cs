using System.Reflection;

namespace gip.core.reporthandler
{
    public class MappingAttributeWrapper
    {
        public PropertyInfo PropertyInfo { get; set; }
        public LinxByteMappingAttribute LinxByteMapping { get; set; }
    }
}
