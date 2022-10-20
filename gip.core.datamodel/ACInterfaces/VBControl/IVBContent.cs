// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-08-2012
// ***********************************************************************
// <copyright file="IVBContent.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for WPF-Controls to enable Right-Management and Data-Validation.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IVBContent'}de{'IVBContent'}", Global.ACKinds.TACInterface)]
    public interface IVBContent : IACInteractiveObject
    {
        /// <summary>
        /// Reference to ACClassProperty to obtain informations about Min/Max-Values, Length, Mask, Rights and Translation 
        /// </summary>
        /// <value>Reference to ACClassProperty</value>
        ACClassProperty VBContentPropertyInfo { get; }

        /// <summary>
        /// Mode of the WPF-Control to restrict User-Interaction.
        /// </summary>
        /// <value>Mode of the WPF-Control</value>
        Global.ControlModes RightControlMode { get; }

        /// <summary>
        /// Commaseparated options which are set in XAML of WPF-control
        /// e.g. "Disabled" => Control is Readonly
        /// If it ist dependend of ACState, then ACStates can be coded here.
        /// The corresponding ACComponent will interpret them
        /// </summary>
        string DisabledModes { get; }

        /// <summary>
        /// DeInitVBControl is used to remove all References which a WPF-Control refers to.
        /// It's needed that the Garbage-Collerctor can delete the object when it's removed from the Logical-Tree.
        /// Controls that implement this interface should bind itself to the InitState of the BSOACComponent.
        /// When the BSOACComponent stops and the property changes to Destructed- or DisposedToPool-State than this method should be called.
        /// </summary>
        /// <param name="bso">The bound BSOACComponent</param>
        void DeInitVBControl(IACComponent bso);
    }
}
