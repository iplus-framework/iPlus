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
using System.Xml.Linq;
using System.ComponentModel;
using System.Windows.Markup;
using gip.ext.chart.avui;
using gip.ext.chart.avui.DataSources;
using System.IO;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using gip.core.layoutengine.avui.Helperclasses;
using gip.core.datamodel;
using System.Collections.ObjectModel;
using System.Transactions;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents a control that displaying logged data.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur Anzeige von geloggten Daten.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBPropertyLogChart'}de{'VBPropertyLogChart'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public partial class VBPropertyLogChart : UserControl, IVBContent, IVBSource, IACObject
    {
        string _DataSource;
        string _DataShowColumns;
        string _DataChilds;

        /// <summary>
        /// Creates a new instace of VBPropertyLogChart.
        /// </summary>
        public VBPropertyLogChart()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            Loaded += VBPropertyLogChart_Loaded;
            Unloaded += VBPropertyLogChart_Unloaded;
            base.OnInitialized(e);
        }

        /// <summary>
        /// Determines is control initialized or not.
        /// </summary>
        protected bool _Initialized = false;
        private void InitVBControl()
        {
            if (_Initialized)
                return;
            _Initialized = true;
            if (BSOACComponent != null)
            {
                Binding binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = new PropertyPath(Const.InitState);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBPropertyLogChart.ACCompInitStateProperty, binding);
            }

            if (SmoothingOn != null)
            {
                SmoothingOn.DisplayMemberPath = "ACCaption";

                Binding binding = new Binding();
                binding.Source = Global.InterpolationMethodList;
                binding.Mode = BindingMode.OneWay;
                SmoothingOn.SetBinding(VBComboBox.ItemsSourceProperty, binding);

            }

            SetConsoleVisibility();
            if (ChartControl != null)
                ChartControl.DisplayAsArchive = DisplayAsArchive;
            ucDatepickerFrom.SelectedDate = DateTime.Now;
            ucDatepickerTo.SelectedDate = DateTime.Now;
        }


        bool _Loaded = false;
        private void VBPropertyLogChart_Loaded(object sender, RoutedEventArgs e)
        {
            InitVBControl();
            if (_Loaded)
                return;
            _Loaded = true;
        }

        void VBPropertyLogChart_Unloaded(object sender, RoutedEventArgs e)
        {
            if (!_Loaded)
                return;

            //if (BSOACComponent != null && !String.IsNullOrEmpty(VBContent))
            //    BSOACComponent.RemoveWPFRef(this.GetHashCode());

            _Loaded = false;
        }

        /// <summary>
        /// Represents the dependency property for ChartControl.
        /// </summary>
        public static readonly DependencyProperty ChartControlProperty = DependencyProperty.Register("ChartControl", typeof(IVBChart), typeof(VBPropertyLogChart));

        /// <summary>
        /// Gets or sets the ChartControl.
        /// </summary>
        [Category("VBControl")]
        public IVBChart ChartControl
        {
            get
            {
                return (IVBChart)GetValue(ChartControlProperty);
            }
            set
            {
                SetValue(ChartControlProperty, value);
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
            this.Loaded -= VBPropertyLogChart_Loaded;
            this.Unloaded -= VBPropertyLogChart_Unloaded;

            if (ChartControl != null)
                ChartControl.DeInitVBControl(bso);

            BindingOperations.ClearBinding(this, VBPropertyLogChart.ACCompInitStateProperty);
            this.ClearAllBindings();
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

        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly DependencyProperty ACCompInitStateProperty =
            DependencyProperty.Register("ACCompInitState",
                typeof(ACInitState), typeof(VBPropertyLogChart),
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
            VBPropertyLogChart thisControl = dependencyObject as VBPropertyLogChart;
            if (thisControl == null)
                return;
            if (args.Property == ACCompInitStateProperty)
                thisControl.InitStateChanged();
            else if (args.Property == BSOACComponentProperty)
            {
                if (args.NewValue == null && args.OldValue != null)
                {
                    IACBSO bso = args.OldValue as IACBSO;
                    if (bso != null && thisControl != null)
                        thisControl.DeInitVBControl(bso);
                }
            }
        }


        #region IDataField Members

        /// <summary>
        /// Represents the dependency property for VBContent.
        /// </summary>
        public static readonly DependencyProperty VBContentProperty
            = DependencyProperty.Register("VBContent", typeof(string), typeof(VBPropertyLogChart));

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

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
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

        string _Caption;
        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get
            {
                return _Caption;
            }
            set
            {
                _Caption = value;
            }
        }

        /// <summary>
        /// Determines is control caption shown or not.
        /// </summary>
        /// <summary xml:lang="de">
        /// Legt fest, ob die Steuertitel angezeigt werden oder nicht.
        /// </summary>
        [Category("VBControl")]
        public bool ShowCaption
        {
            get;
            set;
        }

        #endregion

        #region IVBSource Members

        /// <summary>
        /// Gets or sets the VBSource.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die VBSource.
        /// </summary>
        [Category("VBControl")]
        public string VBSource
        {
            get
            {
                return _DataSource;
            }
            set
            {
                _DataSource = value;
            }
        }

        /// <summary>
        /// Gets or sets the VBShowColumns.
        /// </summary>
        public string VBShowColumns
        {
            get
            {
                return _DataShowColumns;
            }
            set
            {
                _DataShowColumns = value;
            }
        }

        /// <summary>
        /// Gets or sets the VBDisabledColumns.
        /// </summary>
        public string VBDisabledColumns
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the VBChilds.
        /// </summary>
        public string VBChilds
        {
            get
            {
                return _DataChilds;
            }
            set
            {
                _DataChilds = value;
            }
        }

        /// <summary>
        /// Gets or sets the ACQueryDefinition.
        /// </summary>
        public ACQueryDefinition ACQueryDefinition
        {
            get;
            set;
        }

        #endregion

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
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBPropertyLogChart), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
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

        IACType _VBContentPropertyInfo = null;
        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get
            {
                return _VBContentPropertyInfo as ACClassProperty;
            }
        }

        private Nullable<bool> _DisplayAsArchive;
        /// <summary>
        /// Determines is displays as archive or not.
        /// </summary>
        [Category("VBControl")]
        public bool DisplayAsArchive
        {
            get
            {
                if (_DisplayAsArchive.HasValue)
                    return _DisplayAsArchive.Value;
                return false;
            }
            set
            {
                if (_DisplayAsArchive.HasValue)
                    return;
                _DisplayAsArchive = value;
            }
        }

        private bool _DisplayControlConsole = true;
        /// <summary>
        /// Determines is control console displayed or not.
        /// </summary>
        [Category("VBControl")]
        public bool DisplayControlConsole
        {
            get
            {
                return _DisplayControlConsole;
            }
            set
            {
                _DisplayControlConsole = value;
                //if (_Loaded)
                    //UpdateConsoleVisibility();
            }
        }

        #endregion

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

        #region IVBContent Member

        /// <summary>
        /// Checks and corrects the control modes.
        /// </summary>
        public void ControlModeChanged()
        {
        }     

        private IEnumerable<IPropertyLogChartItem> WorkChartItems
        {
            get
            {
                if (ChartControl != null)
                {
                    return ChartControl.PropertyLogItems.Where(c => c is IPropertyLogChartItem).Select(c => c as IPropertyLogChartItem);
                }
                else
                    return null;
            }
        }

        #endregion

        #region LiveConsole members

        private void ucButtonStop_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayAsArchive || (WorkChartItems == null))
                return;
            if (!WorkChartItems.Any())
                return;
            foreach (IPropertyLogChartItem propertyItem in WorkChartItems)
            {
                if (propertyItem.ACProperty == null)
                    continue;
                if (propertyItem.ACProperty.LiveLog == null)
                    continue;
                propertyItem.ACProperty.LiveLog.StopLiveLogging();
                //Console.WriteLine(propertyItem.ACCaption + " " + propertyItem.ACProperty.LiveLog.GetHashCode());
            }
        }

        private void ucButtonPause_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayAsArchive || (WorkChartItems == null))
                return;
            if (!WorkChartItems.Any())
                return;
            foreach (IPropertyLogChartItem propertyItem in WorkChartItems)
            {
                if (propertyItem.ACProperty == null)
                    continue;
                if (propertyItem.ACProperty.LiveLog == null)
                    continue;
                propertyItem.ACProperty.LiveLog.PauseLiveLogging();
            }
        }

        private void ucButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            if (DisplayAsArchive || (WorkChartItems == null))
                return;
            if (!WorkChartItems.Any())
                return;
            foreach (IPropertyLogChartItem propertyItem in WorkChartItems)
            {
                if (propertyItem.ACProperty == null)
                    continue;
                if (propertyItem.ACProperty.LiveLog == null)
                    continue;
                propertyItem.ACProperty.LiveLog.RunLiveLogging();
            }
        }

        private void ucButtonLoadArchive_Click(object sender, RoutedEventArgs e)
        {
            if (WorkChartItems == null)
                return;
            DateTime from = ucDatepickerFrom.SelectedDate.HasValue ? ucDatepickerFrom.SelectedDate.Value : DateTime.Now.AddDays(-1);
            DateTime to =   ucDatepickerTo.SelectedDate.HasValue ? ucDatepickerTo.SelectedDate.Value : DateTime.Now;

            Global.InterpolationMethod interpolEnum = Global.InterpolationMethod.None;
            ACValueItem selectedInterpol = SmoothingOn.SelectedValue as ACValueItem;
            if (selectedInterpol != null)
            {
                interpolEnum = (Global.InterpolationMethod)selectedInterpol.Value;
            }

            ChartControl.InitializeChartArchive(from, to, interpolEnum, Convert.ToInt32(SmoothingRange.Value), SmootingDecay.Value);
        }

        private void ucButtonPrintMode_Click(object sender, RoutedEventArgs e)
        {
            ChartControl.SwitchNormalPrintingMode();
        }

        private XpsDocument _CurrentXpsDoc;
        private void ucButtonPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BitmapSource bitmapSource = ChartControl.CreatePrintableBitmap();
                FixedDocumentSequence _fSeq = new FixedDocumentSequence();
                FixedDocument fDoc = new FixedDocument();

                DocumentReference docRef = new DocumentReference();
                docRef.SetDocument(fDoc);
                _fSeq.References.Add(docRef);

                FixedPage fPage = new FixedPage();
                // Für A4, Querformat
                fPage.Width = 11.69 * 96;
                fPage.Height = 8.27 * 96;

                Grid grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
                fPage.Children.Add(grid);

                TextBlock txt = new TextBlock();
                txt.Text = ContextACObject.ACCaption;
                if (WorkChartItems != null)
                {
                    if (WorkChartItems.Any())
                    {
                        IPropertyLogChartItem item = WorkChartItems.First();
                        if ((item.ACProperty != null) && (item.ACProperty.ParentACComponent != null))
                            txt.Text = WorkChartItems.First().ACProperty.ParentACComponent.ACCaption;
                    }
                }
                txt.SetValue(Grid.RowProperty, 0);
                txt.FontSize = 20;
                grid.Children.Add(txt);

                Image img = new Image();
                img.Source = bitmapSource;
                img.SetValue(Grid.RowProperty, 1);
                double scaleF = 0;
                double scaleFWidth = fPage.Width / bitmapSource.Width;
                double scaleFHeight = fPage.Height / bitmapSource.Height;
                if (scaleFWidth > scaleFHeight)
                    scaleF = scaleFHeight;
                else
                    scaleF = scaleFWidth;
                img.Width = bitmapSource.Width * scaleF;
                img.Height = bitmapSource.Height * scaleF;
                img.Stretch = Stretch.Fill;
                grid.Children.Add(img);

                PageContent pageContent = new PageContent();
                ((IAddChild)pageContent).AddChild(fPage);

                fDoc.Pages.Add(pageContent);

                string fileName = "chart.xps";
                if (File.Exists(fileName))
                    File.Delete(fileName);

                _CurrentXpsDoc = new XpsDocument(fileName, FileAccess.ReadWrite);
                XpsDocumentWriter writer = XpsDocument.CreateXpsDocumentWriter(_CurrentXpsDoc);
                writer.Write(_fSeq);

                VBWindowDialog vbDialog = new VBWindowDialog(this);
                vbDialog.Content = new DocumentViewer();
                vbDialog.Loaded += new RoutedEventHandler(vbDialog_Loaded);
                vbDialog.Show();
                vbDialog.Loaded -= vbDialog_Loaded;
                _CurrentXpsDoc.Close();
            }
            catch (Exception ex)
            {
                this.Root().Messages.LogDebug("Message00002", "", ex.Message);
            }
        }

        void vbDialog_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is VBWindowDialog)
            {
                ((sender as VBWindowDialog).Content as DocumentViewer).Document = _CurrentXpsDoc.GetFixedDocumentSequence();
            }
        }

        private bool _CurrentLogStateInit = true;
        private PropertyLogListInfo.PropertyLogState _CurrentLogState = PropertyLogListInfo.PropertyLogState.Stopped;

        /// <summary>
        /// Refreshes the live console.
        /// </summary>
        public void RefreshLiveConsole()
        {
            if (ucLiveConsole.Visibility == System.Windows.Visibility.Collapsed || WorkChartItems == null)
                return;
            PropertyLogListInfo.PropertyLogState logState = PropertyLogListInfo.PropertyLogState.Stopped;
            if (WorkChartItems.Any())
            {
                IPropertyLogChartItem chartItem = WorkChartItems.Last();
                if (chartItem.ACProperty != null)
                    logState = chartItem.ACProperty.LiveLog.LogState;
            }
            if ((logState != _CurrentLogState) || _CurrentLogStateInit)
            {
                _CurrentLogStateInit = false;
                _CurrentLogState = logState;
                switch (_CurrentLogState)
                {
                    case PropertyLogListInfo.PropertyLogState.LogActive:
                        ucButtonPlay.IsEnabled = false;
                        ucButtonPause.IsEnabled = true;
                        ucButtonStop.IsEnabled = true;
                        break;
                    case PropertyLogListInfo.PropertyLogState.Paused:
                        ucButtonPlay.IsEnabled = true;
                        ucButtonPause.IsEnabled = false;
                        ucButtonStop.IsEnabled = true;
                        break;
                    case PropertyLogListInfo.PropertyLogState.Stopped:
                        ucButtonPlay.IsEnabled = true;
                        ucButtonPause.IsEnabled = false;
                        ucButtonStop.IsEnabled = false;
                        break;
                }
            }
        }

        private void SetConsoleVisibility()
        {
            if (DisplayControlConsole)
            {
                if (DisplayAsArchive)
                {
                    ucArchiveConsole.RightControlMode = Global.ControlModes.Enabled;
                    ucLiveConsole.RightControlMode = Global.ControlModes.Hidden;
                }
                else
                {
                    ucLiveConsole.RightControlMode = Global.ControlModes.Enabled;
                    ucArchiveConsole.RightControlMode = Global.ControlModes.Hidden;
                }
            }
            else
            {
                ucLiveConsole.RightControlMode = Global.ControlModes.Collapsed;
                ucArchiveConsole.RightControlMode = Global.ControlModes.Collapsed;
                ucInterpolationConsole.RightControlMode = Global.ControlModes.Collapsed;
                ucPrintConsole.RightControlMode = Global.ControlModes.Collapsed;
            }
        }

        #endregion

        #region IACInteractiveObject Member

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
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
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

        #endregion

        private void SmoothingOn_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            InterpolationChanged();
        }

        private void SmoothingRange_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            InterpolationChanged();
        }

        private void SmootingDecay_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            InterpolationChanged();
        }

        private void InterpolationChanged()
        {
            if (ChartControl == null || DisplayAsArchive || !this.IsLoaded)
                return;
            Global.InterpolationMethod interpolEnum = Global.InterpolationMethod.None;
            ACValueItem selectedInterpol = SmoothingOn.SelectedValue as ACValueItem;
            if (selectedInterpol != null)
            {
                interpolEnum = (Global.InterpolationMethod)selectedInterpol.Value;
            }

            ChartControl.InterpolationParamsChangedInView(interpolEnum, Convert.ToInt32(SmoothingRange.Value), SmootingDecay.Value);
        }
    }
}
