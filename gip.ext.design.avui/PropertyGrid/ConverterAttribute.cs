using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace gip.ext.design.avui.PropertyGrid
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public class ConverterAttribute : Attribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourcetype"></param>
        /// <param name="targetType"></param>
        public ConverterAttribute(Type sourcetype, Type targetType)
        {
            SourceType = sourcetype;
            TargetType = targetType;
        }

        #region public Member

        /// <summary>
        /// 
        /// </summary>
        public Type SourceType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Type TargetType { get; set; }
        #endregion
    }
}
