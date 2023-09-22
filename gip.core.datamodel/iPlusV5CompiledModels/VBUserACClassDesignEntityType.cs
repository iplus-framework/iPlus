﻿// <auto-generated />
using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#pragma warning disable 219, 612, 618
#nullable disable

namespace gip.core.datamodel
{
    internal partial class VBUserACClassDesignEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "gip.core.datamodel.VBUserACClassDesign",
                typeof(VBUserACClassDesign),
                baseEntityType,
                indexerPropertyInfo: RuntimeEntityType.FindIndexerProperty(typeof(VBUserACClassDesign)));

            var vBUserACClassDesignID = runtimeEntityType.AddProperty(
                "VBUserACClassDesignID",
                typeof(Guid),
                propertyInfo: typeof(VBUserACClassDesign).GetProperty("VBUserACClassDesignID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBUserACClassDesign).GetField("_VBUserACClassDesignID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw);
            vBUserACClassDesignID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCClassDesignID = runtimeEntityType.AddProperty(
                "ACClassDesignID",
                typeof(Guid?),
                propertyInfo: typeof(VBUserACClassDesign).GetProperty("ACClassDesignID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBUserACClassDesign).GetField("_ACClassDesignID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            aCClassDesignID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCIdentifier = runtimeEntityType.AddProperty(
                "ACIdentifier",
                typeof(string),
                propertyInfo: typeof(VBEntityObject).GetProperty("ACIdentifier", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBUserACClassDesign).GetField("_ACIdentifier", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true,
                maxLength: 200,
                unicode: false);
            aCIdentifier.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var insertDate = runtimeEntityType.AddProperty(
                "InsertDate",
                typeof(DateTime),
                propertyInfo: typeof(VBUserACClassDesign).GetProperty("InsertDate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBUserACClassDesign).GetField("_InsertDate", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            insertDate.AddAnnotation("Relational:ColumnType", "datetime");
            insertDate.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var insertName = runtimeEntityType.AddProperty(
                "InsertName",
                typeof(string),
                propertyInfo: typeof(VBUserACClassDesign).GetProperty("InsertName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBUserACClassDesign).GetField("_InsertName", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                maxLength: 20,
                unicode: false);
            insertName.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var updateDate = runtimeEntityType.AddProperty(
                "UpdateDate",
                typeof(DateTime),
                propertyInfo: typeof(VBUserACClassDesign).GetProperty("UpdateDate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBUserACClassDesign).GetField("_UpdateDate", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            updateDate.AddAnnotation("Relational:ColumnType", "datetime");
            updateDate.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var updateName = runtimeEntityType.AddProperty(
                "UpdateName",
                typeof(string),
                propertyInfo: typeof(VBUserACClassDesign).GetProperty("UpdateName", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBUserACClassDesign).GetField("_UpdateName", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                maxLength: 20,
                unicode: false);
            updateName.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var vBUserID = runtimeEntityType.AddProperty(
                "VBUserID",
                typeof(Guid),
                propertyInfo: typeof(VBUserACClassDesign).GetProperty("VBUserID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBUserACClassDesign).GetField("_VBUserID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            vBUserID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var xMLDesign = runtimeEntityType.AddProperty(
                "XMLDesign",
                typeof(string),
                propertyInfo: typeof(VBUserACClassDesign).GetProperty("XMLDesign", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBUserACClassDesign).GetField("_XMLDesign", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            xMLDesign.AddAnnotation("Relational:ColumnType", "text");
            xMLDesign.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var lazyLoader = runtimeEntityType.AddServiceProperty(
                "LazyLoader",
                propertyInfo: typeof(VBUserACClassDesign).GetProperty("LazyLoader", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var key = runtimeEntityType.AddKey(
                new[] { vBUserACClassDesignID });
            runtimeEntityType.SetPrimaryKey(key);

            var nCI_FK_VBUserACClassDesign_ACClassDesignID = runtimeEntityType.AddIndex(
                new[] { aCClassDesignID },
                name: "NCI_FK_VBUserACClassDesign_ACClassDesignID");

            var nCI_FK_VBUserACClassDesign_VBUserID = runtimeEntityType.AddIndex(
                new[] { vBUserID },
                name: "NCI_FK_VBUserACClassDesign_VBUserID");

            return runtimeEntityType;
        }

        public static RuntimeForeignKey CreateForeignKey1(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("ACClassDesignID") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("ACClassDesignID") }),
                principalEntityType,
                deleteBehavior: DeleteBehavior.Cascade);

            var aCClassDesign = declaringEntityType.AddNavigation("ACClassDesign",
                runtimeForeignKey,
                onDependent: true,
                typeof(ACClassDesign),
                propertyInfo: typeof(VBUserACClassDesign).GetProperty("ACClassDesign", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBUserACClassDesign).GetField("_ACClassDesign", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                propertyAccessMode: PropertyAccessMode.Field);

            var vBUserACClassDesign_ACClassDesign = principalEntityType.AddNavigation("VBUserACClassDesign_ACClassDesign",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<VBUserACClassDesign>),
                propertyInfo: typeof(ACClassDesign).GetProperty("VBUserACClassDesign_ACClassDesign", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassDesign).GetField("_VBUserACClassDesign_ACClassDesign", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                propertyAccessMode: PropertyAccessMode.Field);

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_VBUserACClassDesign_ACClassDesignID");
            return runtimeForeignKey;
        }

        public static RuntimeForeignKey CreateForeignKey2(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("VBUserID") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("VBUserID") }),
                principalEntityType,
                deleteBehavior: DeleteBehavior.Cascade,
                required: true);

            var vBUser = declaringEntityType.AddNavigation("VBUser",
                runtimeForeignKey,
                onDependent: true,
                typeof(VBUser),
                propertyInfo: typeof(VBUserACClassDesign).GetProperty("VBUser", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBUserACClassDesign).GetField("_VBUser", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                propertyAccessMode: PropertyAccessMode.Field);

            var vBUserACClassDesign_VBUser = principalEntityType.AddNavigation("VBUserACClassDesign_VBUser",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<VBUserACClassDesign>),
                propertyInfo: typeof(VBUser).GetProperty("VBUserACClassDesign_VBUser", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBUser).GetField("_VBUserACClassDesign_VBUser", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                propertyAccessMode: PropertyAccessMode.Field);

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_VBUserACClassDesign_VBUserID");
            return runtimeForeignKey;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "VBUserACClassDesign");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
