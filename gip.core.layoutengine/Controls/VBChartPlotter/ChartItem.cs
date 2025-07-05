using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Collections;
using System.Collections.Specialized;

using System.Runtime.Serialization;
using gip.core.datamodel;
using System.ComponentModel;

namespace gip.core.layoutengine
{
    public enum VBChartItemDisplayMode
    {
        PropertyLog,

        /// <summary>
        /// BindableChartSeries-Property is used for Displaying Chart
        /// Value1-Property of IVBChartTuple is Mapped to X-Axis and Value2-Property to Y-Axis
        /// </summary>
        MapTupleValue1ToX,

        /// <summary>
        /// BindableChartSeries-Property is used for Displaying Chart
        /// Value1-Property of IVBChartTuple is Mapped to Y-Axis and Value2-Property to X-Axis
        /// </summary>
        MapTupleValue2ToX
    }

    public interface IVBChartItem : IVBContent
    {
        void InitVBControl(IACObject acComponent);
        IACPropertyBase ACProperty { get; }
        string LineColor { get; set; }
        double LineThickness { get; set; }
        VBChartItemDisplayMode DisplayMode { get; set; }
        IEnumerable<IVBChartTuple> DataSeries { get; }
    }

    public class ChartItem : IVBChartItem
    {
        public ChartItem()
        {
        }

        public ChartItem(IACObject acObject, string dataContent)
        {
            VBContent = dataContent;
            InitVBControl(acObject);
        }

        public ChartItem(IACPropertyBase acProperty)
        {
            _ACProperty = acProperty;
        }

        public void InitVBControl(IACObject acObject)
        {
            if (_ACProperty != null)
                return;
            if (acObject == null)
                return;
            if (String.IsNullOrEmpty(_VBContent))
                return;
            IACComponent acComp = null;
            string urlObject = _VBContent;
            _ACMemberPath = _VBContent;
            int posLastBackSlash = _VBContent.LastIndexOf('\\');
            if (posLastBackSlash > 0)
            {
                if ((posLastBackSlash + 1) >= _VBContent.Length)
                    return;
                urlObject = _VBContent.Substring(0, posLastBackSlash);
                _ACMemberPath = _VBContent.Substring(posLastBackSlash + 1);

                object result = acObject.ACUrlCommand(urlObject, null);
                if (result == null)
                    return;
                if (!(result is IACObject))
                    return;
                acObject = (IACObject)result;
                acComp = result as IACComponent;
            }
            else
            {
                acComp = acObject as IACComponent;
            }

            if (acComp != null)
            {
                object member = acComp.GetMember(_ACMemberPath);
                if (member == null)
                    return;
                if (!(member is IACPropertyBase))
                    return;
                _ACProperty = member as IACPropertyBase;
            }
            else
            {
                _ContentACObject = acObject;
            }
        }

        private string _VBContent;
        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get
            {
                return _VBContent;
            }
            set
            {
                _VBContent = value;
            }
        }
        private string _ACMemberPath;

        private string _ACCaption;

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [Category("VBControl")]
        public string ACCaption
        {
            get
            {
                if (!String.IsNullOrEmpty(_ACCaption))
                    return _ACCaption;
                if (_ACProperty != null)
                    return _ACProperty.ACCaption;
                return "";
            }
            set
            {
                _ACCaption = value;
            }
        }

        protected VBChartItemDisplayMode _DisplayMode = VBChartItemDisplayMode.MapTupleValue1ToX;
        public VBChartItemDisplayMode DisplayMode
        {
            get
            {
                return _DisplayMode;
            }

            set
            {
                _DisplayMode = value;
            }
        }


        private string _LineColor;
        [Category("VBControl")]
        public string LineColor
        {
            get
            {
                return _LineColor;
            }

            set
            {
                _LineColor = value;
            }
        }

        private double _LineThickness = 1;
        [Category("VBControl")]
        public double LineThickness
        {
            get
            {
                if (_LineThickness < 0.9)
                    _LineThickness = 1;
                return _LineThickness;
            }
            set
            {
                _LineThickness = value;
            }
        }

        private bool _IsDigitalLine;
        [Category("VBControl")]
        public bool IsDigitalLine
        {
            get
            {
                return _IsDigitalLine;
            }
            set
            {
                _IsDigitalLine = value;
            }
        }


        private IACObject _ContentACObject = null;
        public IACObject ContentACObject
        {
            get
            {
                return _ContentACObject;
            }
        }

        private IACPropertyBase _ACProperty;
        public IACPropertyBase ACProperty
        {
            get
            {
                return _ACProperty;
            }
        }

        public IEnumerable<IVBChartTuple> DataSeries
        {
            get
            {
                if (ACProperty != null)
                    return ACProperty.Value as IEnumerable<IVBChartTuple>;
                else if (ContentACObject != null && !String.IsNullOrEmpty(_ACMemberPath))
                    return ContentACObject.ACUrlCommand(_ACMemberPath) as IEnumerable<IVBChartTuple>;
                return null;
            }
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the ACClassProperty which describes a bounded property by VBContent.
        /// </summary>
        public ACClassProperty VBContentPropertyInfo
        {
            get { return null; }
        }

        /// <summary>
        /// Gets or sets the right control mode.
        /// </summary>
        /// <summary xml:lang="de">
        /// Liest oder setzt den richtigen Kontrollmodus.
        /// </summary>
        public Global.ControlModes RightControlMode
        {
            get { return Global.ControlModes.Enabled; }
        }

        /// <summary>
        /// Checks and corrects the control modes.
        /// </summary>
        public void ControlModeChanged()
        {
        }

        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        public IACObject ContextACObject
        {
            get;
            set;
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


        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get;
            set;
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
            return null;
        }


        /// <summary>
        /// Determines is ACUrlCommand is enabled or disabled.
        /// </summary>
        /// <param name="acUrl">The acUrl of command.</param>
        /// <param name="acParameter">The command parameters.</param>
        ///<returns>Returns true if is ACUrlCommand is enabled, otherwise false.</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter)
        {
            return false;
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get
            {
                return null;
            }
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

        [Category("VBControl")]
        public string AxisId
        {
            get;
            set;
        }

        [Category("VBControl")]
        public string LineId
        {
            get;
            set;
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
        }
    }

}
