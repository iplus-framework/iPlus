// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-08-2012
// ***********************************************************************
// <copyright file="IACInteractiveObject.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace gip.core.datamodel
{
    /// <summary>
    ///   <para>
    /// Interface for classes that are used for the UI-Interaction. Primarily this interface implements WPF-Controls and IACComponents.
    /// </para>
    ///   <para>WPF-Controls are bound to values from IACObjects or IACComponents which can be accessed through the ContextACObject-Property (Is the DataContext-Property of WPF-Frameworkelements).
    /// To bind values use the VBContent-Property instead of using a WPF-Binding on a classic way.
    /// Pass a ACUrl to the VBContent-Property in XAML and the WPF-Control that implement this interface automatically finds the proper Source and Path for it by calling the IACObject.ACUrlBinding()-Method.
    /// IACInteractiveObject's can also be used for Drag-And-Drop-Actions and Context-Menu-Operations.
    /// Whenever such a relevant interaction-event occured this information is delegated from the "Source-IACInteractiveObject" to the "Receiver-IACInteractiveObject" by invoking the ACAction-Method.
    /// e.g. When a user interacts with a WPF-Control this Action is delegated to the bound IACComponent (ContextACObject-Property).
    /// </para>
    /// </summary>
    /// <seealso cref="gip.core.datamodel.IACObject"/>
    [ACClassInfo(Const.PackName_VarioSystem, "en{''}de{''}", Global.ACKinds.TACInterface)]
    public interface IACInteractiveObject : IACObject
    {
        /// <summary>VBContent is used by WPF-Controls. 
        /// By setting a ACUrl in XAML, the Control resolves it by calling the IACObject.ACUrlBinding()-Method. 
        /// The ACUrlBinding()-Method returns a Source and a Path which the Control use to create a WPF-Binding to bind the right value and set the WPF-DataContext.
        /// ACUrl's can be either absolute or relative to the DataContext of the parent WPFControl (or the ContextACObject of the parent IACInteractiveObject)</summary>
        /// <value>Relative or absolute ACUrl</value>
        [ACPropertyInfo(9999)]
        string VBContent { get; }

        /// <summary>
        /// ContextACObject is used by WPF-Controls and mostly it equals to the FrameworkElement.DataContext-Property.
        /// IACInteractiveObject-Childs in the logical WPF-tree resolves relative ACUrl's to this ContextACObject-Property.
        /// </summary>
        /// <value>The Data-Context as IACObject</value>
        IACObject ContextACObject { get; }

        /// <summary>
        /// ACAction is called when one IACInteractiveObject (Source) wants to inform another IACInteractiveObject (Target) about an relevant interaction-event.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        void ACAction(ACActionArgs actionArgs);

        /// <summary>
        /// It's called at the Target-IACInteractiveObject to inform the Source-IACInteractiveObject that ACAction ist allowed to be invoked.
        /// </summary>
        /// <param name="actionArgs">Information about the type of interaction and the source</param>
        /// <returns><c>true</c> if ACAction can be invoked otherwise, <c>false</c>.</returns>
        bool IsEnabledACAction(ACActionArgs actionArgs);
    }
}
