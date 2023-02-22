﻿// <auto-generated />
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using gip.core.datamodel;

#pragma warning disable 219, 612, 618
#nullable disable

namespace iPlusV5CompiledModels
{
    internal partial class ACClassDesignEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "gip.core.datamodel.ACClassDesign",
                typeof(ACClassDesign),
                baseEntityType,
                indexerPropertyInfo: RuntimeEntityType.FindIndexerProperty(typeof(ACClassDesign)));

            var aCClassDesignID = runtimeEntityType.AddProperty(
                "ACClassDesignID",
                typeof(Guid),
                propertyInfo: typeof(ACClassDesign).GetProperty("ACClassDesignID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_ACClassDesignID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw);
            aCClassDesignID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCCaptionTranslation = runtimeEntityType.AddProperty(
                "ACCaptionTranslation",
                typeof(string),
                propertyInfo: typeof(ACClassDesign).GetProperty("ACCaptionTranslation", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_ACCaptionTranslation", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true,
                unicode: false);
            aCCaptionTranslation.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCClassID = runtimeEntityType.AddProperty(
                "ACClassID",
                typeof(Guid),
                propertyInfo: typeof(ACClassDesign).GetProperty("ACClassID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_ACClassID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            aCClassID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCGroup = runtimeEntityType.AddProperty(
                "ACGroup",
                typeof(string),
                propertyInfo: typeof(ACClassDesign).GetProperty("ACGroup", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_ACGroup", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true,
                maxLength: 50,
                unicode: false);
            aCGroup.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCIdentifier = runtimeEntityType.AddProperty(
                "ACIdentifier",
                typeof(string),
                propertyInfo: typeof(VBEntityObject).GetProperty("ACIdentifier", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_ACIdentifier", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                maxLength: 80,
                unicode: false);
            aCIdentifier.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCIdentifierKey = runtimeEntityType.AddProperty(
                "ACIdentifierKey",
                typeof(int?),
                propertyInfo: typeof(ACClassDesign).GetProperty("ACIdentifierKey", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_ACIdentifierKey", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            aCIdentifierKey.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCKindIndex = runtimeEntityType.AddProperty(
                "ACKindIndex",
                typeof(short),
                propertyInfo: typeof(ACClassDesign).GetProperty("ACKindIndex", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_ACKindIndex", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            aCKindIndex.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCUsageIndex = runtimeEntityType.AddProperty(
                "ACUsageIndex",
                typeof(short),
                propertyInfo: typeof(ACClassDesign).GetProperty("ACUsageIndex", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_ACUsageIndex", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            aCUsageIndex.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var bAMLDate = runtimeEntityType.AddProperty(
                "BAMLDate",
                typeof(DateTime?),
                propertyInfo: typeof(ACClassDesign).GetProperty("BAMLDate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_BAMLDate", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            bAMLDate.AddAnnotation("Relational:ColumnType", "datetime");
            bAMLDate.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var bAMLDesign = runtimeEntityType.AddProperty(
                "BAMLDesign",
                typeof(byte[]),
                propertyInfo: typeof(ACClassDesign).GetProperty("BAMLDesign", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_BAMLDesign", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            bAMLDesign.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var branchNo = runtimeEntityType.AddProperty(
                "BranchNo",
                typeof(int),
                propertyInfo: typeof(ACClassDesign).GetProperty("BranchNo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_BranchNo", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            branchNo.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var comment = runtimeEntityType.AddProperty(
                "Comment",
                typeof(string),
                propertyInfo: typeof(ACClassDesign).GetProperty("Comment", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_Comment", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true,
                unicode: false);
            comment.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var designBinary = runtimeEntityType.AddProperty(
                "DesignBinary",
                typeof(byte[]),
                propertyInfo: typeof(ACClassDesign).GetProperty("DesignBinary", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_DesignBinary", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            designBinary.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var designNo = runtimeEntityType.AddProperty(
                "DesignNo",
                typeof(string),
                propertyInfo: typeof(ACClassDesign).GetProperty("DesignNo", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_DesignNo", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true,
                maxLength: 20,
                unicode: false);
            designNo.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var designerMaxRecursion = runtimeEntityType.AddProperty(
                "DesignerMaxRecursion",
                typeof(short?),
                propertyInfo: typeof(ACClassDesign).GetProperty("DesignerMaxRecursion", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_DesignerMaxRecursion", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            designerMaxRecursion.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var insertDate = runtimeEntityType.AddProperty(
                "InsertDate",
                typeof(DateTime),
                propertyInfo: typeof(ACClassDesign).GetProperty("InsertDate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_InsertDate", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            insertDate.AddAnnotation("Relational:ColumnType", "datetime");
            insertDate.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var insertName = runtimeEntityType.AddProperty(
                "InsertName",
                typeof(string),
                propertyInfo: typeof(ACClassDesign).GetProperty("InsertName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_InsertName", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                maxLength: 20,
                unicode: false);
            insertName.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var isDefault = runtimeEntityType.AddProperty(
                "IsDefault",
                typeof(bool),
                propertyInfo: typeof(ACClassDesign).GetProperty("IsDefault", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_IsDefault", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            isDefault.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var isResourceStyle = runtimeEntityType.AddProperty(
                "IsResourceStyle",
                typeof(bool),
                propertyInfo: typeof(ACClassDesign).GetProperty("IsResourceStyle", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_IsResourceStyle", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            isResourceStyle.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var isRightmanagement = runtimeEntityType.AddProperty(
                "IsRightmanagement",
                typeof(bool),
                propertyInfo: typeof(ACClassDesign).GetProperty("IsRightmanagement", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_IsRightmanagement", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            isRightmanagement.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var sortIndex = runtimeEntityType.AddProperty(
                "SortIndex",
                typeof(short),
                propertyInfo: typeof(ACClassDesign).GetProperty("SortIndex", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_SortIndex", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            sortIndex.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var updateDate = runtimeEntityType.AddProperty(
                "UpdateDate",
                typeof(DateTime),
                propertyInfo: typeof(ACClassDesign).GetProperty("UpdateDate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_UpdateDate", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            updateDate.AddAnnotation("Relational:ColumnType", "datetime");
            updateDate.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var updateName = runtimeEntityType.AddProperty(
                "UpdateName",
                typeof(string),
                propertyInfo: typeof(ACClassDesign).GetProperty("UpdateName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_UpdateName", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                maxLength: 20,
                unicode: false);
            updateName.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var valueTypeACClassID = runtimeEntityType.AddProperty(
                "ValueTypeACClassID",
                typeof(Guid?),
                propertyInfo: typeof(ACClassDesign).GetProperty("ValueTypeACClassID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_ValueTypeACClassID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            valueTypeACClassID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var visualHeight = runtimeEntityType.AddProperty(
                "VisualHeight",
                typeof(double),
                propertyInfo: typeof(ACClassDesign).GetProperty("VisualHeight", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_VisualHeight", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            visualHeight.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var visualWidth = runtimeEntityType.AddProperty(
                "VisualWidth",
                typeof(double),
                propertyInfo: typeof(ACClassDesign).GetProperty("VisualWidth", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_VisualWidth", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            visualWidth.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var xMLConfig = runtimeEntityType.AddProperty(
                "XMLConfig",
                typeof(string),
                propertyInfo: typeof(VBEntityObject).GetProperty("XMLConfig", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_XMLConfig", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            xMLConfig.AddAnnotation("Relational:ColumnType", "text");
            xMLConfig.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var xMLDesign = runtimeEntityType.AddProperty(
                "XMLDesign",
                typeof(string),
                propertyInfo: typeof(ACClassDesign).GetProperty("XMLDesign", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_XMLDesign", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            xMLDesign.AddAnnotation("Relational:ColumnType", "text");
            xMLDesign.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var lazyLoader = runtimeEntityType.AddServiceProperty(
                "LazyLoader",
                propertyInfo: typeof(ACClassDesign).GetProperty("LazyLoader", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var key = runtimeEntityType.AddKey(
                new[] { aCClassDesignID });
            runtimeEntityType.SetPrimaryKey(key);

            var nCIFKACClassDesignValueTypeACClassID = runtimeEntityType.AddIndex(
                new[] { valueTypeACClassID },
                name: "NCI_FK_ACClassDesign_ValueTypeACClassID");

            var uIXACClassDesign = runtimeEntityType.AddIndex(
                new[] { aCClassID, aCIdentifier },
                name: "UIX_ACClassDesign",
                unique: true);

            return runtimeEntityType;
        }

        public static RuntimeForeignKey CreateForeignKey1(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("ACClassID") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("ACClassID") }),
                principalEntityType,
                deleteBehavior: DeleteBehavior.Cascade,
                required: true);

            var aCClass = declaringEntityType.AddNavigation("ACClass",
                runtimeForeignKey,
                onDependent: true,
                typeof(ACClass),
                propertyInfo: typeof(ACClassDesign).GetProperty("ACClass", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_ACClass", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                propertyAccessMode: PropertyAccessMode.Field);

            var aCClassDesignACClass = principalEntityType.AddNavigation("ACClassDesign_ACClass",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<ACClassDesign>),
                propertyInfo: typeof(ACClass).GetProperty("ACClassDesign_ACClass", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClass).GetField("_ACClassDesign_ACClass", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                propertyAccessMode: PropertyAccessMode.Field);

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_ACClassDesign_ACClassID");
            return runtimeForeignKey;
        }

        public static RuntimeForeignKey CreateForeignKey2(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("ValueTypeACClassID") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("ACClassID") }),
                principalEntityType);

            var valueTypeACClass = declaringEntityType.AddNavigation("ValueTypeACClass",
                runtimeForeignKey,
                onDependent: true,
                typeof(ACClass),
                propertyInfo: typeof(ACClassDesign).GetProperty("ValueTypeACClass", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_ValueTypeACClass", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                propertyAccessMode: PropertyAccessMode.Field);

            var aCClassDesignValueTypeACClass = principalEntityType.AddNavigation("ACClassDesign_ValueTypeACClass",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<ACClassDesign>),
                propertyInfo: typeof(ACClass).GetProperty("ACClassDesign_ValueTypeACClass", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClass).GetField("_ACClassDesign_ValueTypeACClass", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                propertyAccessMode: PropertyAccessMode.Field);

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_ACClassDesign_ValueTypeACClassID");
            return runtimeForeignKey;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "ACClassDesign");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
