// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.ext.xamldom.avui
{
    public class NullExtension : MarkupExtension
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public NullExtension()
        {
        }

        /// <summary>
        /// Return an object that should be set on the targetObject's targetProperty
        /// for this markup extension. In this case it is simply null.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        /// <returns>
        /// The object to set on this property.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider) => null;
    }
}
