// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using gip.core.datamodel;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace gip.bso.iplus.Manager
{
    public class IPlusDefSQLExportManager
    {

        #region consts
        public const string Const_Format_datetime = @"yyyy-MM-dd HH:mm:ss";
        #endregion

        #region DI
        public Database Database { get; private set; }
        #endregion


        #region ctor's
        public IPlusDefSQLExportManager(Database database)
        {
            Database = database;
        }
        #endregion

        #region Methods

        public void BuildExportSQL(StreamWriter streamWriter)
        {

            // ACPackage
            streamWriter.WriteLine("-- ACPackage");
            List<ACPackage> aCPackages = Database.ACPackage.ToList();
            foreach (ACPackage aCPackage in aCPackages)
                streamWriter.WriteLine(GetInsertSQLACPackage(aCPackage));
            streamWriter.WriteLine();

            // ACProject
            streamWriter.WriteLine("-- ACProject");
            List<ACProject> acProjects = Database.ACProject.ToList();
            foreach (ACProject acProject in acProjects)
                streamWriter.WriteLine(GetInsertSQLACProject(acProject));

            // ACClass
            streamWriter.WriteLine("-- ACClass");
            List<ACClass> acClasses = Database.ACClass.ToList();
            foreach (ACClass acClass in acClasses)
                streamWriter.WriteLine(GetInsertSQLACClass(acClass));


            // ACClassProperty
            streamWriter.WriteLine("-- ACClassProperty");
            List<ACClassProperty> aCClassProperties = Database.ACClassProperty.ToList();
            foreach (ACClassProperty aCClassProperty in aCClassProperties)
                streamWriter.WriteLine(GetInsertSQLACClassProperty(aCClassProperty));

            // ACClassMethod
            streamWriter.WriteLine("-- ACClassMethod");
            List<ACClassMethod> aCClassMethods = Database.ACClassMethod.ToList();
            foreach (ACClassMethod aCClassMethod in aCClassMethods)
                streamWriter.WriteLine(GetInsertSQLACClassMethod(aCClassMethod));

            // ACClassDesign
            //streamWriter.WriteLine("-- ACClassDesign");
            //List<ACClassDesign> acProjects = Database.ACProject.ToList();
            //foreach (ACClass acClass in acClasses)
            //    streamWriter.WriteLine(GetInsertSQLACClass(acClass));

        }

        #region Methods -> SQL Rows


        // ACClass
        private string GetInsertSQLACClass(ACClass item)
        {
            string sql = SQLInsert_ACClass;
            sql = sql.Replace("{ACClassID}", item.ACClassID.ToString().InsertInMarks());
            sql = sql.Replace("{ACProjectID}", item.ACProjectID.ToString().InsertInMarks());
            if (item.BasedOnACClassID == null)
                sql = sql.Replace("{BasedOnACClassID}", "null");
            else
                sql = sql.Replace("{BasedOnACClassID}", item.BasedOnACClassID.ToString().InsertInMarks());

            if (item.ParentACClassID == null)
                sql = sql.Replace("{ParentACClassID}", "null");
            else
                sql = sql.Replace("{ParentACClassID}", item.ParentACClassID.ToString().InsertInMarks());

            sql = sql.Replace("{ACIdentifier}", item.ACIdentifier.ToString().InsertInMarks());
            if (item.ACIdentifierKey == null)
                sql = sql.Replace("{ACIdentifierKey}", "null");
            else
                sql = sql.Replace("{ACIdentifierKey}", item.ACIdentifierKey.ToString());

            if (string.IsNullOrEmpty(item.ACCaptionTranslation))
                sql = sql.Replace("{ACCaptionTranslation}", "null");
            else
                sql = sql.Replace("{ACCaptionTranslation}", item.ACCaptionTranslation.ToString().InsertInMarks());

            sql = sql.Replace("{ACKindIndex}", item.ACKindIndex.ToString());
            sql = sql.Replace("{SortIndex}", item.SortIndex.ToString());
            sql = sql.Replace("{ACPackageID}", item.ACPackageID.ToString().InsertInMarks());
            if (string.IsNullOrEmpty(item.AssemblyQualifiedName))
                sql = sql.Replace("{AssemblyQualifiedName}", "null");
            else
                sql = sql.Replace("{AssemblyQualifiedName}", item.AssemblyQualifiedName.ToString().InsertInMarks());

            if (item.PWACClassID == null)
                sql = sql.Replace("{PWACClassID}", "null");
            else
                sql = sql.Replace("{PWACClassID}", item.PWACClassID.ToString().InsertInMarks());

            if (item.PWMethodACClassID == null)
                sql = sql.Replace("{PWMethodACClassID}", "null");
            else
                sql = sql.Replace("{PWMethodACClassID}", item.PWMethodACClassID.ToString().InsertInMarks());

            if (string.IsNullOrEmpty(item.Comment))
                sql = sql.Replace("{Comment}", "null");
            else
                sql = sql.Replace("{Comment}", item.Comment.ToString().InsertInMarks());

            sql = sql.Replace("{IsAutostart}", item.IsAutostart.ToString());
            sql = sql.Replace("{IsAbstract}", item.IsAbstract.ToString());
            sql = sql.Replace("{ACStartTypeIndex}", item.ACStartTypeIndex.ToString());
            sql = sql.Replace("{ACStorableTypeIndex}", item.ACStorableTypeIndex.ToString());
            sql = sql.Replace("{IsAssembly}", item.IsAssembly.ToString());
            sql = sql.Replace("{IsMultiInstance}", item.IsMultiInstance.ToString());
            sql = sql.Replace("{IsRightmanagement}", item.IsRightmanagement.ToString());
            if (string.IsNullOrEmpty(item.ACSortColumns))
                sql = sql.Replace("{ACSortColumns}", "null");
            else
                sql = sql.Replace("{ACSortColumns}", item.ACSortColumns.ToString().InsertInMarks());

            if (string.IsNullOrEmpty(item.ACFilterColumns))
                sql = sql.Replace("{ACFilterColumns}", "null");
            else
                sql = sql.Replace("{ACFilterColumns}", item.ACFilterColumns.ToString().InsertInMarks());

            sql = sql.Replace("{XMLConfig}", item.XMLConfig.ToString().InsertInMarks());
            if (string.IsNullOrEmpty(item.XMLACClass))
                sql = sql.Replace("{XMLACClass}", "null");
            else
                sql = sql.Replace("{XMLACClass}", item.XMLACClass.ToString().InsertInMarks());

            sql = sql.Replace("{BranchNo}", item.BranchNo.ToString());
            sql = sql.Replace("{InsertName}", item.InsertName.ToString().InsertInMarks());
            sql = sql.Replace("{InsertDate}", item.InsertDate.ToString(Const_Format_datetime).InsertInMarks());
            sql = sql.Replace("{UpdateName}", item.UpdateName.ToString().InsertInMarks());
            sql = sql.Replace("{UpdateDate}", item.UpdateDate.ToString(Const_Format_datetime).InsertInMarks());
            if (item.ChangeLogMax == null)
                sql = sql.Replace("{ChangeLogMax}", "null");
            else
                sql = sql.Replace("{ChangeLogMax}", item.ChangeLogMax.ToString());

            if (string.IsNullOrEmpty(item.ACURLCached))
                sql = sql.Replace("{ACURLCached}", "null");
            else
                sql = sql.Replace("{ACURLCached}", item.ACURLCached.ToString().InsertInMarks());

            if (string.IsNullOrEmpty(item.ACURLComponentCached))
                sql = sql.Replace("{ACURLComponentCached}", "null");
            else
                sql = sql.Replace("{ACURLComponentCached}", item.ACURLComponentCached.ToString().InsertInMarks());

            sql = sql.Replace("{IsStatic}", item.IsStatic.ToString());

            return sql;
        }
        // ACClassDesign
        private string GetInsertSQLACClassDesign(ACClassDesign item)
        {
            string sql = SQLInsert_ACClassDesign;
            sql = sql.Replace("{ACClassDesignID}", item.ACClassDesignID.ToString().InsertInMarks());
            sql = sql.Replace("{ACClassID}", item.ACClassID.ToString().InsertInMarks());
            sql = sql.Replace("{ACIdentifier}", item.ACIdentifier.ToString().InsertInMarks());
            if (item.ACIdentifierKey == null)
                sql = sql.Replace("{ACIdentifierKey}", "null");
            else
                sql = sql.Replace("{ACIdentifierKey}", item.ACIdentifierKey.ToString());

            if (string.IsNullOrEmpty(item.ACCaptionTranslation))
                sql = sql.Replace("{ACCaptionTranslation}", "null");
            else
                sql = sql.Replace("{ACCaptionTranslation}", item.ACCaptionTranslation.ToString().InsertInMarks());

            if (string.IsNullOrEmpty(item.ACGroup))
                sql = sql.Replace("{ACGroup}", "null");
            else
                sql = sql.Replace("{ACGroup}", item.ACGroup.ToString().InsertInMarks());

            sql = sql.Replace("{XMLDesign}", item.XMLDesign.ToString().InsertInMarks());
            if (item.DesignBinary == null)
                sql = sql.Replace("{DesignBinary}", "null");
            else
                sql = sql.Replace("{DesignBinary}", item.DesignBinary.ToString().InsertInMarks());

            if (string.IsNullOrEmpty(item.DesignNo))
                sql = sql.Replace("{DesignNo}", "null");
            else
                sql = sql.Replace("{DesignNo}", item.DesignNo.ToString().InsertInMarks());

            if (item.ValueTypeACClassID == null)
                sql = sql.Replace("{ValueTypeACClassID}", "null");
            else
                sql = sql.Replace("{ValueTypeACClassID}", item.ValueTypeACClassID.ToString().InsertInMarks());

            sql = sql.Replace("{ACKindIndex}", item.ACKindIndex.ToString());
            sql = sql.Replace("{ACUsageIndex}", item.ACUsageIndex.ToString());
            sql = sql.Replace("{SortIndex}", item.SortIndex.ToString());
            sql = sql.Replace("{IsRightmanagement}", item.IsRightmanagement.ToString());
            if (string.IsNullOrEmpty(item.Comment))
                sql = sql.Replace("{Comment}", "null");
            else
                sql = sql.Replace("{Comment}", item.Comment.ToString().InsertInMarks());

            sql = sql.Replace("{IsDefault}", item.IsDefault.ToString());
            sql = sql.Replace("{IsResourceStyle}", item.IsResourceStyle.ToString());
            sql = sql.Replace("{VisualHeight}", item.VisualHeight.ToString());
            sql = sql.Replace("{VisualWidth}", item.VisualWidth.ToString());
            if (string.IsNullOrEmpty(item.XMLConfig))
                sql = sql.Replace("{XMLConfig}", "null");
            else
                sql = sql.Replace("{XMLConfig}", item.XMLConfig.ToString().InsertInMarks());

            sql = sql.Replace("{BranchNo}", item.BranchNo.ToString());
            if (item.DesignerMaxRecursion == null)
                sql = sql.Replace("{DesignerMaxRecursion}", "null");
            else
                sql = sql.Replace("{DesignerMaxRecursion}", item.DesignerMaxRecursion.ToString());

            sql = sql.Replace("{InsertName}", item.InsertName.ToString().InsertInMarks());
            sql = sql.Replace("{InsertDate}", item.InsertDate.ToString(Const_Format_datetime).InsertInMarks());
            sql = sql.Replace("{UpdateName}", item.UpdateName.ToString().InsertInMarks());
            sql = sql.Replace("{UpdateDate}", item.UpdateDate.ToString(Const_Format_datetime).InsertInMarks());
            if (item.BAMLDesign == null)
                sql = sql.Replace("{BAMLDesign}", "null");
            else
                sql = sql.Replace("{BAMLDesign}", item.BAMLDesign.ToString().InsertInMarks());

            if (item.BAMLDate == null)
                sql = sql.Replace("{BAMLDate}", "null");
            else
                sql = sql.Replace("{BAMLDate}", item.BAMLDate?.ToString(Const_Format_datetime).InsertInMarks());


            return sql;
        }
        // ACClassMethod
        private string GetInsertSQLACClassMethod(ACClassMethod item)
        {
            string sql = SQLInsert_ACClassMethod;
            sql = sql.Replace("{ACClassMethodID}", item.ACClassMethodID.ToString().InsertInMarks());
            sql = sql.Replace("{ACClassID}", item.ACClassID.ToString().InsertInMarks());
            sql = sql.Replace("{ACIdentifier}", item.ACIdentifier.ToString().InsertInMarks());
            if (item.ACIdentifierKey == null)
                sql = sql.Replace("{ACIdentifierKey}", "null");
            else
                sql = sql.Replace("{ACIdentifierKey}", item.ACIdentifierKey.ToString());

            if (string.IsNullOrEmpty(item.ACCaptionTranslation))
                sql = sql.Replace("{ACCaptionTranslation}", "null");
            else
                sql = sql.Replace("{ACCaptionTranslation}", item.ACCaptionTranslation.ToString().InsertInMarks());

            if (string.IsNullOrEmpty(item.ACGroup))
                sql = sql.Replace("{ACGroup}", "null");
            else
                sql = sql.Replace("{ACGroup}", item.ACGroup.ToString().InsertInMarks());

            if (string.IsNullOrEmpty(item.Sourcecode))
                sql = sql.Replace("{Sourcecode}", "null");
            else
                sql = sql.Replace("{Sourcecode}", item.Sourcecode.ToString().InsertInMarks());

            sql = sql.Replace("{ACKindIndex}", item.ACKindIndex.ToString());
            sql = sql.Replace("{SortIndex}", item.SortIndex.ToString());
            sql = sql.Replace("{IsRightmanagement}", item.IsRightmanagement.ToString());
            if (string.IsNullOrEmpty(item.Comment))
                sql = sql.Replace("{Comment}", "null");
            else
                sql = sql.Replace("{Comment}", item.Comment.ToString().InsertInMarks());

            sql = sql.Replace("{IsCommand}", item.IsCommand.ToString());
            sql = sql.Replace("{IsInteraction}", item.IsInteraction.ToString());
            sql = sql.Replace("{IsAsyncProcess}", item.IsAsyncProcess.ToString());
            sql = sql.Replace("{IsPeriodic}", item.IsPeriodic.ToString());
            sql = sql.Replace("{IsParameterACMethod}", item.IsParameterACMethod.ToString());
            sql = sql.Replace("{IsSubMethod}", item.IsSubMethod.ToString());
            if (string.IsNullOrEmpty(item.InteractionVBContent))
                sql = sql.Replace("{InteractionVBContent}", "null");
            else
                sql = sql.Replace("{InteractionVBContent}", item.InteractionVBContent.ToString().InsertInMarks());

            sql = sql.Replace("{IsAutoenabled}", item.IsAutoenabled.ToString());
            sql = sql.Replace("{IsPersistable}", item.IsPersistable.ToString());
            if (item.PWACClassID == null)
                sql = sql.Replace("{PWACClassID}", "null");
            else
                sql = sql.Replace("{PWACClassID}", item.PWACClassID.ToString().InsertInMarks());

            sql = sql.Replace("{ContinueByError}", item.ContinueByError.ToString());
            if (item.ValueTypeACClassID == null)
                sql = sql.Replace("{ValueTypeACClassID}", "null");
            else
                sql = sql.Replace("{ValueTypeACClassID}", item.ValueTypeACClassID.ToString().InsertInMarks());

            if (string.IsNullOrEmpty(item.GenericType))
                sql = sql.Replace("{GenericType}", "null");
            else
                sql = sql.Replace("{GenericType}", item.GenericType.ToString().InsertInMarks());

            if (item.ParentACClassMethodID == null)
                sql = sql.Replace("{ParentACClassMethodID}", "null");
            else
                sql = sql.Replace("{ParentACClassMethodID}", item.ParentACClassMethodID.ToString().InsertInMarks());

            if (string.IsNullOrEmpty(item.XMLACMethod))
                sql = sql.Replace("{XMLACMethod}", "null");
            else
                sql = sql.Replace("{XMLACMethod}", item.XMLACMethod.ToString().InsertInMarks());

            if (string.IsNullOrEmpty(item.XMLDesign))
                sql = sql.Replace("{XMLDesign}", "null");
            else
                sql = sql.Replace("{XMLDesign}", item.XMLDesign.ToString().InsertInMarks());

            if (string.IsNullOrEmpty(item.XMLConfig))
                sql = sql.Replace("{XMLConfig}", "null");
            else
                sql = sql.Replace("{XMLConfig}", item.XMLConfig.ToString().InsertInMarks());

            sql = sql.Replace("{BranchNo}", item.BranchNo.ToString());
            sql = sql.Replace("{InsertName}", item.InsertName.ToString().InsertInMarks());
            sql = sql.Replace("{InsertDate}", item.InsertDate.ToString(Const_Format_datetime).InsertInMarks());
            sql = sql.Replace("{UpdateName}", item.UpdateName.ToString().InsertInMarks());
            sql = sql.Replace("{UpdateDate}", item.UpdateDate.ToString(Const_Format_datetime).InsertInMarks());
            if (item.ContextMenuCategoryIndex == null)
                sql = sql.Replace("{ContextMenuCategoryIndex}", "null");
            else
                sql = sql.Replace("{ContextMenuCategoryIndex}", item.ContextMenuCategoryIndex.ToString());

            sql = sql.Replace("{IsRPCEnabled}", item.IsRPCEnabled.ToString());
            if (item.AttachedFromACClassID == null)
                sql = sql.Replace("{AttachedFromACClassID}", "null");
            else
                sql = sql.Replace("{AttachedFromACClassID}", item.AttachedFromACClassID.ToString().InsertInMarks());

            sql = sql.Replace("{IsStatic}", item.IsStatic.ToString());
            sql = sql.Replace("{ExecuteByDoubleClick}", item.ExecuteByDoubleClick.ToString());

            return sql;
        }
        // ACClassProperty
        private string GetInsertSQLACClassProperty(ACClassProperty item)
        {
            string sql = SQLInsert_ACClassProperty;
            sql = sql.Replace("{ACClassPropertyID}", item.ACClassPropertyID.ToString().InsertInMarks());
            sql = sql.Replace("{ACClassID}", item.ACClassID.ToString().InsertInMarks());
            sql = sql.Replace("{ACIdentifier}", item.ACIdentifier.ToString().InsertInMarks());
            if (item.ACIdentifierKey == null)
                sql = sql.Replace("{ACIdentifierKey}", "null");
            else
                sql = sql.Replace("{ACIdentifierKey}", item.ACIdentifierKey.ToString());

            if (string.IsNullOrEmpty(item.ACCaptionTranslation))
                sql = sql.Replace("{ACCaptionTranslation}", "null");
            else
                sql = sql.Replace("{ACCaptionTranslation}", item.ACCaptionTranslation.ToString().InsertInMarks());

            if (string.IsNullOrEmpty(item.ACGroup))
                sql = sql.Replace("{ACGroup}", "null");
            else
                sql = sql.Replace("{ACGroup}", item.ACGroup.ToString().InsertInMarks());

            sql = sql.Replace("{BasedOnACClassPropertyID}", item.BasedOnACClassPropertyID.ToString().InsertInMarks());
            sql = sql.Replace("{ACKindIndex}", item.ACKindIndex.ToString());
            sql = sql.Replace("{SortIndex}", item.SortIndex.ToString());
            sql = sql.Replace("{IsRightmanagement}", item.IsRightmanagement.ToString());
            if (string.IsNullOrEmpty(item.ACSource))
                sql = sql.Replace("{ACSource}", "null");
            else
                sql = sql.Replace("{ACSource}", item.ACSource.ToString().InsertInMarks());

            if (string.IsNullOrEmpty(item.Comment))
                sql = sql.Replace("{Comment}", "null");
            else
                sql = sql.Replace("{Comment}", item.Comment.ToString().InsertInMarks());

            sql = sql.Replace("{IsInteraction}", item.IsInteraction.ToString());
            sql = sql.Replace("{ValueTypeACClassID}", item.ValueTypeACClassID.ToString().InsertInMarks());
            if (string.IsNullOrEmpty(item.GenericType))
                sql = sql.Replace("{GenericType}", "null");
            else
                sql = sql.Replace("{GenericType}", item.GenericType.ToString().InsertInMarks());

            if (item.ConfigACClassID == null)
                sql = sql.Replace("{ConfigACClassID}", "null");
            else
                sql = sql.Replace("{ConfigACClassID}", item.ConfigACClassID.ToString().InsertInMarks());

            sql = sql.Replace("{ACPropUsageIndex}", item.ACPropUsageIndex.ToString());
            sql = sql.Replace("{DeleteActionIndex}", item.DeleteActionIndex.ToString());
            sql = sql.Replace("{IsBroadcast}", item.IsBroadcast.ToString());
            sql = sql.Replace("{ForceBroadcast}", item.ForceBroadcast.ToString());
            sql = sql.Replace("{IsProxyProperty}", item.IsProxyProperty.ToString());
            sql = sql.Replace("{IsInput}", item.IsInput.ToString());
            sql = sql.Replace("{IsOutput}", item.IsOutput.ToString());
            sql = sql.Replace("{IsContent}", item.IsContent.ToString());
            sql = sql.Replace("{IsPersistable}", item.IsPersistable.ToString());
            sql = sql.Replace("{IsSerializable}", item.IsSerializable.ToString());
            sql = sql.Replace("{IsEnumerable}", item.IsEnumerable.ToString());
            sql = sql.Replace("{ACPointCapacity}", item.ACPointCapacity.ToString());
            if (string.IsNullOrEmpty(item.CallbackMethodName))
                sql = sql.Replace("{CallbackMethodName}", "null");
            else
                sql = sql.Replace("{CallbackMethodName}", item.CallbackMethodName.ToString().InsertInMarks());

            if (item.ParentACClassPropertyID == null)
                sql = sql.Replace("{ParentACClassPropertyID}", "null");
            else
                sql = sql.Replace("{ParentACClassPropertyID}", item.ParentACClassPropertyID.ToString().InsertInMarks());

            sql = sql.Replace("{DataTypeLength}", item.DataTypeLength.ToString());
            sql = sql.Replace("{IsNullable}", item.IsNullable.ToString());
            if (string.IsNullOrEmpty(item.InputMask))
                sql = sql.Replace("{InputMask}", "null");
            else
                sql = sql.Replace("{InputMask}", item.InputMask.ToString().InsertInMarks());

            if (item.MinLength == null)
                sql = sql.Replace("{MinLength}", "null");
            else
                sql = sql.Replace("{MinLength}", item.MinLength.ToString());

            if (item.MaxLength == null)
                sql = sql.Replace("{MaxLength}", "null");
            else
                sql = sql.Replace("{MaxLength}", item.MaxLength.ToString());

            if (item.MinValue == null)
                sql = sql.Replace("{MinValue}", "null");
            else
                sql = sql.Replace("{MinValue}", item.MinValue.ToString());

            if (item.MaxValue == null)
                sql = sql.Replace("{MaxValue}", "null");
            else
                sql = sql.Replace("{MaxValue}", item.MaxValue.ToString());

            if (string.IsNullOrEmpty(item.XMLValue))
                sql = sql.Replace("{XMLValue}", "null");
            else
                sql = sql.Replace("{XMLValue}", item.XMLValue.ToString().InsertInMarks());

            sql = sql.Replace("{LogRefreshRateIndex}", item.LogRefreshRateIndex.ToString());
            if (item.LogFilter == null)
                sql = sql.Replace("{LogFilter}", "null");
            else
                sql = sql.Replace("{LogFilter}", item.LogFilter.ToString());

            if (item.Precision == null)
                sql = sql.Replace("{Precision}", "null");
            else
                sql = sql.Replace("{Precision}", item.Precision.ToString());

            if (string.IsNullOrEmpty(item.XMLACEventArgs))
                sql = sql.Replace("{XMLACEventArgs}", "null");
            else
                sql = sql.Replace("{XMLACEventArgs}", item.XMLACEventArgs.ToString().InsertInMarks());

            if (string.IsNullOrEmpty(item.XMLConfig))
                sql = sql.Replace("{XMLConfig}", "null");
            else
                sql = sql.Replace("{XMLConfig}", item.XMLConfig.ToString().InsertInMarks());

            sql = sql.Replace("{BranchNo}", item.BranchNo.ToString());
            sql = sql.Replace("{InsertName}", item.InsertName.ToString().InsertInMarks());
            sql = sql.Replace("{InsertDate}", item.InsertDate.ToString(Const_Format_datetime).InsertInMarks());
            sql = sql.Replace("{UpdateName}", item.UpdateName.ToString().InsertInMarks());
            sql = sql.Replace("{UpdateDate}", item.UpdateDate.ToString(Const_Format_datetime).InsertInMarks());
            sql = sql.Replace("{IsRPCEnabled}", item.IsRPCEnabled.ToString());
            sql = sql.Replace("{RemotePropID}", item.RemotePropID.ToString());
            if (item.ChangeLogMax == null)
                sql = sql.Replace("{ChangeLogMax}", "null");
            else
                sql = sql.Replace("{ChangeLogMax}", item.ChangeLogMax.ToString());

            sql = sql.Replace("{LogBufferSize}", item.LogBufferSize.ToString());
            sql = sql.Replace("{IsStatic}", item.IsStatic.ToString());

            return sql;
        }
        // ACPackage
        private string GetInsertSQLACPackage(ACPackage item)
        {
            string sql = SQLInsert_ACPackage;
            sql = sql.Replace("{ACPackageID}", item.ACPackageID.ToString().InsertInMarks());
            sql = sql.Replace("{ACPackageName}", item.ACPackageName.ToString().InsertInMarks());
            if (string.IsNullOrEmpty(item.Comment))
                sql = sql.Replace("{Comment}", "null");
            else
                sql = sql.Replace("{Comment}", item.Comment.ToString().InsertInMarks());

            sql = sql.Replace("{BranchNo}", item.BranchNo.ToString());
            sql = sql.Replace("{InsertName}", item.InsertName.ToString().InsertInMarks());
            sql = sql.Replace("{InsertDate}", item.InsertDate.ToString(Const_Format_datetime).InsertInMarks());
            sql = sql.Replace("{UpdateName}", item.UpdateName.ToString().InsertInMarks());
            sql = sql.Replace("{UpdateDate}", item.UpdateDate.ToString(Const_Format_datetime).InsertInMarks());

            return sql;
        }
        // ACProject
        private string GetInsertSQLACProject(ACProject item)
        {
            string sql = SQLInsert_ACProject;
            sql = sql.Replace("{ACProjectID}", item.ACProjectID.ToString().InsertInMarks());
            sql = sql.Replace("{ACProjectNo}", item.ACProjectNo.ToString().InsertInMarks());
            sql = sql.Replace("{ACProjectName}", item.ACProjectName.ToString().InsertInMarks());
            sql = sql.Replace("{ACProjectTypeIndex}", item.ACProjectTypeIndex.ToString());
            if (item.BasedOnACProjectID == null)
                sql = sql.Replace("{BasedOnACProjectID}", "null");
            else
                sql = sql.Replace("{BasedOnACProjectID}", item.BasedOnACProjectID.ToString().InsertInMarks());

            if (item.PAAppClassAssignmentACClassID == null)
                sql = sql.Replace("{PAAppClassAssignmentACClassID}", "null");
            else
                sql = sql.Replace("{PAAppClassAssignmentACClassID}", item.PAAppClassAssignmentACClassID.ToString().InsertInMarks());

            sql = sql.Replace("{IsEnabled}", item.IsEnabled.ToString());
            sql = sql.Replace("{IsGlobal}", item.IsGlobal.ToString());
            sql = sql.Replace("{IsWorkflowEnabled}", item.IsWorkflowEnabled.ToString());
            sql = sql.Replace("{IsControlCenterEnabled}", item.IsControlCenterEnabled.ToString());
            sql = sql.Replace("{IsVisualisationEnabled}", item.IsVisualisationEnabled.ToString());
            sql = sql.Replace("{IsProduction}", item.IsProduction.ToString());
            sql = sql.Replace("{IsDataAccess}", item.IsDataAccess.ToString());
            if (string.IsNullOrEmpty(item.Comment))
                sql = sql.Replace("{Comment}", "null");
            else
                sql = sql.Replace("{Comment}", item.Comment.ToString().InsertInMarks());

            if (string.IsNullOrEmpty(item.XMLConfig))
                sql = sql.Replace("{XMLConfig}", "null");
            else
                sql = sql.Replace("{XMLConfig}", item.XMLConfig.ToString().InsertInMarks());

            sql = sql.Replace("{InsertName}", item.InsertName.ToString().InsertInMarks());
            sql = sql.Replace("{InsertDate}", item.InsertDate.ToString(Const_Format_datetime).InsertInMarks());
            sql = sql.Replace("{UpdateName}", item.UpdateName.ToString().InsertInMarks());
            sql = sql.Replace("{UpdateDate}", item.UpdateDate.ToString(Const_Format_datetime).InsertInMarks());

            return sql;
        }



        #endregion

        #endregion

        #region Templates
        public const string SQLInsert_ACClass = "insert into [dbo].[ACClass](ACClassID, ACProjectID, BasedOnACClassID, ParentACClassID, ACIdentifier, ACIdentifierKey, ACCaptionTranslation, ACKindIndex, SortIndex, ACPackageID, AssemblyQualifiedName, PWACClassID, PWMethodACClassID, Comment, IsAutostart, IsAbstract, ACStartTypeIndex, ACStorableTypeIndex, IsAssembly, IsMultiInstance, IsRightmanagement, ACSortColumns, ACFilterColumns, XMLConfig, XMLACClass, BranchNo, InsertName, InsertDate, UpdateName, UpdateDate, ChangeLogMax, ACURLCached, ACURLComponentCached, IsStatic) values({ACClassID}, {ACProjectID}, {BasedOnACClassID}, {ParentACClassID}, {ACIdentifier}, {ACIdentifierKey}, {ACCaptionTranslation}, {ACKindIndex}, {SortIndex}, {ACPackageID}, {AssemblyQualifiedName}, {PWACClassID}, {PWMethodACClassID}, {Comment}, {IsAutostart}, {IsAbstract}, {ACStartTypeIndex}, {ACStorableTypeIndex}, {IsAssembly}, {IsMultiInstance}, {IsRightmanagement}, {ACSortColumns}, {ACFilterColumns}, {XMLConfig}, {XMLACClass}, {BranchNo}, {InsertName}, {InsertDate}, {UpdateName}, {UpdateDate}, {ChangeLogMax}, {ACURLCached}, {ACURLComponentCached}, {IsStatic})";
        public const string SQLInsert_ACClassDesign = "insert into [dbo].[ACClassDesign](ACClassDesignID, ACClassID, ACIdentifier, ACIdentifierKey, ACCaptionTranslation, ACGroup, XMLDesign, DesignBinary, DesignNo, ValueTypeACClassID, ACKindIndex, ACUsageIndex, SortIndex, IsRightmanagement, Comment, IsDefault, IsResourceStyle, VisualHeight, VisualWidth, XMLConfig, BranchNo, DesignerMaxRecursion, InsertName, InsertDate, UpdateName, UpdateDate, BAMLDesign, BAMLDate) values({ACClassDesignID}, {ACClassID}, {ACIdentifier}, {ACIdentifierKey}, {ACCaptionTranslation}, {ACGroup}, {XMLDesign}, {DesignBinary}, {DesignNo}, {ValueTypeACClassID}, {ACKindIndex}, {ACUsageIndex}, {SortIndex}, {IsRightmanagement}, {Comment}, {IsDefault}, {IsResourceStyle}, {VisualHeight}, {VisualWidth}, {XMLConfig}, {BranchNo}, {DesignerMaxRecursion}, {InsertName}, {InsertDate}, {UpdateName}, {UpdateDate}, {BAMLDesign}, {BAMLDate})";
        public const string SQLInsert_ACClassMethod = "insert into [dbo].[ACClassMethod](ACClassMethodID, ACClassID, ACIdentifier, ACIdentifierKey, ACCaptionTranslation, ACGroup, Sourcecode, ACKindIndex, SortIndex, IsRightmanagement, Comment, IsCommand, IsInteraction, IsAsyncProcess, IsPeriodic, IsParameterACMethod, IsSubMethod, InteractionVBContent, IsAutoenabled, IsPersistable, PWACClassID, ContinueByError, ValueTypeACClassID, GenericType, ParentACClassMethodID, XMLACMethod, XMLDesign, XMLConfig, BranchNo, InsertName, InsertDate, UpdateName, UpdateDate, ContextMenuCategoryIndex, IsRPCEnabled, AttachedFromACClassID, IsStatic, ExecuteByDoubleClick) values({ACClassMethodID}, {ACClassID}, {ACIdentifier}, {ACIdentifierKey}, {ACCaptionTranslation}, {ACGroup}, {Sourcecode}, {ACKindIndex}, {SortIndex}, {IsRightmanagement}, {Comment}, {IsCommand}, {IsInteraction}, {IsAsyncProcess}, {IsPeriodic}, {IsParameterACMethod}, {IsSubMethod}, {InteractionVBContent}, {IsAutoenabled}, {IsPersistable}, {PWACClassID}, {ContinueByError}, {ValueTypeACClassID}, {GenericType}, {ParentACClassMethodID}, {XMLACMethod}, {XMLDesign}, {XMLConfig}, {BranchNo}, {InsertName}, {InsertDate}, {UpdateName}, {UpdateDate}, {ContextMenuCategoryIndex}, {IsRPCEnabled}, {AttachedFromACClassID}, {IsStatic}, {ExecuteByDoubleClick})";
        public const string SQLInsert_ACClassProperty = "insert into [dbo].[ACClassProperty](ACClassPropertyID, ACClassID, ACIdentifier, ACIdentifierKey, ACCaptionTranslation, ACGroup, BasedOnACClassPropertyID, ACKindIndex, SortIndex, IsRightmanagement, ACSource, Comment, IsInteraction, ValueTypeACClassID, GenericType, ConfigACClassID, ACPropUsageIndex, DeleteActionIndex, IsBroadcast, ForceBroadcast, IsProxyProperty, IsInput, IsOutput, IsContent, IsPersistable, IsSerializable, IsEnumerable, ACPointCapacity, CallbackMethodName, ParentACClassPropertyID, DataTypeLength, IsNullable, InputMask, MinLength, MaxLength, MinValue, MaxValue, XMLValue, LogRefreshRateIndex, LogFilter, Precision, XMLACEventArgs, XMLConfig, BranchNo, InsertName, InsertDate, UpdateName, UpdateDate, IsRPCEnabled, RemotePropID, ChangeLogMax, LogBufferSize, IsStatic) values({ACClassPropertyID}, {ACClassID}, {ACIdentifier}, {ACIdentifierKey}, {ACCaptionTranslation}, {ACGroup}, {BasedOnACClassPropertyID}, {ACKindIndex}, {SortIndex}, {IsRightmanagement}, {ACSource}, {Comment}, {IsInteraction}, {ValueTypeACClassID}, {GenericType}, {ConfigACClassID}, {ACPropUsageIndex}, {DeleteActionIndex}, {IsBroadcast}, {ForceBroadcast}, {IsProxyProperty}, {IsInput}, {IsOutput}, {IsContent}, {IsPersistable}, {IsSerializable}, {IsEnumerable}, {ACPointCapacity}, {CallbackMethodName}, {ParentACClassPropertyID}, {DataTypeLength}, {IsNullable}, {InputMask}, {MinLength}, {MaxLength}, {MinValue}, {MaxValue}, {XMLValue}, {LogRefreshRateIndex}, {LogFilter}, {Precision}, {XMLACEventArgs}, {XMLConfig}, {BranchNo}, {InsertName}, {InsertDate}, {UpdateName}, {UpdateDate}, {IsRPCEnabled}, {RemotePropID}, {ChangeLogMax}, {LogBufferSize}, {IsStatic})";
        public const string SQLInsert_ACPackage = "insert into [dbo].[ACPackage](ACPackageID, ACPackageName, Comment, BranchNo, InsertName, InsertDate, UpdateName, UpdateDate) values({ACPackageID}, {ACPackageName}, {Comment}, {BranchNo}, {InsertName}, {InsertDate}, {UpdateName}, {UpdateDate})";
        public const string SQLInsert_ACProject = "insert into [dbo].[ACProject](ACProjectID, ACProjectNo, ACProjectName, ACProjectTypeIndex, BasedOnACProjectID, PAAppClassAssignmentACClassID, IsEnabled, IsGlobal, IsWorkflowEnabled, IsControlCenterEnabled, IsVisualisationEnabled, IsProduction, IsDataAccess, Comment, XMLConfig, InsertName, InsertDate, UpdateName, UpdateDate) values({ACProjectID}, {ACProjectNo}, {ACProjectName}, {ACProjectTypeIndex}, {BasedOnACProjectID}, {PAAppClassAssignmentACClassID}, {IsEnabled}, {IsGlobal}, {IsWorkflowEnabled}, {IsControlCenterEnabled}, {IsVisualisationEnabled}, {IsProduction}, {IsDataAccess}, {Comment}, {XMLConfig}, {InsertName}, {InsertDate}, {UpdateName}, {UpdateDate})";
        public const string SQLInsert_sysdiagrams = "insert into [dbo].[sysdiagrams](name, principal_id, diagram_id, version, definition) values({name}, {principal_id}, {diagram_id}, {version}, {definition})";

        #endregion
    }
}
