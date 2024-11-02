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
// <copyright file="ACQueryInfo.cs" company="gip mbh, Oftersheim, Germany">
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
    /// Class ACQueryInfo
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true)]
    public class ACQueryInfo : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACQueryInfo"/> class.
        /// </summary>
        /// <param name="acPackageName">Name of the ac package.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        /// <param name="acCaptionTranslation">The ac caption translation.</param>
        /// <param name="queryType">Type of the query.</param>
        /// <param name="childACUrl">The child AC URL.</param>
        /// <param name="acFilterColumns">The ac filter columns.</param>
        /// <param name="acSortColumns">The ac sort columns.</param>
        public ACQueryInfo(string acPackageName, string acIdentifier, string acCaptionTranslation, Type queryType, string childACUrl, string acFilterColumns, string acSortColumns)
        {
            ACPackageName = acPackageName;
            ACIdentifier = acIdentifier;
            ACCaptionTranslation = acCaptionTranslation;
            QueryType = queryType;
            ACFilterColumns = acFilterColumns;
            ACSortColumns = acSortColumns;

            ChildACUrl = childACUrl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACQueryInfo"/> class.
        /// </summary>
        /// <param name="acPackageName">Name of the ac package.</param>
        /// <param name="acIdentifier">The ac identifier.</param>
        /// <param name="acCaptionTranslation">The ac caption translation.</param>
        /// <param name="queryType">Type of the query.</param>
        /// <param name="childACUrl">The child AC URL.</param>
        /// <param name="acFilterColumns">The ac filter columns.</param>
        /// <param name="acSortColumns">The ac sort columns.</param>
        /// <param name="acQueryChilds">The ac query childs.</param>
        /// <exception cref="System.Exception"></exception>
        public ACQueryInfo(string acPackageName, string acIdentifier, string acCaptionTranslation, Type queryType, string childACUrl, string acFilterColumns, string acSortColumns, object[] acQueryChilds)
        {
            ACPackageName = acPackageName;
            ACIdentifier = acIdentifier;
            ACCaptionTranslation = acCaptionTranslation;
            QueryType = queryType;
            ACFilterColumns = acFilterColumns;
            ACSortColumns = acSortColumns;
            ChildACUrl = childACUrl;
            
            if (acQueryChilds != null)
            {
                foreach (var child in acQueryChilds)
                {
                    object[] ochild = child as object[];
                    switch (ochild.Count())
                    {
                        case 6:
                            ACQueryInfo acQueryChild = new ACQueryInfo(acPackageName, ochild[0] as string, ochild[1] as string, ochild[2] as Type, ochild[3] as string, ochild[4] as string, ochild[5] as string);
                            if (ACQueryChilds == null)
                                ACQueryChilds = new List<ACQueryInfo>();
                            ACQueryChilds.Add(acQueryChild);
                            break;
                        case 7:
                            ACQueryInfo acQueryChild1 = new ACQueryInfo(acPackageName, ochild[0] as string, ochild[1] as string, ochild[2] as Type, ochild[3] as string, ochild[4] as string, ochild[5] as string, ochild[6] as object[]);
                            if (ACQueryChilds == null)
                                ACQueryChilds = new List<ACQueryInfo>();
                            ACQueryChilds.Add(acQueryChild1);
                            break;
                        default:
                            throw new Exception();
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the AC package.
        /// </summary>
        /// <value>The name of the AC package.</value>
        public string ACPackageName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the AC identifier.
        /// </summary>
        /// <value>The AC identifier.</value>
        public string ACIdentifier
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the AC caption translation.
        /// </summary>
        /// <value>The AC caption translation.</value>
        public string ACCaptionTranslation 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// Gets or sets the type of the query.
        /// </summary>
        /// <value>The type of the query.</value>
        public Type QueryType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the child AC URL.
        /// </summary>
        /// <value>The child AC URL.</value>
        public string ChildACUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the AC filter columns.
        /// </summary>
        /// <value>The AC filter columns.</value>
        public String ACFilterColumns
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the AC sort columns.
        /// </summary>
        /// <value>The AC sort columns.</value>
        public string ACSortColumns
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the AC query childs.
        /// </summary>
        /// <value>The AC query childs.</value>
        public List<ACQueryInfo> ACQueryChilds
        {
            get;
            set;
        }
    }
    /// <summary>
    /// Class ACQueryInfoPrimary
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class, AllowMultiple = true)]
    public class ACQueryInfoPrimary : ACQueryInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACQueryInfoPrimary"/> class.
        /// </summary>
        /// <param name="acPackageName">Name of the ac package.</param>
        /// <param name="acName">Name of the ac.</param>
        /// <param name="acCaptionTranslation">The ac caption translation.</param>
        /// <param name="queryType">Type of the query.</param>
        /// <param name="childACUrl">The child AC URL.</param>
        /// <param name="vbFilter">The vb filter.</param>
        /// <param name="vbSort">The vb sort.</param>
        public ACQueryInfoPrimary(string acPackageName, string acName, string acCaptionTranslation, Type queryType, string childACUrl, string vbFilter, string vbSort)
            : base(acPackageName, acName, acCaptionTranslation, queryType, childACUrl, vbFilter, vbSort)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ACQueryInfoPrimary"/> class.
        /// </summary>
        /// <param name="acPackageName">Name of the ac package.</param>
        /// <param name="acName">Name of the ac.</param>
        /// <param name="acCaptionTranslation">The ac caption translation.</param>
        /// <param name="queryType">Type of the query.</param>
        /// <param name="childACUrl">The child AC URL.</param>
        /// <param name="vbFilter">The vb filter.</param>
        /// <param name="vbSort">The vb sort.</param>
        /// <param name="acQueryChilds">The ac query childs.</param>
        public ACQueryInfoPrimary(string acPackageName, string acName, string acCaptionTranslation, Type queryType, string childACUrl, string vbFilter, string vbSort, object[] acQueryChilds)
            : base(acPackageName, acName, acCaptionTranslation, queryType, childACUrl, vbFilter, vbSort, acQueryChilds)
        {
        }
    }
}
