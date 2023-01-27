﻿// <auto-generated />
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Metadata;
using gip.core.datamodel;

#pragma warning disable 219, 612, 618
#nullable disable

namespace iPlusV5CompiledModels
{
    internal partial class ACProjectEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "gip.core.datamodel.ACProject",
                typeof(ACProject),
                baseEntityType,
                indexerPropertyInfo: RuntimeEntityType.FindIndexerProperty(typeof(ACProject)));

            var aCProjectID = runtimeEntityType.AddProperty(
                "ACProjectID",
                typeof(Guid),
                propertyInfo: typeof(ACProject).GetProperty("ACProjectID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_ACProjectID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw);
            aCProjectID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCProjectName = runtimeEntityType.AddProperty(
                "ACProjectName",
                typeof(string),
                propertyInfo: typeof(ACProject).GetProperty("ACProjectName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_ACProjectName", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                maxLength: 50,
                unicode: false);
            aCProjectName.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCProjectNo = runtimeEntityType.AddProperty(
                "ACProjectNo",
                typeof(string),
                propertyInfo: typeof(ACProject).GetProperty("ACProjectNo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_ACProjectNo", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                maxLength: 20,
                unicode: false);
            aCProjectNo.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCProjectTypeIndex = runtimeEntityType.AddProperty(
                "ACProjectTypeIndex",
                typeof(short),
                propertyInfo: typeof(ACProject).GetProperty("ACProjectTypeIndex", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_ACProjectTypeIndex", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            aCProjectTypeIndex.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var basedOnACProjectID = runtimeEntityType.AddProperty(
                "BasedOnACProjectID",
                typeof(Guid?),
                propertyInfo: typeof(ACProject).GetProperty("BasedOnACProjectID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_BasedOnACProjectID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            basedOnACProjectID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var comment = runtimeEntityType.AddProperty(
                "Comment",
                typeof(string),
                propertyInfo: typeof(ACProject).GetProperty("Comment", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_Comment", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true,
                unicode: false);
            comment.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var insertDate = runtimeEntityType.AddProperty(
                "InsertDate",
                typeof(DateTime),
                propertyInfo: typeof(ACProject).GetProperty("InsertDate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_InsertDate", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            insertDate.AddAnnotation("Relational:ColumnType", "datetime");
            insertDate.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var insertName = runtimeEntityType.AddProperty(
                "InsertName",
                typeof(string),
                propertyInfo: typeof(ACProject).GetProperty("InsertName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_InsertName", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                maxLength: 20,
                unicode: false);
            insertName.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var isControlCenterEnabled = runtimeEntityType.AddProperty(
                "IsControlCenterEnabled",
                typeof(bool),
                propertyInfo: typeof(ACProject).GetProperty("IsControlCenterEnabled", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_IsControlCenterEnabled", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            isControlCenterEnabled.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var isDataAccess = runtimeEntityType.AddProperty(
                "IsDataAccess",
                typeof(bool),
                propertyInfo: typeof(ACProject).GetProperty("IsDataAccess", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_IsDataAccess", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            isDataAccess.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var isEnabled = runtimeEntityType.AddProperty(
                "IsEnabled",
                typeof(bool),
                propertyInfo: typeof(ACProject).GetProperty("IsEnabled", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_IsEnabled", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            isEnabled.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var isGlobal = runtimeEntityType.AddProperty(
                "IsGlobal",
                typeof(bool),
                propertyInfo: typeof(ACProject).GetProperty("IsGlobal", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_IsGlobal", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            isGlobal.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var isProduction = runtimeEntityType.AddProperty(
                "IsProduction",
                typeof(bool),
                propertyInfo: typeof(ACProject).GetProperty("IsProduction", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_IsProduction", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            isProduction.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var isVisualisationEnabled = runtimeEntityType.AddProperty(
                "IsVisualisationEnabled",
                typeof(bool),
                propertyInfo: typeof(ACProject).GetProperty("IsVisualisationEnabled", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_IsVisualisationEnabled", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            isVisualisationEnabled.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var isWorkflowEnabled = runtimeEntityType.AddProperty(
                "IsWorkflowEnabled",
                typeof(bool),
                propertyInfo: typeof(ACProject).GetProperty("IsWorkflowEnabled", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_IsWorkflowEnabled", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            isWorkflowEnabled.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var pAAppClassAssignmentACClassID = runtimeEntityType.AddProperty(
                "PAAppClassAssignmentACClassID",
                typeof(Guid?),
                propertyInfo: typeof(ACProject).GetProperty("PAAppClassAssignmentACClassID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_PAAppClassAssignmentACClassID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            pAAppClassAssignmentACClassID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var updateDate = runtimeEntityType.AddProperty(
                "UpdateDate",
                typeof(DateTime),
                propertyInfo: typeof(ACProject).GetProperty("UpdateDate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_UpdateDate", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            updateDate.AddAnnotation("Relational:ColumnType", "datetime");
            updateDate.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var updateName = runtimeEntityType.AddProperty(
                "UpdateName",
                typeof(string),
                propertyInfo: typeof(ACProject).GetProperty("UpdateName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_UpdateName", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                maxLength: 20,
                unicode: false);
            updateName.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var xMLConfig = runtimeEntityType.AddProperty(
                "XMLConfig",
                typeof(string),
                propertyInfo: typeof(ACProject).GetProperty("XMLConfig", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("_XMLConfig", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            xMLConfig.AddAnnotation("Relational:ColumnType", "text");
            xMLConfig.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var database = runtimeEntityType.AddServiceProperty(
                "Database",
                propertyInfo: typeof(ACProject).GetProperty("Database", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var key = runtimeEntityType.AddKey(
                new[] { aCProjectID });
            runtimeEntityType.SetPrimaryKey(key);

            var nCIFKACProjectBasedOnACProjectID = runtimeEntityType.AddIndex(
                new[] { basedOnACProjectID },
                name: "NCI_FK_ACProject_BasedOnACProjectID");

            var nCIFKACProjectPAAppClassAssignmentACClassID = runtimeEntityType.AddIndex(
                new[] { pAAppClassAssignmentACClassID },
                name: "NCI_FK_ACProject_PAAppClassAssignmentACClassID");

            var uIXACProjectACProjectNo = runtimeEntityType.AddIndex(
                new[] { aCProjectNo },
                name: "UIX_ACProject_ACProjectNo",
                unique: true);

            return runtimeEntityType;
        }

        public static RuntimeForeignKey CreateForeignKey1(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("BasedOnACProjectID") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("ACProjectID") }),
                principalEntityType);

            var aCProject1BasedOnACProject = declaringEntityType.AddNavigation("ACProject1_BasedOnACProject",
                runtimeForeignKey,
                onDependent: true,
                typeof(ACProject),
                propertyInfo: typeof(ACProject).GetProperty("ACProject1_BasedOnACProject", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("<ACProject1_BasedOnACProject>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var aCProjectBasedOnACProject = principalEntityType.AddNavigation("ACProject_BasedOnACProject",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<ACProject>),
                propertyInfo: typeof(ACProject).GetProperty("ACProject_BasedOnACProject", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("<ACProject_BasedOnACProject>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_ACProject_BasedOnACProjectID");
            return runtimeForeignKey;
        }

        public static RuntimeForeignKey CreateForeignKey2(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("PAAppClassAssignmentACClassID") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("ACClassID") }),
                principalEntityType);

            var pAAppClassAssignmentACClass = declaringEntityType.AddNavigation("PAAppClassAssignmentACClass",
                runtimeForeignKey,
                onDependent: true,
                typeof(ACClass),
                propertyInfo: typeof(ACProject).GetProperty("PAAppClassAssignmentACClass", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACProject).GetField("<PAAppClassAssignmentACClass>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var aCProjectPAAppClassAssignmentACClass = principalEntityType.AddNavigation("ACProject_PAAppClassAssignmentACClass",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<ACProject>),
                propertyInfo: typeof(ACClass).GetProperty("ACProject_PAAppClassAssignmentACClass", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClass).GetField("<ACProject_PAAppClassAssignmentACClass>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_ACProject_PAAppClassAssignmentACClassID");
            return runtimeForeignKey;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "ACProject");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}