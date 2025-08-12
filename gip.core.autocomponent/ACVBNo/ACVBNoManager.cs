// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Number Manager'}de{'Nummern Manager'}", Global.ACKinds.TPARole, Global.ACStorableTypes.NotStorable, false, false)]
    public class ACVBNoManager : PARole, IACVBNoManager
    {
        public const string ClassName = "ACVBNoManager";
        public const string C_DefaultServiceACIdentifier = "NoManager";

        #region c´tors

        public ACVBNoManager(ACClass acType, IACObject content, IACObject parentACObject, ACValueList parameter, string acIdentifier = "")
            : base(acType, content, parentACObject, parameter, acIdentifier)
        {
        }

        public override bool ACInit(Global.ACStartTypes startChildMode = Global.ACStartTypes.Automatic)
        {
            if (!base.ACInit(startChildMode))
                return false;

            return true;
        }
        #endregion

        #region PrecompiledQueries
        static readonly Func<Database, Guid, IEnumerable<VBNoConfiguration>> s_cQry_ConfigID =
            EF.CompileQuery<Database, Guid, IEnumerable<VBNoConfiguration>>(
#if EFCR
            (ctx, noConfigurationID) => ctx.VBNoConfiguration.Where(c => c.VBNoConfigurationID == noConfigurationID).Refresh(MergeOption.OverwriteChanges)
#else
            (ctx, noConfigurationID) => ctx.VBNoConfiguration.Where(c => c.VBNoConfigurationID == noConfigurationID)
#endif
        );

        static readonly Func<Database, string, IEnumerable<VBNoConfiguration>> s_cQry_ConfigName =
            EF.CompileQuery<Database, string, IEnumerable<VBNoConfiguration>>(
#if EFCR
            (ctx, noConfigurationName) => ctx.VBNoConfiguration.Where(c => c.VBNoConfigurationName == noConfigurationName).Refresh(MergeOption.OverwriteChanges)
#else
            (ctx, noConfigurationName) => ctx.VBNoConfiguration.Where(c => c.VBNoConfigurationName == noConfigurationName)
#endif
        );
#endregion

        #region static Methods
        public static ACVBNoManager GetServiceInstance(ACComponent requester)
        {
            return GetServiceInstance<ACVBNoManager>(requester, C_DefaultServiceACIdentifier, CreationBehaviour.FirstOrDefault|CreationBehaviour.OnlyLocal);
        }

        public static ACRef<ACVBNoManager> ACRefToServiceInstance(ACComponent requester)
        {
            ACVBNoManager serviceInstance = GetServiceInstance(requester) as ACVBNoManager;
            if (serviceInstance != null)
                return new ACRef<ACVBNoManager>(serviceInstance, requester);
            return null;
        }
        #endregion

        #region IACVBNoManager

        /// <summary>
        /// Returns a new secondary key for a database table (Entity-Class).
        /// Use the businessobject gip.bso.BSONoConfiguration to configure a individual rule for generating this key.
        /// You can override this method if you want to implement a project specific behaviour.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="type">Type of the entity class (Database table)</param>
        /// <param name="entityNoFieldName">Name of the Field in the database table that stores the secondary key.</param>
        /// <param name="formatNewNo">A Format-String how the sequantially generated nummer should be formatted.</param>
        /// <param name="invoker">The invoker who calls this method.</param>
        /// <returns>A unique secondary key</returns>
        public virtual string GetNewNo(IACEntityObjectContext context, Type type, string entityNoFieldName, string formatNewNo, IACComponent invoker = null)
        {
            using (Database db = new datamodel.Database())
            {
                for (int tries = 0; tries < 3; tries++)
                {
                    string newNo = GetNewNo(db, context, type, entityNoFieldName, formatNewNo, invoker);
                    if (!String.IsNullOrEmpty(newNo))
                        return newNo;
                }
                return null;
            }

        }

        private string GetNewNo(Database iplusContext, IACEntityObjectContext appContext, Type type, string entityNoFieldName, string formatNewNo, IACComponent invoker = null)
        {
            VBNoConfiguration vbNoConfiguration = GetVBNoConfiguration(iplusContext, entityNoFieldName);
            IACVBNoProvider child = FindChildComponents<IACVBNoProvider>(c => c is IACVBNoProvider && (c as IACVBNoProvider).IsHandlerForType(type, entityNoFieldName)).FirstOrDefault();

            string nextNo = child == null ? GetNextNo(vbNoConfiguration, formatNewNo) : child.GetNewNo(this, iplusContext, appContext, type, entityNoFieldName, formatNewNo, vbNoConfiguration, invoker);
            if (iplusContext.ACSaveChanges(true, true) == null)
                return nextNo;
            else
            {
                iplusContext.ACUndoChanges();
                return null;
            }
        }

        public string GetNextNo(VBNoConfiguration vbNoConfiguration, string formatNewNo)
        {
            string nextNo;
            string format = "";
            if (vbNoConfiguration.UsedPrefix != null)
            {
                format = vbNoConfiguration.UsedPrefix;
                if (!format.Contains("{"))
                    format = format + "{0}";
            }
            else
                format = formatNewNo;
            if (format != null)
                format = format.Trim();

            if (vbNoConfiguration.MaxCounter < 0)
                throw new ArgumentOutOfRangeException(String.Format("MaxCounter {0} is less than Zero", vbNoConfiguration.MaxCounter));
            else if (vbNoConfiguration.MinCounter < 0)
                throw new ArgumentOutOfRangeException(String.Format("MinCounter {0} is less than Zero", vbNoConfiguration.MinCounter));
            else if (vbNoConfiguration.MaxCounter <= vbNoConfiguration.MinCounter)
                throw new ArgumentOutOfRangeException(String.Format("MaxCounter {0} is less than or equal to MinCounter {1}", vbNoConfiguration.MaxCounter, vbNoConfiguration.MinCounter));

            if (vbNoConfiguration.CurrentCounter <= 0)
            {
                if (vbNoConfiguration.MinCounter > 0)
                    vbNoConfiguration.CurrentCounter = vbNoConfiguration.MinCounter;
                else
                    vbNoConfiguration.CurrentCounter = 1;
            }

            string localFormat;
            if (vbNoConfiguration.UseDate)
            {
                DateTime date = DateTime.Today;
                if (vbNoConfiguration.CurrentDate != date)
                {
                    vbNoConfiguration.CurrentDate = date;
                    vbNoConfiguration.CurrentCounter = vbNoConfiguration.MinCounter;
                }
                localFormat = "{0:D1}{1:D2}{2:D2}" + vbNoConfiguration.UsedDelimiter + "{3:D" + string.Format("{0:D2}", vbNoConfiguration.MaxCounter.ToString().Length) + "}";
                string year = vbNoConfiguration.CurrentDate.Year.ToString();
                if (!string.IsNullOrEmpty(format))
                    format = string.Format(format, localFormat);
                else
                    format = localFormat;
                nextNo = string.Format(format, year.Substring(year.Length - 1, 1), vbNoConfiguration.CurrentDate.Month, vbNoConfiguration.CurrentDate.Day, vbNoConfiguration.CurrentCounter);
            }
            else
            {
                localFormat = "{0:D" + string.Format("{0:D2}", vbNoConfiguration.MaxCounter.ToString().Length) + "}";
                if (!string.IsNullOrEmpty(format))
                    format = string.Format(format, localFormat);
                else
                    format = localFormat;
                nextNo = string.Format(format, vbNoConfiguration.CurrentCounter);
            }

            vbNoConfiguration.CurrentCounter++;
            if (vbNoConfiguration.CurrentCounter > vbNoConfiguration.MaxCounter)
            {
                if (vbNoConfiguration.MinCounter > 0)
                    vbNoConfiguration.CurrentCounter = vbNoConfiguration.MinCounter;
                else
                    vbNoConfiguration.CurrentCounter = 0;
            }

            return nextNo;
        }
        #endregion

        #region Manager->Modify->NoConfiguration

        public VBNoConfiguration GetVBNoConfiguration(Database db, string entityNoFieldName)
        {
            VBNoConfiguration noConfiguration = null;
            noConfiguration = LoadNoConfigurationByConfigurationName(entityNoFieldName, db);

            if (noConfiguration == null)
            {
                noConfiguration = NewNoConfiguration(db, false);
                noConfiguration.VBNoConfigurationName = entityNoFieldName;
            }
            return noConfiguration;
        }

        public VBNoConfiguration LoadNoConfigurationByConfigurationName(string entityNoFieldName, Database database)
        {
            if (database == null)
                return null;
            //Originally query was of IQueryable Type but EF.CompileQuery's return type cannot be of IQueryable, use IEnumerable instead. https://github.com/dotnet/efcore/issues/27657
            IEnumerable<VBNoConfiguration> query = s_cQry_ConfigName(database, entityNoFieldName);
            return query.FirstOrDefault();
        }

        public VBNoConfiguration LoadNoConfiguration(Guid noConfigurationID, Database database)
        {
            if (database == null)
                return null;
            //Originally query was of IQueryable Type but EF.CompileQuery's return type cannot be of IQueryable, use IEnumerable instead. https://github.com/dotnet/efcore/issues/27657
            IEnumerable<VBNoConfiguration> query = s_cQry_ConfigID(database, noConfigurationID);
            return query.FirstOrDefault();
        }

        public VBNoConfiguration NewNoConfiguration(Database database, bool autoSave)
        {
            VBNoConfiguration noConfiguration = VBNoConfiguration.NewACObject(database, null);
            database.VBNoConfiguration.Add(noConfiguration);
            if (autoSave)
            {
                if (database.ACSaveChanges(true, true) == null)
                    return noConfiguration;
                return
                    null;
            }
            return noConfiguration;
        }
        #endregion
    }
}
