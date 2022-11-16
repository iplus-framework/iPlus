// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IACConfig.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.ComponentModel;

namespace gip.core.datamodel
{
    /// <summary>
    ///   <para>
    /// The IACConfig interface allows you to store and read any data (even complex ones) for objects that are related to each other.
    /// In classic database programming, you would have to define another table for each M:N relationship between two tables, thus updating the EF model.
    /// This is above all a disadvantage for the individual application programming, in which it is only determined which tables would have to be linked additionally
    /// depending on the project.
    /// The relationship between one or more entities is described using ACUrl.
    /// For this purpose, the interface provides the properties KeyACUrl, PreConfigACUrl, LocalConfigACUrl.
    /// An entity framework class that is intended to serve as a configuration store must implement this interface.
    /// Do not define too many tables that implement IACConfig. </para>
    ///   <para>We recommend one table per group semantically related tables (e.g. for production orders, BOMs, purchase orders...).
    /// The Entity-Class that is the "Main Table" in this group must implement the interface IACConfigStore.
    /// Other tables inside this group can also implement IACConfigStore if they want save additional data for itself.
    /// Via IACConfigStore you can access this corresponding Config-Table (IACConfig) on an abstract way.
    /// The Value of a Configuration can be access by the Value-Property (Derived from |> ACContainerWithItems |> IACContainer.Value)
    /// </para>
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{''}de{''}", Global.ACKinds.TACInterface)]
    public interface IACConfig : IACContainerWithItems, IACObject, IACEntityProperty
    {
        /// <summary>
        /// The KeyACUrl property contains a relative database URL from another (sub)table to the "main table" in a group of semantically related tables.
        /// Relative means to the secondary keys that may already be set in the ACConfig table (e.g. MaterialID if it is a material configuration table). 
        /// If the KeyACUrl is empty, it means that it is a configuration for the main table (e.g. material).
        /// Examples: ".\MaterialCalculation(4711)" means that this configuration is for the Child-Table MaterialCalculation where MaterialCalculationNo is 4711.
        /// The KeyACUrl will automatically be set if the Configuration is added via IACConfigStore.NewACConfig(), because the IACConfigStore.ACConfigKeyACUrl returns this relative Url by itself.
        /// A KeyACUrl can also contain a "Live"-ACUrl to a instance from a application-tree. e.g. "\Mixery\WeigherA\Motor"
        /// For a relationship between two objects use the OR-Operator "||" (ACUrlHelper.Delimiter_Relationship) for separating more ACUrls.
        /// e.g. "ACClass(Mixery)\ACClass(HopperscaleTypeA1)\PAPointMatOut1||ACClass(Mixery)\ACClass(Mixer)\PAPointMatIn1"
        /// </summary>
        /// <value>ACUrl that points to another object</value>
        string KeyACUrl { get; set; }


        /// <summary>
        /// The LocalConfigACUrl points to a Property that is relative to the the object that is specified by KeyACUrl.
        /// It can also used for something else, if you program a individual logic.
        /// In most cases LocalConfigACUrl is used for addressing Properties of ACMethod's in Workflows or their configuratio properties.
        /// e.g. "MixeryDef\Mixer(0)\Dosing(0)\SMStarting\SkipComponents" or "MixeryDef\Mixer(0)\Dosing(0)\Dosing\FlowRate1"
        /// </summary>
        /// <value>ACUrl that points to a property</value>
        [ACPropertyInfo(100, Const.PN_LocalConfigACUrl, "en{'LocalConfigACUrl'}de{'LocalConfigACUrl'}")]
        string LocalConfigACUrl { get; set; }


        /// <summary>
        /// Calling subworkflows is similar to calling a subprogram. 
        /// When calling subprograms, parameters must be passed for which only one call is valid for this one. 
        /// If the same subprogram is called from another program, other parameters must be passed. 
        /// This is also the case with workflows. 
        /// If you want to define other parameters for the subworkflow, you need to know where the subworkflow was called. 
        /// This is stored in this property.
        /// </summary>
        /// <value>ACUrl of the parent Workflow, that invoked this Workflow</value>
        [ACPropertyInfo(99, Const.PN_PreConfigACUrl, "en{'PreConfigACUrl'}de{'PreConfigACUrl'}")]
        string PreConfigACUrl { get; set; }


        /// <summary>
        /// Complete ACUrl: Composition of PreConfigACUrl and LocalConfigACUrl.
        /// ConfigACUrl = PreConfigACUrl + LocalConfigACUrl
        /// </summary>
        /// <value>Complete ACUrl</value>
        [ACPropertyInfo(98, "ConfigACUrl", "en{'ConfigACUrl'}de{'ConfigACUrl'}")]
        string ConfigACUrl { get; }

        /// <summary>
        /// String for individual usage.
        /// </summary>
        /// <value>The expression.</value>
        [ACPropertyInfo(102, "Expression", "en{'Expression'}de{'Phrase'}")]
        string Expression { get; set; }

        /// <summary>
        /// Comment
        /// </summary>
        /// <value>Comment</value>
        [ACPropertyInfo(103, "Comment", "en{'Comment'}de{'Kommentar'}")]
        string Comment { get; set; }


        /// <summary>
        /// Reference to the "main table" of the group of semantically related tables. e.g. Material, BOM, ProductionOrder....
        /// </summary>
        /// <value>Reference to a entity object which is the "main table"</value>
        [ACPropertyInfo(104)]
        IACConfigStore ConfigStore { get; }


        /// <summary>If the configuration is for a concrete instance from the application tree, than this ID is set.</summary>
        /// <value>Guid of Table ACClass</value>
        Guid? VBiACClassID { get; set; }


        /// <summary>Reference to a ACClass-entity. See VBiACClassID</summary>
        /// <value>Reference to a ACClass-entity</value>
        [ACPropertyInfo(105)]
        ACClass VBACClass { get; }


        /// <summary>If the Configuration is for a Workflow-Node, then this is a additional key for faster access.</summary>
        /// <value>Guid of Table ACClassWF</value>
        Guid? ACClassWFID { get;}


        /// <summary>Sets the Metadata (iPlus-Type) of the Value-Property.</summary>
        /// <param name="typeOfValue">Metadata (iPlus-Type) of the Value-Property.</param>
        void SetValueTypeACClass(ACClass typeOfValue);
    }

    public interface IACConfigT<T> : IACConfig, IACContainerWithItemsT<T, object> where T : IACContainerWithItems
    {
    }
}
