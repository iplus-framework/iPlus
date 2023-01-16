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
    internal partial class ACChangeLogEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "gip.core.datamodel.ACChangeLog",
                typeof(ACChangeLog),
                baseEntityType,
                indexerPropertyInfo: RuntimeEntityType.FindIndexerProperty(typeof(ACChangeLog)));

            var aCChangeLogID = runtimeEntityType.AddProperty(
                "ACChangeLogID",
                typeof(Guid),
                propertyInfo: typeof(ACChangeLog).GetProperty("ACChangeLogID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACChangeLog).GetField("_ACChangeLogID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw);
            aCChangeLogID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCClassID = runtimeEntityType.AddProperty(
                "ACClassID",
                typeof(Guid),
                propertyInfo: typeof(ACChangeLog).GetProperty("ACClassID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACChangeLog).GetField("_ACClassID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            aCClassID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCClassPropertyID = runtimeEntityType.AddProperty(
                "ACClassPropertyID",
                typeof(Guid),
                propertyInfo: typeof(ACChangeLog).GetProperty("ACClassPropertyID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACChangeLog).GetField("_ACClassPropertyID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            aCClassPropertyID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var changeDate = runtimeEntityType.AddProperty(
                "ChangeDate",
                typeof(DateTime),
                propertyInfo: typeof(ACChangeLog).GetProperty("ChangeDate", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACChangeLog).GetField("_ChangeDate", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            changeDate.AddAnnotation("Relational:ColumnType", "datetime");
            changeDate.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var deleted = runtimeEntityType.AddProperty(
                "Deleted",
                typeof(bool),
                propertyInfo: typeof(ACChangeLog).GetProperty("Deleted", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACChangeLog).GetField("_Deleted", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            deleted.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var entityKey = runtimeEntityType.AddProperty(
                "EntityKey",
                typeof(Guid),
                propertyInfo: typeof(ACChangeLog).GetProperty("EntityKey", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACChangeLog).GetField("_EntityKey", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            entityKey.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var vBUserID = runtimeEntityType.AddProperty(
                "VBUserID",
                typeof(Guid),
                propertyInfo: typeof(ACChangeLog).GetProperty("VBUserID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACChangeLog).GetField("_VBUserID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            vBUserID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var xMLValue = runtimeEntityType.AddProperty(
                "XMLValue",
                typeof(string),
                propertyInfo: typeof(ACChangeLog).GetProperty("XMLValue", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACChangeLog).GetField("_XMLValue", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                unicode: false);
            xMLValue.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var key = runtimeEntityType.AddKey(
                new[] { aCChangeLogID });
            runtimeEntityType.SetPrimaryKey(key);

            var index = runtimeEntityType.AddIndex(
                new[] { aCClassID });

            var index0 = runtimeEntityType.AddIndex(
                new[] { aCClassPropertyID });

            var index1 = runtimeEntityType.AddIndex(
                new[] { vBUserID });

            return runtimeEntityType;
        }

        public static RuntimeForeignKey CreateForeignKey1(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("ACClassID") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("ACClassID") }),
                principalEntityType,
                required: true);

            var aCClass = declaringEntityType.AddNavigation("ACClass",
                runtimeForeignKey,
                onDependent: true,
                typeof(ACClass),
                propertyInfo: typeof(ACChangeLog).GetProperty("ACClass", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACChangeLog).GetField("<ACClass>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var aCChangeLogACClass = principalEntityType.AddNavigation("ACChangeLog_ACClass",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<ACChangeLog>),
                propertyInfo: typeof(ACClass).GetProperty("ACChangeLog_ACClass", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClass).GetField("<ACChangeLog_ACClass>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_ACChangeLog_ACClass");
            return runtimeForeignKey;
        }

        public static RuntimeForeignKey CreateForeignKey2(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("ACClassPropertyID") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("ACClassPropertyID") }),
                principalEntityType,
                required: true);

            var aCClassProperty = declaringEntityType.AddNavigation("ACClassProperty",
                runtimeForeignKey,
                onDependent: true,
                typeof(ACClassProperty),
                propertyInfo: typeof(ACChangeLog).GetProperty("ACClassProperty", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACChangeLog).GetField("<ACClassProperty>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var aCChangeLogACClassProperty = principalEntityType.AddNavigation("ACChangeLog_ACClassProperty",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<ACChangeLog>),
                propertyInfo: typeof(ACClassProperty).GetProperty("ACChangeLog_ACClassProperty", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassProperty).GetField("<ACChangeLog_ACClassProperty>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_ACChangeLog_ACClassProperty");
            return runtimeForeignKey;
        }

        public static RuntimeForeignKey CreateForeignKey3(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("VBUserID") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("VBUserID") }),
                principalEntityType,
                required: true);

            var vBUser = declaringEntityType.AddNavigation("VBUser",
                runtimeForeignKey,
                onDependent: true,
                typeof(VBUser),
                propertyInfo: typeof(ACChangeLog).GetProperty("VBUser", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACChangeLog).GetField("<VBUser>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var aCChangeLogVBUser = principalEntityType.AddNavigation("ACChangeLog_VBUser",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<ACChangeLog>),
                propertyInfo: typeof(VBUser).GetProperty("ACChangeLog_VBUser", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(VBUser).GetField("<ACChangeLog_VBUser>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_ACChangeLog_VBUser");
            return runtimeForeignKey;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "ACChangeLog");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
