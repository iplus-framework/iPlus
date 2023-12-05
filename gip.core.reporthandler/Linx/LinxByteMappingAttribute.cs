using System;

namespace gip.core.reporthandler
{
    public class LinxByteMappingAttribute : Attribute
    {
        public int Order { get;set; }
        public int Length { get;set; }
    }
}
