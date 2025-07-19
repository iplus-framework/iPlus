// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACObject.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for implementing simple objects with the life-cycle phases of iPlus 
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACObjectWithInit'}de{'IACObjectWithInit'}", Global.ACKinds.TACInterface)]
    public interface IACObjectWithInit : IACObject, IACMember
    {
        /// <summary>
        /// The ACInit method is called directly after construction. 
        /// You can also overwrite the ACInit method. 
        /// When booting or dynamically reloading ACComponent trees, such as loading a workflow, the trees pass through the "Depth-First" + "Pre-Order" algorithm. 
        /// This means that the generation of child ACComponents is always carried out in depth first and then the next ACComponent at the same recursion depth.<para />
        /// The algorithm is executed in the ACInit method of the ACComponent class. 
        /// Therefore, you must always make the base call. 
        /// This means that as soon as you execute your initialization logic after the basic call, you know that the child components were created and that they are in initialization state.
        /// </summary>
        /// <param name="startChildMode">The persisted start mode from database</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise</returns>
        bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic);

        /// <summary>
        /// After all new instances have been initialized by the pre-initialization phase, the post-initialization phase starts.
        /// After passing the same Depth-First + Post-Order algorithm, the ACPostInit method is called.
        /// The parent element is passed if there are no more child elements at the same recursion depth, starting with the lowest recursion depth.<para />
        /// You can also overwrite the ACPostInit method to program the remaining initialization logic. 
        /// If these are dependent on all instances having to exist. 
        /// In the Base-ACPostInit method of the ACComponent, the remaining property bindings are performed so that the target properties then have the value of the bound source property. 
        /// Therefore, you must always execute the base call when overwriting this method.
        /// </summary>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise</returns>
        bool ACPostInit();

        /// <summary>
        /// Callbackmethod if a Exeption occured during ACInit- and ACPostInit-Phase
        /// </summary>
        /// <param name="reason">Exception</param>
        void OnInitFailed(Exception reason);

        /// <summary>
        /// The termination of an ACComponent instance is initiated by the StopComponent() method.
        /// The ACPreDeInit method informs the affected child instances in advance that deinitialization is taking place.
        /// The ACPreDeInit is called after the "Depth-First" + "Pre-Order" algorithm.        
        /// </summary>
        /// <param name="deleteACClassTask">Should instance be removed from persistable application tree.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise</returns>
        bool ACPreDeInit(bool deleteACClassTask = false);

        /// <summary>
        /// After the notification of all affected instances by calling the ACPreDeInit method, the actual deinitialization starts. 
        /// This runs through the instances according to the "Depth-First" + "Post-Order" algorithm and calls the ACDeInit method. 
        /// You should overwrite this so that you can release references or unsubscribe from events, among other things.<para />
        /// Don't forget to make the base call! The basic ACDeInit method of theCComponent executes some functions to clear up the garbage collector. Before calling this function, first:
        /// the state is set to Destructing.
        /// If the instance is "poolable" (IsDisposable == true), it is given the status DisposingToPool instead.
        /// At the end of the deinitialization process:
        /// the state is set to Destructed,
        /// or for a "poolable" instance on DisposedToPool.In this case, the instance is not cleaned up by the.NET garbage collector, but waits in a pool to be reactivated.
        /// </summary>
        /// <param name="deleteACClassTask">Should instance be removed from persistable application tree.</param>
        /// <returns><c>true</c> if succeeded, <c>false</c> otherwise</returns>
        bool ACDeInit(bool deleteACClassTask = false);

        /// <summary>
        /// Adds a child to this instance
        /// </summary>
        /// <param name="acObject">The child</param>
        void AddChild(IACObject acObject);

        IEnumerable<IACObject> ACObjectChilds { get; }
    }
}
