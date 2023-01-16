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
    internal partial class ACPropertyLogEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "gip.core.datamodel.ACPropertyLog",
                typeof(ACPropertyLog),
                baseEntityType,
                indexerPropertyInfo: RuntimeEntityType.FindIndexerProperty(typeof(ACPropertyLog)));

            var aCPropertyLogID = runtimeEntityType.AddProperty(
                "ACPropertyLogID",
                typeof(Guid),
                propertyInfo: typeof(ACPropertyLog).GetProperty("ACPropertyLogID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACPropertyLog).GetField("_ACPropertyLogID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw);
            aCPropertyLogID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCClassID = runtimeEntityType.AddProperty(
                "ACClassID",
                typeof(Guid),
                propertyInfo: typeof(ACPropertyLog).GetProperty("ACClassID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACPropertyLog).GetField("_ACClassID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            aCClassID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCClassPropertyID = runtimeEntityType.AddProperty(
                "ACClassPropertyID",
                typeof(Guid),
                propertyInfo: typeof(ACPropertyLog).GetProperty("ACClassPropertyID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACPropertyLog).GetField("_ACClassPropertyID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            aCClassPropertyID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCProgramLogID = runtimeEntityType.AddProperty(
                "ACProgramLogID",
                typeof(Guid?),
                propertyInfo: typeof(ACPropertyLog).GetProperty("ACProgramLogID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACPropertyLog).GetField("_ACProgramLogID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                nullable: true);
            aCProgramLogID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var eventTime = runtimeEntityType.AddProperty(
                "EventTime",
                typeof(DateTime),
                propertyInfo: typeof(ACPropertyLog).GetProperty("EventTime", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACPropertyLog).GetField("_EventTime", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            eventTime.AddAnnotation("Relational:ColumnType", "datetime");
            eventTime.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var value = runtimeEntityType.AddProperty(
                "Value",
                typeof(string),
                propertyInfo: typeof(ACPropertyLog).GetProperty("Value", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACPropertyLog).GetField("_Value", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                unicode: false);
            value.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var key = runtimeEntityType.AddKey(
                new[] { aCPropertyLogID });
            runtimeEntityType.SetPrimaryKey(key);

            var index = runtimeEntityType.AddIndex(
                new[] { aCClassID });

            var index0 = runtimeEntityType.AddIndex(
                new[] { aCClassPropertyID });

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
                propertyInfo: typeof(ACPropertyLog).GetProperty("ACClass", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACPropertyLog).GetField("<ACClass>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var aCPropertyLogACClass = principalEntityType.AddNavigation("ACPropertyLog_ACClass",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<ACPropertyLog>),
                propertyInfo: typeof(ACClass).GetProperty("ACPropertyLog_ACClass", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClass).GetField("<ACPropertyLog_ACClass>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_ACPropertyLog_ACClass");
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
                propertyInfo: typeof(ACPropertyLog).GetProperty("ACClassProperty", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACPropertyLog).GetField("<ACClassProperty>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var aCPropertyLogACClassProperty = principalEntityType.AddNavigation("ACPropertyLog_ACClassProperty",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<ACPropertyLog>),
                propertyInfo: typeof(ACClassProperty).GetProperty("ACPropertyLog_ACClassProperty", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassProperty).GetField("<ACPropertyLog_ACClassProperty>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_ACPropertyLog_ACClassProperty");
            return runtimeForeignKey;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "ACPropertyLog");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
