using combit.ListLabel17;
using System;

namespace gip.core.reporthandler
{
    public class LinxByteMappingAttribute : Attribute
    {
        public int Order { get; set; }
        public int Length { get; set; }

        /// <summary>
        ///  Avoid null value problem by attribute
        /// </summary>
        public int? _DefaultValue { get; set; }
        public int DefaultValue
        {
            get
            {
                return _DefaultValue ?? 0;
            }
            set
            {
                _DefaultValue = value;
            }
        }
    }
}
