using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using gip.ext.chart;
using gip.core.datamodel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.ComponentModel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents the item in VBPropertyLogChart.
    /// </summary>
    public class VBChartItem : FrameworkElement, IVBChartItem
    {
        /// <summary>
        /// Creates a new instance of VBChartItem.
        /// </summary>
        public VBChartItem() : base()
        {
        }

        /// <summary>
        /// The event hander for Initialized event.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        /// <summary>
        /// Determines is control initialized or not.
        /// </summary>
        protected bool _Initialized = false;

        /// <summary>
        /// Initializes the VBControl.
        /// </summary>
        /// <param name="acComponent">The acComponent parameter.</param>
        public void InitVBControl(IACObject acComponent)
        {
            if (_Initialized)
                return;
            if (DataContext == null)
            {
                if (acComponent == null)
                    return;
                Binding binding = new Binding();
                binding.Source = acComponent;
                this.SetBinding(FrameworkElement.DataContextProperty, binding);
            }
            _Initialized = true;

            ChartItem.InitVBControl(acComponent != null ? acComponent : DataContext as IACObject);

            if (BSOACComponent != null)
            {
                Binding binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = new PropertyPath(Const.InitState);
                binding.Mode = BindingMode.OneWay;
                SetBinding(VBChartItem.ACCompInitStateProperty, binding);

                if (ACProperty != null)
                {
                    IACObject boundToObject = ACProperty.ParentACComponent as IACObject;
                    try
                    {
                        if (boundToObject != null)
                            BSOACComponent.AddWPFRef(this.GetHashCode(), boundToObject);
                    }
                    catch (Exception exw)
                    {
                        this.Root().Messages.LogDebug("VBChartItem", "AddWPFRef", exw.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Sets VB bindings.
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        /// <param name="bsoContext">The BSO context.</param>
        public void SetVBBindings(object dataContext, IACBSO bsoContext)
        {
            if (!BindingOperations.IsDataBound(this, FrameworkElement.DataContextProperty))
            {
                Binding binding = new Binding();
                binding.Source = dataContext;
                this.SetBinding(FrameworkElement.DataContextProperty, binding);
            }
            if (!BindingOperations.IsDataBound(this, VBChartItem.BSOACComponentProperty))
            {
                Binding binding = new Binding();
                binding.Source = bsoContext;
                this.SetBinding(VBChartItem.BSOACComponentProperty, binding);
            }
        }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public void DeInitVBControl(IACComponent bso)
        {
            if (!_Initialized)
                return;
            _Initialized = false;
            if (bso != null && bso is IACBSO)
                (bso as IACBSO).RemoveWPFRef(this.GetHashCode());
            BindingOperations.ClearBinding(this, VBChartItem.ACCompInitStateProperty);
            BindingOperations.ClearBinding(this, FrameworkElement.DataContextProperty);
            BindingOperations.ClearBinding(this, VBChartItem.BSOACComponentProperty);
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

        protected ChartItem _ChartItem = null;
        public virtual ChartItem ChartItem
        {
            get
            {
                if (_ChartItem == null)
                    _ChartItem = new ChartItem();
                return _ChartItem;
            }
        }


        /// <summary>
        /// Gets the ACProperty.
        /// </summary>
        public IACPropertyBase ACProperty
        {
            get { return ChartItem != null ? ChartItem.ACProperty : null; }
        }

        public VBChartItemDisplayMode DisplayMode
        {
            get
            {
                return ChartItem.DisplayMode;
            }
            set
            {
                ChartItem.DisplayMode = value;
            }
        }
        /// <summary>
        /// Gets or sets the color of line.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die Farbe der Linie.
        /// </summary>
        [Category("VBControl")]
        public string LineColor
        {
            get
            {
                return ChartItem.LineColor;
            }
            set
            {
                ChartItem.LineColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the thickess of line.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die Dicke der Linie.
        /// </summary>
        [Category("VBControl")]
        public double LineThickness
        {
            get
            {
                return ChartItem.LineThickness;
            }
            set
            {
                ChartItem.LineThickness = value;
            }
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get
            {
                return ChartItem.ACIdentifier;
            }
            set
            {
                ChartItem.ACIdentifier = value;
            }
        }

        /// <summary>
        /// Determines is line digital or not.
        /// </summary>
        /// <summary xml:lang="de">
        /// Bestimmt, ob die Leitung digital ist oder nicht.
        /// </summary>
        [Category("VBControl")]
        public bool IsDigitalLine
        {
            get
            {
                return ChartItem.IsDigitalLine;
            }
            set
            {
                ChartItem.IsDigitalLine = value;
            }
        }

        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get { return ChartItem.VBContentPropertyInfo; }
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
            get { return ChartItem.RightControlMode; }
        }

        /// <summary>
        /// Checks and corrects the control modes.
        /// </summary>
        public void ControlModeChanged()
        {
            ChartItem.ControlModeChanged();
        }

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get
            {
                return ChartItem.VBContent;
            }
            set
            {
                ChartItem.VBContent = value;
            }
        }

        /// <summary>
        /// Represents the dependency property for BSOACComponent.
        /// </summary>
        public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBChartItem), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits, new PropertyChangedCallback(OnDepPropChanged)));
        /// <summary>
        /// Gets or sets the BSOACComponent.
        /// </summary>
        public IACBSO BSOACComponent
        {
            get { return (IACBSO)GetValue(BSOACComponentProperty); }
            set { SetValue(BSOACComponentProperty, value); }
        }


        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get { return ChartItem.ContextACObject; }
        }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        public void ACAction(ACActionArgs actionArgs)
        {
            ChartItem.ACAction(actionArgs);
        }

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        public bool IsEnabledACAction(ACActionArgs actionArgs)
        {
            return ChartItem.IsEnabledACAction(actionArgs);
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get { return ChartItem.ACCaption; }
            set { ChartItem.ACCaption = value; }
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get { return ChartItem.ACType; }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get { return ChartItem.ACContentList; }
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
            return ChartItem.ACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Determines is ACUrlCommand is enabled or disabled.
        /// </summary>
        /// <param name="acUrl">The acUrl of command.</param>
        /// <param name="acParameter">The command parameters.</param>
        ///<returns>Returns true if is ACUrlCommand is enabled, otherwise false.</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return ChartItem.IsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get { return ChartItem.ParentACObject; }
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

        #endregion

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

        /// <summary>
        /// Gets or sets the AxisId.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die AxisId.
        /// </summary>
        [Category("VBControl")]
        public string AxisId
        {
            get
            {
                return ChartItem.AxisId;
            }
            set
            {
                ChartItem.AxisId = value;
            }
        }

        /// <summary>
        /// Gets or sets the LineId.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt die LineId.
        /// </summary>
        [Category("VBControl")]
        public string LineId
        {
            get
            {
                return ChartItem.LineId;
            }
            set
            {
                ChartItem.LineId = value;
            }
        }


        #region Reference-Handling
        /// <summary>
        /// Represents the dependency property for ACCompInitState.
        /// </summary>
        public static readonly DependencyProperty ACCompInitStateProperty = DependencyProperty.Register("ACCompInitState", typeof(ACInitState), typeof(VBChartItem), new PropertyMetadata(new PropertyChangedCallback(OnDepPropChanged)));

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
            VBChartItem thisControl = dependencyObject as VBChartItem;
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
        #endregion

    }

    /// <summary>
    /// Represents the extension for VBChartItem.
    /// </summary>
    public static class VBChartItemExtension
    {
        /// <summary>
        /// Gets the line color.
        /// </summary>
        /// <param name="chartItem">The chart item.</param>
        /// <returns>Returns the color of line.</returns>
        public static Color GetLineColor(this IVBChartItem chartItem)
        {
            if (String.IsNullOrEmpty(chartItem.LineColor))
            {
                Color color = ColorHelper.CreateRandomHsbColor();
                chartItem.LineColor = color.ToString();
                return color;
            }
            else
            {
                return (Color)ColorConverter.ConvertFromString(chartItem.LineColor);
            }
        }
    }
}
