// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IAccess.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Linq;
using System.Data.Objects;
using System.Collections;
using System.ComponentModel;

namespace gip.core.datamodel
{

    /// <summary>
    /// Interface that encapsulates a ACQueryDefinition, that stores a user defined query.
    /// This user defined query is passed to ACQuery.ACSelect()-Method when the NavSearch()-Method is invoked.
    /// The ACSelect()-Method builds a dynamic LINQ-expression tree and returns a IQueryable{T}.
    /// The result can be read in the NavObjectList-Property.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IAccess'}de{'IAccess'}", Global.ACKinds.TACInterface)]
    public interface IAccess : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the query definition
        /// </summary>
        /// <value>ACQueryDefinition</value>
        [ACPropertyInfo(9999)]
        ACQueryDefinition NavACQueryDefinition { get; set; }


        /// <summary>
        /// The Result of the NavSearch() -Method
        /// </summary>
        /// <value>A list of EF-Objects</value>
        [ACPropertyInfo(9999)]
        IEnumerable NavObjectList { get; }


        /// <summary>Executes a Query according to the filter and sort entries in ACQueryDefinition the result is copied to the NavObjectList.</summary>
        /// <param name="parentACObject">Reference to a database context</param>
        /// <param name="mergeOption">The merge option.</param>
        /// <returns>True, if query was successful</returns>
        bool NavSearch(IACObject parentACObject, MergeOption mergeOption = MergeOption.AppendOnly);


        /// <summary>
        /// Executes a Query according to the filter and sort entries in ACQueryDefinition. The result is copied to the NavObjectList. The Query-Context is automatically Determined by the local ParentACObject-Member. If you want to specify another Query-Context, then use Method NavSearch(IACObject parentACObject, MergeOption mergeOption = MergeOption.AppendOnly)<br /></summary>
        /// <param name="mergeOption">The merge option.</param>
        /// <returns>True, if query was successful</returns>
        bool NavSearch(MergeOption mergeOption = MergeOption.AppendOnly);


        /// <summary>
        /// Executes a Query to the NavObjectList
        /// </summary>
        bool NavSearchInObjectList();


        /// <summary>Executes a Query according to the filter and sort entries in ACQueryDefinition without changing the NavObjectList. The result is returned directly.</summary>
        /// <param name="searchWord">The search word.</param>
        /// <param name="mergeOption">The merge option.</param>
        /// <returns>A IQueryable</returns>
        IQueryable OneTimeSearch(string searchWord, MergeOption mergeOption = MergeOption.AppendOnly);


        /// <summary>Invokes the OneTimeSearch()-Method an returns the first element in the result</summary>
        /// <param name="searchWord">The search word.</param>
        /// <param name="mergeOption">The merge option.</param>
        /// <returns>The first element in the result</returns>
        object OneTimeSearchFirstOrDefault(string searchWord, MergeOption mergeOption = MergeOption.AppendOnly);


        /// <summary>
        /// Returns the count of objects in NavObjectList.
        /// </summary>
        /// <value>Count of objects in NavObjectList.</value>
        [ACPropertyInfo(9999)]
        int NavRowCount { get; }


        /// <summary>Opens the a dialog (VBBSOQueryDialog) on the gui for the manipulation of the ACQueryDefinition.</summary>
        /// <returns>True, if OK-Button was clicked</returns>
        bool ShowACQueryDialog();


        /// <summary>Opens the a dialog (VBBSOQueryDialog) on the gui for chaging the filter values in the ACQueryDefinition.</summary>
        /// <returns>True, if OK-Button was clicked</returns>
        bool ShowChangeColumnValuesDialog(ACColumnItem column);

    }
}
