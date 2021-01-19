// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACRelationInfo.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class ACRelationInfo
    /// </summary>
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Property, AllowMultiple = true)]
    public class ACRelationInfo : Attribute
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="ACRelationInfo"/> class.
        /// </summary>
        /// <param name="acUrlRelation">The ac URL relation.</param>
        public ACRelationInfo(string acUrlRelation)
        {
            ACUrlRelation = acUrlRelation;
        }

        /// <summary>
        /// Gets or sets the AC URL relation.
        /// </summary>
        /// <value>The AC URL relation.</value>
        public string ACUrlRelation { get; set; }

    }

    /// <summary>
    /// Class ACPointStateInfo
    /// </summary>
    public class ACPointStateInfo : ACRelationInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACPointStateInfo"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="valueWhenActive">The value when active.</param>
        /// <param name="logicalOperation">The logical operation.</param>
        public ACPointStateInfo(string propertyName, object valueWhenActive, Global.Operators logicalOperation = Global.Operators.and, short displayGroup = 0)
            : base(propertyName)
        {
            ValueWhenActive = valueWhenActive;
            LogicalOperation = logicalOperation;
            DisplayGroup = displayGroup;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACPointStateInfo"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="valueWhenActive">The value when active.</param>
        /// <param name="logicalOperation">The logical operation.</param>
        public ACPointStateInfo(string propertyName, object valueWhenActive, string groupName, string stateName, Global.Operators logicalOperation = Global.Operators.and)
            : base(propertyName)
        {
            ValueWhenActive = valueWhenActive;
            LogicalOperation = logicalOperation;
            GroupName = groupName;
            StateName = stateName;
            LogicalOperation = logicalOperation;
        }

        /// <summary>
        /// Gets or sets the logical operation.
        /// </summary>
        /// <value>The logical operation.</value>
        public Global.Operators LogicalOperation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value when active.
        /// </summary>
        /// <value>The value when active.</value>
        public object ValueWhenActive
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the value string when active.
        /// </summary>
        /// <value>The value string when active.</value>
        public string ValueStringWhenActive
        {
            get
            {
                return ValueWhenActive.ToString();
            }
        }

        /// <summary>
        /// Gets or sets the display group for analysis.
        /// </summary>
        public short DisplayGroup
        {
            get;
            set;
        }

        public string GroupName
        {
            get;
            set;
        }

        public string StateName
        {
            get;
            set;
        }

        /// <summary>
        /// Inserts the or update.
        /// </summary>
        /// <param name="database">The database.</param>
        /// <param name="acClassOfPoint">The ac class of point.</param>
        /// <param name="acClassPropertyOfPoint">The ac class property of point.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public bool InsertOrUpdate(Database database, ACClass acClassOfPoint, ACClassProperty acClassPropertyOfPoint)
        {
            if (String.IsNullOrEmpty(ACUrlRelation))
                return false;
            ACClass acClassTarget = null;
            ACClassProperty acClassTargetProperty = null;
            var queryTargetProp = acClassOfPoint.Properties.Where(c => c.ACIdentifier == ACUrlRelation);
            if (queryTargetProp.Any())
            {
                acClassTargetProperty = queryTargetProp.First();
                acClassTarget = acClassOfPoint;
            }
            else
            {
                if (!FindChildProperty(ACUrlRelation, acClassOfPoint, out acClassTarget, out acClassTargetProperty))
                    return false;
            }

            if (acClassTargetProperty == null)
                return false;

            ACClassPropertyRelation relation;
            var queryRelation = acClassPropertyOfPoint.TopBaseACClassProperty.ACClassPropertyRelation_SourceACClassProperty.Where(c => c.TargetACClassPropertyID == acClassTargetProperty.TopBaseACClassProperty.ACClassPropertyID
                && c.TargetACClassID == acClassTarget.ACClassID
                && c.SourceACClassID == acClassOfPoint.ACClassID);
            if (!queryRelation.Any())
            {
                relation = ACClassPropertyRelation.NewACClassPropertyRelation(database, acClassOfPoint, acClassPropertyOfPoint.TopBaseACClassProperty, acClassTarget, acClassTargetProperty.TopBaseACClassProperty);
                acClassPropertyOfPoint.TopBaseACClassProperty.ACClassPropertyRelation_SourceACClassProperty.Add(relation);
            }
            else
            {
                relation = queryRelation.First();
            }
            relation.ConnectionType = Global.ConnectionTypes.PointState;
            relation.LogicalOperation = LogicalOperation;
            relation.Value = ValueStringWhenActive;
            if (DisplayGroup > 0)
                relation.DisplayGroup = DisplayGroup;

            relation.GroupName = GroupName;
            relation.StateName = StateName;

            return true;
        }

        /// <summary>
        /// Finds the child property.
        /// </summary>
        /// <param name="acUrlComponent">The ac URL component.</param>
        /// <param name="searchAt">The search at.</param>
        /// <param name="foundAt">The found at.</param>
        /// <param name="foundProperty">The found property.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        private bool FindChildProperty(string acUrlComponent, ACClass searchAt, out ACClass foundAt, out ACClassProperty foundProperty)
        {
            if (string.IsNullOrEmpty(acUrlComponent))
            {
                foundAt = null;
                foundProperty = null;
                return false;
            }

            ACUrlHelper acUrlHelper = new ACUrlHelper(acUrlComponent);
            if (acUrlHelper.UrlKey != ACUrlHelper.UrlKeys.Child)
            {
                foundAt = null;
                foundProperty = null;
                return false;
            }

            var query = searchAt.ACClass_ParentACClass.Where(c => c.ACIdentifier == acUrlHelper.ACUrlPart);
            if (!query.Any())
            {
                var queryProp = searchAt.Properties.Where(c => c.ACIdentifier == acUrlHelper.ACUrlPart);
                if (queryProp.Any())
                {
                    foundAt = searchAt;
                    foundProperty = queryProp.First();
                    return true;
                }
                foundAt = null;
                foundProperty = null;
                return false;
            }
            foundAt = query.First();
            if (string.IsNullOrEmpty(acUrlHelper.NextACUrl))
            {
                foundProperty = null;
                return false;
            }
            return FindChildProperty(acUrlHelper.NextACUrl, foundAt, out foundAt, out foundProperty);
        }

    }
}
