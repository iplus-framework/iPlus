using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.design.avui;
using gip.ext.design.avui.PropertyGrid;
using gip.ext.designer.avui.OutlineView;
using System;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a editor for conditions.
    /// </summary>
    public class VBConditionEditor : ConditionEditor
    {

        protected override void CreateWrapper()
        {
            //if (_DesignObjectCondition.Component is Condition)
                _Wrapper = new VBConditionWrapper(_DesignObjectCondition, null);
        }

        protected override void OnToolEvents(object sender, ToolEventArgs e)
        {
        }

        public IACComponentDesignManager DesignManager
        {
            get
            {
                if (DesignObjectCondition == null)
                    return null;
                VBDesignEditor editor = VBVisualTreeHelper.FindParentObjectInVisualTree(DesignObjectCondition.Context.Services.DesignPanel as AvaloniaObject, typeof(VBDesignEditor)) as VBDesignEditor;
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

        public override Binding CreateNewBinding()
        {
            return new VBBinding();
        }

        public override Control TriggerBindingEditor
        {
            get
            {
                if (Wrapper == null)
                    return null;
                if (Wrapper.Binding.Value != null)
                {
                    if (Wrapper.Binding.Value is MultiBinding)
                    {
                        VBMultiBindingEditor editor = new VBMultiBindingEditor();
                        editor.LoadItemsCollection(Wrapper.Binding.ValueItem);
                        return editor;
                    }
                    else if ((Wrapper.Binding.Value is Binding) || (Wrapper.Binding.Value is VBBindingExt))
                    {
                        VBBindingEditor editor = new VBBindingEditor();
                        editor.LoadItemsCollection(Wrapper.Binding.ValueItem);
                        return editor;
                    }
                }
                return new Control();
            }
        }

        public Control CurrentTriggerBindingEditor
        {
            get
            {
                if ((PART_BindingEditor != null) && (PART_BindingEditor.Content != null))
                {
                    if (PART_BindingEditor.Content is BindingEditor)
                        return PART_BindingEditor.Content as Control;
                    if (PART_BindingEditor.Content is MultiBindingEditor)
                        return PART_BindingEditor.Content as Control;
                }
                return null;
            }
        }


        public override Control TriggerValueEditor
        {
            get
            {
                _LockValueEditorRefresh = true;
                if (ParentTriggerNode is MultiDataTriggerOutlineNode)
                {
                    if (Wrapper != null)
                    {
                        if ((CurrentTriggerBindingEditor != null) && (CurrentTriggerBindingEditor is VBBindingEditor))
                        {
                            IACComponentDesignManager designManager = DesignManager;
                            if (designManager != null && !String.IsNullOrEmpty((CurrentTriggerBindingEditor as VBBindingEditor).TriggerInfoText))
                            {
                                String acUrl = (CurrentTriggerBindingEditor as VBBindingEditor).TriggerInfoText;
                                acUrl = acUrl.Replace("\\\\", "\\");
                                IACType typeInfo = designManager.GetTypeFromDBACUrl(acUrl);
                                if (typeInfo != null && (Wrapper.Value is VBPropertyNode))
                                {
                                    Type dataType = typeInfo.ObjectType;
                                    VBPropertyNode node = Wrapper.Value as VBPropertyNode;
                                    node.ChangeTypeOfTriggerValue(dataType);
                                    node.RefreshEditor();
                                }
                            }
                        }
                        return Wrapper.Value.Editor;
                    }
                }
                else
                {
                    if (PART_SelectorTriggerable != null && SelectedTriggerableProperty != null)
                    {
                        //PropertyNode selectedNode = PART_SelectorTriggerable.SelectedItem as PropertyNode;
                        Control editor = SelectedTriggerableProperty.Editor;
                        if (editor != null)
                            editor.DataContext = Wrapper.Value;
                        _LockValueEditorRefresh = false;
                        return editor;
                    }
                }
                _LockValueEditorRefresh = false;
                return new ContentControl();
            }
        }


        protected override PropertyNode CreatePropertyNode()
        {
            return new VBPropertyNode();
        }

    }
}
