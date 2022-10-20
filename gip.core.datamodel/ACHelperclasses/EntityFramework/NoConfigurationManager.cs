// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="NoConfigurationManager.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Objects;

namespace gip.core.datamodel
{
    /// <summary>
    /// 1 VBNoConfiguration
    /// </summary>
    public class NoConfigurationManager 
    {
        #region c´tors
        /// <summary>
        /// Initializes a new instance of the <see cref="NoConfigurationManager"/> class.
        /// </summary>
        /// <param name="database">The database.</param>
        public NoConfigurationManager(Database database)
        {
            Database = database;
        }
        #endregion

        #region PrecompiledQueries
        static readonly Func<Database, Guid, IQueryable<VBNoConfiguration>> s_cQry_ConfigID =
            CompiledQuery.Compile<Database, Guid, IQueryable<VBNoConfiguration>>(
            (ctx, noConfigurationID) => from c in ctx.VBNoConfiguration where c.VBNoConfigurationID == noConfigurationID select c
        );

        static readonly Func<Database, string, IQueryable<VBNoConfiguration>> s_cQry_ConfigName =
            CompiledQuery.Compile<Database, string , IQueryable<VBNoConfiguration>>(
            (ctx, noConfigurationName) => from c in ctx.VBNoConfiguration where c.VBNoConfigurationName == noConfigurationName select c
        );
        #endregion

        /// <summary>
        /// The _ database
        /// </summary>
        Database _Database;
        /// <summary>
        /// Gets or sets the database.
        /// </summary>
        /// <value>The database.</value>
        Database Database
        {
            get
            {
                return _Database;
            }
            set
            {
                _Database = value;
            }
        }
        #region Manager->Nummerngenerierung

        /// <summary>
        /// Ermittelt die nächste gültige No für den noConfigurationName und speichert die neue Nummer sofort in der Datenbank ab.
        /// </summary>
        /// <param name="noConfigurationName"></param>
        /// <returns></returns>
        public static string GetNewNo(string noConfigurationName)
        {
            using (Database db = new Database())
            {
                for (int tries = 0; tries < 3; tries++)
                {
                    string newNo = GetNewNo(noConfigurationName, db, true);
                    if (!String.IsNullOrEmpty(newNo))
                        return newNo;
                }
            }
            return null;
        }

        /// <summary>
        /// Ermittelt die nächste gültige No für den noConfigurationName
        /// </summary>
        /// <param name="noConfigurationName">Der "noConfigurationName" ist meist der Tabellenname.
        /// Es dürfen aber auch andere Bezeichnungen verwendet werden</param>
        /// <param name="db">ObjectContext. Am besten sollte eine eigener Database-Context in einer using-Anweisung verwendet werden, damit es nicht zu Problemen beim Speichern kommt wenn der übergebende Kontext sich schon im modifizierten Status befinden sollte.</param>
        /// <param name="autoSave">Wenn die Funktion in einer umgebenenden Transkation ausgeführt werden soll, dann autoSave auf false setze.
        /// Achtung: Aufgrund von Mulituser/Concurrency-Problemen sollte sofort gespeichert werden, damit es zu keiner OptimisticConcurrencyException kommt!</param>
        /// <returns></returns>
        public static string GetNewNo(string noConfigurationName, Database db, bool autoSave = true)
        {
            VBNoConfiguration noConfiguration = null;
            noConfiguration = LoadNoConfigurationByConfigurationName(noConfigurationName, db);

            if ( noConfiguration == null )
            {
                noConfiguration = NewNoConfiguration(db, false);
                noConfiguration.VBNoConfigurationName = noConfigurationName;
            }
            string nextNo = noConfiguration.GetNextNo();
            if (autoSave)
            {
                if (db.ACSaveChanges(true, SaveOptions.AcceptAllChangesAfterSave, true) == null)
                    return nextNo;
                else
                {
                    db.ACUndoChanges();
                    return null;
                }
            }

            return nextNo;
        }
        #endregion



        #region Manager->Modify->NoConfiguration
        /// <summary>
        /// Loads the name of the no configuration by configuration.
        /// </summary>
        /// <param name="noConfigurationName">Name of the no configuration.</param>
        /// <returns>VBNoConfiguration.</returns>
        public static VBNoConfiguration LoadNoConfigurationByConfigurationName(string noConfigurationName, Database database)
        {
            if (database == null)
                return null;
            ObjectQuery<VBNoConfiguration> query = (ObjectQuery<VBNoConfiguration>) s_cQry_ConfigName(database, noConfigurationName);
            query.MergeOption = MergeOption.OverwriteChanges;
            return query.FirstOrDefault();
        }

        /// <summary>
        /// Loads the no configuration.
        /// </summary>
        /// <param name="noConfigurationID">The no configuration ID.</param>
        /// <returns>VBNoConfiguration.</returns>
        public static VBNoConfiguration LoadNoConfiguration(Guid noConfigurationID, Database database)
        {
            if (database == null)
                return null;
            ObjectQuery<VBNoConfiguration> query = (ObjectQuery<VBNoConfiguration>)s_cQry_ConfigID(database, noConfigurationID);
            query.MergeOption = MergeOption.OverwriteChanges;
            return query.FirstOrDefault();
        }

        /// <summary>
        /// News the no configuration.
        /// </summary>
        /// <returns>VBNoConfiguration.</returns>
        public static VBNoConfiguration NewNoConfiguration(Database database, bool autoSave)
        {
            VBNoConfiguration noConfiguration = VBNoConfiguration.NewACObject(database, null);
            database.VBNoConfiguration.AddObject(noConfiguration);
            if (autoSave)
            {
                if (database.ACSaveChanges(true, SaveOptions.AcceptAllChangesAfterSave, true) == null)
                    return noConfiguration;
                return
                    null;
            }
            return noConfiguration;
        }
        #endregion
    }
}
