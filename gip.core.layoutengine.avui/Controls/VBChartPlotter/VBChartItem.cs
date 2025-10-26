using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using gip.core.datamodel;
using gip.core.layoutengine.avui.Helperclasses;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Represents the item in VBPropertyLogChart.
    /// </summary>
    public class VBChartItem : TemplatedControl, IVBChartItem
    {
        /// <summary>
        /// Creates a new instance of VBChartItem.
        /// </summary>
        public VBChartItem() : base()
        {
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
                this.Bind(DataContextProperty, binding);
            }
            _Initialized = true;

            ChartItem.InitVBControl(acComponent != null ? acComponent : DataContext as IACObject);

            if (BSOACComponent != null)
            {
                Binding binding = new Binding();
                binding.Source = BSOACComponent;
                binding.Path = Const.InitState;
                binding.Mode = BindingMode.OneWay;
                Bind(VBChartItem.ACCompInitStateProperty, binding);

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
            if (BindingOperations.GetBindingExpressionBase(this, DataContextProperty) == null)
            {
                Binding binding = new Binding();
                binding.Source = dataContext;
                this.Bind(DataContextProperty, binding);
            }
            if (BindingOperations.GetBindingExpressionBase(this, VBChartItem.BSOACComponentProperty) == null)
            {
                Binding binding = new Binding();
                binding.Source = bsoContext;
                this.Bind(VBChartItem.BSOACComponentProperty, binding);
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

        public IEnumerable<IVBChartTuple> DataSeries
        {
            get { return ChartItem != null ? ChartItem.DataSeries : null; }
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
        public static readonly StyledProperty<IACBSO> BSOACComponentProperty =
            ContentPropertyHandler.BSOACComponentProperty.AddOwner<VBChartItem>();
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

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
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
        public static readonly StyledProperty<ACInitState> ACCompInitStateProperty =
            AvaloniaProperty.Register<VBChartItem, ACInitState>(nameof(ACCompInitState));
        /// <summary>
        /// Gets or sets the ACCompInitState.
        /// </summary>
        public ACInitState ACCompInitState
        {
            get { return (ACInitState)GetValue(ACCompInitStateProperty); }
            set { SetValue(ACCompInitStateProperty, value); }
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ACCompInitStateProperty)
                InitStateChanged();
            else if (change.Property == BSOACComponentProperty)
            {
                if (change.NewValue == null && change.OldValue != null && !String.IsNullOrEmpty(VBContent))
                {
                    IACBSO bso = change.OldValue as IACBSO;
                    if (bso != null)
                        DeInitVBControl(bso);
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
                Color color = GetRandomColor();
                chartItem.LineColor = color.ToString();
                return color;
            }
            else
            {
                return (Color)Color.Parse(chartItem.LineColor);
            }
        }

        public static Color GetRandomColor()
        {
            Random random = new Random(DateTime.Now.Millisecond);

            Color rgb = new Color(
                255,
                (byte)random.Next(0, 255),
                (byte)random.Next(0, 255),
                (byte)random.Next(0, 255));

            return rgb;
        }
    }
}
