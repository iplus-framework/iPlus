// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.ext.design;
using System.Collections.ObjectModel;
using System.Collections;
using gip.ext.designer;
using gip.ext.xamldom;
using gip.ext.design.PropertyGrid;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Markup;

namespace gip.ext.designer.OutlineView
{
    [CLSCompliant(false)]
    public abstract class TriggerOutlineNodeBase : OutlineNodeBase, IPropertyNode
    {

        protected override OutlineNodeBase OnCreateChildrenNode(DesignItem child)
        {
            return null;
        }


        public TriggerOutlineNodeBase(DesignItem trigger, DesignItem designItem)
            : base(trigger)
        {
            _DesignObject = designItem;
            _PropertyNodeChildren = new ObservableCollection<PropertyNode>();
            _PropertyNodeMoreChildren = new ObservableCollection<PropertyNode>();

            Load();
        }

        public void Load()
        {
            if (_Properties != null)
            {
                // detach events from old properties
                foreach (var property in _Properties)
                {
                    property.ValueChanged -= new EventHandler(property_ValueChanged);
                    property.ValueOnInstanceChanged -= new EventHandler(property_ValueOnInstanceChanged);
                }
            }

            _Properties = null;

            foreach (var property in Properties)
            {
                property.ValueChanged += new EventHandler(property_ValueChanged);
                property.ValueOnInstanceChanged += new EventHandler(property_ValueOnInstanceChanged);
            }

            if (FirstProperty != null)
            {
                hasStringConverter =
                    FirstProperty.TypeConverter.CanConvertFrom(typeof(string)) &&
                    FirstProperty.TypeConverter.CanConvertTo(typeof(string));
            }

            OnValueChanged();
        }


        protected  DesignItem _DesignObject;

        public DesignItem TriggerItem
        {
            get
            {
                return this.DesignItem;
            }
        }

        public abstract string TriggerInfoText
        {
            get;
        }

        public abstract object Description
        {
            get;
        }


        protected FrameworkElement _Editor;
        public virtual FrameworkElement Editor 
        {
            get
            {
                if (_Editor == null)
                    _Editor = new TextBox();
                return _Editor;
            }

            protected set
            {
                _Editor = value;
            }
        }

        protected ReadOnlyCollection<DesignItemProperty> _Properties;
        public abstract ReadOnlyCollection<DesignItemProperty> Properties
        {
            get;
        }

        public bool IsEvent
        {
            get 
            {
                return false;
            }
        }

        public abstract bool IsDependencyProperty
        {
            get;
        }

        public DesignContext Context
        {
            get 
            {
                return this.DesignItem.Context;
            }
        }

        public ServiceContainer Services
        {
            get 
            {
                return this.DesignItem.Services;
            }
        }

        public abstract DesignItemProperty FirstProperty
        {
            get;
        }

        public new PropertyNode Parent
        {
            get 
            {
                return null;
            }
        }

        public int Level
        {
            get { return 0; }
        }

        public Category Category
        {
            get; set;
        }

        ObservableCollection<PropertyNode> _PropertyNodeChildren;
        /// <summary>
        /// Gets the list of child nodes.
        /// </summary>
        ObservableCollection<PropertyNode> IPropertyNode.Children 
        {
            get
            {
                return _PropertyNodeChildren;
            }
        }

        ObservableCollection<PropertyNode> _PropertyNodeMoreChildren;
        /// <summary>
        /// Gets the list of advanced child nodes (not visible by default).
        /// </summary>
        ObservableCollection<PropertyNode> IPropertyNode.MoreChildren 
        {
            get
            {
                return _PropertyNodeMoreChildren;
            }
        }

        public bool HasChildren
        {
            get
            {
                return (this as IPropertyNode).Children.Count > 0 || (this as IPropertyNode).MoreChildren.Count > 0;
            }
        }

        public abstract object Value
        {
            get; set;
        }

        public abstract string ValueString
        {
            get; set; 
        }

        protected bool hasStringConverter;
        public virtual bool IsEnabled
        {
            get 
            {
                if ((TriggerItem.Component != null) && (TriggerItem.Component as TriggerBase).IsSealed)
                    return false;
                return true;
            }
        }

        public abstract bool IsSet { get; }

        public Brush NameForeground
        {
			get {
				if (ValueItem != null) {
					object component = ValueItem.Component;
					if (component is BindingBase)
						return Brushes.DarkGoldenrod;
					if (component is StaticResourceExtension || component is DynamicResourceExtension)
						return Brushes.DarkGreen;
				}
				return SystemColors.WindowTextBrush;
			}
        }

        public abstract DesignItem ValueItem
        {
            get;
        }

        public bool IsAmbiguous
        {
            get
            {
                foreach (var p in Properties)
                {
                    if (!object.Equals(p.ValueOnInstance, FirstProperty.ValueOnInstance))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        bool isVisible;
        public bool IsVisible
        {
            get
            {
                return isVisible;
            }
            set
            {
                isVisible = value;
                RaisePropertyChanged("IsVisible");
            }
        }

        public bool CanReset
        {
            get { return IsSet; }
        }

        protected static object Unset = new object();
        public virtual void Reset()
        {
            SetValueCore(Unset);
        }

        public virtual void CreateBindings()
        {
        }

        public virtual void CreateMultiBindings()
        {
        }

        protected bool raiseEvents = true;
        protected abstract void SetValueCore(object value);

        protected virtual void OnValueChanged()
        {
            RaisePropertyChanged("IsSet");
            RaisePropertyChanged("Value");
            RaisePropertyChanged("ValueString");
            RaisePropertyChanged("IsAmbiguous");
            RaisePropertyChanged("FontWeight");
            RaisePropertyChanged("IsEnabled");
            RaisePropertyChanged("NameForeground");
        }

        protected virtual void OnValueOnInstanceChanged()
        {
            RaisePropertyChanged("Value");
            RaisePropertyChanged("ValueString");
        }

        protected virtual void property_ValueOnInstanceChanged(object sender, EventArgs e)
        {
            if (raiseEvents) OnValueOnInstanceChanged();
        }

        protected virtual void property_ValueChanged(object sender, EventArgs e)
        {
            if (raiseEvents) OnValueChanged();
        }
    }
}
