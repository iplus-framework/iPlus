// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Windows;
using gip.ext.xamldom.avui;
using gip.ext.design.avui;

namespace gip.ext.designer.avui.Xaml
{
    sealed class XamlModelPropertyCollection : DesignItemPropertyCollection
    {
        XamlDesignItem _item;

        public XamlModelPropertyCollection(XamlDesignItem item)
        {
            this._item = item;
        }

        public override DesignItemProperty HasProperty(string name)
        {
            XamlProperty property = _item.XamlObject.FindProperty(name);
            if (property == null)
                return null;
            return new XamlModelProperty(_item, property);
        }

        public override DesignItemProperty GetProperty(string name)
        {
            return new XamlModelProperty(_item, _item.XamlObject.FindOrCreateProperty(name));
        }

        public override DesignItemProperty GetAttachedProperty(Type ownerType, string name)
        {
            return new XamlModelProperty(_item, _item.XamlObject.FindOrCreateAttachedProperty(ownerType, name));
        }

        public override System.Collections.Generic.IEnumerator<DesignItemProperty> GetEnumerator()
        {
            yield break;
        }
    }
}
