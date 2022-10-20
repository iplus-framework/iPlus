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
using gip.core.layoutengine.Helperclasses;
using gip.ext.designer.OutlineView;
using gip.ext.designer;
using System.Windows.Markup;
using gip.core.layoutengine.PropertyGrid.Editors;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents a editor for data triggers.
    /// </summary>
    public class VBDataTriggerEditor : DataTriggerEditor
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "DataTriggerEditorStyleGip", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBDesignEditor/Themes/DataTriggerEditorStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "DataTriggerEditorStyleAero", 
                                         styleUri = "/gip.core.layoutengine;Component/Controls/VBDesignEditor/Themes/DataTriggerEditorStyleAero.xaml" },
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

        static VBDataTriggerEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBDataTriggerEditor), new FrameworkPropertyMetadata(typeof(VBDataTriggerEditor)));
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

        public override FrameworkElement TriggerBindingEditor
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

        public override MarkupExtension CreateNewBinding()
        {
            return new VBBinding();
        }

        public override MarkupExtension CreateNewMultiBinding()
        {
            return new MultiBinding();
        }

        public override FrameworkElement TriggerValueEditor
        {
            get
            {
                _LockValueEditorRefresh = true;

                // TODO: Provide Value um Type heruaszufinden => Editor-Generieren für Type
                //if (_NodeDataTrigger.BindingProperty.ValueOnInstance is MultiBinding)
                //{
                //}
                //else if (_NodeDataTrigger.BindingProperty.ValueOnInstance is Binding)
                //{
                //}
                //return new VBComboBoxEditor() { ItemsSource = query.Select(c => c.ACIdentifier).ToList() };

                //Database.ConvertACUrlComponentToDBUrl(acUrlComponent, acIdentifierProperty);
                //IACObjectEntityWithCheckTrans entityObj = Database.GlobalDatabase.ACUrlCommand(acDBUrl, Const.ParamInheritedMember) as IACObjectEntityWithCheckTrans;
                //if (entityObj == null)
                //    return acIdentifierProperty;

                IACComponentDesignManager designManager = DesignManager;
                if (designManager != null && !String.IsNullOrEmpty(TriggerInfoText) && (TriggerBindingEditor is VBBindingEditor))
                {
                    FrameworkElement editor;
                    String acUrl = TriggerInfoText;
                    acUrl = acUrl.Replace("\\\\", "\\");
                    IACType typeInfo = designManager.GetTypeFromDBACUrl(acUrl);
                    if (typeInfo != null)
                    {
                        Type dataType = typeInfo.ObjectType;
                        if (dataType.IsEnum)
                        {
                            var standardValues = gip.ext.design.Metadata.GetStandardValues(dataType);

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
                VBDesignEditor editor = VBVisualTreeHelper.FindParentObjectInVisualTree(_DesignObject.Context.Services.DesignPanel as DependencyObject, typeof(VBDesignEditor)) as VBDesignEditor;
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
