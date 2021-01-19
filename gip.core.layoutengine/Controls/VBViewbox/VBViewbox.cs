using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using gip.core.datamodel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows.Input;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Control element for automatic size adjustment.
    /// </summary>
    /// <summary xml:lang="de">
    /// Steuerelement zur automatischen Größenanpassung.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBViewbox'}de{'VBViewbox'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.Required, true, false)]
    public class VBViewbox : Viewbox, IVBContent
    {
        /// <summary>
        /// Creates a new instance of the VBViewBox.
        /// </summary>
        public VBViewbox() : base()
        {
            _ZoomValue = 100;
            _newZoomValue = 100;
        }

        private double _InitialHeight;
        private double _InitialWidth;
        private double _ZoomValue;
        private double _newZoomValue;

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (IsEnabledZoom)
            {
                FrameworkElement fe = this.Parent as FrameworkElement;
                if (fe != null)
                    fe.SizeChanged += Fe_SizeChanged;
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
        }

        protected override void OnChildDesiredSizeChanged(UIElement child)
        {
            base.OnChildDesiredSizeChanged(child);
            FrameworkElement fe = this.Parent as FrameworkElement;
            if (fe != null)
            {
                ChangeSize(new Size(fe.ActualWidth, fe.ActualHeight));
            }
        }

        private void Fe_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ChangeSize(e.NewSize);
        }

        private void ChangeSize(Size newSize)
        {
            double newHeight = newSize.Height - 10;
            if (newHeight <= 0)
                newHeight = 0;

            double newWidth = newSize.Width - 10;
            if (newWidth <= 0)
                newWidth = 0;

            this.Height = newHeight;
            this.Width = newWidth;

            this._InitialHeight = newHeight;
            this._InitialWidth = newWidth;
            _newZoomValue = 100;
        }

        [Category("VBControl")]
        public bool IsEnabledZoom
        {
            get;
            set;
        }

        private double _MinZoomValue = 40;
        [Category("VBControl")]
        public double MinZoomValue
        {
            get
            {
                return _MinZoomValue;
            }
            set
            {
                if (value < 40)
                    _MinZoomValue = 40;
                else if (value > 100)
                    _MinZoomValue = 100;
                else
                    _MinZoomValue = value;
            }
        }

        private double _MaxZoomValue = 200;
        [Category("VBControl")]
        public double MaxZoomValue
        {
            get
            {
                return _MaxZoomValue;
            }
            set
            {
                if (value > 300)
                    _MaxZoomValue = 300;
                else if (value < 100)
                    _MaxZoomValue = 100;
                else
                    _MaxZoomValue = value;
            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (IsEnabledZoom && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (_InitialHeight == 0)
                {
                    _InitialHeight = this.ActualHeight;
                    _InitialWidth = this.ActualWidth;
                }

                double zoomPercent = 0;

                if (e.Delta > 0)
                    _newZoomValue += 10;
                else
                    _newZoomValue -= 10;

                if (_newZoomValue > MaxZoomValue)
                    _newZoomValue = MaxZoomValue;
                else if (_newZoomValue < MinZoomValue)
                    _newZoomValue = MinZoomValue;

                double calculatedZoomValue = _newZoomValue - _ZoomValue;

                if (calculatedZoomValue > 0)
                {
                    zoomPercent = calculatedZoomValue / _ZoomValue;
                    Height = _InitialHeight + (zoomPercent * _InitialHeight);
                    Width = _InitialWidth + (zoomPercent * _InitialWidth);
                }
                else
                {
                    zoomPercent = Math.Abs(calculatedZoomValue) / _ZoomValue;
                    Height = _InitialHeight - (zoomPercent * _InitialHeight);
                    Width = _InitialWidth - (zoomPercent * _InitialWidth);
                }
            }
            base.OnPreviewMouseWheel(e);
        }

        #region IVBContent members

        public ACClassProperty VBContentPropertyInfo
        {
            get;
            set;
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

        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [Category("VBControl")]
        public string VBContent
        {
            get;
            set;
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

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get;
            set;
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public string ACCaption
        {
            get;
            set;
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get;
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get { return null; }
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
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        public void DeInitVBControl(IACComponent bso)
        {
            FrameworkElement fe = this.Parent as FrameworkElement;
            if(fe != null)
                fe.SizeChanged -= Fe_SizeChanged;
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
        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return false;
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
    }
}
