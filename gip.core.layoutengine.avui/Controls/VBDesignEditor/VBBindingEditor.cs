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

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represent a editor for binding.
    /// </summary>
    public class VBBindingEditor : BindingEditor, IBindingDropHandler, IACInteractiveObject
    {
        #region c'tors
        private static List<CustomControlStyleInfo> _styleInfoList = new List<CustomControlStyleInfo> { 
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Gip, 
                                         styleName = "BindingEditorStyleGip", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBDesignEditor/Themes/BindingEditorStyleGip.xaml" },
            new CustomControlStyleInfo { wpfTheme = eWpfTheme.Aero, 
                                         styleName = "BindingEditorStyleAero", 
                                         styleUri = "/gip.core.layoutengine.avui;Component/Controls/VBDesignEditor/Themes/BindingEditorStyleAero.xaml" },
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

        static VBBindingEditor()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VBBindingEditor), new FrameworkPropertyMetadata(typeof(VBBindingEditor)));
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
            AllowDrop = true;
            PreviewDragEnter += new DragEventHandler(OnDragEnter);
            PreviewDragLeave += new DragEventHandler(OnDragLeave);
            PreviewDrop += new DragEventHandler(OnDrop);
            PreviewDragOver += new DragEventHandler(OnDragOver);
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
            if ((_DesignObjectBinding.Component is Binding) || (_DesignObjectBinding.Component is VBBindingExt))
                _Wrapper = new VBBindingEditorWrapperSingle(_DesignObjectBinding, null);
        }


        protected override void OnToolEvents(object sender, ToolEventArgs e)
        {
            if (e.EventDescID == "VBConnectorClicked")
            {
                if ((Wrapper == null) || (Wrapper.ParentMultiWrapper != null))
                    return;

                DesignPanelHitTestResult hitResult = (DesignPanelHitTestResult)((object[])e.Details)[0];
                VBConnectorDrawingBehavior.VBConnectorPlacement placement = (VBConnectorDrawingBehavior.VBConnectorPlacement)((object[])e.Details)[1];
                if ((placement.ConnectorPoint == null)
                    || (hitResult.AdornerHit == null)
                    || (hitResult.AdornerHit.AdornedDesignItem == null)
                    || (hitResult.AdornerHit.AdornedDesignItem.View == null)
                    || String.IsNullOrEmpty(placement.ConnectorPoint.VBContent))
                    return;
                if (!(hitResult.AdornerHit.AdornedDesignItem.View is IACInteractiveObject))
                    return;
                if ((Wrapper as VBBindingEditorWrapperSingle).VBContent == null || CurrentDesignContext == null)
                    return;
                IACInteractiveObject interactionObject = hitResult.AdornerHit.AdornedDesignItem.View as IACInteractiveObject;
                if (Wrapper != null && placement.ConnectorPoint.ACClassOfComponent != null)
                {
                    if ((placement.ConnectorPoint.PointStatesInfo != null) && (placement.ConnectorPoint.PointStatesInfo.Any()))
                    {
                        foreach (ACClassPropertyRelation relation in placement.ConnectorPoint.PointStatesInfo)
                        {
                            if (relation.TargetACClassProperty == null)
                                continue;
                            string acURLBaseRelative = placement.ConnectorPoint.ACClassOfComponent.GetACUrlComponent(CurrentDesignContext);

                            string urlPart2 = "";
                            //string urlPart2 = relation.TargetACClassProperty.TopBaseACClassProperty.GetACUrlComponent(placement.ConnectorPoint.ACClassOfComponent);
                            int indexOfChildPart = placement.ConnectorPoint.VBContent.LastIndexOf('\\');
                            if (indexOfChildPart > 0)
                                urlPart2 = placement.ConnectorPoint.VBContent.Substring(0, indexOfChildPart + 1) + relation.TargetACClassProperty.TopBaseACClassProperty.ACIdentifier;
                            else
                                urlPart2 = relation.TargetACClassProperty.TopBaseACClassProperty.ACIdentifier;
                            
                            string newACUrl;
                            if (urlPart2.StartsWith("\\"))
                                newACUrl = acURLBaseRelative + urlPart2;
                            else
                                newACUrl = acURLBaseRelative + "\\" + urlPart2;

                            (Wrapper as VBBindingEditorWrapperSingle).VBContent.Value = newACUrl;
                            break;
                        }
                    }
                }
            }
        }

        public IACObject CurrentDesignContext
        {
            get
            {
                if (_DesignObjectBinding == null)
                    return null;
                VBDesignEditor editor = VBVisualTreeHelper.FindParentObjectInVisualTree(_DesignObjectBinding.Context.Services.DesignPanel as DependencyObject, typeof(VBDesignEditor)) as VBDesignEditor;
                if (editor == null)
                    return null;
                IACComponentDesignManager designManager = editor.GetDesignManager();
                if (designManager == null)
                    return null;
                return designManager.CurrentDesignContext;
            }
        }

        #region DragAndDrop
        void OnDragEnter(object sender, DragEventArgs e)
        {
            HandleDragOver(sender, e);
        }

        void OnDragLeave(object sender, DragEventArgs e)
        {
            HandleDragOver(sender, e);
        }

        void OnDragOver(object sender, DragEventArgs e)
        {
            HandleDragOver(sender, e);
        }

        void OnDrop(object sender, DragEventArgs e)
        {
            HandleDrop(sender, e);
        }

        public void HandleDragEnter(object sender, DragEventArgs e)
        {
        }

        public void HandleDragLeave(object sender, DragEventArgs e)
        {
        }

        public void HandleDragOver(object sender, DragEventArgs e)
        {
            UIElement uiElement = e.OriginalSource as UIElement;

            if ((uiElement == null) || (Wrapper == null) || (Wrapper.ParentMultiWrapper != null))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            switch (e.AllowedEffects)
            {
                case DragDropEffects.Move: // Vorhandene Elemente verschieben
                    e.Handled = true;
                    return;
                case DragDropEffects.Copy: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move:
                case DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link:
                    HandleDragOver_Copy(sender, 0, 0, e);
                    return;
                default:
                    e.Effects = DragDropEffects.None;
                    e.Handled = true;
                    return;
            }
        }

        private void HandleDragOver_Copy(object sender, double x, double y, DragEventArgs e)
        {
            UIElement uiElement = e.OriginalSource as UIElement;

            if (uiElement == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            // Wenn alle generellen Vorbedingungen erfüllt sind, dann wird
            // noch das BSO gefragt, ob das kopieren (einfügen) erlaubt ist
            ACActionArgs actionArgs = new ACActionArgs(dropObject, x, y, Global.ElementActionType.Drop);
            if (IsEnabledACAction(actionArgs))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            // Drag und Drop auf Unterelement nicht erlauben!!
            e.Handled = true;
        }

        public void HandleDrop(object sender, DragEventArgs e)
        {
            UIElement uiElement = e.OriginalSource as UIElement;

            if (uiElement == null || (Wrapper == null) || (Wrapper.ParentMultiWrapper != null))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            IACInteractiveObject dropObject = VBDragDrop.GetDropObject(e);
            if (dropObject == null)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
                return;
            }

            switch (e.AllowedEffects)
            {
                case DragDropEffects.Copy: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move: // Neue Elemente einfügen
                case DragDropEffects.Copy | DragDropEffects.Move | DragDropEffects.Link:
                    {
                        ACActionArgs actionArgs = new ACActionArgs(dropObject, 0, 0, Global.ElementActionType.Drop);
                        ACAction(actionArgs);
                        e.Handled = true;
                    }
                    return;
                default:
                    e.Effects = DragDropEffects.None;
                    return;
            }
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            // TODO: Logik über BSOACComponent abbildbar ??
            if (!(ContextACObject is IACComponent))
                return;
            (ContextACObject as IACComponent).ACActionToTarget(this, actionArgs);
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            if (!(ContextACObject is IACComponent))
                return false;
            return (ContextACObject as IACComponent).IsEnabledACActionToTarget(this, actionArgs);
        }

        public void AddOrUpdateBindingWithMethod(String VBContent, bool isGlobalFunc, IACObject acObjectSource)
        {
            string oldExpression = "";
            string newExpression = "";
            bool editExpression = false;
            VBConverterEditorView editor = DetermineConverterInfos(ref editExpression, ref oldExpression, ref newExpression, true);
            if (editor == null)
                return;
            if (editor.ConversionBy == ConverterBase.ConvType.Expression)
            {
                newExpression = "";
                editor.UpdateExpression(newExpression);
                editor.ConversionBy = ConverterBase.ConvType.ScriptEngine;
            }
            editor.ACUrlCommand = VBContent;
            editor.GlobalFunction = isGlobalFunc;
        }

        public void AddOrUpdateBindingWithProperty(String VBContent, IACObject acObjectSource)
        {
            if ((Wrapper == null) || (Wrapper.ParentMultiWrapper != null) || !(Wrapper is VBBindingEditorWrapperSingle))
                return;

            (Wrapper as VBBindingEditorWrapperSingle).VBContent.Value = VBContent;
        }

        private VBConverterEditorView DetermineConverterInfos(ref bool editExpression, ref string oldExpression, ref string newExpression, bool scriptEngineIfNotSet)
        {
            VBConverterEditorView editor = null;
            if ((Wrapper.Converter != null) && (Wrapper.Converter.ValueItem == null))
            {
                Wrapper.Converter.Value = new ConverterBoolean();
                if (Wrapper.Converter.Editor != null)
                {
                    editor = Wrapper.Converter.Editor as VBConverterEditorView;
                    if (editor == null && Wrapper.Converter.Editor is VBConverterTypeEditor)
                        editor = (Wrapper.Converter.Editor as VBConverterTypeEditor).ConverterEditorView;
                    if (editor != null)
                    {
                        if (scriptEngineIfNotSet)
                        {
                            editor.ConversionBy = ConverterBase.ConvType.ScriptEngine;
                            editExpression = false;
                        }
                        else
                        {
                            editor.ConversionBy = ConverterBase.ConvType.Expression;
                            editExpression = true;
                        }
                    }
                }
                if ((ParentTriggerNode != null) && (ParentTriggerNode is DataTriggerOutlineNode))
                {
                    (ParentTriggerNode as DataTriggerOutlineNode).TriggerValue.SetValue("True");
                    (ParentTriggerNode as DataTriggerOutlineNode).Load();
                }
            }
            else if ((Wrapper.Converter != null)
                && (Wrapper.Converter.ValueItem != null)
                && (typeof(ConverterBase).IsAssignableFrom(Wrapper.Converter.ValueItem.ComponentType) && Wrapper.Converter.Editor != null))
            {
                editor = Wrapper.Converter.Editor as VBConverterEditorView;
                if (editor == null && Wrapper.Converter.Editor is VBConverterTypeEditor)
                    editor = (Wrapper.Converter.Editor as VBConverterTypeEditor).ConverterEditorView;
                if (editor != null)
                {
                    if (editor.ConversionBy == ConverterBase.ConvType.Expression)
                    {
                        oldExpression = editor.Expression;
                        newExpression = oldExpression;
                        editExpression = true;
                    }
                }
            }
            return editor;
        }

        public IACComponentDesignManager DesignManager
        {
            get
            {
                if (_DesignObjectBinding == null)
                    return null;
                VBDesignEditor editor = VBVisualTreeHelper.FindParentObjectInVisualTree(_DesignObjectBinding.Context.Services.DesignPanel as DependencyObject, typeof(VBDesignEditor)) as VBDesignEditor;
                if (editor == null)
                    return null;
                return editor.GetDesignManager();
            }
        }
        #endregion

        #region IACInteractiveObject
        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        public string VBContent
        {
            get;set;
        }

        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get
            {
                return DesignManager;
            }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get { return "Bindingeditor"; }
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get { return null; }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get { throw new NotImplementedException(); }
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
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                return Parent as IACObject;
            }
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get;
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


        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return false;
        }
    }
}
