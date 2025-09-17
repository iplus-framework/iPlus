using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using gip.ext.designer.avui.OutlineView;
using gip.ext.designer.avui;
using gip.ext.design.avui;
using gip.core.layoutengine.avui.PropertyGrid.Editors;
using System.Windows.Markup;
using gip.ext.design.avui.PropertyGrid;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a editor for conditions.
    /// </summary>
    public class VBConditionEditor : ConditionEditor
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "ConditionEditorStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBDesignEditor/Themes/ConditionEditorStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "ConditionEditorStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBDesignEditor/Themes/ConditionEditorStyleAero.xaml" },
        };

        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public static List<CustomControlStyleInfo> StyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        /// <summary>
        /// Gets the list of custom styles.
        /// </summary>
        public virtual List<CustomControlStyleInfo> MyStyleInfoList
        {
            get
            {
                return _styleInfoList;
            }
        }

        static VBConditionEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBConditionEditor), new FrameworkPropertyMetadata(typeof(VBConditionEditor)));
        }

        bool _themeApplied = false;

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ActualizeTheme(true);
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (!_themeApplied)
                ActualizeTheme(false);
        }

        /// <summary>
        /// Actualizes current theme.
        /// </summary>
        /// <param name="bInitializingCall">Determines is initializing call or not.</param>
        public void ActualizeTheme(bool bInitializingCall)
        {
            _themeApplied = ControlManager.RegisterImplicitStyle(this, MyStyleInfoList, bInitializingCall);
        }
        #endregion

        protected override void CreateWrapper()
        {
            if (_DesignObjectCondition.Component is Condition)
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
                VBDesignEditor editor = VBVisualTreeHelper.FindParentObjectInVisualTree(DesignObjectCondition.Context.Services.DesignPanel as DependencyObject, typeof(VBDesignEditor)) as VBDesignEditor;
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

        public override MarkupExtension CreateNewBinding()
        {
            return new VBBinding();
        }

        public override FrameworkElement TriggerBindingEditor
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

        public FrameworkElement CurrentTriggerBindingEditor
        {
            get
            {
                if ((PART_BindingEditor != null) && (PART_BindingEditor.Content != null))
                {
                    if (PART_BindingEditor.Content is BindingEditor)
                        return PART_BindingEditor.Content as FrameworkElement;
                    if (PART_BindingEditor.Content is MultiBindingEditor)
                        return PART_BindingEditor.Content as FrameworkElement;
                }
                return null;
            }
        }


        public override FrameworkElement TriggerValueEditor
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
                        FrameworkElement editor = SelectedTriggerableProperty.Editor;
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
