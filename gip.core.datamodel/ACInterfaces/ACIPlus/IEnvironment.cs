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
// <copyright file="IEnvironment.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using gip.core.datamodel.Licensing;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface IEnvironment
    /// </summary>
    public interface IEnvironment : IACComponent
    {
        #region Icon/Bitmap
        /// <summary>
        /// Gets the icon.
        /// </summary>
        /// <param name="acNameIdentifier">The ac name identifier.</param>
        /// <returns>ACClassDesign.</returns>
        ACClassDesign GetIcon(string acNameIdentifier);

        /// <summary>
        /// Gets the bitmap.
        /// </summary>
        /// <param name="acNameIdentifier">The ac name identifier.</param>
        /// <returns>ACClassDesign.</returns>
        ACClassDesign GetBitmap(string acNameIdentifier);
        #endregion


        #region Translation
        /// <summary>
        /// Translates the text.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        /// <param name="acName">Name of the ac.</param>
        /// <returns>System.String.</returns>
        string TranslateText(IACObject acObject, string acName);

        string TranslateText(IACObject acObject, string acName, params object[] parameter);

        /// <summary>
        /// Translates the text LC.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        /// <param name="acName">Name of the ac.</param>
        /// <param name="VBLanguageCode">The md language code.</param>
        /// <returns>System.String.</returns>
        string TranslateTextLC(IACObject acObject, string acName, string VBLanguageCode);

        string TranslateTextACClass(ACClass acClass, string acName, string VBLanguageCode);

        /// <summary>
        /// Translates the message.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        /// <param name="acName">Name of the ac.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>System.String.</returns>
        string TranslateMessage(IACObject acObject, string acName, params object[] parameter);

        /// <summary>
        /// Translates the message LC.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        /// <param name="acName">Name of the ac.</param>
        /// <param name="VBLanguageCode">The md language code.</param>
        /// <param name="parameter">The parameter.</param>
        /// <returns>System.String.</returns>
        string TranslateMessageLC(IACObject acObject, string acName, string VBLanguageCode, params object[] parameter);
        #endregion


        #region Environment

        /// <summary>
        /// Gets the licence.
        /// </summary>
        /// <value>The licence.</value>
        License License { get; }

        /// <summary>
        /// Location of the current assembly (AppDomain.CurrentDomain.BaseDirectory)
        /// </summary>
        /// <value>The rootpath.</value>
        string Rootpath { get; }

        /// <summary>
        /// Defaultpath for exporting/importing of configuration data
        /// </summary>
        /// <value>The datapath.</value>
        string Datapath { get; }

        /// <summary>
        /// Gets the name of the computer.
        /// </summary>
        /// <value>The name of the computer.</value>
        string ComputerName { get; }

        /// <summary>
        /// Windows Session-ID
        /// </summary>
        int? SessionID { get; }
        #endregion


        #region User Login/Logout and Rights
        
        /// <summary>
        /// Current logged in user
        /// </summary>
        VBUser User { get; }


        /// <summary>
        /// Current language I18N-Code
        /// </summary>
        /// <value>Current language I18N-Code</value>
        string VBLanguageCode { get; }

        /// <summary>
        /// Current logged in user
        /// </summary>
        /// <value>The user instance.</value>
        VBUserInstance UserInstance { get; }


        /// <summary>
        /// Gets the manager for Rights for the current user
        /// </summary>
        /// <param name="acClass">The ac class.</param>
        /// <returns>ClassRightManager.</returns>
        ClassRightManager GetClassRightManager(ACClass acClass);


        /// <summary>
        /// Overrides temporary the Default-Language of the User. If param is null, the default language will be set.
        /// </summary>
        /// <param name="code"></param>
        void SetLanguageCode(string code);

        #endregion


        #region Database

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        /// <value>The name of the database.</value>
        string DatabaseName { get; }


        /// <summary>
        /// Gets the name of the data source (SQL-Server-Name)
        /// </summary>
        /// <value>The name of the data source.</value>
        string DataSourceName { get; }


        /// <summary>
        /// Informations from Connection-String
        /// </summary>
        SqlConnectionStringBuilder SQLConnectionInfo { get; }

        /// <summary>
        /// Maximum of records that should be returned via ACAccess
        /// </summary>
        Int32 AccessDefaultTakeCount { get; }

        bool UseDynLINQ { get; }

        #endregion


            #region Connection Statistics

            /// <summary>
            /// Largest number of concurrent database connections
            /// </summary>
        int MaxDBConnectionCount { get; }


        /// <summary>
        /// Current number of concurrent database connections
        /// This works only if loggen on sql-user has administrative sql-rights for querying sys.sysprocesses.
        /// Otherwise it returns null or only one connection
        /// </summary>
        int? CurrentDBConnectionCount { get; }


        /// <summary>
        /// Largest number of concurrent network connections
        /// </summary>
        int MaxWCFConnectionCount { get;  }


        /// <summary>
        /// Largest number of concurrent windows sessions
        /// </summary>
        int MaxWinSessionCount { get; }


        /// <summary>
        /// Current number of concurrent windows sessions
        /// </summary>
        int CurrentWinSessionCount { get; }


        /// <summary>
        /// Maximum licensed windows sessions
        /// </summary>
        int MaxLicensedWinSessions { get; }


        /// <summary>
        /// Returns true if MaxLicensedWinSessions are exceeded
        /// </summary>
        bool IsMaxWinSessionsExceeded { get; }


        /// <summary>
        /// Maximum licensed network connections
        /// </summary>
        int MaxLicensedWCFConnections { get; }


        #endregion
    }
}