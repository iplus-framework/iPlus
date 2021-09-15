using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;

namespace gip.core.layoutengine
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBFlowDocumentScrollViewer'}de{'VBFlowDocumentScrollViewer'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBFlowDocumentScrollViewer : FlowDocumentScrollViewer, IVBContent
    {
        public VBFlowDocumentScrollViewer()
        {
        }

        protected override void OnInitialized(EventArgs e)
        {
            Loaded += VBFlowDocumentScrollViewer_Loaded;
            base.OnInitialized(e);
        }

        private void VBFlowDocumentScrollViewer_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
        }

        /// <summary>
        /// Determines is control initialized or not.
        /// </summary>
        protected bool _Initialized = false;

        IACType _ACTypeInfo = null;

        protected virtual void InitVBControl()
        {
            if (_Initialized || DataContext == null || ContextACObject == null)
                return;
            _Initialized = true;
            if (String.IsNullOrEmpty(VBContent))
                return;

            IACType dcACTypeInfo = null;
            object dcSource = null;
            string dcPath = "";
            Global.ControlModes dcRightControlMode = Global.ControlModes.Hidden;

            if (!ContextACObject.ACUrlBinding(VBContent, ref dcACTypeInfo, ref dcSource, ref dcPath, ref dcRightControlMode))
            {
                this.Root().Messages.LogDebug("Error00003", "VBTextBox", VBContent);
                return;
            }
            _ACTypeInfo = dcACTypeInfo;
            RightControlMode = dcRightControlMode;

            if (String.IsNullOrEmpty(VBContent))
                return;

            Binding binding = new Binding();
            binding.Source = dcSource;
            binding.Path = new PropertyPath(dcPath);
            binding.Mode = BindingMode.OneWay;
            binding.NotifyOnSourceUpdated = true;
            binding.NotifyOnTargetUpdated = true;
            this.SetBinding(DocumentContentProperty, binding);

            if (BSOACComponent != null)
            {
                binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = new PropertyPath(Const.InitState);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBFlowDocumentScrollViewer.ACCompInitStateProperty, binding);
            }
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBFlowDocumentScrollViewer), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
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
                typeof(ACUrlCmdMessage), typeof(VBFlowDocumentScrollViewer),
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
                typeof(ACInitState), typeof(VBFlowDocumentScrollViewer),
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
            VBFlowDocumentScrollViewer thisControl = dependencyObject as VBFlowDocumentScrollViewer;
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
        /// Invoked on when initialization state is changed.
        /// </summary>
        protected void InitStateChanged()
        {
            if (BSOACComponent != null &&
                (ACCompInitState == ACInitState.Destructed || ACCompInitState == ACInitState.DisposedToPool))
                DeInitVBControl(BSOACComponent);
        }

        public string DocumentContent
        {
            get { return (string)GetValue(DocumentContentProperty); }
            set { SetValue(DocumentContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DocumentContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentContentProperty =
            DependencyProperty.Register("DocumentContent", typeof(string), typeof(VBFlowDocumentScrollViewer), new PropertyMetadata(null, DocumnetContentChanged));

        private static void DocumnetContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            VBFlowDocumentScrollViewer thisControl = d as VBFlowDocumentScrollViewer;
            if (thisControl != null)
            {
                if (string.IsNullOrEmpty(thisControl.DocumentContent))
                {
                    thisControl.Document = null;
                    return;
                }

                try
                {
                    StringReader stringReader = new StringReader(thisControl.DocumentContent);
                    System.Xml.XmlReader xmlReader = System.Xml.XmlReader.Create(stringReader);
                    FlowDocument flowDoc = XamlReader.Load(xmlReader) as FlowDocument;
                    thisControl.Document = flowDoc;
                }
                catch
                {
                    thisControl.Document = null;
                }
            }
        }



        #region IVBContent members

        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return _ACTypeInfo as ACClassProperty;
            }
        }

        public Global.ControlModes RightControlMode
        {
            get;
            set;
        }

        public string DisabledModes
        {
            get;
            set;
        }

        public string VBContent
        {
            get { return (string)GetValue(VBContentProperty); }
            set { SetValue(VBContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VBContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VBContentProperty =
            DependencyProperty.Register("VBContent", typeof(string), typeof(VBFlowDocumentScrollViewer), new PropertyMetadata(null));


        public IACObject ContextACObject => DataContext as IACObject;

        public IACObject ParentACObject => this.Parent as IACObject;

        public IACType ACType => this.ReflectACType();

        public IEnumerable<IACObject> ACContentList => this.ReflectGetACContentList();

        public string ACIdentifier => this.ReflectGetACIdentifier();

        public string ACCaption => ACIdentifier;

        public void ACAction(ACActionArgs actionArgs)
        {
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ACCommand:
                    var query = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand);
                    if (query.Any())
                    {
                        ACCommand acCommand = query.First() as ACCommand;
                        ACUrlCommand(acCommand.GetACUrl(), null);
                    }
                    break;
            }
        }

        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return this.ReflectACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }

        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        public void DeInitVBControl(IACComponent bso)
        {
            Document = null;
            Loaded -= VBFlowDocumentScrollViewer_Loaded;
            BindingOperations.ClearBinding(this, DocumentContentProperty);
            BindingOperations.ClearBinding(this, BSOACComponentProperty);
            BindingOperations.ClearAllBindings(this);
        }

        public string GetACUrl(IACObject rootACObject = null)
        {
            return this.ReflectGetACUrl(rootACObject);
        }

        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            switch (actionArgs.ElementAction)
            {
                case Global.ElementActionType.ACCommand:
                    {
                        var query = actionArgs.DropObject.ACContentList.Where(c => c is ACCommand);
                        if (query.Any())
                        {
                            ACCommand acCommand = query.First() as ACCommand;
                            return this.ReflectIsEnabledACUrlCommand(acCommand.GetACUrl(), null);
                        }
                    }
                    break;
            }
            return false;
        }

        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        #endregion
    }
}
