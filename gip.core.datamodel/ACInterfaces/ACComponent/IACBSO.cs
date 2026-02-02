// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACBSO.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Threading.Tasks;


namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for all Businessobjects
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACBSO'}de{'IACBSO'}", Global.ACKinds.TACInterface)]
    public interface IACBSO : IACComponent, ICloneable
    {
        /// <summary>
        /// WPF-Control that register itself and the bounded object (in most cases a ACComponentProxy-Object) to this Reference-Point
        /// </summary>
        /// <param name="hashOfDepObj">Hashcode of the calling WPF-Control</param>
        /// <param name="boundedObject">IACObject which is bound via WPF-Binding to the WPF-Control</param>
        void AddWPFRef(int hashOfDepObj, IACObject boundedObject);


        /// <summary>
        /// WPF-Control that removes itself
        /// </summary>
        /// <param name="hashOfDepObj">Hashcode of the calling WPF-Control</param>
        /// <param name="searchInChilds">searchInChilds</param>
        /// <returns>true if WPF-Control was remove from ReferencePoint</returns>
        bool RemoveWPFRef(int hashOfDepObj, bool searchInChilds=false);


        /// <summary>When the database context has changed, a dialog is opened that asks the user whether they want to save the changes. If yes then the OnSave()-Method will be invoked. If not then ACUndoChanges() will be invoked. If cancelled then nothing will happen.</summary>
        /// <returns>Fals, if user has cancelled saving or undoing.</returns>
        Task<bool> ACSaveOrUndoChanges();


        /// <summary>
        /// Its invoked from a WPF-Itemscontrol that wants to refresh its CollectionView because the user has changed the LINQ-Expressiontree in the ACQueryDefinition-Property of IAccess. 
        /// The BSO should execute the query on the database first, to get the new results for refreshing the CollectionView of the control.
        /// If the bso don't want to handle this request or manipulate the ACQueryDefinition it returns false. The WPF-control invokes then the IAccess.NavSearch()-Method itself.  
        /// </summary>
        /// <param name="acAccess">Reference to IAccess that contains the changed query (Property NavACQueryDefinition)</param>
        /// <returns>True if the bso has handled this request and queried the database context. Otherwise it returns false.</returns>
        bool ExecuteNavSearch(IAccess acAccess);
    }
}
