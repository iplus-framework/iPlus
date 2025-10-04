using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Styling;
using gip.core.datamodel;
using gip.core.layoutengine.avui.PropertyGrid.Editors;
using gip.ext.design.avui;
using gip.ext.design.avui.PropertyGrid;
using gip.ext.designer.avui.PropertyGrid;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a manager for VBEditor
    /// </summary>
    public static class VBEditorManager
    {
        public static Control CreateEditor(DesignItemProperty property)
        {
            Type editorType;
            if ((property.Name == "VBDesignName") && (typeof(VBVisual).IsAssignableFrom(property.DesignItem.ComponentType)))
            {
                if ((property.DesignItem.View as VBVisual).ContentACObject != null)
                {
                    var query = (property.DesignItem.View as VBVisual).ContentACObject.ACType.Designs.Where(c => c.ACUsageIndex == (short)Global.ACUsages.DUControl);
                    if (((property.DesignItem.View as VBVisual).ContentACObject is IACComponent) && query.Any())
                    {
                        return new VBComboBoxEditor() { ItemsSource = query.Select(c => c.ACIdentifier).ToList() };
                    }
                    else
                    {
                        ACClass acClass = (property.DesignItem.View as VBVisual).ContentACObject as ACClass;
                        if (acClass != null)
                        {
                            var query2 = acClass.Designs.Where(c => c.ACUsageIndex == (short)Global.ACUsages.DUControl);
                            if (query2.Any())
                            {
                                return new VBComboBoxEditor() { ItemsSource = query2.Select(c => c.ACIdentifier).ToList() };
                            }
                        }
                    }
                }
            }
            if (!EditorManager.propertyEditors.TryGetValue(property.FullName, out editorType))
            {
                var type = property.ReturnType;
                while (type != null)
                {
                    if (EditorManager.typeEditors.TryGetValue(type, out editorType))
                    {
                        break;
                    }
                    type = type.BaseType;
                }
                if (editorType == null)
                {
                    var standardValues = Metadata.GetStandardValues(property.ReturnType);
                    if (standardValues != null)
                    {
                        return new VBComboBoxEditor() { ItemsSource = standardValues };
                    }
                    return new VBTextBoxEditor();
                }
            }
            Control result = (Control)Activator.CreateInstance(editorType);
            if (result is VBComboBoxEditor)
            {
                var standardValues = Metadata.GetStandardValues(property.ReturnType);
                if (standardValues != null)
                {
                    (result as VBComboBoxEditor).ItemsSource = standardValues;
                }
            }
            return result;
        }
    }

    /// <summary>
    /// Represents a node for properties.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt einen Knoten für Eigenschaften dar.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBPropertyNode'}de{'VBPropertyNode'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class VBPropertyNode : PropertyNode, IACInteractiveObject, IBindingDropHandler, IACObject
    {
        protected override Control OnCreateEditor(DesignItemProperty property)
        {
            if ((TypeOfTriggerValue != null) && (TypeOfTriggerValue.IsEnum) && ((Editor == null) || !(Editor is VBComboBoxEditor)))
            {
                var standardValues = gip.ext.design.avui.Metadata.GetStandardValues(TypeOfTriggerValue);
                Editor = new VBComboBoxEditor() { ItemsSource = standardValues };
                Editor.DataContext = this;
            }
            else if ((FirstProperty.Value != null) && (   typeof(BindingBase).IsAssignableFrom(FirstProperty.Value.ComponentType)
                                                  || typeof(VBBindingExt).IsAssignableFrom(FirstProperty.Value.ComponentType)))
            {
                if ( (Editor == null) || 
                    ((Editor != null) && !(Editor is VBBindingTypeEditor)))
                {
                    Editor = new VBBindingTypeEditor();
                    RaisePropertyChanged("Editor");
                }
            }
            else if ((Editor != null) && (Editor is VBBindingTypeEditor) && (FirstProperty.Value == null))
            {
                Editor = VBEditorManager.CreateEditor(FirstProperty);
                RaisePropertyChanged("Editor");
            }
            else if ((Editor != null) && (Editor is VBComboBoxEditor) && (FirstProperty.Name == "VBDesignName"))
            {
                if ((FirstProperty.DesignItem.View as VBVisual).ContentACObject != null)
                {
                    Editor = VBEditorManager.CreateEditor(FirstProperty);
                    RaisePropertyChanged("Editor");
                }
            }

            if (Editor == null)
            {
                Editor = VBEditorManager.CreateEditor(FirstProperty);
                //RaisePropertyChanged("Editor");
            }
            return Editor;
        }

        public VBPropertyNode()
            : base()
        {
        }

        protected VBPropertyNode(DesignItemProperty[] properties, PropertyNode parent)
            : base(properties, parent)
		{
		}

        public override object Value
        {
            get
            {
                object result = base.Value;
                if (TypeOfTriggerValue != null && result != null)
                {
                    if (TypeOfTriggerValue.IsEnum && (result is String))
                    {
                        try
                        {
                            result = Enum.Parse(TypeOfTriggerValue, result as String);
                        }
                        catch (Exception e)
                        {
                            string msg = e.Message;
                            if (e.InnerException != null && e.InnerException.Message != null)
                                msg += " Inner:" + e.InnerException.Message;

                            if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                datamodel.Database.Root.Messages.LogException("VBPropertyGrid", "Value", msg);
                        }
                    }
                }
                return result;
            }
            set
            {
                SetValueCore(value);
            }
        }



        #region Create Binding
        public override void CreateBindings()
        {
            CreateBindings(null);
        }

        private void CreateBindings(IACObject acObjectSource)
        {
            List<VBBinding> bindings = CreateBindingsIntern(acObjectSource);
            if ((bindings == null) || (bindings.Count <= 0))
                return;
            if (bindings.Count != Properties.Count)
                return;
            int i = 0;
            raiseEvents = false;
            foreach (DesignItemProperty property in Properties)
            {
                VBBinding binding = bindings[i];
                property.SetValue(binding);
                if ((property.Value != null) && (binding.Converter != null))
                    property.Value.Properties["Converter"].SetValue(binding.Converter);
                i++;
            }
            raiseEvents = true;
            OnValueChanged();
            //Value = binding;

            IsExpanded = true;
        }

        private void CreateBinding(DesignItemProperty property, IACObject acObjectSource)
        {
            VBBinding binding = CreateBindingIntern(property, acObjectSource);
            if (binding == null)
                return;
            raiseEvents = false;
            property.SetValue(binding);
            if ((property.Value != null) && (binding.Converter != null))
                property.Value.Properties["Converter"].SetValue(binding.Converter);
            raiseEvents = true;
            OnValueChanged();
            //Value = binding;

            IsExpanded = true;
        }

        private List<VBBinding> CreateBindingsIntern(IACObject acObjectSource)
        {
            if (IsEvent || !IsDependencyProperty || Properties.Count <= 0)
                return null;
            List<VBBinding> bindings = new List<VBBinding>();
            foreach (DesignItemProperty property in Properties)
            {
                VBBinding binding = CreateBindingIntern(property, acObjectSource);
                bindings.Add(binding);
            }
            return bindings;
        }

        private VBBinding CreateBindingIntern(DesignItemProperty property, IACObject acObjectSource)
        {
            IValueConverter converter = ConverterManager.CreateValueConverter(property.ReturnType);
            if (converter == null)
            {
                if ((acObjectSource != null)
                    && (acObjectSource is IACType)
                    && (property.ReturnType == (acObjectSource as IACType).ObjectType))
                {
                    converter = new ConverterObject();
                }
                else
                    return null;
            }
            VBBinding binding = new VBBinding();
            binding.Converter = converter;
            return binding;
        }

        #endregion

        #region Create MultiBinding
        /// <summary>
        /// Replaces the value of this node with a new binding.
        /// </summary>
        public override void CreateMultiBindings()
        {
            CreateMultiBindings(null);
        }

        private void CreateMultiBindings(IACObject acObjectSource)
        {
            List<MultiBinding> bindings = CreateMultiBindingsIntern(acObjectSource);
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

            IsExpanded = true;
        }

        protected List<MultiBinding> CreateMultiBindingsIntern(IACObject acObjectSource)
        {
            if (IsEvent || !IsDependencyProperty || Properties.Count <= 0)
                return null;
            List<MultiBinding> bindings = new List<MultiBinding>();
            foreach (DesignItemProperty property in Properties)
            {
                MultiBinding binding = CreateMultiBindingIntern(property, acObjectSource);
                bindings.Add(binding);
            }
            return bindings;
        }

        private MultiBinding CreateMultiBindingIntern(DesignItemProperty property, IACObject acObjectSource)
        {
            IMultiValueConverter converter = ConverterManager.CreateMultiValueConverter(property.ReturnType);
            if (converter == null)
            {
                if ((acObjectSource != null)
                    && (acObjectSource is IACType)
                    && (property.ReturnType == (acObjectSource as IACType).ObjectType))
                {
                    converter = new ConverterObjectMulti();
                }
                else
                    return null;
            }
            MultiBinding binding = new MultiBinding();
            binding.Converter = converter;
            return binding;
        }


        #endregion

        #region Create Delegate for events
        private void CreateDelegates(IACObject acObjectSource)
        {
            List<VBDelegateExtension> vbDelegates = CreateDelegatesIntern(acObjectSource);
            if (vbDelegates == null || vbDelegates.Count <= 0)
                return;
            if (vbDelegates.Count != Properties.Count)
                return;
            int i = 0;
            raiseEvents = false;
            foreach (DesignItemProperty property in Properties)
            {
                VBDelegateExtension vbDelegate = vbDelegates[i];
                property.SetValue(vbDelegate);
                i++;
            }
            raiseEvents = true;
        }

        private void CreateDelegate(DesignItemProperty property, IACObject acObjectSource)
        {
            VBDelegateExtension vbDelegate = CreateDelegateIntern(property);
            if (vbDelegate == null)
                return;
            raiseEvents = false;
            property.SetValue(vbDelegate);
            raiseEvents = true;
        }

        private List<VBDelegateExtension> CreateDelegatesIntern(IACObject acObjectSource)
        {
            if (!IsEvent || IsDependencyProperty || Properties.Count <= 0)
                return null;
            List<VBDelegateExtension> vbDelegates = new List<VBDelegateExtension>();
            foreach (DesignItemProperty property in Properties)
            {
                VBDelegateExtension vbDelegate = CreateDelegateIntern(property);
                vbDelegates.Add(vbDelegate);
            }
            return vbDelegates;
        }

        private VBDelegateExtension CreateDelegateIntern(DesignItemProperty property)
        {
            if (!IsEvent || IsDependencyProperty)
                return null;
            VBDelegateExtension vbDelegate = new VBDelegateExtension();
            return vbDelegate;
        }

        private void AnalyzeMethodSignature(IACObject acObjectSource, out string parameter, out bool passEventArgs)
        {
            parameter = "";
            passEventArgs = false;
            if (!(acObjectSource is ACClassMethod))
                return;
            ACClassMethod acClassMethod = acObjectSource as ACClassMethod;

            var paramList = acClassMethod.GetParamList();
            var query = paramList.Where(c => c.ACIdentifier.Contains("EventArgs")).Select(c => c.ACIdentifier);
            if (query.Any())
            {
                passEventArgs = true;
                foreach (var name in query)
                {
                    if (string.IsNullOrEmpty(parameter))
                        parameter += ",";
                    parameter += name;
                }
            }
        }

        #endregion

        public void AddOrUpdateBindingWithMethod(String VBContent, bool isGlobalFunc, IACObject acObjectSource)
        {
            if (Properties.Count <= 0)
                return;
            var queryOld = this.MoreChildren.Where(c => c.Name == "Bindings");
            int countOld = queryOld.Count();
            bool hasBinding = false;

            foreach (DesignItemProperty property in Properties)
            {
                AddOrUpdateBindingWithMethod(property, VBContent, isGlobalFunc, acObjectSource);
                if ((property.Value != null) && (typeof(BindingBase).IsAssignableFrom(property.Value.ComponentType)
                                                  || typeof(VBBindingExt).IsAssignableFrom(property.Value.ComponentType)))
                {
                    hasBinding = true;
                }
            }

            var query = this.MoreChildren.Where(c => c.Name == "Bindings");
            int countNew = query.Count();
            
            if (countOld != countNew || hasBinding)
            {
                OnCreateEditor(FirstProperty);
            }
        }

        protected void AddOrUpdateBindingWithMethod(DesignItemProperty property, String VBContent, bool isGlobalFunc, IACObject acObjectSource)
        {
            // Falls Ziel DependencyProperty
            if (!this.IsEvent)
            {
                if ((property.ValueOnInstance == null) || (property.Value == null) || (!(property.Value.Component is Binding) && !(property.Value.Component is MultiBinding)))
                {
                    CreateBinding(property,acObjectSource);
                }
                if (property.Value != null)
                {
                    DesignItem converterItem = property.Value.Properties["Converter"].Value;
                    if ((converterItem != null) && (converterItem.Component is ConverterBase))
                    {
                        converterItem.Properties["ACUrlCommand"].SetValue(VBContent);
                        if (isGlobalFunc)
                            converterItem.Properties["GlobalFunction"].SetValue(isGlobalFunc);
                    }
                }
            }
            else
            {
                if ((property.ValueOnInstance == null) || (property.Value == null) || (!(property.Value.Component is VBDelegateExtension)))
                {
                    CreateDelegate(property,acObjectSource);
                }
                if (property.Value != null)
                {
                    string Parameter;
                    bool PassEventArgs;
                    AnalyzeMethodSignature(acObjectSource, out Parameter, out PassEventArgs);

                    VBDelegateExtension vbDelegate = property.Value.Component as VBDelegateExtension;
                    if (!String.IsNullOrEmpty(Parameter))
                    {
                        //vbDelegate.Parameter = Parameter;
                        property.Value.Properties["Parameter"].SetValue(vbDelegate.Parameter);
                    }
                    if (PassEventArgs)
                    {
                        //vbDelegate.PassEventArgs = PassEventArgs;
                        property.Value.Properties["PassEventArgs"].SetValue(vbDelegate.PassEventArgs);
                    }
                    if (isGlobalFunc)
                        property.Value.Properties["GlobalFunction"].SetValue(isGlobalFunc);
                    property.Value.Properties["ACUrlCmd"].SetValue(VBContent);
                }
            }
        }

        public void AddOrUpdateBindingWithProperty(String VBContent, IACObject acObjectSource)
        {
            if (IsEvent || !IsDependencyProperty || Properties.Count <= 0)
                return;
            var queryOld = this.MoreChildren.Where(c => c.Name == "Bindings");
            int countOld = queryOld.Count();
            bool hasBinding = false;

            foreach (DesignItemProperty property in Properties)
            {
                AddOrUpdateBindingWithProperty(property, VBContent, acObjectSource);
                if ((property.Value != null) && (typeof(BindingBase).IsAssignableFrom(property.Value.ComponentType)
                                                  || typeof(VBBindingExt).IsAssignableFrom(property.Value.ComponentType)))
                {
                    hasBinding = true;
                }
            }

            var query = this.MoreChildren.Where(c => c.Name == "Bindings");
            int countNew = query.Count();
            if (countNew > 0)
            {
                PropertyNode subNode = query.First();
                if (subNode != null)
                {
                    if ((subNode.Editor != null) && (subNode.Editor is ItemsControl))
                    {
                        // TODO: How to solve in Avalonia ?
                        //(subNode.Editor as ItemsControl).Items.Refresh();
                    }
                }
            }

            if (countOld != countNew || hasBinding)
            {
                OnCreateEditor(FirstProperty);
            }
        }

        protected void AddOrUpdateBindingWithProperty(DesignItemProperty property, String VBContent, IACObject acObjectSource)
        {
            if ((property.ValueOnInstance == null) || (property.Value == null) || (!(property.Value.Component is Binding) && !(property.Value.Component is MultiBinding)))
            {
                CreateBinding(property,acObjectSource);
                if (property.Value != null)
                    property.Value.Properties["VBContent"].SetValue(VBContent);
                return;
            }
            else if (property.Value.Component is Binding)
            {
                // Falls VBContent leer war, weil Binding zuerst erzeugt wurde als eine Methode gedroppt wurde
                // Trage VBContent ein und springe zurück.
                String currentVBContent = property.Value.Properties["VBContent"].ValueOnInstance as String;
                if (String.IsNullOrEmpty(currentVBContent))
                {
                    property.Value.Properties["VBContent"].SetValue(VBContent);
                    return;
                }
            }

            VBBinding newBinding = CreateBindingIntern(property,acObjectSource);
            if (newBinding == null)
                return;

            if (property.Value.Component is Binding)
            {
                MultiBinding multiBinding = CreateMultiBindingIntern(property,acObjectSource);
                if (multiBinding == null)
                    return;

                // Clone altes Binding
                VBBinding cloneBinding = CreateBindingIntern(property,acObjectSource);
                if (cloneBinding == null)
                    return;
                DesignItem newClonedItem = Services.Component.RegisterComponentForDesigner(cloneBinding);


                var cloneItemMeta = new { Name = "_X_protoType", Value = new object() };
                var propertiesToClone = (new[] { cloneItemMeta }).ToList();

                foreach (DesignItemProperty settedProperty in property.Value.SettedProperties)
                {
                    // Converter wird nur im Multibinding gesetzt
                    if (settedProperty.Name == "Converter")
                        continue;

                    object clonedValue = null;
                    DesignItem designItem = Services.Component.GetDesignItem(settedProperty.ValueOnInstance);
                    if (designItem != null)
                    {
                        clonedValue = Activator.CreateInstance(settedProperty.ValueOnInstance.GetType());
                        //var b = new Binding();
                        foreach (PropertyDescriptor pd in TypeDescriptor.GetProperties(settedProperty.ValueOnInstance))
                        {
                            if (pd.IsReadOnly) continue;
                            try
                            {
                                var val1 = pd.GetValue(clonedValue);
                                var val2 = pd.GetValue(settedProperty.ValueOnInstance);
                                if (object.Equals(val1, val2)) continue;
                                pd.SetValue(clonedValue, val2);
                            }
                            catch (Exception e)
                            {
                                string msg = e.Message;
                                if (e.InnerException != null && e.InnerException.Message != null)
                                    msg += " Inner:" + e.InnerException.Message;

                                if (datamodel.Database.Root != null && datamodel.Database.Root.Messages != null && datamodel.Database.Root.InitState == ACInitState.Initialized)
                                    datamodel.Database.Root.Messages.LogException("VBPropertyNode", "AddOrUpdateBindingWithProperty", msg);
                            }
                        }
                    }
                    else
                        clonedValue = settedProperty.ValueOnInstance;
                    var cloneItem = new { Name = settedProperty.Name, Value = clonedValue };

                    propertiesToClone.Add(new { Name = settedProperty.Name, Value = clonedValue });
                }

                // Ersetze Value mit neuem Multibinding
                property.SetValue(multiBinding);
                if ((property.Value != null) && (multiBinding.Converter != null))
                    property.Value.Properties["Converter"].SetValue(multiBinding.Converter);

                // Füge altes Binding dem Multibinding hinzu
                if (property.Value.ContentProperty.IsCollection)
                {
                    property.Value.ContentProperty.CollectionElements.Add(newClonedItem);
                    foreach (var newClonedItemProp in propertiesToClone)
                    {
                        if (newClonedItemProp.Name == "_X_protoType")
                            continue;
                        newClonedItem.Properties[newClonedItemProp.Name].SetValue(newClonedItemProp.Value);
                    }
                    if (!(property.Value.Component as MultiBinding).Bindings.Contains(cloneBinding))
                    {
                        (property.Value.Component as MultiBinding).Bindings.Add(cloneBinding);
                    }
                }
            }

            DesignItem newItem = Services.Component.RegisterComponentForDesigner(newBinding);
            // Füge neues Binding dem Multibinding hinzu
            if (property.Value.ContentProperty.IsCollection)
            {
                property.Value.ContentProperty.CollectionElements.Add(newItem);
                if (!(property.Value.Component as MultiBinding).Bindings.Contains(newBinding))
                {
                    (property.Value.Component as MultiBinding).Bindings.Add(newBinding);
                }
            }
            newItem.Properties["VBContent"].SetValue(VBContent);
        }


        override protected void UpdateChildren()
        {
            Children.Clear();
            MoreChildren.Clear();

            if (this.Name == "Style")
            {
                int i = 0;
                i++;
            }

            if (Parent == null || Parent.IsExpanded)
            {
                if (ValueItem != null)
                {
                    bool withReadonlyProperties = false;
                    if ((ValueItem.Component is MultiBinding) || (ValueItem.Component is ControlTheme))
                        withReadonlyProperties = true;

                    var list = TypeHelper.GetAvailableProperties(ValueItem.Component, withReadonlyProperties)
                        .OrderBy(d => d.Name)
                        .Select(d => new VBPropertyNode(new[] { ValueItem.Properties[d.Name] }, this));

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


        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get { return this.Name; }
        }

        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the Control.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get 
            {
                return null;
            }
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return false;
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get { return this.Name; }
        }


        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return new List<IACObject>();
            }
        }

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines is ACUrlCommand is enabled or disabled.
        /// </summary>
        /// <param name="acUrl">The acUrl of command.</param>
        /// <param name="acParameter">The command parameters.</param>
        ///<returns>Returns true if is ACUrlCommand is enabled, otherwise false.</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get { return this.Parent as IACObject; }
        }

        #region IACObject Member

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return this.ACIdentifier;
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get { return this.Name; }
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return false;
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }

        #endregion

        protected Type TypeOfTriggerValue
        {
            get;
            set;
        }

        internal void ChangeTypeOfTriggerValue(Type typeOfTriggerValue)
        {
            TypeOfTriggerValue = typeOfTriggerValue;
        }

    }

    /// <summary>
    /// Represents a grid for properties
    /// </summary>
    public class VBPropertyGrid : gip.ext.designer.avui.PropertyGrid.PropertyGrid
    {
        private const string CN_VBControl = "VBControl";
        private const string CN_VBContent = "VBControl Content";
        private const string CN_VBSource = "VBControl Source";
        private const string CN_Layout = "Layout";
        private const string CN_Brushes = "Brushes";
        private const string CN_Text = "Text";
        private const string CN_Appearance = "Appearance";
        private const string CN_Style = "Style";
        private const string CN_Transform = "Transform";
        private const string CN_CommonProperties = "Common Properties";
        private const string CN_Window = "Window";
        private const string CN_UIAutomation = "UI Automation";
        private const string CN_Miscellaneous = "Miscellaneous";

        public VBPropertyGrid()
            : base()
        {
            Categories = new CategoriesCollection();
            Categories.Add(CategoryVBControl);
            Categories.Add(CategoryVBContent);
            Categories.Add(CategoryVBSource);
            Categories.Add(CategoryLayout);
            Categories.Add(CategoryBrushes);
            Categories.Add(CategoryText);
            Categories.Add(CategoryAppearance);
            Categories.Add(CategoryStyle);
            Categories.Add(CategoryTransform);
            Categories.Add(CategoryCommonProperties);
            Categories.Add(CategoryWindow);
            Categories.Add(CategoryUIAutomation);
            Categories.Add(popularCategory);
            Categories.Add(attachedCategory);
            Categories.Add(CategoryMisc);
        }

        protected Category CategoryVBControl = new Category(CN_VBControl);
        protected Category CategoryVBContent = new Category(CN_VBContent);
        protected Category CategoryVBSource = new Category(CN_VBSource);
        protected Category CategoryLayout = new Category(CN_Layout);
        protected Category CategoryBrushes = new Category(CN_Brushes);
        protected Category CategoryText = new Category(CN_Text);
        protected Category CategoryAppearance = new Category(CN_Appearance);
        protected Category CategoryStyle = new Category(CN_Style);
        protected Category CategoryTransform = new Category(CN_Transform);
        protected Category CategoryCommonProperties = new Category(CN_CommonProperties);
        protected Category CategoryWindow = new Category(CN_Window);
        protected Category CategoryUIAutomation = new Category(CN_UIAutomation);
        protected Category CategoryMisc = new Category(CN_Miscellaneous);


        protected override PropertyNode CreatePropertyNode()
        {
            return new VBPropertyNode();
        }

        protected override Category PickCategory(PropertyNode node)
        {
            switch (node.FirstProperty.Name)
            {
                // IVBContent
                case Const.ACCaptionPrefix:
                case "VBContent":
                case Const.ACIdentifierPrefix:
                case "ShowCaption":
                case "DblClick":
                case "AutoFocus":
                case "DragEnabled":
                case "DropEnabled":
                case "RightControlMode":
                case "DisabledModes":
                    return CategoryVBContent;

                // IVBSource
                case "VBSource":
                case "VBShowColumns":
                case "VBDisabledColumns":
                case "VBChilds":
                    return CategoryVBSource;
            }

            // VBControl
            if (node.FirstProperty.DeclaringType.IsDefined(typeof(ACClassInfo), false))
            {
                PropertyInfo propInfo = node.FirstProperty.DeclaringType.GetProperty(node.FirstProperty.Name);
                if ((propInfo != null) && propInfo.GetCustomAttributes(true).Where(c => c.GetType().Name.StartsWith(Const.ACPropertyPrefix)).Any())
                    return CategoryVBControl;
            }

            switch (node.FirstProperty.Name)
            {
                // Appearance
                case "Opacity":
                case "Visibility":
                case "BorderThickness":
                case "Effect":
                case "ClipToBounds":
                case "SnapsToDevicePixels":
                    return CategoryAppearance;

                // Layout
                case "Width":
                case "Height":
                case "ZIndex":
                case "HorizontalAlignment":
                case "VerticalAlignment":
                case "Margin":
                case "Padding":
                case "Grid.Column":
                case "Grid.Row":
                case "Grid.ColumnSpan":
                case "Grid.RowSpan":
                case "Grid.IsSharedSizeScope":
                case "DockPanel.Dock":
                case "Canvas.Left":
                case "Canvas.Top":
                case "Canvas.Right":
                case "Canvas.Bottom":
                case "HorizontalContentAlignment":
                case "VerticalContentAlignment":
                case "MaxWidth":
                case "MaxHeight":
                case "MinWidth":
                case "MinHeight":
                case "ActualWidth":
                case "ActualHeight":
                    return CategoryLayout;

                // Common Properties
                case "Content":
                case "IsCancel":
                case "IsDefault":
                case "Cursor":
                case "IsEnabled":
                case "ToolTip":
                    return CategoryCommonProperties;

                // Style
                case "Style":
                    return CategoryStyle;

                // Transform
                case "LayoutTransform":
                case "RenderTransform":
                    return CategoryTransform;

                // Text
                case "FontFamily":
                case "FontSize":
                case "FontStyle":
                case "FontStretch":
                case "FontWeight":
                    return CategoryText;

                // Window
                case "Title":
                case "WindowState":
                case "WindowStyle":
                case "WindowStartupLocation":
                case "Topmost":
                case "Top":
                case "Left":
                case "ShowInTaskBar":
                case "ShowActivated":
                case "OwnedWindows":
                    return CategoryWindow;

                // UI Automation
                case "AutomationProperties.Name":
                case "AutomationProperties.HelpText":
                    return CategoryUIAutomation;
                
                default:
                    break;
            }

            if (typeof(Brush).IsAssignableFrom(node.FirstProperty.ReturnType))
                return CategoryBrushes;
            if (typeof(Color).IsAssignableFrom(node.FirstProperty.ReturnType))
                return CategoryBrushes;

            if (Metadata.IsPopularProperty(node.FirstProperty))
                return popularCategory;
            if (node.FirstProperty.Name.Contains("."))
                return attachedCategory;
            //var typeName = node.FirstProperty.DeclaringType.FullName;
            //if (typeName.StartsWith("System.Windows.") || typeName.StartsWith("gip.ext.designer.avui.Controls."))
                //return otherCategory;
            return CategoryMisc;
        }

    }
}

