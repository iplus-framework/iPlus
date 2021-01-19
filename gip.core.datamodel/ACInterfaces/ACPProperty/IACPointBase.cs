// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 11-08-2012
// ***********************************************************************
// <copyright file="IACPointBase.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************


namespace gip.core.datamodel
{
    /// <summary>
    /// An IACPointBase is used to abstractly describe the relationships between any object. It is called a Connectonpoint because in graph theory objects are represented as points and relationships as lines. Using this abstract interface, IPlus is able to graphically represent any relationships between objects (e.g. workflows, visualizations, routes...). The relationships are stored in the IEnumerable&amp;lt;T&amp;gt; ConnectionList property.
    /// </summary>
    /// <seealso cref="gip.core.datamodel.IACMember" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Abstraction of a point'}de{'Abstraktion eines Punkts'}", Global.ACKinds.TACInterface)]
    public interface IACPointBase : IACMember
    {
        /// <summary>  Is called when the parent ACComponent is stopping/unloading.</summary>
        /// <param name="deleteACClassTask">if set to <c>true if the parent ACComponent should be removed from the persistable Application-Tree.</c></param>
        void ACDeInit(bool deleteACClassTask = false);


        /// <summary> The ConnectionList as serialized string (XML).</summary>
        /// <param name="xmlIndented">if set to <c>true</c> the XML is indented.</param>
        /// <returns>XML</returns>
        string ValueSerialized(bool xmlIndented = false);


        /// <summary>  Maximum capacity of the point (of the ConnectionList). 0 = Unlimited</summary>
        /// <value>The maximum capacity.</value>
        uint MaxCapacity { get; }


        /// <summary>iPlus-Type (Metadata) of this point.</summary>
        /// <value>ACClassProperty</value>
        ACClassProperty PropertyInfo { get; }
    }
}
