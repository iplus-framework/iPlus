using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.layoutengine.avui.PropertyGrid.Editors;
using gip.ext.design.avui;
using gip.ext.designer.avui.OutlineView;
using System;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a editor for data triggers.
    /// </summary>
    public class VBDataTriggerEditor : DataTriggerEditor
    {
        protected override Type StyleKeyOverride => typeof(VBDataTriggerEditor);
        
        public override Control TriggerBindingEditor
        {
            get
            {
                if (_NodeDataTrigger == null)
                    return null;

                var bindingDesignItem = CurrentBindingDesignItem;
                var bindingDefinition = CurrentBindingDefinition;
                if (bindingDefinition != null && bindingDesignItem != null)
                {
                    if (bindingDefinition is MultiBinding)
                    {
                        EnsureBooleanMultiConverter(bindingDesignItem, bindingDefinition as MultiBinding);
                        VBMultiBindingEditor editor = new VBMultiBindingEditor();
                        editor.InitEditor(bindingDesignItem, _NodeDataTrigger);
                        return editor;
                    }
                    else if ((bindingDefinition is Binding) || (bindingDefinition is VBBindingExt))
                    {
                        VBBindingEditor editor = new VBBindingEditor();
                        editor.InitEditor(bindingDesignItem, _NodeDataTrigger);
                        return editor;
                    }
                }
                return new Control();
            }
        }

        public override Binding CreateNewBinding()
        {
            return new VBBinding();
        }

        public override MultiBinding CreateNewMultiBinding()
        {
            return new MultiBinding()
            {
                Converter = new ConverterBooleanMulti()
            };
        }

        private void EnsureBooleanMultiConverter(DesignItem bindingDesignItem, MultiBinding multiBinding)
        {
            if (multiBinding == null || multiBinding.Converter != null)
                return;

            if (bindingDesignItem?.Properties != null)
            {
                var converterProperty = bindingDesignItem.Properties.HasProperty("Converter");
                if (converterProperty != null)
                {
                    converterProperty.SetValue(new ConverterBooleanMulti());
                    return;
                }
            }

            multiBinding.Converter = new ConverterBooleanMulti();
        }

        public override Control TriggerValueEditor
        {
            get
            {
                _LockValueEditorRefresh = true;

                IACComponentDesignManager designManager = DesignManager;
                if (designManager != null && !String.IsNullOrEmpty(TriggerInfoText) && CurrentBindingDefinition is VBBinding)
                {
                    Control editor;
                    String acUrl = TriggerInfoText;
                    acUrl = acUrl.Replace("\\\\", "\\");
                    IACType typeInfo = designManager.GetTypeFromDBACUrl(acUrl);
                    if (typeInfo != null)
                    {
                        Type dataType = typeInfo.ObjectType;
                        if (dataType.IsEnum)
                        {
                            var standardValues = gip.ext.design.avui.Metadata.GetStandardValues(dataType);

                            editor = new VBComboBoxEditor() { ItemsSource = standardValues };
                            editor.DataContext = _NodeDataTrigger;
                            (_NodeDataTrigger as VBDataTriggerOutlineNode).ChangeTypeOfTriggerValue(dataType);
                            return editor;
                        }
                    }

                    editor = new VBTextBoxEditor();
                    editor.DataContext = _NodeDataTrigger;
                    return editor;
                }
                else
                {
                    Control editor = new VBTextBoxEditor();
                    editor.DataContext = _NodeDataTrigger;
                    return editor;
                }
            }
        }

        public IACComponentDesignManager DesignManager
        {
            get
            {
                if (_DesignObject == null)
                    return null;
                VBDesignEditor editor = VBVisualTreeHelper.FindParentObjectInVisualTree(_DesignObject.Context.Services.DesignPanel as AvaloniaObject, typeof(VBDesignEditor)) as VBDesignEditor;
                if (editor == null)
                    return null;
                IACComponentDesignManager designManager = editor.GetDesignManager();
                if (designManager == null)
                    return null;
                return designManager;
            }
        }

        public IACObject CurrentDesignContext
        {
            get
            {
                if (DesignManager == null)
                    return null;
                return DesignManager.CurrentDesignContext;
            }
        }

    }
}
