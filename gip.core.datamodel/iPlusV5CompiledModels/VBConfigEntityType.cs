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
    internal partial class VBConfigEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "gip.core.datamodel.VBConfig",
                typeof(VBConfig),
                baseEntityType,
                indexerPropertyInfo: RuntimeEntityType.FindIndexerProperty(typeof(VBConfig)));

            var vBConfigID = runtimeEntityType.AddProperty(
                "VBConfigID",
                typeof(Guid),
                propertyInfo: typeof(VBConfig).GetProperty("VBConfigID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("_VBConfigID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw);
            vBConfigID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCClassID = runtimeEntityType.AddProperty(
                "ACClassID",
                typeof(Guid?),
                propertyInfo: typeof(VBConfig).GetProperty("ACClassID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("_ACClassID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            aCClassID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCClassPropertyRelationID = runtimeEntityType.AddProperty(
                "ACClassPropertyRelationID",
                typeof(Guid?),
                propertyInfo: typeof(VBConfig).GetProperty("ACClassPropertyRelationID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("_ACClassPropertyRelationID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            aCClassPropertyRelationID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var comment = runtimeEntityType.AddProperty(
                "Comment",
                typeof(string),
                propertyInfo: typeof(VBConfig).GetProperty("Comment", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("_Comment", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true,
                unicode: false);
            comment.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var expression = runtimeEntityType.AddProperty(
                "Expression",
                typeof(string),
                propertyInfo: typeof(VBConfig).GetProperty("Expression", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("_Expression", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            expression.AddAnnotation("Relational:ColumnType", "text");
            expression.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var insertDate = runtimeEntityType.AddProperty(
                "InsertDate",
                typeof(DateTime),
                propertyInfo: typeof(VBConfig).GetProperty("InsertDate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("_InsertDate", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            insertDate.AddAnnotation("Relational:ColumnType", "datetime");
            insertDate.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var insertName = runtimeEntityType.AddProperty(
                "InsertName",
                typeof(string),
                propertyInfo: typeof(VBConfig).GetProperty("InsertName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("_InsertName", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                maxLength: 20,
                unicode: false);
            insertName.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var keyACUrl = runtimeEntityType.AddProperty(
                "KeyACUrl",
                typeof(string),
                propertyInfo: typeof(VBConfig).GetProperty("KeyACUrl", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("_KeyACUrl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true,
                unicode: false);
            keyACUrl.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var localConfigACUrl = runtimeEntityType.AddProperty(
                "LocalConfigACUrl",
                typeof(string),
                propertyInfo: typeof(VBConfig).GetProperty("LocalConfigACUrl", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("_LocalConfigACUrl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true,
                unicode: false);
            localConfigACUrl.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var parentVBConfigID = runtimeEntityType.AddProperty(
                "ParentVBConfigID",
                typeof(Guid?),
                propertyInfo: typeof(VBConfig).GetProperty("ParentVBConfigID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("_ParentVBConfigID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            parentVBConfigID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var preConfigACUrl = runtimeEntityType.AddProperty(
                "PreConfigACUrl",
                typeof(string),
                propertyInfo: typeof(VBConfig).GetProperty("PreConfigACUrl", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("_PreConfigACUrl", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true,
                unicode: false);
            preConfigACUrl.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var updateDate = runtimeEntityType.AddProperty(
                "UpdateDate",
                typeof(DateTime),
                propertyInfo: typeof(VBConfig).GetProperty("UpdateDate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("_UpdateDate", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            updateDate.AddAnnotation("Relational:ColumnType", "datetime");
            updateDate.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var updateName = runtimeEntityType.AddProperty(
                "UpdateName",
                typeof(string),
                propertyInfo: typeof(VBConfig).GetProperty("UpdateName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("_UpdateName", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                maxLength: 20,
                unicode: false);
            updateName.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var vBiACClassID = runtimeEntityType.AddProperty(
                "VBiACClassID",
                typeof(Guid?),
                propertyInfo: typeof(VBConfig).GetProperty("VBiACClassID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("<VBiACClassID>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            vBiACClassID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var valueTypeACClassID = runtimeEntityType.AddProperty(
                "ValueTypeACClassID",
                typeof(Guid),
                propertyInfo: typeof(VBConfig).GetProperty("ValueTypeACClassID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("_ValueTypeACClassID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            valueTypeACClassID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var xMLConfig = runtimeEntityType.AddProperty(
                "XMLConfig",
                typeof(string),
                propertyInfo: typeof(VBConfig).GetProperty("XMLConfig", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("_XMLConfig", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            xMLConfig.AddAnnotation("Relational:ColumnType", "text");
            xMLConfig.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var key = runtimeEntityType.AddKey(
                new[] { vBConfigID });
            runtimeEntityType.SetPrimaryKey(key);

            var nCIFKVBConfigACClassID = runtimeEntityType.AddIndex(
                new[] { aCClassID },
                name: "NCI_FK_VBConfig_ACClassID");

            var nCIFKVBConfigACClassPropertyRelationID = runtimeEntityType.AddIndex(
                new[] { aCClassPropertyRelationID },
                name: "NCI_FK_VBConfig_ACClassPropertyRelationID");

            var nCIFKVBConfigParentVBConfigID = runtimeEntityType.AddIndex(
                new[] { parentVBConfigID },
                name: "NCI_FK_VBConfig_ParentVBConfigID");

            var nCIFKVBConfigValueTypeACClassID = runtimeEntityType.AddIndex(
                new[] { valueTypeACClassID },
                name: "NCI_FK_VBConfig_ValueTypeACClassID");

            return runtimeEntityType;
        }

        public static RuntimeForeignKey CreateForeignKey1(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("ACClassID") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("ACClassID") }),
                principalEntityType);

            var aCClass = declaringEntityType.AddNavigation("ACClass",
                runtimeForeignKey,
                onDependent: true,
                typeof(ACClass),
                propertyInfo: typeof(VBConfig).GetProperty("ACClass", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("<ACClass>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var vBConfigACClass = principalEntityType.AddNavigation("VBConfig_ACClass",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<VBConfig>),
                propertyInfo: typeof(ACClass).GetProperty("VBConfig_ACClass", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClass).GetField("<VBConfig_ACClass>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_VBConfig_ACClassID");
            return runtimeForeignKey;
        }

        public static RuntimeForeignKey CreateForeignKey2(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("ACClassPropertyRelationID") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("ACClassPropertyRelationID") }),
                principalEntityType);

            var aCClassPropertyRelation = declaringEntityType.AddNavigation("ACClassPropertyRelation",
                runtimeForeignKey,
                onDependent: true,
                typeof(ACClassPropertyRelation),
                propertyInfo: typeof(VBConfig).GetProperty("ACClassPropertyRelation", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("<ACClassPropertyRelation>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var vBConfigACClassPropertyRelation = principalEntityType.AddNavigation("VBConfig_ACClassPropertyRelation",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<VBConfig>),
                propertyInfo: typeof(ACClassPropertyRelation).GetProperty("VBConfig_ACClassPropertyRelation", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassPropertyRelation).GetField("<VBConfig_ACClassPropertyRelation>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_VBConfig_ACClassPropertyRelationID");
            return runtimeForeignKey;
        }

        public static RuntimeForeignKey CreateForeignKey3(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("ParentVBConfigID") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("VBConfigID") }),
                principalEntityType);

            var vBConfig1ParentVBConfig = declaringEntityType.AddNavigation("VBConfig1_ParentVBConfig",
                runtimeForeignKey,
                onDependent: true,
                typeof(VBConfig),
                propertyInfo: typeof(VBConfig).GetProperty("VBConfig1_ParentVBConfig", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("<VBConfig1_ParentVBConfig>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var vBConfigParentVBConfig = principalEntityType.AddNavigation("VBConfig_ParentVBConfig",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<VBConfig>),
                propertyInfo: typeof(VBConfig).GetProperty("VBConfig_ParentVBConfig", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("<VBConfig_ParentVBConfig>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_VBConfig_ParentVBConfigID");
            return runtimeForeignKey;
        }

        public static RuntimeForeignKey CreateForeignKey4(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("ValueTypeACClassID") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("ACClassID") }),
                principalEntityType,
                required: true);

            var valueTypeACClass = declaringEntityType.AddNavigation("ValueTypeACClass",
                runtimeForeignKey,
                onDependent: true,
                typeof(ACClass),
                propertyInfo: typeof(VBConfig).GetProperty("ValueTypeACClass", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBConfig).GetField("<ValueTypeACClass>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var vBConfigValueTypeACClass = principalEntityType.AddNavigation("VBConfig_ValueTypeACClass",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<VBConfig>),
                propertyInfo: typeof(ACClass).GetProperty("VBConfig_ValueTypeACClass", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClass).GetField("<VBConfig_ValueTypeACClass>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_VBConfig_ValueTypeACClassID");
            return runtimeForeignKey;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "VBConfig");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
