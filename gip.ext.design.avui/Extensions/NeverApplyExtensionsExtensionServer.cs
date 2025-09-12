// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;

namespace gip.ext.design.avui.Extensions
{
    // An extension server that never applies its extensions - used for special extensions
    // like CustomInstanceFactory
    sealed class NeverApplyExtensionsExtensionServer : ExtensionServer
    {
        public override bool ShouldApplyExtensions(DesignItem extendedItem)
        {
            return false;
        }

        public override Extension CreateExtension(Type extensionType, DesignItem extendedItem)
        {
            throw new NotImplementedException();
        }

        public override void RemoveExtension(Extension extension)
        {
            throw new NotImplementedException();
        }

        // since the event is never raised, we don't have to store the event handlers
        public override event EventHandler<DesignItemCollectionEventArgs> ShouldApplyExtensionsInvalidated
        {
            add { }
            remove { }
        }
    }
}
