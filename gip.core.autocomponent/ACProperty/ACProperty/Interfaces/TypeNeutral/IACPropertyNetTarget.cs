// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public enum PropBindingMode : short
    {
        /// <summary>
        /// After binding the new target-value will be broadcated to the clients
        /// </summary>
        BindAndBroadcast = 0,

        /// <summary>
        /// After binding the new target-value will not be broadcated to the clients.
        /// Call BindPropertyToSource() again with PropBindingMode.BroadcastOnly if you wan't to delay the broadcating to the clients.
        /// </summary>
        BindOnly = 1,

        /// <summary>
        /// Don't bind to source-property. Force broadcasting the target-Value to the clients. This option should only be used if the binding was already done with the option PropBindingMode.BindOnly.
        /// </summary>
        BroadcastOnly = 2
    }

    /// <summary>
    /// Result of the call of IACPropertyNetTarget.BindPropertyToSource()
    /// </summary>
    [Flags]
    public enum PropBindingBindingResult : short
    {
        /// <summary>
        /// Binding can't be done
        /// </summary>
        NotPossible = 0x00,

        /// <summary>
        /// Type of Source-Property is different to Type of Target-Property.
        /// Call second BindPropertyToSource-Method which creates <para />
        /// wether a new Target-Property of the ACPropertyNetTargetConverter-Class (PropBindingBindingResult.TargetPropReplaced is set)<para />
        /// or repaires the Datatype of the Source-Property (PropBindingBindingResult.TypeOfSourceWasChanged is set)<para />
        /// </summary>
        NotCompatibleTypes = 0x01,

        /// <summary>
        /// Binding succeeded
        /// </summary>
        Succeeded = 0x02,

        /// <summary>
        /// Target-Property was replaced with a new instance from the ACPropertyNetTargetConverter-Class
        /// </summary>
        TargetPropReplaced = 0x04,

        /// <summary>
        /// Datatype of the Source-Property was changed/repaired in the datase. The IPlus-Service must be restarted again.
        /// </summary>
        TypeOfSourceWasChanged = 0x08,

        /// <summary>
        /// Type of Source-Property is different to Type of Target-Property and is already bound with another target.
        /// A reparation of the Source-Type is not possible.
        /// </summary>
        AlreadyBound = 0x10
    }


    /// <summary>
    /// Interface for Network-capable properties that resides on a "Real-Instance" on server-side. Network-capable properties automatically distribute their values across the network in the event of a change. Network-capable properties offer the possibility of so-called "property binding". The concept of property binding enables you to "connect" a property of component A with another property of component B in order to achieve automatic forwarding of value changes.
    /// A "Target-Property" is a network capable property, that doesn't contain or holds a original value. "Target-Properties" have bindings to a "Source-Property" where they get the original value from.
    /// </summary>
    public interface IACPropertyNetTarget : IACPropertyNetServer
    {
        /// <summary>
        /// Reference to a "Source-Property" where this "Target-Property" is bound to.
        /// </summary>
        IACPropertyNetSource Source { get; }


        /// <summary>
        /// Binds this target property to the passed source-Property.
        /// Method succeeds only if both types are equal (Source and Taregt)
        /// </summary>
        /// <param name="acPropertySource">Source-Property to bind</param>
        /// <param name="bindingMode"></param>
        /// <returns>PropBindingBindingResult</returns>
        PropBindingBindingResult BindPropertyToSource(IACPropertyNetSource acPropertySource, PropBindingMode bindingMode = PropBindingMode.BindAndBroadcast);


        /// <summary>
        /// Binds this target property to the passed source-Property.
        /// If types are not compatible it creates a new target property of the ACPropertyNetTargetConverter-Class that is replaced with this. In this case newTarget is not null.
        /// If a creation is not possible because the Datatype of the source-property must be replaced (e.g. the Type is another BitAccess-Wrapper) the IPlus-Service must me restarted afterwards.
        /// </summary>
        /// <param name="acPropertySource">Source-Property to bind</param>
        /// <param name="newTarget"></param>
        /// <param name="message"></param>
        /// <param name="bindInDBIfConverterNeeded">If true a binding in the database will be defined. At the next IPlus-Restart the Target-Property will be created as a ACPropertyNetTargetConverter automatically</param>
        /// <param name="bindingMode"></param>
        /// <returns>PropBindingBindingResult</returns>
        PropBindingBindingResult BindPropertyToSource(IACPropertyNetSource acPropertySource, 
                                                    out IACPropertyNetTarget newTarget, out string message, 
                                                    bool bindInDBIfConverterNeeded = true, PropBindingMode bindingMode = PropBindingMode.BindAndBroadcast);


        /// <summary>
        /// Unbinds the source property.
        /// </summary>
        void UnbindSourceProperty();


        /// <summary>
        /// Called when a value has changed in the "Source-Property"
        /// </summary>
        /// <param name="eventArgs">The event arguments.</param>
        void OnValueEventReceivedFromSource(IACPropertyNetValueEvent eventArgs);
    }
}
