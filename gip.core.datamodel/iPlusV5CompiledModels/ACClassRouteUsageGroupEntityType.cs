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
    internal partial class ACClassRouteUsageGroupEntityType
    {
        public static RuntimeEntityType Create(RuntimeModel model, RuntimeEntityType baseEntityType = null)
        {
            var runtimeEntityType = model.AddEntityType(
                "gip.core.datamodel.ACClassRouteUsageGroup",
                typeof(ACClassRouteUsageGroup),
                baseEntityType,
                indexerPropertyInfo: RuntimeEntityType.FindIndexerProperty(typeof(ACClassRouteUsageGroup)));

            var aCClassRouteUsageGroupID = runtimeEntityType.AddProperty(
                "ACClassRouteUsageGroupID",
                typeof(Guid),
                propertyInfo: typeof(ACClassRouteUsageGroup).GetProperty("ACClassRouteUsageGroupID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassRouteUsageGroup).GetField("_ACClassRouteUsageGroupID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                afterSaveBehavior: PropertySaveBehavior.Throw);
            aCClassRouteUsageGroupID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var aCClassRouteUsageID = runtimeEntityType.AddProperty(
                "ACClassRouteUsageID",
                typeof(Guid),
                propertyInfo: typeof(ACClassRouteUsageGroup).GetProperty("ACClassRouteUsageID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassRouteUsageGroup).GetField("_ACClassRouteUsageID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            aCClassRouteUsageID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var groupID = runtimeEntityType.AddProperty(
                "GroupID",
                typeof(Guid),
                propertyInfo: typeof(ACClassRouteUsageGroup).GetProperty("GroupID", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassRouteUsageGroup).GetField("_GroupID", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            groupID.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var useFactor = runtimeEntityType.AddProperty(
                "UseFactor",
                typeof(int),
                propertyInfo: typeof(ACClassRouteUsageGroup).GetProperty("UseFactor", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassRouteUsageGroup).GetField("_UseFactor", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            useFactor.AddAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.None);

            var lazyLoader = runtimeEntityType.AddServiceProperty(
                "LazyLoader",
                propertyInfo: typeof(ACClassRouteUsageGroup).GetProperty("LazyLoader", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

            var key = runtimeEntityType.AddKey(
                new[] { aCClassRouteUsageGroupID });
            runtimeEntityType.SetPrimaryKey(key);

            var index = runtimeEntityType.AddIndex(
                new[] { aCClassRouteUsageID });

            return runtimeEntityType;
        }

        public static RuntimeForeignKey CreateForeignKey1(RuntimeEntityType declaringEntityType, RuntimeEntityType principalEntityType)
        {
            var runtimeForeignKey = declaringEntityType.AddForeignKey(new[] { declaringEntityType.FindProperty("ACClassRouteUsageID") },
                principalEntityType.FindKey(new[] { principalEntityType.FindProperty("ACClassRouteUsageID") }),
                principalEntityType,
                required: true);

            var aCClassRouteUsage = declaringEntityType.AddNavigation("ACClassRouteUsage",
                runtimeForeignKey,
                onDependent: true,
                typeof(ACClassRouteUsage),
                propertyInfo: typeof(ACClassRouteUsageGroup).GetProperty("ACClassRouteUsage", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassRouteUsageGroup).GetField("_ACClassRouteUsage", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                propertyAccessMode: PropertyAccessMode.Field);

            var aCClassRouteUsageGroup_ACClassRouteUsage = principalEntityType.AddNavigation("ACClassRouteUsageGroup_ACClassRouteUsage",
                runtimeForeignKey,
                onDependent: false,
                typeof(ICollection<ACClassRouteUsageGroup>),
                propertyInfo: typeof(ACClassRouteUsage).GetProperty("ACClassRouteUsageGroup_ACClassRouteUsage", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                fieldInfo: typeof(ACClassRouteUsage).GetField("_ACClassRouteUsageGroup_ACClassRouteUsage", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly),
                propertyAccessMode: PropertyAccessMode.Field);

            runtimeForeignKey.AddAnnotation("Relational:Name", "FK_ACClassRouteUsageGroup_ACClassRouteUsage");
            return runtimeForeignKey;
        }

        public static void CreateAnnotations(RuntimeEntityType runtimeEntityType)
        {
            runtimeEntityType.AddAnnotation("Relational:FunctionName", null);
            runtimeEntityType.AddAnnotation("Relational:Schema", null);
            runtimeEntityType.AddAnnotation("Relational:SqlQuery", null);
            runtimeEntityType.AddAnnotation("Relational:TableName", "ACClassRouteUsageGroup");
            runtimeEntityType.AddAnnotation("Relational:ViewName", null);
            runtimeEntityType.AddAnnotation("Relational:ViewSchema", null);

            Customize(runtimeEntityType);
        }

        static partial void Customize(RuntimeEntityType runtimeEntityType);
    }
}
