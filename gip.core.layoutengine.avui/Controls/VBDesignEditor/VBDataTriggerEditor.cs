using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.layoutengine.avui.PropertyGrid.Editors;
using gip.ext.designer.avui.OutlineView;
using System;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a editor for data triggers.
    /// </summary>
    public class VBDataTriggerEditor : DataTriggerEditor
    {
        public override Control TriggerBindingEditor
        {
            get
            {
                if (_NodeDataTrigger == null)
                    return null;
                if (_NodeDataTrigger.BindingProperty.ValueOnInstance != null)
                {
                    if (_NodeDataTrigger.BindingProperty.ValueOnInstance is MultiBinding)
                    {
                        VBMultiBindingEditor editor = new VBMultiBindingEditor();
                        editor.InitEditor(_NodeDataTrigger.BindingProperty.Value, _NodeDataTrigger);
                        return editor;
                    }
                    else if ((_NodeDataTrigger.BindingProperty.ValueOnInstance is Binding) || (_NodeDataTrigger.BindingProperty.ValueOnInstance is VBBindingExt))
                    {
                        VBBindingEditor editor = new VBBindingEditor();
                        editor.InitEditor(_NodeDataTrigger.BindingProperty.Value, _NodeDataTrigger);
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
            return new MultiBinding();
        }

        public override Control TriggerValueEditor
        {
            get
            {
                _LockValueEditorRefresh = true;

                IACComponentDesignManager designManager = DesignManager;
                if (designManager != null && !String.IsNullOrEmpty(TriggerInfoText) && (TriggerBindingEditor is VBBindingEditor))
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
                    return new VBTextBoxEditor();
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
