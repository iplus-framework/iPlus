﻿using gip.core.datamodel;
using gip.core.layoutengine.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents the control for panel bar.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt die Steuerung für die Panel-Leiste dar.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBRibbonMobile'}de{'VBRibbonMobile'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public partial class VBRibbonMobile : UserControl, IVBContent, IACObject
    {

        const string _MobileCommandList =
            Const.CmdNew
            + "," + Const.CmdDelete
            + "," + Const.CmdRestore
            + "," + Const.CmdCopy
            + "," + Const.CmdPaste
            + "," + Const.CmdUndo
            + "," + Const.CmdRedo
            + "," + Const.CmdQueryPrintDlg
            + "," + Const.CmdQueryPreviewDlg
            + "," + Const.CmdCut
            + "," + Const.CmdLoad
            + "," + Const.CmdSave
            + "," + Const.CmdUndoSave
            + "," + Const.CmdSearch;

        string[] _EditString = { Const.CmdCut, Const.CmdCopy, Const.CmdPaste, Const.CmdUndo, Const.CmdRedo };
        string[] _EditImageSourceStyleGip = { "Editcut", "Editcopy", "EditPaste", Const.CmdNameUndo, Const.CmdNameRedo };
        string[] _EditImageSourceStyleAero = { "Editcut", "Editcopy", "EditPaste", Const.CmdNameUndo, Const.CmdNameRedo };

        string[] _FileString = { Const.CmdNew, Const.CmdLoad, Const.CmdSave, Const.CmdUndoSave, Const.CmdDelete, Const.CmdRestore, Const.CmdQueryPrintDlg, Const.CmdQueryPreviewDlg, Const.CmdQueryDesignDlg };
        string[] _FileImageSourceGip = { "AddNew", "LoadRefresh", "Filesave", "FilesaveUndo", Const.CmdNameDelete, Const.CmdNameRestore, "Print", "Preview", "Design" };
        string[] _FileImageSourceAero = { "AddNew", "LoadRefresh", "Filesave", "FilesaveUndo", Const.CmdNameDelete, Const.CmdNameRestore, "Print", "Preview", "Design" };

        string[] _SearchString = { Const.CmdSearch, Const.CmdExport };
        string[] _SearchImageSourceGip = { "Find", "DataExport" };
        string[] _SearchImageSourceAero = { "Find", "DataExport" };

        VBTextBox _SearchBox = null;
        string _SearchCommand = "";
        public static readonly DependencyProperty VBRibbonMobileResourcesProperty = DependencyProperty.Register("VBRibbonMobileResources", typeof(Object), typeof(UserControl));

        public VBRibbonMobile()
        {
            InitializeComponent();

            Uri uri = new Uri("/gip.core.layoutengine;Component/Controls/VBRibbon/Themes/RibbonTrayBorderStyle.xaml", UriKind.Relative);
            ResourceDictionary dict = this.Resources.MergedDictionaries.Where(c => c.Source == uri).First();
            if (dict != null)
            {
                if (ControlManager.WpfTheme == eWpfTheme.Aero)
                    _BorderRibbonPanel.Style = dict["RibbonTrayBorderStyleAero"] as Style;
                else
                    _BorderRibbonPanel.Style = dict["RibbonTrayBorderStyleGip"] as Style;
            }

            this.SizeChanged += RibbonBar_SizeChanged;
        }

        /// <summary>
        /// Overides the OnApplyTemplate method and run VBControl initialization.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            InitVBControl();
        }


        #region IDataContent Members
        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get
            {
                return DataContext as IACObject;
            }
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBRibbonMobile), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACUrlCmdMessage.
        /// </summary>
        public static readonly DependencyProperty ACUrlCmdMessageProperty =
            DependencyProperty.Register("ACUrlCmdMessage",
                typeof(ACUrlCmdMessage), typeof(VBRibbonMobile),
                new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the ACUrlCmdMessage.
        /// </summary>
        public ACUrlCmdMessage ACUrlCmdMessage
        {
            get { return (ACUrlCmdMessage)GetValue(ACUrlCmdMessageProperty); }
            set { SetValue(ACUrlCmdMessageProperty, value); }
        }

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly DependencyProperty ACCompInitStateProperty =
            DependencyProperty.Register("ACCompInitState",
                typeof(ACInitState), typeof(VBRibbonMobile),
                new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return (ACInitState)GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        private static void OnDepPropChanged(DependencyObject dependencyObject,
               DependencyPropertyChangedEventArgs args)
        {
            VBRibbonMobile thisControl = dependencyObject as VBRibbonMobile;
            if (thisControl == null)
                return;
            if (args.Property == ACCompInitStateProperty)
                thisControl.InitStateChanged();
            else if (args.Property == BSOACComponentProperty)
            {
                if (args.NewValue == null && args.OldValue != null && !String.IsNullOrEmpty(thisControl.VBContent))
                {
                    IACBSO bso = args.OldValue as IACBSO;
                    if (bso != null)
                        thisControl.DeInitVBControl(bso);
                }
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

        //public string ToolTip
        //{
        //    get
        //    {
        //        return "";
        //    }
        //    set
        //    {
        //    }
        //}

        private bool Visible
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Determines is control enabled or disabled.
        /// </summary>
        public bool Enabled
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Checks and corrects the control modes.
        /// </summary>
        public void ControlModeChanged()
        {
        }
        #endregion

        public VBButton ButtonToggleRibbonBar;
        public VBGrid GridContainer;

        VBRibbonButtonMobile _ButtonSearchMobile;
        List<CommandBinding> _BindingsInRoot = new List<CommandBinding>();
        List<VBRibbonButtonMobile> _ButtonListMobile = new List<VBRibbonButtonMobile>();
        bool _Initialized = false;

        /// <summary>
        /// Initializes the VB control.
        /// </summary>
        protected virtual void InitVBControl()
        {
            _Initialized = true;
            if (_RibbonWrapPanel.Children.Count > 0)
                return;

            RightControlMode = Global.ControlModes.Enabled;
            string solutionLayoutString = "";
            if (solutionLayoutString == string.Empty)
            {
                solutionLayoutString = _MobileCommandList;
            }

            string[] allCommands = solutionLayoutString.Split(',');

            if (ControlManager.WpfTheme == eWpfTheme.Gip)
            {
                _BorderRibbonPanel.Style = (Style)this.Resources["RibbonTrayBorderStyleGip"];
                CreatePanelMobile(_FileImageSourceGip, _FileString, allCommands, Database.Root.Environment.TranslateText(BSOACComponent, "Content"), false);
                CreatePanelMobile(_SearchImageSourceGip, _SearchString, allCommands, Database.Root.Environment.TranslateText(BSOACComponent, "Search"), false);
            }
            else
            {
                _BorderRibbonPanel.Style = (Style)this.Resources["RibbonTrayBorderStyleAero"];
                CreatePanelMobile(_FileImageSourceAero, _FileString, allCommands, Database.Root.Environment.TranslateText(BSOACComponent, "Content"), false);
                CreatePanelMobile(_SearchImageSourceAero, _SearchString, allCommands, Database.Root.Environment.TranslateText(BSOACComponent, "Search"), false);
            }

            if (BSOACComponent != null)
            {
                Binding binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = new PropertyPath(Const.InitState);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBRibbonMobile.ACCompInitStateProperty, binding);
            }
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public virtual void DeInitVBControl(IACComponent bso)
        {
            if (!_Initialized)
                return;
            _Initialized = false;

            if (_BindingsInRoot != null)
            {
                FrameworkElement rootParentOfBSO = RootParentOfBSO;
                if (rootParentOfBSO != null)
                {
                    foreach (CommandBinding cb in _BindingsInRoot)
                    {
                        rootParentOfBSO.CommandBindings.Remove(cb);
                    }
                }
            }
            _BindingsInRoot = null;
            if (_ButtonListMobile != null)
            {
                foreach (VBRibbonButtonMobile button in _ButtonListMobile)
                {
                    if (button.Command != null)
                    {
                        AppCommands.RemoveVBApplicationCommand(button.Command);
                        button.Command = null;
                    }
                }
            }
            _ButtonListMobile = null;


            CommandBindings.Clear();
            if (_ButtonSearchMobile != null)
            {
                _ButtonSearchMobile.MouseRightButtonUp -= searchbutton_MouseRightButtonUp;
                _ButtonSearchMobile.Click -= searchbutton_Click;
                _ButtonSearchMobile = null;
            }

            if (_SearchBox != null)
            {
                _SearchBox.KeyDown -= textbox_KeyDown;
                BindingOperations.ClearBinding(_SearchBox, TextBox.TextProperty);
            }
            if (_BorderRibbonPanel != null)
            {
                _BorderRibbonPanel.Child = null;
            }
            BindingOperations.ClearBinding(this, VBRibbonMobile.ACUrlCmdMessageProperty);
            BindingOperations.ClearBinding(this, VBRibbonMobile.ACCompInitStateProperty);
            BindingOperations.ClearAllBindings(this);

        }

        /// <summary>
        /// Calls on when initialization state is changed.
        /// </summary>
        protected void InitStateChanged()
        {
            if (BSOACComponent != null &&
                (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
                DeInitVBControl(BSOACComponent);
        }

        private void RibbonBar_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            double totalRibbonWidth = 0;
            foreach (ToolBar ribbonBar in _RibbonWrapPanel.Children)
            {
                if (ribbonBar != null)
                {
                    totalRibbonWidth += ribbonBar.ActualWidth;
                    ribbonBar.VerticalAlignment = VerticalAlignment.Top;
                }
            }

            if (this.ActualWidth < totalRibbonWidth)
            {
                var parentGrid = this.Parent as Grid;
                if (parentGrid != null)
                    parentGrid.Height = 120;
                _BorderRibbonPanel.Height = 120;
                _RibbonWrapPanel.Height = 120;
            }
            else
            {
                var parentGrid = this.Parent as Grid;
                if (parentGrid != null)
                    parentGrid.Height = 60;
                _BorderRibbonPanel.Height = 60;
                _RibbonWrapPanel.Height = 60;
            }
        }

        private void CreatePanelMobile(string[] imageSource, string[] specifiedCommandList, string[] allCommands, string caption, bool ignoreRights)
        {
            List<string> commands = new List<string>();
            List<int> commandIndexs = new List<int>();
            if (ContextACObject == null)
                return;

            int index = 0;
            foreach (string acUrl in specifiedCommandList)
            {
                if (!ignoreRights)
                {
                    IACType dcACTypeInfo = null;
                    object dcSource = null;
                    string dcPath = "";
                    Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

                    if (!ContextACObject.ACUrlBinding(acUrl, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
                    {
                        index++;
                        continue;
                    }

                    if (allCommands.Contains(acUrl) && (dcRightControlMode >= Global.ControlModes.Enabled || _EditString.Contains(acUrl)))
                    {
                        commands.Add(acUrl);
                        commandIndexs.Add(index);
                    }
                }
                else
                {
                    commands.Add(acUrl);
                    commandIndexs.Add(index);
                }
                index++;
            }

            if (commands.Count <= 0)
                return;

            FrameworkElement rootParentOfBSO = RootParentOfBSO;

            VBRibbonBarMobile ribbonBar = new VBRibbonBarMobile();
            ribbonBar.SetValue(FocusManager.IsFocusScopeProperty, false);

            index = 0;
            foreach (string command in commands)
            {
                if (command == Const.CmdSearch)
                {
                    IACType dcACTypeInfo = null;
                    object dcSource = null;
                    string dcPath = "";
                    Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;
                    if (ContextACObject.ACUrlBinding("AccessPrimary\\NavACQueryDefinition\\SearchWord", ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
                    {

                        _SearchBox = new VBTextBox();
                        _SearchBox.ShowCaption = false;
                        _SearchBox.KeyDown += new KeyEventHandler(textbox_KeyDown);
                        _SearchBox.VerticalAlignment = VerticalAlignment.Center;
                        _SearchBox.HorizontalAlignment = HorizontalAlignment.Center;
                        _SearchBox.Name = "tbFilter";
                        _SearchBox.Height = 48;
                        _SearchBox.MinHeight = 48;
                        _SearchBox.Width = 240;
                        _SearchBox.MinWidth = 240;
                        _SearchBox.FontSize = 24;
                        _SearchBox.Margin = new Thickness(0, 0, 10, 0);

                        Binding binding = new Binding();
                        binding.Source = dcSource;
                        binding.Path = new PropertyPath(dcPath);
                        _SearchBox.SetBinding(TextBox.TextProperty, binding);

                        ribbonBar.Items.Add(_SearchBox);

                        _SearchBox.SetValue(ToolBar.OverflowModeProperty, OverflowMode.Never);
                    }
                    _SearchCommand = command;
                }

                VBRibbonButtonMobile button = new VBRibbonButtonMobile();
                button.BeginInit();
                button.VerticalAlignment = VerticalAlignment.Center;
                button.IconName = imageSource[commandIndexs[index]];
                button.SetValue(ToolBar.OverflowModeProperty, OverflowMode.Never);

                if (command == Const.CmdSearch)
                {
                    //button.IsDefault = true;
                    button.MouseRightButtonUp += new MouseButtonEventHandler(searchbutton_MouseRightButtonUp);
                    button.Click += searchbutton_Click;
                    _ButtonSearchMobile = button;
                }

                ACValueList parameterList = null;
                string acCaption = "";
                switch (command)
                {
                    case Const.CmdNew:
                        button.ToolTip = Database.Root.Environment.TranslateText(BSOACComponent, "New <Ctrl>-N");
                        acCaption = Database.Root.Environment.TranslateText(BSOACComponent, Const.CmdNameNew);
                        break;
                    case Const.CmdLoad:
                        button.ToolTip = Database.Root.Environment.TranslateText(BSOACComponent, "Load <Ctrl>-L");
                        acCaption = Database.Root.Environment.TranslateText(BSOACComponent, Const.CmdNameLoad);
                        break;
                    case Const.CmdDelete:
                        button.ToolTip = Database.Root.Environment.TranslateText(BSOACComponent, "Delete <Ctrl>-D");
                        acCaption = Database.Root.Environment.TranslateText(BSOACComponent, Const.CmdNameDelete);
                        break;
                    case Const.CmdRestore:
                        button.ToolTip = Database.Root.Environment.TranslateText(BSOACComponent, "Restore <Ctrl>-<Shift>-D");
                        acCaption = Database.Root.Environment.TranslateText(BSOACComponent, Const.CmdNameRestore);
                        break;
                    case Const.CmdSave:
                        button.ToolTip = Database.Root.Environment.TranslateText(BSOACComponent, "Save <Ctrl>-S");
                        acCaption = Database.Root.Environment.TranslateText(BSOACComponent, Const.CmdNameSave);
                        break;
                    case Const.CmdUndoSave:
                        button.ToolTip = Database.Root.Environment.TranslateText(BSOACComponent, "Undo save");
                        acCaption = Database.Root.Environment.TranslateText(BSOACComponent, "Undo save");
                        break;
                    case Const.CmdSearch:
                        button.ToolTip = Database.Root.Environment.TranslateText(BSOACComponent, "Search <Ctrl>-F");
                        acCaption = Database.Root.Environment.TranslateText(BSOACComponent, "Search");
                        break;
                    case Const.CmdQueryPrintDlg:
                        button.ToolTip = Database.Root.Environment.TranslateText(BSOACComponent, "Print <Ctrl>-P");
                        acCaption = Database.Root.Environment.TranslateText(BSOACComponent, "Print");
                        break;
                    case Const.CmdQueryPreviewDlg:
                        button.ToolTip = Database.Root.Environment.TranslateText(BSOACComponent, "Preview");
                        acCaption = Database.Root.Environment.TranslateText(BSOACComponent, "Preview");
                        break;
                    case Const.CmdRedo:
                        button.ToolTip = ("Retry");
                        acCaption = Database.Root.Environment.TranslateText(BSOACComponent, "Retry");
                        break;
                    case Const.CmdExport:
                        button.ToolTip = ("Export");
                        acCaption = Database.Root.Environment.TranslateText(BSOACComponent, "Export");
                        break;
                }

                button.Command = AppCommands.AddApplicationCommand(new ACCommand(acCaption, command, parameterList));
                _ButtonListMobile.Add(button);

                CommandBinding cb = new CommandBinding(button.Command, ExecutedCommandHandler, CanExecuteCommandHandler);
                if (rootParentOfBSO != null)
                {
                    _BindingsInRoot.Add(cb);
                    rootParentOfBSO.CommandBindings.Add(cb);
                }
                else
                    CommandBindings.Add(new CommandBinding(button.Command, ExecutedCommandHandler, CanExecuteCommandHandler));

                button.Focusable = false;
                button.EndInit();

                button.RightControlMode = Global.ControlModes.Enabled;
                if (ContextACObject is IACBSO)
                {
                    ACClassMethod mth = (ContextACObject as IACBSO).ACClassMethods.FirstOrDefault(x => x.ACIdentifier == command.Replace("!", ""));
                    if (mth != null && mth.IsRightmanagement)
                        button.RightControlMode = mth.ACClass.RightManager.GetControlMode(mth);
                }

                ribbonBar.Items.Add(button);
                index++;
            }

            ribbonBar.SizeChanged += RibbonBar_SizeChanged;
            _RibbonWrapPanel.Children.Add(ribbonBar);
        }

        private FrameworkElement RootParentOfBSO
        {
            get
            {
                FrameworkElement lastRootFound = this;
                FrameworkElement rootToFind = this;
                while ((rootToFind != null) && (rootToFind.GetValue(ContentPropertyHandler.BSOACComponentProperty) as IACComponent == BSOACComponent))
                {
                    if (rootToFind.Parent == null)
                        return rootToFind;
                    lastRootFound = rootToFind;
                    if (rootToFind.Parent is FrameworkElement)
                        rootToFind = rootToFind.Parent as FrameworkElement;
                    else
                        return rootToFind;
                }
                return lastRootFound;
            }
        }

        void textbox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender == _SearchBox && e.Key == Key.Return)
            {
                ContextACObject.ACUrlCommand("AccessPrimary\\NavACQueryDefinition\\SearchWord", _SearchBox.Text);
                ContextACObject.ACUrlCommand(Const.CmdSearch);
                e.Handled = true;
            }
        }

        void ExecutedCommandHandler(object sender, RoutedEventArgs e)
        {
            ACCommand acCommand = RoutedEventHelper.GetACCommand(e);
            if (!acCommand.ParameterList.Any())
            {
                ExecuteCommand(acCommand.GetACUrl());
            }
            else
            {
                ExecuteCommand(acCommand.GetACUrl(), acCommand.ParameterList.ToValueArray());
            }
            e.Handled = true;
        }

        void CanExecuteCommandHandler(object sender, CanExecuteRoutedEventArgs e)
        {
            ACCommand acCommand = RoutedEventHelper.GetACCommand(e);
            if (string.IsNullOrEmpty(acCommand.GetACUrl()))
            {
                e.CanExecute = true;
            }
            else
            {
                if (!acCommand.ParameterList.Any())
                {
                    e.CanExecute = ContextACObject.IsEnabledACUrlCommand(acCommand.GetACUrl());
                }
                else
                {
                    e.CanExecute = ContextACObject.IsEnabledACUrlCommand(acCommand.GetACUrl(), acCommand.ParameterList);
                }
            }
        }

        void searchbutton_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            searchbutton_Click(sender, e);
        }

        void searchbutton_Click(object sender, RoutedEventArgs e)
        {
            if (ContextACObject == null)
                return;
            object searchResult = ContextACObject.ACUrlCommand(Const.CmdShowACQueryDialogPrimary);
            if (searchResult == null || !(searchResult is bool))
                return;
            bool result = (bool)searchResult;
            if (result && sender is VBRibbonButton)
            {
                VBRibbonButton vbRibbonButton = sender as VBRibbonButton;
                RoutedUICommandEx x = vbRibbonButton.Command as RoutedUICommandEx;
                ContextACObject.ACUrlCommand(x.ACCommand.GetACUrl());
            }
        }


        public void ExecuteCommand(string internalName, params Object[] acParameter)
        {
            if (ContextACObject == null)
                return;
            ContextACObject.ACUrlCommand(internalName, acParameter);
        }

        /// <summary>
        /// Gets or sets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den richtigen Kontrollmodus.
        /// </summary>
        [Category("VBControl")]
        public Global.ControlModes RightControlMode
        {
            get;
            set;
        }

        /// <summary>
        /// Disables the control. XAML sample: DisabledModes="Disabled"
        /// </summary>
        /// <summary xml:lang="de">
        /// Deaktiviert die Steuerung. XAML-Probe: DisabledModes="Disabled"
        /// </summary>
        [Category("VBControl")]
        public string DisabledModes
        {
            get;
            set;
        }

        #region IDataContent Member
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get; set;
        }

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBRibbonMobile));

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        /// <summary>
        /// Gets or sets the ACIdentifier.
        /// </summary>
        public string ACIdentifier
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
            }
        }

        #endregion


        #region IACObject
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
        /// Determines is ACUrlCommand is enabled or disabled.
        /// </summary>
        /// <param name="acUrl">The acUrl of command.</param>
        /// <param name="acParameter">The command parameters.</param>
        ///<returns>Returns true if is ACUrlCommand is enabled, otherwise false.</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
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
        #endregion

        #region IACObject Member

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
    }
}
