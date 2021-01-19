// Copyright (c) AlphaSierraPapa for the SharpDevelop Team (for details please see \doc\copyright.txt)
// This code is distributed under the GNU LGPL (for details please see \doc\license.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Markup;

namespace gip.ext.design.PropertyGrid
{
    /// <summary>
    /// View-Model class for the property grid.
    /// </summary>
    public class PropertyNode : IPropertyNode
    {
        protected static object Unset = new object();

        /// <summary>
        /// Gets the properties that are presented by this node.
        /// This might be multiple properties if multiple controls are selected.
        /// </summary>
        public ReadOnlyCollection<DesignItemProperty> Properties { get; private set; }

        protected bool raiseEvents = true;
        protected bool hasStringConverter;

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        public string Name { get { return FirstProperty.Name; } }

        /// <summary>
        /// Gets if this property node represents an event.
        /// </summary>
        public bool IsEvent { get { return FirstProperty.IsEvent; } }

        /// <summary>
        /// Gets if this property node represents an event.
        /// </summary>
        public bool IsDependencyProperty { get { return FirstProperty.IsDependencyProperty; } }

        /// <summary>
        /// Gets the design context associated with this set of properties.
        /// </summary>
        public DesignContext Context { get { return FirstProperty.DesignItem.Context; } }

        /// <summary>
        /// Gets the service container associated with this set of properties.
        /// </summary>
        public ServiceContainer Services { get { return FirstProperty.DesignItem.Services; } }

        /// <summary>
        /// Gets the editor control that edits this property.
        /// </summary>
        public FrameworkElement Editor { get; protected set; }

        /// <summary>
        /// Gets the first property (equivalent to Properties[0])
        /// </summary>
        public DesignItemProperty FirstProperty { get { return Properties[0]; } }

        /// <summary>
        /// For nested property nodes, gets the parent node.
        /// </summary>
        public PropertyNode Parent { get; private set; }

        /// <summary>
        /// For nested property nodes, gets the level of this node.
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// Gets the category of this node.
        /// </summary>
        public Category Category { get; set; }

        /// <summary>
        /// Gets the list of child nodes.
        /// </summary>
        public ObservableCollection<PropertyNode> Children { get; private set; }

        /// <summary>
        /// Gets the list of advanced child nodes (not visible by default).
        /// </summary>
        public ObservableCollection<PropertyNode> MoreChildren { get; private set; }

        bool isExpanded;

        /// <summary>
        /// Gets whether this property node is currently expanded.
        /// </summary>
        public bool IsExpanded
        {
            get
            {
                return isExpanded;
            }
            set
            {
                isExpanded = value;
                UpdateChildren();
                RaisePropertyChanged("IsExpanded");
            }
        }

        /// <summary>
        /// Gets whether this property node has children.
        /// </summary>
        public bool HasChildren
        {
            get
            {
                return Children.Count > 0 || MoreChildren.Count > 0;
            }
        }

        /// <summary>
        /// Gets the description object using the IPropertyDescriptionService.
        /// </summary>
        public object Description
        {
            get
            {
                IPropertyDescriptionService s = Services.GetService<IPropertyDescriptionService>();
                if (s != null)
                {
                    return s.GetDescription(FirstProperty);
                }
                return null;
            }
        }

        /// <summary>
        /// Gets/Sets the value of this property.
        /// </summary>
        public virtual object Value
        {
            get
            {
                if (IsAmbiguous) return null;
                var result = FirstProperty.ValueOnInstance;
                if (result == DependencyProperty.UnsetValue) return null;
                return result;
            }
            set
            {
                SetValueCore(value);
            }
        }

        /// <summary>
        /// Gets/Sets the value of this property in string form
        /// </summary>
        public string ValueString
        {
            get
            {
                if (ValueItem == null || ValueItem.Component is MarkupExtension)
                {
                    if (Value == null) return null;
                    if (hasStringConverter)
                    {
                        try
                        {
                            return FirstProperty.TypeConverter.ConvertToInvariantString(Value);
                        }
                        catch (Exception /*e*/)
                        {
                            return "(" + Value.GetType().Name + ")";
                        }
                    }
                    return "(" + Value.GetType().Name + ")";
                }
                return "(" + ValueItem.ComponentType.Name + ")";
            }
            set
            {
                // make sure we only catch specific exceptions
                // and/or show the error message to the user
                //try {
                Value = FirstProperty.TypeConverter.ConvertFromInvariantString(value);
                //} catch {
                //	OnValueOnInstanceChanged();
                //}
            }
        }

        /// <summary>
        /// Gets whether the property node is enabled for editing.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return ValueItem == null && hasStringConverter;
            }
        }

        /// <summary>
        /// Gets whether this property was set locally.
        /// </summary>
        public bool IsSet
        {
            get
            {
                foreach (var p in Properties)
                {
                    if (p.IsSet) return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Gets the color of the name.
        /// Depends on the type of the value (binding/resource/etc.)
        /// </summary>
        public Brush NameForeground
        {
            get
            {
                if (ValueItem != null)
                {
                    object component = ValueItem.Component;
                    if (component is BindingBase)
                        return Brushes.DarkGoldenrod;
                    if (component is StaticResourceExtension || component is DynamicResourceExtension)
                        return Brushes.DarkGreen;
                }
                return SystemColors.WindowTextBrush;
            }
        }

        /// <summary>
        /// Returns the DesignItem that owns the property (= the DesignItem that is currently selected).
        /// Returns null if multiple DesignItems are selected.
        /// </summary>
        public DesignItem ValueItem
        {
            get
            {
                if (Properties.Count == 1)
                {
                    return FirstProperty.Value;
                }
                return null;
            }
        }

        public IEnumerable<DesignItem> ValueItems
        {
            get
            {
                List<DesignItem> designItems = new List<DesignItem>();
                foreach (DesignItemProperty property in Properties)
                {
                    designItems.Add(property.Value);
                }
                return designItems;
            }
        }

        /// <summary>
        /// Gets whether the property value is ambiguous (multiple controls having different values are selected).
        /// </summary>
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

        /// <summary>
        /// Gets/Sets whether the property is visible.
        /// </summary>
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

        /// <summary>
        /// Gets whether resetting the property is possible.
        /// </summary>
        public bool CanReset
        {
            get { return IsSet; }
        }

        /// <summary>
        /// Resets the property.
        /// </summary>
        public void Reset()
        {
            SetValueCore(Unset);
            OnCreateEditor(FirstProperty);
        }

        public void RefreshEditor()
        {
            OnCreateEditor(FirstProperty);
        }

        /// <summary>
        /// Replaces the value of this node with a new binding.
        /// </summary>
        public virtual void CreateBindings()
        {
            if (IsEvent || !IsDependencyProperty || Properties.Count <= 0)
                return;
            raiseEvents = false;
            foreach (DesignItemProperty property in Properties)
            {
                property.SetValue(new Binding());
            }
            raiseEvents = true;
            OnValueChanged();
            IsExpanded = true;
        }

        /// <summary>
        /// Replaces the value of this node with a new binding.
        /// </summary>
        public virtual void CreateMultiBindings()
        {
            List<MultiBinding> bindings = CreateMultiBindingIntern();
            if ((bindings == null) || (bindings.Count <= 0))
                return;
            if (bindings.Count != Properties.Count)
                return;
            int i = 0;
            raiseEvents = false;
            foreach (DesignItemProperty property in Properties)
            {
                MultiBinding binding = bindings[i];
                property.SetValue(binding);
                if ((property.Value != null) && (binding.Converter != null))
                    property.Value.Properties["Converter"].SetValue(binding.Converter);
                i++;
            }
            raiseEvents = true;
            OnValueChanged();
            //Value = binding;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected List<MultiBinding> CreateMultiBindingIntern()
        {
            if (IsEvent || !IsDependencyProperty || Properties.Count <= 0)
                return null;
            List<MultiBinding> bindings = new List<MultiBinding>();
            foreach (DesignItemProperty property in Properties)
            {
                IMultiValueConverter converter = ConverterManager.CreateMultiValueConverter(this.FirstProperty.ReturnType);
                if (converter == null)
                    return null;
                MultiBinding binding = new MultiBinding();
                binding.Converter = converter;
                bindings.Add(binding);
            }
            return bindings;
        }

        protected virtual void SetValueCore(object value)
        {
            raiseEvents = false;
            if (value == Unset)
            {
                foreach (var p in Properties)
                {
                    p.Reset();
                }
            }
            else
            {
                foreach (var p in Properties)
                {
                    p.SetValue(value);
                }
            }
            raiseEvents = true;
            OnValueChanged();
        }

        protected void OnValueChanged()
        {
            RaisePropertyChanged("IsSet");
            RaisePropertyChanged("Value");
            RaisePropertyChanged("ValueString");
            RaisePropertyChanged("IsAmbiguous");
            RaisePropertyChanged("FontWeight");
            RaisePropertyChanged("IsEnabled");
            RaisePropertyChanged("NameForeground");

            UpdateChildren();
        }

        void OnValueOnInstanceChanged()
        {
            RaisePropertyChanged("Value");
            RaisePropertyChanged("ValueString");
        }

        /// <summary>
        /// Creates a new PropertyNode instance.
        /// </summary>
        public PropertyNode()
        {
            Children = new ObservableCollection<PropertyNode>();
            MoreChildren = new ObservableCollection<PropertyNode>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="properties"></param>
        /// <param name="parent"></param>
        protected PropertyNode(DesignItemProperty[] properties, PropertyNode parent)
            : this()
        {
            this.Parent = parent;
            this.Level = parent == null ? 0 : parent.Level + 1;
            Load(properties);
        }

        /// <summary>
        /// Initializes this property node with the specified properties.
        /// </summary>
        public void Load(DesignItemProperty[] properties)
        {
            if (this.Properties != null)
            {
                // detach events from old properties
                foreach (var property in this.Properties)
                {
                    property.ValueChanged -= new EventHandler(property_ValueChanged);
                    property.ValueOnInstanceChanged -= new EventHandler(property_ValueOnInstanceChanged);
                }
            }

            this.Properties = new ReadOnlyCollection<DesignItemProperty>(properties);

            OnCreateEditor(FirstProperty);

            foreach (var property in properties)
            {
                property.ValueChanged += new EventHandler(property_ValueChanged);
                property.ValueOnInstanceChanged += new EventHandler(property_ValueOnInstanceChanged);
            }

            hasStringConverter =
                FirstProperty.TypeConverter.CanConvertFrom(typeof(string)) &&
                FirstProperty.TypeConverter.CanConvertTo(typeof(string));

            OnValueChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        protected virtual FrameworkElement OnCreateEditor(DesignItemProperty property)
        {
            if (Editor == null)
            {
                Editor = EditorManager.CreateEditor(FirstProperty);
            }
            return Editor;
        }

        void property_ValueOnInstanceChanged(object sender, EventArgs e)
        {
            if (raiseEvents) OnValueOnInstanceChanged();
        }

        void property_ValueChanged(object sender, EventArgs e)
        {
            if (raiseEvents) OnValueChanged();
        }

        /// <summary>
        /// 
        /// </summary>
        virtual protected void UpdateChildren()
        {
            Children.Clear();
            MoreChildren.Clear();

            if (Parent == null || Parent.IsExpanded)
            {
                if (ValueItem != null)
                {
                    var list = TypeHelper.GetAvailableProperties(ValueItem.Component)
                        .OrderBy(d => d.Name)
                        .Select(d => new PropertyNode(new[] { ValueItem.Properties[d.Name] }, this));

                    foreach (var node in list)
                    {
                        if (Metadata.IsBrowsable(node.FirstProperty))
                        {
                            node.IsVisible = true;
                            if (Metadata.IsPopularProperty(node.FirstProperty))
                            {
                                Children.Add(node);
                            }
                            else
                            {
                                MoreChildren.Add(node);
                            }
                        }
                    }
                }
            }

            RaisePropertyChanged("HasChildren");
        }

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property has changed. Used to support WPF data binding.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        protected void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion
    }
}
