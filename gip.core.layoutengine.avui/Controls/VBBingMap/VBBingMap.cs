// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Windows.Data;
//using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Input;
//using gip.core.datamodel;
//using Microsoft.Maps.MapControl.WPF;

//namespace gip.core.layoutengine.avui
//{
//    /// <summary>
//    /// Control for maps
//    /// </summary>
//    /// <summary xml:lang="de">
//    /// Steuerelement für Landkarten
//    /// </summary>
//    [ACClassInfo(Const.PackName_VarioSystem, "en{'VBBingMap'}de{'VBBingMap'}", Global.ACKinds.TACVBControl, Global.ACStorableTypes.NotStorable, true, false)]
//    public class VBBingMap : Map, IACInteractiveObject
//    {
//        /// <summary>By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
//        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
//        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
//        /// <value>Relative or absolute ACUrl</value>
//        public string VBContent
//        {
//            get;set;
//        }

//        /// <summary>
//        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
//        /// </summary>
//        /// <param name="actionArgs">Information about the type of interaction and the source</param>
//        public void ACAction(ACActionArgs actionArgs)
//        {
//            throw new NotImplementedException();
//        }

//        /// <summary>
//        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
//        /// </summary>
//        /// <param name="actionArgs">Information about the type of interaction and the source</param>
//        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
//        public bool IsEnabledACAction(ACActionArgs actionArgs)
//        {
//            throw new NotImplementedException();
//        }

//        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
//        /// <value>  Translated description</value>
//        public string ACCaption
//        {
//            get;
//        }

//        /// <summary>
//        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
//        /// </summary>
//        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
//        public IACType ACType
//        {
//            get { return null; }
//        }

//        /// <summary>
//        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
//        /// </summary>
//        /// <value> A nullable list ob IACObjects.</value>
//        public IEnumerable<IACObject> ACContentList
//        {
//            get { throw new NotImplementedException(); }
//        }

//        /// <summary>
//        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
//        /// 1. get references to components,
//        /// 2. query property values,
//        /// 3. execute method calls,
//        /// 4. start and stop Components,
//        /// 5. and send messages to other components.
//        /// </summary>
//        /// <param name="acUrl">String that adresses a command</param>
//        /// <param name="acParameter">Parameters if a method should be invoked</param>
//        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
//        public object ACUrlCommand(string acUrl, params object[] acParameter)
//        {
//            throw new NotImplementedException();
//        }

//        /// <summary>
//        /// Returns the parent object
//        /// </summary>
//        /// <value>Reference to the parent object</value>
//        public IACObject ParentACObject
//        {
//            get
//            {
//                return Parent as IACObject;
//            }
//        }

//        /// <summary>
//        /// Returns a ACUrl relatively to the passed object.
//        /// If the passed object is null then the absolute path is returned
//        /// </summary>
//        /// <param name="rootACObject">Object for creating a realtive path to it</param>
//        /// <returns>ACUrl as string</returns>
//        public string GetACUrl(IACObject rootACObject = null)
//        {
//            return ACIdentifier;
//        }


//        /// <summary>
//        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
//        /// </summary>
//        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
//        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
//        /// <param name="source">The Source for WPF-Databinding</param>
//        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
//        /// <param name="rightControlMode">Information about access rights for the requested object</param>
//        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
//        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
//        {
//            return false;
//        }

        //public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        //{
        //    return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        //}

        ///// <summary>
        ///// Represents the dependency property for BSOACComponent.
        ///// </summary>
        //public static readonly DependencyProperty BSOACComponentProperty = ContentPropertyHandler.BSOACComponentProperty.AddOwner(typeof(VBBingMap), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.Inherits));
        ///// <summary>
        ///// Gets or sets the BSOACComponent.
        ///// </summary>
        //public IACBSO BSOACComponent
        //{
        //    get { return (IACBSO)GetValue(BSOACComponentProperty); }
        //    set { SetValue(BSOACComponentProperty, value); }
        //}



//        /// <summary>
//        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
//        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
//        /// </summary>
//        /// <value>The Data-Context as IACObject</value>
//        public IACObject ContextACObject
//        {
//            get 
//            {
//                return DataContext as IACComponent;
//            }
//        }

//        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
//        /// <value>The Unique Identifier as string</value>
//        public string ACIdentifier
//        {
//            get { return this.Name; }
//        }

//        /// <summary>
//        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
//        /// </summary>
//        /// <param name="acUrl">String that adresses a command</param>
//        /// <param name="acParameter">Parameters if a method should be invoked</param>
//        /// <returns>true if ACUrlCommand can be invoked</returns>
//        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
//        {
//            return false;
//        }
//    }

//    public class VBMapLayer : MapLayer
//    {
//        public static readonly DependencyProperty ContentPopupPositionProperty
//            = DependencyProperty.Register("ContentPopupPosition", typeof(Location), typeof(VBMapLayer), new PropertyMetadata(new PropertyChangedCallback(ContentPopupPosition_Changed)));

//        public Location ContentPopupPosition
//        {
//            get { return (Location)GetValue(ContentPopupPositionProperty); }
//            set { SetValue(ContentPopupPositionProperty, value); }
//        }

//        private static void ContentPopupPosition_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
//        {
//            if (d is VBMapLayer)
//            {
//                VBMapLayer mapLayer = d as VBMapLayer;
//                //MapItemsControl
//                MapLayer.SetPosition(mapLayer, mapLayer.ContentPopupPosition);
//                MapLayer.SetPositionOffset(mapLayer, new System.Windows.Point(20, -15));
//            }
//        }

//        protected override Size ArrangeOverride(Size finalSize)
//        {
//            return base.ArrangeOverride(finalSize);
//        }

//        protected override Size MeasureOverride(Size availableSize)
//        {
//            return base.MeasureOverride(availableSize);
//        }

//    }
//}
