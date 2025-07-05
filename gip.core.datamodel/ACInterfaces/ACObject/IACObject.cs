// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;

namespace gip.core.datamodel
{
    /// <summary>
    /// Base interface for classes that are registered (or known) in the iPlus-Type-System. 
    /// They appear in the iPlus-Development-Environment in the Variolibrary-Project or Root-Project.
    /// Instances of IACObject have a unique address (ACUrl) in Application-Trees and can be accessed by ACUrlCommand. 
    /// Therefore every implmentation of IACObject should implement ACUrlCommand to be accessible.
    /// IACObject's also can have Designs to be able to be presented on the GUI.
    /// </summary>
    /// <seealso cref="gip.core.datamodel.IACObjectBase" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACObject'}de{'IACObject'}", Global.ACKinds.TACInterface)]
    public interface IACObject : IACObjectBase
    {
        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        [ACPropertyInfo(3, "", "en{'Parent-Object'}de{'Eltern-Objekt'}")]
        IACObject ParentACObject { get; }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        IACType ACType { get; }


        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        IEnumerable<IACObject> ACContentList { get; }


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
        object ACUrlCommand(string acUrl, params Object[] acParameter);


        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        bool IsEnabledACUrlCommand(string acUrl, params Object[] acParameter);


        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        string GetACUrl(IACObject rootACObject = null);


        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode);


        /// <summary>
        /// Resolves the type information for each segment of the passed ACUrl.
        /// </summary>
        /// <param name="acUrl"></param>
        /// <param name="acUrlTypeInfo"></param>
        /// <returns></returns>
        bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo);
    }
}
