// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

// enable this define to test that event handlers are removed correctly
//#define EventHandlerDebugging

using System;
using System.Diagnostics;
using System.Windows;
using gip.ext.xamldom;
using gip.ext.designer.Services;
using System.Windows.Markup;
using System.Collections.Generic;
using gip.ext.design;
using System.Windows.Controls;

namespace gip.ext.designer.Xaml
{
//    [DebuggerDisplay("XamlDesignItem: {ComponentType.Name}")]
    sealed class XamlDesignItem : DesignItem
    {
        readonly XamlObject _xamlObject;
        readonly XamlDesignContext _designContext;
        readonly XamlModelPropertyCollection _properties;
        UIElement _view;

        public XamlDesignItem(XamlObject xamlObject, XamlDesignContext designContext)
        {
            this._xamlObject = xamlObject;
            this._designContext = designContext;
            this._properties = new XamlModelPropertyCollection(this);
        }

        internal XamlComponentService ComponentService
        {
            get
            {
                return _designContext._componentService;
            }
        }

        internal XamlObject XamlObject
        {
            get { return _xamlObject; }
        }

        public override object Component
        {
            get
            {
                return _xamlObject.Instance;
            }
        }

        public override Type ComponentType
        {
            get { return _xamlObject.ElementType; }
        }

        public override string Name
        {
            get
            {
                DesignItemProperty property = HasProperty("Name");
                if (property == null)
                    return "";
                return (string)this.Properties["Name"].ValueOnInstance;
            }
            set
            {
                this.Properties["Name"].SetValue(value);
            }
        }

#if EventHandlerDebugging
		static int totalEventHandlerCount;
#endif

        /// <summary>
        /// Is raised when the name of the design item changes.
        /// </summary>
        public override event EventHandler NameChanged
        {
            add
            {
#if EventHandlerDebugging
				Debug.WriteLine("Add event handler to " + this.ComponentType.Name + " (handler count=" + (++totalEventHandlerCount) + ")");
#endif
                this.Properties["Name"].ValueChanged += value;
            }
            remove
            {
#if EventHandlerDebugging
				Debug.WriteLine("Remove event handler from " + this.ComponentType.Name + " (handler count=" + (--totalEventHandlerCount) + ")");
#endif
                this.Properties["Name"].ValueChanged -= value;
            }
        }

        public override DesignItem Parent
        {
            get
            {
                if (_xamlObject.ParentProperty == null)
                    return null;
                else
                    return ComponentService.GetDesignItem(_xamlObject.ParentProperty.ParentObject.Instance);
            }
        }

        public override DesignItemProperty ParentProperty
        {
            get
            {
                DesignItem parent = this.Parent;
                if (parent == null)
                    return null;
                XamlProperty prop = _xamlObject.ParentProperty;
                if (prop.IsAttached)
                {
                    return parent.Properties.GetAttachedProperty(prop.PropertyTargetType, prop.PropertyName);
                }
                else
                {
                    return parent.Properties.GetProperty(prop.PropertyName);
                }
            }
        }

        public override DesignItem Style
        {
            get
            {
                if (!(this.View is FrameworkElement))
                    return null;

                DesignItemProperty styleProp = Properties.GetProperty(FrameworkElement.StyleProperty);
                if ((styleProp.Value == null) || !(styleProp.Value is DesignItem))
                {
                    TypeExtension typeExtension = new TypeExtension(ComponentType);
                    DesignItem typeExtensionItem = Services.Component.RegisterComponentForDesigner(typeExtension);

                    Style style = new Style(ComponentType);
                    DesignItem styleItem = Services.Component.RegisterComponentForDesigner(style);
                    styleItem.Properties["TargetType"].SetValue(typeExtensionItem);

                    typeExtensionItem.Properties["Type"].SetValue(ComponentType);

                    styleProp.SetValue(styleItem);
                }
                return styleProp.Value;
            }
        }

        /// <summary>
        /// Occurs when the parent of this design item changes.
        /// </summary>
        public override event EventHandler ParentChanged
        {
            add { _xamlObject.ParentPropertyChanged += value; }
            remove { _xamlObject.ParentPropertyChanged += value; }
        }

        public override UIElement View
        {
            get
            {
                if (_view != null)
                    return _view;
                else
                    return this.Component as UIElement;
            }
        }

        internal void SetView(UIElement newView)
        {
            _view = newView;
        }

        public override DesignContext Context
        {
            get { return _designContext; }
        }

        public override DesignItemPropertyCollection Properties
        {
            get { return _properties; }
        }

        public override DesignItemProperty HasProperty(string name)
        {
            return Properties.HasProperty(name);
        }

        public override IList<DesignItemProperty> SettedProperties
        {
            get
            {
                List<DesignItemProperty> result = new List<DesignItemProperty>();
                if (XamlObject == null)
                    return result;
                foreach (XamlProperty xamlProperty in XamlObject.Properties)
                {
                    if (xamlProperty.IsSet)
                        result.Add(new XamlModelProperty(this, xamlProperty));
                }
                return result;
            }
        }


        internal void NotifyPropertyChanged(XamlModelProperty property)
        {
            Debug.Assert(property != null);
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs(property.Name));
        }

        public override string ContentPropertyName
        {
            get
            {
                return XamlObject.ContentPropertyName;
            }
        }

        public override DesignItem Clone()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            if (this.Parent.View is Canvas)
            {
                DesignItemProperty top = Properties.GetAttachedProperty(Canvas.TopProperty);
                DesignItemProperty left = Properties.GetAttachedProperty(Canvas.LeftProperty);
                DesignItemProperty height = Properties[FrameworkElement.HeightProperty];
                DesignItemProperty width = Properties[FrameworkElement.WidthProperty];

                if (top != null && left != null && height != null && width != null)
                {
                    return string.Format("XamlDesignItem: {0} ({1},{2},{3},{4})", View.ToString(), left.CurrentValue, top.CurrentValue, width.CurrentValue, height.CurrentValue);
                }
            }
            if (ComponentType != null && View != null)
                return string.Format("XamlDesignItem: {0}", View.ToString());

            return base.ToString();
        } 
    }
}
