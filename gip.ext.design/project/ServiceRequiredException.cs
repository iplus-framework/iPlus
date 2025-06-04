// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace gip.ext.design
{
    /// <summary>  
    /// Exception class used for designer failures.  
    /// </summary>  
    [Serializable]
    public class ServiceRequiredException : DesignerException
    {
        /// <summary>  
        /// Gets the missing service.  
        /// </summary>  
        public Type ServiceType { get; private set; }

        /// <summary>  
        /// Create a new ServiceRequiredException instance.  
        /// </summary>  
        public ServiceRequiredException(Type serviceType)
            : base("Service " + serviceType.FullName + " is required.")
        {
            this.ServiceType = serviceType;
        }

        /// <summary>  
        /// Create a new ServiceRequiredException instance.  
        /// </summary>  
        public ServiceRequiredException()
        {
        }

        /// <summary>  
        /// Create a new ServiceRequiredException instance.  
        /// </summary>  
        public ServiceRequiredException(string message) : base(message)
        {
        }

        /// <summary>  
        /// Create a new ServiceRequiredException instance.  
        /// </summary>  
        public ServiceRequiredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>  
        /// Create a new ServiceRequiredException instance.  
        /// </summary>  
        //protected ServiceRequiredException(SerializationInfo info, StreamingContext context)
        //    : base(info, context)
        //{
        //    if (info == null)
        //        throw new ArgumentNullException(nameof(info));
        //    this.ServiceType = (Type)info.GetValue("ServiceType", typeof(Type));
        //}

        /// <inheritdoc/>  
        [Obsolete]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            base.GetObjectData(info, context);
            info.AddValue("ServiceType", this.ServiceType);
        }
    }
}
