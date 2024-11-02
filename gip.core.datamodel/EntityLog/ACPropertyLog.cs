// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace gip.core.datamodel
{
    /// <summary>
    ///   <para>ACProperyLog is used to store changes of properties of ACComponents.</para>
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACPropertyLog'}de{'ACPropertyLog'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, true, "", "")]
    [ACPropertyEntity(1, "EventTime", "en{'Event time'}de{'Ereigniszeit'}")]
    [ACPropertyEntity(2, "Value","en{'Value'}de{'Wert'}")]
    [ACPropertyEntity(3, ACClass.ClassName, "en{'ACClass'}de{'ACClass'}", Database.ClassName + "\\" + ACClass.ClassName + Const.DBSetAsEnumerablePostfix)]
    [ACPropertyEntity(4, ACClassProperty.ClassName, "en{'ACClassProperty'}de{'ACClassProperty'}", Database.ClassName + "\\" + ACClassProperty.ClassName + Const.DBSetAsEnumerablePostfix)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + ACPropertyLog.ClassName, "en{'ACPropertyLog'}de{'ACPropertyLog'}", typeof(ACPropertyLog), ACPropertyLog.ClassName, "", "EventTime")]
    [NotMapped]
    public partial class ACPropertyLog
    {
        public const string ClassName = "ACPropertyLog";

        /// <summary>
        /// Creates a new object of the ACPropertyLog.
        /// </summary>
        /// <param name="db">The database context.</param>
        /// <param name="acClass">The ACClass reference.</param>
        /// <returns>The created object.</returns>
        public static ACPropertyLog NewACObject(Database db, ACClass acClass)
        {
            ACPropertyLog entity = new ACPropertyLog();
            entity.ACPropertyLogID = Guid.NewGuid();
            entity.Context = db;
            entity.ACClassID = acClass.ACClassID;
            return entity;
        }


        /// <summary>
        /// Aggregates the duration of property logs by value(state).
        /// </summary>
        /// <param name="from">Aggregates from this date time point.</param>
        /// <param name="to">Aggregates to this date time point.</param>
        /// <param name="acClassID">The ID of ACClass related to the property log.</param>
        /// <param name="acClassPropertyID">The ID of ACClassProperty related to the log.</param>
        /// <returns>Aggregate values of property logs.</returns>
        public static IEnumerable<ACPropertyLogSum> AggregateDurationOfPropertyValues(DateTime from, DateTime to, Guid acClassID, Guid acClassPropertyID)
        {
            List<ACPropertyLogInfo> result = null;

            using(Database db = new Database())
            {
                var propertyLogs = db.ACPropertyLog.Include(c => c.ACClassProperty)
                                                   .Include(c => c.ACClass)
                                                   .Join(db.ACProgramLog, propLog => propLog.ACProgramLogID, programLog => programLog.ACProgramLogID, (propLog, programLog) => new { propLog, programLog })
                                                   .Where(c => c.propLog.ACClassID == acClassID 
                                                                && c.propLog.ACClassPropertyID == acClassPropertyID 
                                                                && c.propLog.EventTime >= from 
                                                                && c.propLog.EventTime <= to)
                                                   .OrderBy(p => p.propLog.EventTime)
                                                   .Select(c => new ACPropertyLog_ACProgramLog() { PropertyLog = c.propLog, ProgramLog = c.programLog })
                                                   .ToArray();

                result = BuildACPropertyLogInfo(from, to, acClassID, db, propertyLogs);
            }

            return AggregateDurationOfPropertyValues(null, null, result);
        }


        /// <summary>
        /// Aggregates the duration of property logs by value(state).
        /// </summary>
        /// <param name="from">Aggregates from this date time point.</param>
        /// <param name="to">Aggregates to this date time point.</param>
        /// <param name="acClassID">The ID of ACClass related to the property log.</param>
        /// <param name="acClassPropertyACIdentifier">The ACIdentifier of ACClassProperty related to the log.</param>
        /// <returns>Aggregate values of property logs.</returns>
        public static IEnumerable<ACPropertyLogSum> AggregateDurationOfPropertyValues(DateTime from, DateTime to, Guid acClassID, string acClassPropertyACIdentifier)
        {
            using (Database db = new Database())
            {
                return AggregateDurationOfPropertyValues(db, from, to, acClassID, acClassPropertyACIdentifier);
            }
        }


        public static IEnumerable<ACPropertyLogSum> AggregateDurationOfPropertyValues(Database db, DateTime from, DateTime to, Guid acClassID, string acClassPropertyACIdentifier)
        {
            List<ACPropertyLogInfo> result = null;
            var propertyLogs = db.ACPropertyLog.Include(c => c.ACClassProperty)
                                                .Include(c => c.ACClass)
                                                .Join(db.ACProgramLog, propLog => propLog.ACProgramLogID, programLog => programLog.ACProgramLogID, (propLog, programLog) => new { propLog, programLog })
                                                .Where(c => c.propLog.ACClassID == acClassID
                                                        && c.propLog.ACClassProperty.ACIdentifier == acClassPropertyACIdentifier
                                                        && c.propLog.EventTime >= from && c.propLog.EventTime <= to)
                                                .OrderBy(p => p.propLog.EventTime)
                                                .Select(c => new ACPropertyLog_ACProgramLog() { PropertyLog = c.propLog, ProgramLog = c.programLog })
                                                .ToArray();

            result = BuildACPropertyLogInfo(from, to, acClassID, db, propertyLogs);
            return AggregateDurationOfPropertyValues(null, null, result);
        }


        private static List<ACPropertyLogInfo> BuildACPropertyLogInfo(DateTime? from, DateTime? to, Guid acClassID, Database db, IEnumerable<ACPropertyLog_ACProgramLog> propertyLogs)
        {
            List<ACPropertyLogInfo> result = new List<ACPropertyLogInfo>();

            if (!propertyLogs.Any())
                return null;

            ACPropertyLog_ACProgramLog tempLog = propertyLogs.FirstOrDefault();
            if (from.HasValue && tempLog.PropertyLog.EventTime != from)
            {
                DateTime dateTime = tempLog.PropertyLog.EventTime.AddMilliseconds(-2);
                ACPropertyLog_ACProgramLog previousLog = db.ACPropertyLog.Include(c => c.ACClassProperty)
                                                            .Include(c => c.ACClass)
                                                            .Join(db.ACProgramLog, propLog => propLog.ACProgramLogID, programLog => programLog.ACProgramLogID, (propLog, programLog) => new { propLog, programLog })
                                                            .Where(c => c.propLog.ACClassID == acClassID 
                                                                     && c.propLog.ACClassPropertyID == tempLog.PropertyLog.ACClassPropertyID && c.propLog.EventTime < dateTime)
                                                            .OrderByDescending(p => p.propLog.EventTime)
                                                            .Select(c => new ACPropertyLog_ACProgramLog() { PropertyLog = c.propLog, ProgramLog = c.programLog })
                                                            .FirstOrDefault();
                if (previousLog != null)
                {
                    previousLog.PropertyLog.EventTime = from.Value;
                    tempLog = previousLog;
                }
            }

            Type valueType = tempLog.PropertyLog.ACClassProperty.ObjectType;
            string acCaption = tempLog.PropertyLog.ACClassProperty.ACCaption;
            string acUrl = tempLog.PropertyLog.ACClass.ACUrlComponent;

            foreach (ACPropertyLog_ACProgramLog propLog in propertyLogs)
            {
                object logValue = ACConvert.XMLToObject(valueType, tempLog.PropertyLog.Value, true, db);
                ACPropertyLogInfo logInfo = new ACPropertyLogInfo(tempLog.PropertyLog.EventTime, propLog.PropertyLog.EventTime, logValue, acCaption) { ACUrl = acUrl, PropertyLog = tempLog.PropertyLog, ProgramLog = tempLog.ProgramLog };
                result.Add(logInfo);
                tempLog = propLog;
            }

            if (to.HasValue && tempLog.PropertyLog.EventTime != to)
            {
                object logValue = ACConvert.XMLToObject(valueType, tempLog.PropertyLog.Value, true, db);
                ACPropertyLogInfo logInfo = new ACPropertyLogInfo(tempLog.PropertyLog.EventTime, to, logValue, acCaption) { ACUrl = acUrl, PropertyLog = tempLog.PropertyLog, ProgramLog = tempLog.ProgramLog };
                result.Add(logInfo);
            }

            return result;
        }


        /// <summary>
        /// Aggregates the duration of property logs by value(state) from given propertyLogs.
        /// </summary>
        /// <param name="from">Aggregates from this date time point. If this parameter is null then function skips from to check and aggregates over all given property logs.</param>
        /// <param name="to">Aggregates to this date time point. If this parameter is null then function skips from to check and aggregates over all given property logs.</param>
        /// <param name="propertyLogs">The list of property logs from which function aggregate result.</param>
        /// <returns>Aggregate values of property logs.</returns>
        public static IEnumerable<ACPropertyLogSum> AggregateDurationOfPropertyValues(DateTime? from, DateTime? to, IEnumerable<ACPropertyLogInfo> propertyLogs)
        {
            if (propertyLogs == null || !propertyLogs.Any())
                return null;

            List<ACPropertyLogSum> propetyLogSum = new List<ACPropertyLogSum>();
            if (from != null && to != null)
            {
                List<ACPropertyLogInfo> queryList = propertyLogs
                                                    .Where(c =>     (  (c.StartDate <= from && c.EndDate > from)
                                                                    || (c.StartDate >= from && c.EndDate > from))
                                                                && (    c.StartDate < to && c.EndDate <= to)
                                                                    || (c.StartDate < to && c.EndDate >= to))
                                                    .OrderBy(p => p.StartDate)
                                                    .ToList();

                if (!queryList.Any())
                    return null;

                if (queryList.Count() == 1)
                {
                    ACPropertyLogInfo tempLogInfo = queryList.FirstOrDefault();
                    ACPropertyLogInfo adjustedLog = new ACPropertyLogInfo(tempLogInfo.StartDate, tempLogInfo.EndDate, tempLogInfo.PropertyValue, tempLogInfo.ACCaption)
                                                                         { ACUrl = tempLogInfo.ACUrl, PropertyLog = tempLogInfo.PropertyLog, ProgramLog = tempLogInfo.ProgramLog };
                    if (tempLogInfo.StartDate < from)
                        adjustedLog.StartDate = from;

                    if (tempLogInfo.EndDate > to)
                        adjustedLog.EndDate = to;

                    queryList.Remove(tempLogInfo);
                    queryList.Add(adjustedLog);
                }
                else
                {
                    var query = queryList.Where(c => c.StartDate < from).ToArray();

                    foreach(ACPropertyLogInfo logInfo in query)
                    {
                        ACPropertyLogInfo adjustedLog = new ACPropertyLogInfo(from, logInfo.EndDate, logInfo.PropertyValue, logInfo.ACCaption)
                                                                             { ACUrl = logInfo.ACUrl, PropertyLog = logInfo.PropertyLog, ProgramLog = logInfo.ProgramLog };
                        queryList.Remove(logInfo);
                        queryList.Insert(0, adjustedLog);
                    }

                    query = queryList.Where(c => c.EndDate > to).ToArray();

                    foreach (ACPropertyLogInfo logInfo in query)
                    {
                        ACPropertyLogInfo adjustedLog = new ACPropertyLogInfo(logInfo.StartDate, to, logInfo.PropertyValue, logInfo.ACCaption)
                                                                             { ACUrl = logInfo.ACUrl, PropertyLog = logInfo.PropertyLog, ProgramLog = logInfo.ProgramLog };
                        queryList.Remove(logInfo);
                        queryList.Add(adjustedLog);
                    }
                }
                propertyLogs = queryList;
            }
            else
            {
                propertyLogs = propertyLogs.OrderBy(c => c.StartDate).ToArray();
            }

            var groupedByPropertyValue = propertyLogs.GroupBy(c => c.PropertyValue);
            foreach (var groupedPropLog in groupedByPropertyValue)
            {
                ACPropertyLogInfo firstItem = groupedPropLog.FirstOrDefault();
                ACPropertyLogInfo lastItem = groupedPropLog.OrderByDescending(c => c.EndDate.HasValue ? c.EndDate : c.StartDate).FirstOrDefault();
                DateTime? startTime = null;
                if (firstItem != null)
                {
                    if (firstItem.EndDate.HasValue)
                        startTime = firstItem.StartDate.Value;
                    if (!startTime.HasValue && firstItem.ProgramLog != null)
                        startTime = firstItem.ProgramLog.StartDate;
                }

                DateTime? endTime = null;
                if (lastItem != null)
                {
                    if (lastItem.EndDate.HasValue)
                        endTime = lastItem.EndDate.Value;
                    else if (lastItem.StartDate.HasValue)
                        endTime = lastItem.StartDate.Value;
                    if (!endTime.HasValue && lastItem.ProgramLog != null)
                        endTime = lastItem.ProgramLog.EndDate;
                }

                propetyLogSum.Add(
                    new ACPropertyLogSum(groupedPropLog.Key, startTime, endTime,
                                         TimeSpan.FromSeconds(groupedPropLog.Where(x => x.StartDate.HasValue && x.EndDate.HasValue).Sum(d => (d.EndDate - d.StartDate).Value.TotalSeconds)),
                                         firstItem?.ACCaption, firstItem?.ACUrl)
                    { ProgramLog = firstItem?.ProgramLog, PropertyLog = firstItem?.PropertyLog }
                );
            }

            return propetyLogSum;
        }


        public static IEnumerable<ACPropertyLogSumOfProgram> GetSummarizedDurationsOfProgram(Guid acProgramID, string[] properties)
        {
            using (Database db = new Database())
            {
                return GetSummarizedDurationsOfProgram(db, acProgramID, properties);
            }
        }


        public static IEnumerable<ACPropertyLogSumOfProgram> GetSummarizedDurationsOfProgram(Database db, Guid acProgramID, string[] properties)
        {
            List<ACPropertyLogSumOfProgram> result = new List<ACPropertyLogSumOfProgram>();

            //var query = db.ACPropertyLog
            //    .Include(c => c.ACClass)
            //    .Include(c => c.ACClassProperty)
            //    .Join(db.ACProgramLog, propLog => propLog.ACProgramLogID, programLog => programLog.ACProgramLogID, (propLog, programLog) => new { propLog, programLog.ACProgramID })
            //    //.Join(db.ACProgram, programLog => programLog.ACProgramID, program => program.ACProgramID, (programLog, program) => new { program.ACProgramID })
            //    .Where(c => c.ACProgramID == acProgramID && properties.Contains(c.propLog.ACClassProperty.ACIdentifier))
            //    .AsEnumerable()
            //    .GroupBy(c => new { c.propLog.ACClass, c.propLog.ACClassProperty })
            //    .ToArray();

            var query = db.ACPropertyLog
                .Include(c => c.ACClass)
                //.Include(c => c.ACClass.ACClass1_BasedOnACClass)
                .Include(c => c.ACClassProperty)
                .Join(db.ACProgramLog, propLog => propLog.ACProgramLogID, programLog => programLog.ACProgramLogID, (propLog, programLog) => new { propLog, programLog })
                //.Join(db.ACProgram, programLog => programLog.ACProgramID, program => program.ACProgramID, (programLog, program) => new { program.ACProgramID })
                .Where(c => c.programLog.ACProgramID == acProgramID && properties.Contains(c.propLog.ACClassProperty.ACIdentifier))
                .AsEnumerable()
                .GroupBy(c => new { c.propLog.ACClass, c.propLog.ACClassProperty })
                .ToArray();

            //var query = from propLog in db.ACPropertyLog
            //            join programLog in db.ACProgramLog on propLog.ACProgramLogID equals programLog.ACProgramLogID
            //            //join program in db.ACProgram on programLog.ACProgramID equals program.ACProgramID
            //            //join acclass in db.ACClass on propLog.ACClassID equals acclass.ACClassID
            //            //where program.ACProgramID == acProgramID
            //            where programLog.ACProgramID == acProgramID
            //            orderby propLog.EventTime
            //            group propLog by new { propLog.ACClassID, propLog.ACClassPropertyID } into g
            //            select g;
            foreach (var propLogs in query)
            {
                IEnumerable<ACPropertyLog_ACProgramLog> sortedLogs = propLogs.Select(c => new ACPropertyLog_ACProgramLog() { PropertyLog = c.propLog, ProgramLog = c.programLog }).OrderBy(c => c.PropertyLog.EventTime).ToArray();
                List<ACPropertyLogInfo> infos = BuildACPropertyLogInfo(null, null, propLogs.Key.ACClass.ACClassID, db, sortedLogs);
                result.Add(new ACPropertyLogSumOfProgram(propLogs.Key.ACClass, propLogs.Key.ACClassProperty, AggregateDurationOfPropertyValues(null, null, infos)));
            }
            return result;
        }


        public static IEnumerable<ACPropertyLog_ACProgramLog> GetLogs(Database db, DateTime from, DateTime to, Guid? projectID = null, Guid? componentClassID = null, string searchText = null)
        {
            IEnumerable<ACPropertyLog_ACProgramLog> relevantLogs = db.ACPropertyLog
                                                        .Include(c => c.ACClass.ACClass1_ParentACClass.ACClass1_ParentACClass)
                                                        .Include(c => c.ACClassProperty)
                                                        .Join(db.ACProgramLog, propLog => propLog.ACProgramLogID, programLog => programLog.ACProgramLogID, (propLog, programLog) => new { propLog, programLog })
                                                        .Where(c =>    c.propLog.EventTime > from 
                                                                    && c.propLog.EventTime < to 
                                                                    && (!projectID.HasValue || c.propLog.ACClass.ACProjectID == projectID.Value) )
                                                        .Select(c => new ACPropertyLog_ACProgramLog() { PropertyLog = c.propLog, ProgramLog = c.programLog })
                                                        .ToArray();

            if (componentClassID.HasValue && componentClassID.Value != Guid.Empty)
            {
                ACClass compClass = db.ACClass.FirstOrDefault(c => c.ACClassID == componentClassID.Value);
                if (compClass != null)
                {
                    relevantLogs = relevantLogs.Where(c => c.PropertyLog.ACClassID == componentClassID
                                                        || c.PropertyLog.ACClass?.ParentACClassID == componentClassID  // Show logs of child components
                                                        || c.PropertyLog.ACClass?.ACClass1_ParentACClass?.ParentACClassID == componentClassID
                                                        || c.PropertyLog.ACClass?.ACClass1_ParentACClass?.ACClass1_ParentACClass?.ParentACClassID == componentClassID);
                }
            }

            if (!String.IsNullOrEmpty(searchText))
            {
                searchText = searchText.ToLower();
                relevantLogs = relevantLogs.Where(c => c.PropertyLog.ACClass.ACUrl.ToLower().Contains(searchText)
                                                        || c.PropertyLog.ACClassProperty.ACCaption.ToLower().Contains(searchText)
                                                        || c.PropertyLog.ACClassProperty.ACIdentifier.ToLower().Contains(searchText));
            }

            return relevantLogs;
        }
    }


    public class ACPropertyLog_ACProgramLog
    {
        public ACPropertyLog PropertyLog { get; set; }
        public ACProgramLog ProgramLog { get; set; }
    }


    [ACClassInfo(Const.PackName_VarioSystem, "en{'PropertyLog Info'}de{'PropertyLog Info'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, true)]
    [NotMapped]
    public class ACPropertyLogInfo : IACTimeLog
    {
        public ACPropertyLogInfo()
        {

        }

        public ACPropertyLogInfo(DateTime? start, DateTime? end, object value, string acCaption, IACObject parent = null)
        {
            StartDate = start;
            EndDate = end;
            PropertyValue = value;
            _ACCaption = acCaption;
            ParentACObject = parent;
        }

        /// <summary>
        /// Gets or sets the StartDate.
        /// </summary>
        [ACPropertyInfo(101, "", "en{'Start date'}de{'Startzeitpunkt'}")]
        [NotMapped]
        public DateTime? StartDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the EndDate.
        /// </summary>
        [ACPropertyInfo(102, "", "en{'End date'}de{'Endzeitpunkt'}")]
        [NotMapped]
        public DateTime? EndDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the Duration. (EndDate - StartDate)
        /// </summary>
        [ACPropertyInfo(103, "", "en{'Duration'}de{'Dauer'}")]
        [NotMapped]
        public virtual TimeSpan Duration
        {
            get
            {
                if (StartDate.HasValue && EndDate.HasValue)
                    return EndDate.Value - StartDate.Value;
                return new TimeSpan();
            }
            set 
            {
            }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        [ACPropertyInfo(103, "", "en{'State value'}de{'State value'}")]
        [NotMapped]
        public object PropertyValue
        {
            get;
            set;
        }

        [NotMapped]
        public ACPropertyLog PropertyLog
        {
            get;
            set;
        }

        public ACProgramLog ProgramLog
        {
            get;
            set;
        }

        /// <summary>
        /// The PropertyChanged event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invokes the OnPropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property name parameter.</param>
        public void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #region IACObject members

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        [NotMapped]
        public string ACIdentifier { get; set; }

        [NotMapped]
        public string _ACCaption;

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(108)]
        [NotMapped]
        public string ACCaption
        {
            get => _ACCaption;
        }

        [ACPropertyInfo(109)]
        [NotMapped]
        public string ACUrl
        {
            get;
            set;
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        [NotMapped]
        public IACType ACType => this.ReflectACType();

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        [NotMapped]
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        [NotMapped]
        public IACObject ParentACObject
        {
            get;
            set;
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return this.ReflectACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Determins is enabled ACUrlCommand.
        /// </summary>
        /// <param name="acUrl">The acUrl.</param>
        /// <param name="acParameter">The acParameter.</param>
        /// <returns>True if is enabled, otherwise returns false.</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }
        #endregion
    }


    [ACClassInfo(Const.PackName_VarioSystem, "en{'PropertyLog Sum'}de{'PropertyLog Sum'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, true)]
    [NotMapped]
    public class ACPropertyLogSum : ACPropertyLogInfo
    {
        public ACPropertyLogSum() : base()
        {

        }

        public ACPropertyLogSum(object propertyValue, DateTime? start, DateTime? end, TimeSpan duration, string acCaption, string acUrl) : base(start, end, propertyValue, acCaption)
        {
            Duration = duration;
            ACUrl = acUrl;
        }

        /// <summary>
        /// Gets or sets the Duration.
        /// </summary>
        [ACPropertyInfo(103, "", "en{'Duration'}de{'Dauer'}")]
        [NotMapped]
        public override TimeSpan Duration
        {
            get;
            set;
        }
    }


    public class ACPropertyLogSumOfProgram
    {
        public ACPropertyLogSumOfProgram(ACClass acClass, ACClassProperty acClassProperty, IEnumerable<ACPropertyLogSum> sum)
        {
            ACClass = acClass;
            ACClassProperty = acClassProperty;
            Sum = sum;
        }
        [NotMapped]
        public ACClass ACClass { get; private set; }
        [NotMapped]
        public ACClassProperty ACClassProperty { get; private set; }
        [NotMapped]
        public IEnumerable<ACPropertyLogSum> Sum { get; private set; }
    }

}
