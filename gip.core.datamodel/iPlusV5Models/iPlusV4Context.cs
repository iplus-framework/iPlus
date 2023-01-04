using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace gip.core.datamodel;

public partial class iPlusV4Context : DbContext
{
    public iPlusV4Context()
    {
    }

    public iPlusV4Context(DbContextOptions<iPlusV4Context> options)
        : base(options)
    {
    }

    public virtual DbSet<ACAssembly> ACAssembly { get; set; }

    public virtual DbSet<ACChangeLog> ACChangeLog { get; set; }

    public virtual DbSet<ACClass> ACClass { get; set; }

    public virtual DbSet<ACClassConfig> ACClassConfig { get; set; }

    public virtual DbSet<ACClassDesign> ACClassDesign { get; set; }

    public virtual DbSet<ACClassMessage> ACClassMessage { get; set; }

    public virtual DbSet<ACClassMethod> ACClassMethod { get; set; }

    public virtual DbSet<ACClassMethodConfig> ACClassMethodConfig { get; set; }

    public virtual DbSet<ACClassProperty> ACClassProperty { get; set; }

    public virtual DbSet<ACClassPropertyRelation> ACClassPropertyRelation { get; set; }

    public virtual DbSet<ACClassTask> ACClassTask { get; set; }

    public virtual DbSet<ACClassTaskValue> ACClassTaskValue { get; set; }

    public virtual DbSet<ACClassTaskValuePos> ACClassTaskValuePos { get; set; }

    public virtual DbSet<ACClassText> ACClassText { get; set; }

    public virtual DbSet<ACClassWF> ACClassWF { get; set; }

    public virtual DbSet<ACClassWFEdge> ACClassWFEdge { get; set; }

    public virtual DbSet<ACPackage> ACPackage { get; set; }

    public virtual DbSet<ACProgram> ACProgram { get; set; }

    public virtual DbSet<ACProgramConfig> ACProgramConfig { get; set; }

    public virtual DbSet<ACProgramLog> ACProgramLog { get; set; }

    public virtual DbSet<ACProgramLogTask> ACProgramLogTask { get; set; }

    public virtual DbSet<ACProject> ACProject { get; set; }

    public virtual DbSet<ACPropertyLog> ACPropertyLog { get; set; }

    public virtual DbSet<ACPropertyLogRule> ACPropertyLogRule { get; set; }

    public virtual DbSet<MsgAlarmLog> MsgAlarmLog { get; set; }

    public virtual DbSet<VBConfig> VBConfig { get; set; }

    public virtual DbSet<VBGroup> VBGroup { get; set; }

    public virtual DbSet<VBGroupRight> VBGroupRight { get; set; }

    public virtual DbSet<VBLanguage> VBLanguage { get; set; }

    public virtual DbSet<VBLicense> VBLicense { get; set; }

    public virtual DbSet<VBNoConfiguration> VBNoConfiguration { get; set; }

    public virtual DbSet<VBSystem> VBSystem { get; set; }

    public virtual DbSet<VBSystemColumn> VBSystemColumn { get; set; }

    public virtual DbSet<VBTranslationView> VBTranslationView { get; set; }

    public virtual DbSet<VBUser> VBUser { get; set; }

    public virtual DbSet<VBUserACClassDesign> VBUserACClassDesign { get; set; }

    public virtual DbSet<VBUserACProject> VBUserACProject { get; set; }

    public virtual DbSet<VBUserGroup> VBUserGroup { get; set; }

    public virtual DbSet<VBUserInstance> VBUserInstance { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ACAssembly>(entity =>
        {
            entity.ToTable("ACAssembly");

            entity.HasIndex(e => e.AssemblyName, "UIX_Assembly").IsUnique();

            entity.Property(e => e.ACAssemblyID).ValueGeneratedNever();
            entity.Property(e => e.AssemblyDate).HasColumnType("datetime");
            entity.Property(e => e.AssemblyName)
                .IsRequired()
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.LastReflectionDate).HasColumnType("datetime");
            entity.Property(e => e.SHA1)
                .IsRequired()
                .HasMaxLength(40)
                .IsFixedLength();
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ACChangeLog>(entity =>
        {
            entity.ToTable("ACChangeLog");

            entity.Property(e => e.ACChangeLogID).ValueGeneratedNever();
            entity.Property(e => e.ChangeDate).HasColumnType("datetime");
            entity.Property(e => e.XMLValue)
                .IsRequired()
                .IsUnicode(false);

            entity.HasOne(d => d.ACClass).WithMany(p => p.ACChangeLog_ACClass)
                .HasForeignKey(d => d.ACClassID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACChangeLog_ACClass");

            entity.HasOne(d => d.ACClassProperty).WithMany(p => p.ACChangeLog_ACClassProperty)
                .HasForeignKey(d => d.ACClassPropertyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACChangeLog_ACClassProperty");

            entity.HasOne(d => d.VBUser).WithMany(p => p.ACChangeLog_VBUser)
                .HasForeignKey(d => d.VBUserID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACChangeLog_VBUser");
        });

        modelBuilder.Entity<ACClass>(entity =>
        {
            entity.ToTable("ACClass");

            entity.HasIndex(e => new { e.ACKindIndex, e.IsAbstract }, "NCI_ACClass_ACKindIndex_IsAbstract");

            entity.HasIndex(e => e.AssemblyQualifiedName, "NCI_ACClass_AssemblyQualifiedName");

            entity.HasIndex(e => e.ACPackageID, "NCI_FK_ACClass_ACPackageID");

            entity.HasIndex(e => e.ACProjectID, "NCI_FK_ACClass_ACProjectID");

            entity.HasIndex(e => e.BasedOnACClassID, "NCI_FK_ACClass_BasedOnACClassID");

            entity.HasIndex(e => e.PWACClassID, "NCI_FK_ACClass_PWACClassID");

            entity.HasIndex(e => e.PWMethodACClassID, "NCI_FK_ACClass_PWMethodACClassID");

            entity.HasIndex(e => e.ParentACClassID, "NCI_FK_ACClass_ParentACClassID");

            entity.HasIndex(e => new { e.ACProjectID, e.ParentACClassID, e.ACIdentifier }, "UIX_ACClass").IsUnique();

            entity.Property(e => e.ACClassID).ValueGeneratedNever();
            entity.Property(e => e.ACCaptionTranslation).IsUnicode(false);
            entity.Property(e => e.ACFilterColumns)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ACIdentifier)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ACSortColumns)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ACURLCached).IsUnicode(false);
            entity.Property(e => e.ACURLComponentCached).IsUnicode(false);
            entity.Property(e => e.AssemblyQualifiedName)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Comment).IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLACClass).IsUnicode(false);
            entity.Property(e => e.XMLConfig)
                .IsRequired()
                .HasColumnType("text");

            entity.HasOne(d => d.ACPackage).WithMany(p => p.ACClass_ACPackage)
                .HasForeignKey(d => d.ACPackageID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACClass_ACPackageID");

            entity.HasOne(d => d.ACProject).WithMany(p => p.ACClass_ACProject)
                .HasForeignKey(d => d.ACProjectID)
                .HasConstraintName("FK_ACClass_ACProjectID");

            entity.HasOne(d => d.ACClass1_BasedOnACClass).WithMany(p => p.ACClass_BasedOnACClass)
                .HasForeignKey(d => d.BasedOnACClassID)
                .HasConstraintName("FK_ACClass_BasedOnACClassID");

            entity.HasOne(d => d.ACClass1_PWACClass).WithMany(p => p.ACClass_PWACClass)
                .HasForeignKey(d => d.PWACClassID)
                .HasConstraintName("FK_ACClass_PWACClassID");

            entity.HasOne(d => d.ACClass1_PWMethodACClass).WithMany(p => p.ACClass_PWMethodACClass)
                .HasForeignKey(d => d.PWMethodACClassID)
                .HasConstraintName("FK_ACClass_PWMethodACClassID");

            entity.HasOne(d => d.ACClass1_ParentACClass).WithMany(p => p.ACClass_ParentACClass)
                .HasForeignKey(d => d.ParentACClassID)
                .HasConstraintName("FK_ACClass_ParentACClassID");
        });

        modelBuilder.Entity<ACClassConfig>(entity =>
        {
            entity.ToTable("ACClassConfig");

            entity.HasIndex(e => e.ACClassID, "NCI_FK_ACClassConfig_ACClassID");

            entity.HasIndex(e => e.ACClassPropertyRelationID, "NCI_FK_ACClassConfig_ACClassPropertyRelationID");

            entity.HasIndex(e => e.ParentACClassConfigID, "NCI_FK_ACClassConfig_ParentACClassConfigID");

            entity.HasIndex(e => e.ValueTypeACClassID, "NCI_FK_ACClassConfig_ValueTypeACClassID");

            entity.Property(e => e.ACClassConfigID).ValueGeneratedNever();
            entity.Property(e => e.Comment).IsUnicode(false);
            entity.Property(e => e.Expression).HasColumnType("text");
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.KeyACUrl).IsUnicode(false);
            entity.Property(e => e.LocalConfigACUrl).IsUnicode(false);
            entity.Property(e => e.PreConfigACUrl).IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLConfig)
                .IsRequired()
                .HasColumnType("text");

            entity.HasOne(d => d.ACClass).WithMany(p => p.ACClassConfig_ACClass)
                .HasForeignKey(d => d.ACClassID)
                .HasConstraintName("FK_ACClassConfig_ACClassID");

            entity.HasOne(d => d.ACClassPropertyRelation).WithMany(p => p.ACClassConfig_ACClassPropertyRelation)
                .HasForeignKey(d => d.ACClassPropertyRelationID)
                .HasConstraintName("FK_ACClassConfig_ACClassPropertyRelationID");

            entity.HasOne(d => d.ACClassConfig1_ParentACClassConfig).WithMany(p => p.ACClassConfig_ParentACClassConfig)
                .HasForeignKey(d => d.ParentACClassConfigID)
                .HasConstraintName("FK_ACClassConfig_ParentACClassConfigID");

            entity.HasOne(d => d.ValueTypeACClass).WithMany(p => p.ACClassConfig_ValueTypeACClass)
                .HasForeignKey(d => d.ValueTypeACClassID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACClassConfig_ValueTypeACClassID");
        });

        modelBuilder.Entity<ACClassDesign>(entity =>
        {
            entity.ToTable("ACClassDesign");

            entity.HasIndex(e => e.ValueTypeACClassID, "NCI_FK_ACClassDesign_ValueTypeACClassID");

            entity.HasIndex(e => new { e.ACClassID, e.ACIdentifier }, "UIX_ACClassDesign").IsUnique();

            entity.Property(e => e.ACClassDesignID).ValueGeneratedNever();
            entity.Property(e => e.ACCaptionTranslation).IsUnicode(false);
            entity.Property(e => e.ACGroup)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ACIdentifier)
                .IsRequired()
                .HasMaxLength(80)
                .IsUnicode(false);
            entity.Property(e => e.BAMLDate).HasColumnType("datetime");
            entity.Property(e => e.Comment).IsUnicode(false);
            entity.Property(e => e.DesignNo)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLConfig).HasColumnType("text");
            entity.Property(e => e.XMLDesign)
                .IsRequired()
                .HasColumnType("text");

            entity.HasOne(d => d.ACClass).WithMany(p => p.ACClassDesign_ACClass)
                .HasForeignKey(d => d.ACClassID)
                .HasConstraintName("FK_ACClassDesign_ACClassID");

            entity.HasOne(d => d.ValueTypeACClass).WithMany(p => p.ACClassDesign_ValueTypeACClass)
                .HasForeignKey(d => d.ValueTypeACClassID)
                .HasConstraintName("FK_ACClassDesign_ValueTypeACClassID");
        });

        modelBuilder.Entity<ACClassMessage>(entity =>
        {
            entity.ToTable("ACClassMessage");

            entity.HasIndex(e => new { e.ACClassID, e.ACIdentifier }, "UIX_ACClassMessage").IsUnique();

            entity.Property(e => e.ACClassMessageID).ValueGeneratedNever();
            entity.Property(e => e.ACCaptionTranslation).IsUnicode(false);
            entity.Property(e => e.ACIdentifier)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.ACClass).WithMany(p => p.ACClassMessage_ACClass)
                .HasForeignKey(d => d.ACClassID)
                .HasConstraintName("FK_ACClassMessage_ACClassID");
        });

        modelBuilder.Entity<ACClassMethod>(entity =>
        {
            entity.ToTable("ACClassMethod");

            entity.HasIndex(e => e.PWACClassID, "NCI_FK_ACClassMethod_PWACClassID");

            entity.HasIndex(e => e.ParentACClassMethodID, "NCI_FK_ACClassMethod_ParentACClassMethodID");

            entity.HasIndex(e => e.ValueTypeACClassID, "NCI_FK_ACClassMethod_ValueTypeACClassID");

            entity.HasIndex(e => new { e.ACClassID, e.ACIdentifier }, "UIX_ACClassMethod").IsUnique();

            entity.Property(e => e.ACClassMethodID).ValueGeneratedNever();
            entity.Property(e => e.ACCaptionTranslation).IsUnicode(false);
            entity.Property(e => e.ACGroup)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ACIdentifier)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Comment).IsUnicode(false);
            entity.Property(e => e.GenericType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.InteractionVBContent)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.Sourcecode).HasColumnType("text");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLACMethod).IsUnicode(false);
            entity.Property(e => e.XMLConfig).HasColumnType("text");
            entity.Property(e => e.XMLDesign).HasColumnType("text");

            entity.HasOne(d => d.ACClass).WithMany(p => p.ACClassMethod_ACClass)
                .HasForeignKey(d => d.ACClassID)
                .HasConstraintName("FK_ACClassMethod_ACClassID");

            entity.HasOne(d => d.AttachedFromACClass).WithMany(p => p.ACClassMethod_AttachedFromACClass)
                .HasForeignKey(d => d.AttachedFromACClassID)
                .HasConstraintName("FK_ACClassMethod_AttachedFromACClass");

            entity.HasOne(d => d.PWACClass).WithMany(p => p.ACClassMethod_PWACClass)
                .HasForeignKey(d => d.PWACClassID)
                .HasConstraintName("FK_ACClassMethod_PWACClassID");

            entity.HasOne(d => d.ACClassMethod1_ParentACClassMethod).WithMany(p => p.ACClassMethod_ParentACClassMethod)
                .HasForeignKey(d => d.ParentACClassMethodID)
                .HasConstraintName("FK_ACClassMethod_ParentACClassMethodID");

            entity.HasOne(d => d.ValueTypeACClass).WithMany(p => p.ACClassMethod_ValueTypeACClass)
                .HasForeignKey(d => d.ValueTypeACClassID)
                .HasConstraintName("FK_ACClassMethod_ValueTypeACClassID");
        });

        modelBuilder.Entity<ACClassMethodConfig>(entity =>
        {
            entity.ToTable("ACClassMethodConfig");

            entity.Property(e => e.ACClassMethodConfigID).ValueGeneratedNever();
            entity.Property(e => e.Comment).IsUnicode(false);
            entity.Property(e => e.Expression).HasColumnType("text");
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.KeyACUrl).IsUnicode(false);
            entity.Property(e => e.LocalConfigACUrl).IsUnicode(false);
            entity.Property(e => e.PreConfigACUrl).IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLConfig)
                .IsRequired()
                .HasColumnType("text");

            entity.HasOne(d => d.ACClassMethod).WithMany(p => p.ACClassMethodConfig_ACClassMethod)
                .HasForeignKey(d => d.ACClassMethodID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACClassMethodConfig_ACClassMethodID");

            entity.HasOne(d => d.ACClassWF).WithMany(p => p.ACClassMethodConfig_ACClassWF)
                .HasForeignKey(d => d.ACClassWFID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ACClassMethodConfig_ACClassWFID");

            entity.HasOne(d => d.ACClassMethodConfig1_ParentACClassMethodConfig).WithMany(p => p.ACClassMethodConfig_ParentACClassMethodConfig)
                .HasForeignKey(d => d.ParentACClassMethodConfigID)
                .HasConstraintName("FK_ACClassMethodConfig_ParentACClassMethodConfigID");

            entity.HasOne(d => d.VBiACClass).WithMany(p => p.ACClassMethodConfig_VBiACClass)
                .HasForeignKey(d => d.VBiACClassID)
                .HasConstraintName("FK_ACClassMethodConfig_VBiACClassID");

            entity.HasOne(d => d.VBiACClassPropertyRelation).WithMany(p => p.ACClassMethodConfig_VBiACClassPropertyRelation)
                .HasForeignKey(d => d.VBiACClassPropertyRelationID)
                .HasConstraintName("FK_ACClassMethodConfig_VBiACClassPropertyRelationID");

            entity.HasOne(d => d.ValueTypeACClass).WithMany(p => p.ACClassMethodConfig_ValueTypeACClass)
                .HasForeignKey(d => d.ValueTypeACClassID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACClassMethodConfig_ValueTypeACClassID");
        });

        modelBuilder.Entity<ACClassProperty>(entity =>
        {
            entity.ToTable("ACClassProperty");

            entity.HasIndex(e => e.BasedOnACClassPropertyID, "NCI_FK_ACClassProperty_BasedOnACClassPropertyID");

            entity.HasIndex(e => e.ConfigACClassID, "NCI_FK_ACClassProperty_ConfigACClassID");

            entity.HasIndex(e => e.ParentACClassPropertyID, "NCI_FK_ACClassProperty_ParentACClassPropertyID");

            entity.HasIndex(e => e.ValueTypeACClassID, "NCI_FK_ACClassProperty_ValueTypeACClassID");

            entity.HasIndex(e => new { e.ACClassID, e.ACIdentifier }, "UIX_ACClassProperty").IsUnique();

            entity.Property(e => e.ACClassPropertyID).ValueGeneratedNever();
            entity.Property(e => e.ACCaptionTranslation).IsUnicode(false);
            entity.Property(e => e.ACGroup)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ACIdentifier)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ACSource)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.CallbackMethodName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Comment).IsUnicode(false);
            entity.Property(e => e.GenericType)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.InputMask)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLACEventArgs).IsUnicode(false);
            entity.Property(e => e.XMLConfig).HasColumnType("text");
            entity.Property(e => e.XMLValue).HasColumnType("text");

            entity.HasOne(d => d.ACClass).WithMany(p => p.ACClassProperty_ACClass)
                .HasForeignKey(d => d.ACClassID)
                .HasConstraintName("FK_ACClassProperty_ACClassID");

            entity.HasOne(d => d.ACClassProperty1_BasedOnACClassProperty).WithMany(p => p.ACClassProperty_BasedOnACClassProperty)
                .HasForeignKey(d => d.BasedOnACClassPropertyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACClassProperty_BasedOnACClassPropertyID");

            entity.HasOne(d => d.ConfigACClass).WithMany(p => p.ACClassProperty_ConfigACClass)
                .HasForeignKey(d => d.ConfigACClassID)
                .HasConstraintName("FK_ACClassProperty_ConfigACClassID");

            entity.HasOne(d => d.ACClassProperty1_ParentACClassProperty).WithMany(p => p.ACClassProperty_ParentACClassProperty)
                .HasForeignKey(d => d.ParentACClassPropertyID)
                .HasConstraintName("FK_ACClassProperty_ParentACClassPropertyID");

            entity.HasOne(d => d.ValueTypeACClass).WithMany(p => p.ACClassProperty_ValueTypeACClass)
                .HasForeignKey(d => d.ValueTypeACClassID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACClassProperty_ValueTypeACClassID");
        });

        modelBuilder.Entity<ACClassPropertyRelation>(entity =>
        {
            entity.ToTable("ACClassPropertyRelation");

            entity.HasIndex(e => e.SourceACClassID, "NCI_FK_ACClassPropertyRelation_SourceACClassID");

            entity.HasIndex(e => e.SourceACClassPropertyID, "NCI_FK_ACClassPropertyRelation_SourceACClassPropertyID");

            entity.HasIndex(e => e.TargetACClassID, "NCI_FK_ACClassPropertyRelation_TargetACClassID");

            entity.HasIndex(e => e.TargetACClassPropertyID, "NCI_FK_ACClassPropertyRelation_TargetACClassPropertyID");

            entity.Property(e => e.ACClassPropertyRelationID).ValueGeneratedNever();
            entity.Property(e => e.ConvExpressionS)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.ConvExpressionT)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.GroupName).IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.LastManipulationDT).HasColumnType("datetime");
            entity.Property(e => e.StateName).IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLValue).HasColumnType("text");

            entity.HasOne(d => d.SourceACClass).WithMany(p => p.ACClassPropertyRelation_SourceACClass)
                .HasForeignKey(d => d.SourceACClassID)
                .HasConstraintName("FK_ACClassPropertyRelation_SourceACClassID");

            entity.HasOne(d => d.SourceACClassProperty).WithMany(p => p.ACClassPropertyRelation_SourceACClassProperty)
                .HasForeignKey(d => d.SourceACClassPropertyID)
                .HasConstraintName("FK_ACClassPropertyRelation_SourceACClassPropertyID");

            entity.HasOne(d => d.TargetACClass).WithMany(p => p.ACClassPropertyRelation_TargetACClass)
                .HasForeignKey(d => d.TargetACClassID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACClassPropertyRelation_TargetACClassID");

            entity.HasOne(d => d.TargetACClassProperty).WithMany(p => p.ACClassPropertyRelation_TargetACClassProperty)
                .HasForeignKey(d => d.TargetACClassPropertyID)
                .HasConstraintName("FK_ACClassPropertyRelation_TargetACClassPropertyID");
        });

        modelBuilder.Entity<ACClassTask>(entity =>
        {
            entity.ToTable("ACClassTask");

            entity.HasIndex(e => e.ACProgramID, "NCI_FK_ACClassTask_ACProgramID");

            entity.HasIndex(e => e.ContentACClassWFID, "NCI_FK_ACClassTask_ContentACClassWFID");

            entity.HasIndex(e => e.ParentACClassTaskID, "NCI_FK_ACClassTask_ParentACClassTaskID");

            entity.HasIndex(e => e.TaskTypeACClassID, "NCI_FK_ACClassTask_TaskTypeACClassID");

            entity.Property(e => e.ACClassTaskID).ValueGeneratedNever();
            entity.Property(e => e.ACIdentifier)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLACMethod).IsUnicode(false);
            entity.Property(e => e.XMLConfig).HasColumnType("text");

            entity.HasOne(d => d.ACProgram).WithMany(p => p.ACClassTask_ACProgram)
                .HasForeignKey(d => d.ACProgramID)
                .HasConstraintName("FK_ACClassTask_ACProgramID");

            entity.HasOne(d => d.ContentACClassWF).WithMany(p => p.ACClassTask_ContentACClassWF)
                .HasForeignKey(d => d.ContentACClassWFID)
                .HasConstraintName("FK_ACClassTask_ContentACClassWFID");

            entity.HasOne(d => d.ACClassTask1_ParentACClassTask).WithMany(p => p.ACClassTask_ParentACClassTask)
                .HasForeignKey(d => d.ParentACClassTaskID)
                .HasConstraintName("FK_ACClassTask_ParentACClassTaskID");

            entity.HasOne(d => d.TaskTypeACClass).WithMany(p => p.ACClassTask_TaskTypeACClass)
                .HasForeignKey(d => d.TaskTypeACClassID)
                .HasConstraintName("FK_ACClassTask_TaskTypeACClassID");
        });

        modelBuilder.Entity<ACClassTaskValue>(entity =>
        {
            entity.ToTable("ACClassTaskValue");

            entity.HasIndex(e => e.ACClassPropertyID, "NCI_FK_ACClassTaskValue_ACClassPropertyID");

            entity.HasIndex(e => e.ACClassTaskID, "NCI_FK_ACClassTaskValue_ACClassTaskID");

            entity.HasIndex(e => e.VBUserID, "NCI_FK_ACClassTaskValue_VBUserID");

            entity.Property(e => e.ACClassTaskValueID).ValueGeneratedNever();
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLValue)
                .IsRequired()
                .HasColumnType("text");
            entity.Property(e => e.XMLValue2)
                .IsRequired()
                .HasColumnType("text");

            entity.HasOne(d => d.ACClassProperty).WithMany(p => p.ACClassTaskValue_ACClassProperty)
                .HasForeignKey(d => d.ACClassPropertyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACClassTaskValue_ACClassPropertyID");

            entity.HasOne(d => d.ACClassTask).WithMany(p => p.ACClassTaskValue_ACClassTask)
                .HasForeignKey(d => d.ACClassTaskID)
                .HasConstraintName("FK_ACClassTaskValue_ACClassTaskID");

            entity.HasOne(d => d.VBUser).WithMany(p => p.ACClassTaskValue_VBUser)
                .HasForeignKey(d => d.VBUserID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_ACClassTaskValue_VBUserID");
        });

        modelBuilder.Entity<ACClassTaskValuePos>(entity =>
        {
            entity.HasKey(e => e.ACClassTaskValuePosID);

            entity.HasIndex(e => e.ACClassTaskValueID, "NCI_FK_ACClassTaskValuePos_ACClassTaskValueID");

            entity.HasIndex(e => e.RequestID, "NCI_FK_ACClassTaskValuePos_RequestID");

            entity.Property(e => e.ACClassTaskValuePosID).ValueGeneratedNever();
            entity.Property(e => e.ACIdentifier)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ACUrl)
                .IsRequired()
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.AsyncCallbackDelegateName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ClientPointName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ExecutingInstanceURL)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLACMethod)
                .IsRequired()
                .HasColumnType("text");

            entity.HasOne(d => d.ACClassTaskValue).WithMany(p => p.ACClassTaskValuePos_ACClassTaskValue)
                .HasForeignKey(d => d.ACClassTaskValueID)
                .HasConstraintName("FK_ACClassTaskValuePos_ACClassTaskValueID");
        });

        modelBuilder.Entity<ACClassText>(entity =>
        {
            entity.ToTable("ACClassText");

            entity.HasIndex(e => new { e.ACClassID, e.ACIdentifier }, "UIX_ACClassText").IsUnique();

            entity.Property(e => e.ACClassTextID).ValueGeneratedNever();
            entity.Property(e => e.ACCaptionTranslation).IsUnicode(false);
            entity.Property(e => e.ACIdentifier)
                .IsRequired()
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.ACClass).WithMany(p => p.ACClassText_ACClass)
                .HasForeignKey(d => d.ACClassID)
                .HasConstraintName("FK_ACClassText_ACClassID");
        });

        modelBuilder.Entity<ACClassWF>(entity =>
        {
            entity.ToTable("ACClassWF");

            entity.HasIndex(e => e.PWACClassID, "NCI_FK_ACClassWF_PWACClassID");

            entity.HasIndex(e => e.RefPAACClassID, "NCI_FK_ACClassWF_RefPAACClassID");

            entity.HasIndex(e => e.RefPAACClassMethodID, "NCI_FK_ACClassWF_RefPAACClassMethodID");

            entity.HasIndex(e => new { e.ACClassMethodID, e.ParentACClassWFID, e.ACIdentifier }, "UIX_ACClassWF").IsUnique();

            entity.Property(e => e.ACClassWFID).ValueGeneratedNever();
            entity.Property(e => e.ACIdentifier)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Comment).IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PhaseIdentifier)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLConfig).HasColumnType("text");
            entity.Property(e => e.XName)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.ACClassMethod).WithMany(p => p.ACClassWF_ACClassMethod)
                .HasForeignKey(d => d.ACClassMethodID)
                .HasConstraintName("FK_ACClassWF_ACClassMethodID");

            entity.HasOne(d => d.PWACClass).WithMany(p => p.ACClassWF_PWACClass)
                .HasForeignKey(d => d.PWACClassID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACClassWF_PWACClassID");

            entity.HasOne(d => d.ACClassWF1_ParentACClassWF).WithMany(p => p.ACClassWF_ParentACClassWF)
                .HasForeignKey(d => d.ParentACClassWFID)
                .HasConstraintName("FK_ACClassWF_ParentACClassWFID");

            entity.HasOne(d => d.RefPAACClass).WithMany(p => p.ACClassWF_RefPAACClass)
                .HasForeignKey(d => d.RefPAACClassID)
                .HasConstraintName("FK_ACClassWF_RefPAACClassID");

            entity.HasOne(d => d.RefPAACClassMethod).WithMany(p => p.ACClassWF_RefPAACClassMethod)
                .HasForeignKey(d => d.RefPAACClassMethodID)
                .HasConstraintName("FK_ACClassWF_RefPAACClassMethodID");
        });

        modelBuilder.Entity<ACClassWFEdge>(entity =>
        {
            entity.ToTable("ACClassWFEdge");

            entity.HasIndex(e => e.ACClassMethodID, "NCI_FK_ACClassWFEdge_ACClassMethodID");

            entity.HasIndex(e => e.SourceACClassMethodID, "NCI_FK_ACClassWFEdge_SourceACClassMethodID");

            entity.HasIndex(e => e.SourceACClassPropertyID, "NCI_FK_ACClassWFEdge_SourceACClassPropertyID");

            entity.HasIndex(e => e.SourceACClassWFID, "NCI_FK_ACClassWFEdge_SourceACClassWFID");

            entity.HasIndex(e => e.TargetACClassMethodID, "NCI_FK_ACClassWFEdge_TargetACClassMethodID");

            entity.HasIndex(e => e.TargetACClassPropertyID, "NCI_FK_ACClassWFEdge_TargetACClassPropertyID");

            entity.HasIndex(e => e.TargetACClassWFID, "NCI_FK_ACClassWFEdge_TargetACClassWFID");

            entity.Property(e => e.ACClassWFEdgeID).ValueGeneratedNever();
            entity.Property(e => e.ACIdentifier)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.XName)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.ACClassMethod).WithMany(p => p.ACClassWFEdge_ACClassMethod)
                .HasForeignKey(d => d.ACClassMethodID)
                .HasConstraintName("FK_ACClassWFEdge_ACClassMethodID");

            entity.HasOne(d => d.SourceACClassMethod).WithMany(p => p.ACClassWFEdge_SourceACClassMethod)
                .HasForeignKey(d => d.SourceACClassMethodID)
                .HasConstraintName("FK_ACClassWFEdge_SourceACClassMethodID");

            entity.HasOne(d => d.SourceACClassProperty).WithMany(p => p.ACClassWFEdge_SourceACClassProperty)
                .HasForeignKey(d => d.SourceACClassPropertyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACClassWFEdge_SourceACClassPropertyID");

            entity.HasOne(d => d.SourceACClassWF).WithMany(p => p.ACClassWFEdge_SourceACClassWF)
                .HasForeignKey(d => d.SourceACClassWFID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACClassWFEdge_SourceACClassWFID");

            entity.HasOne(d => d.TargetACClassMethod).WithMany(p => p.ACClassWFEdge_TargetACClassMethod)
                .HasForeignKey(d => d.TargetACClassMethodID)
                .HasConstraintName("FK_ACClassWFEdge_TargetACClassMethodID");

            entity.HasOne(d => d.TargetACClassProperty).WithMany(p => p.ACClassWFEdge_TargetACClassProperty)
                .HasForeignKey(d => d.TargetACClassPropertyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACClassWFEdge_TargetACClassPropertyID");

            entity.HasOne(d => d.TargetACClassWF).WithMany(p => p.ACClassWFEdge_TargetACClassWF)
                .HasForeignKey(d => d.TargetACClassWFID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACClassWFEdge_TargetACClassWFID");
        });

        modelBuilder.Entity<ACPackage>(entity =>
        {
            entity.ToTable("ACPackage");

            entity.Property(e => e.ACPackageID).ValueGeneratedNever();
            entity.Property(e => e.ACPackageName)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Comment).IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ACProgram>(entity =>
        {
            entity.ToTable("ACProgram");

            entity.HasIndex(e => e.ProgramACClassMethodID, "NCI_FK_ACProgram_ProgramACClassMethodID");

            entity.HasIndex(e => e.WorkflowTypeACClassID, "NCI_FK_ACProgram_WorkflowTypeACClassID");

            entity.Property(e => e.ACProgramID).ValueGeneratedNever();
            entity.Property(e => e.Comment).IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PlannedStartDate).HasColumnType("datetime");
            entity.Property(e => e.ProgramName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ProgramNo)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLConfig).HasColumnType("text");

            entity.HasOne(d => d.ProgramACClassMethod).WithMany(p => p.ACProgram_ProgramACClassMethod)
                .HasForeignKey(d => d.ProgramACClassMethodID)
                .HasConstraintName("FK_ACProgram_ProgramACClassMethodID");

            entity.HasOne(d => d.WorkflowTypeACClass).WithMany(p => p.ACProgram_WorkflowTypeACClass)
                .HasForeignKey(d => d.WorkflowTypeACClassID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACProgram_WorkflowTypeACClassID");
        });

        modelBuilder.Entity<ACProgramConfig>(entity =>
        {
            entity.ToTable("ACProgramConfig");

            entity.HasIndex(e => e.ACClassID, "NCI_FK_ACProgramConfig_ACClassID");

            entity.HasIndex(e => e.ACClassPropertyRelationID, "NCI_FK_ACProgramConfig_ACClassPropertyRelationID");

            entity.HasIndex(e => e.ACProgramID, "NCI_FK_ACProgramConfig_ACProgramID");

            entity.HasIndex(e => e.ParentACProgramConfigID, "NCI_FK_ACProgramConfig_ParentACProgramConfigID");

            entity.HasIndex(e => e.ValueTypeACClassID, "NCI_FK_ACProgramConfig_ValueTypeACClassID");

            entity.Property(e => e.ACProgramConfigID).ValueGeneratedNever();
            entity.Property(e => e.Comment).IsUnicode(false);
            entity.Property(e => e.Expression).HasColumnType("text");
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.KeyACUrl).IsUnicode(false);
            entity.Property(e => e.LocalConfigACUrl).IsUnicode(false);
            entity.Property(e => e.PreConfigACUrl).IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLConfig)
                .IsRequired()
                .HasColumnType("text");

            entity.HasOne(d => d.ACClass).WithMany(p => p.ACProgramConfig_ACClass)
                .HasForeignKey(d => d.ACClassID)
                .HasConstraintName("FK_ACProgramConfig_ACClassID");

            entity.HasOne(d => d.ACClassPropertyRelation).WithMany(p => p.ACProgramConfig_ACClassPropertyRelation)
                .HasForeignKey(d => d.ACClassPropertyRelationID)
                .HasConstraintName("FK_ACProgramConfig_ACClassPropertyRelationID");

            entity.HasOne(d => d.ACProgram).WithMany(p => p.ACProgramConfig_ACProgram)
                .HasForeignKey(d => d.ACProgramID)
                .HasConstraintName("FK_ACProgramConfig_ACProgramID");

            entity.HasOne(d => d.ACProgramConfig1_ParentACProgramConfig).WithMany(p => p.ACProgramConfig_ParentACProgramConfig)
                .HasForeignKey(d => d.ParentACProgramConfigID)
                .HasConstraintName("FK_ACProgramConfig_ParentACProgramConfigID");

            entity.HasOne(d => d.ValueTypeACClass).WithMany(p => p.ACProgramConfig_ValueTypeACClass)
                .HasForeignKey(d => d.ValueTypeACClassID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACProgramConfig_ValueTypeACClassID");
        });

        modelBuilder.Entity<ACProgramLog>(entity =>
        {
            entity.ToTable("ACProgramLog");

            entity.HasIndex(e => e.ACProgramID, "NCI_FK_ACProgramLog_ACProgramID");

            entity.HasIndex(e => e.ParentACProgramLogID, "NCI_FK_ACProgramLog_ParentACProgramLogID");

            entity.Property(e => e.ACProgramLogID).ValueGeneratedNever();
            entity.Property(e => e.ACUrl)
                .IsRequired()
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.EndDatePlan).HasColumnType("datetime");
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Message).IsUnicode(false);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.StartDatePlan).HasColumnType("datetime");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLConfig)
                .IsRequired()
                .HasColumnType("text");

            entity.HasOne(d => d.ACProgram).WithMany(p => p.ACProgramLog_ACProgram)
                .HasForeignKey(d => d.ACProgramID)
                .HasConstraintName("FK_ACProgramLog_ACProgramID");

            entity.HasOne(d => d.ACProgramLog1_ParentACProgramLog).WithMany(p => p.ACProgramLog_ParentACProgramLog)
                .HasForeignKey(d => d.ParentACProgramLogID)
                .HasConstraintName("FK_ACProgramLog_ParentACProgramLogID");
        });

        modelBuilder.Entity<ACProgramLogTask>(entity =>
        {
            entity.ToTable("ACProgramLogTask");

            entity.HasIndex(e => e.ACProgramLogID, "NCI_FK_ACProgramLogTask_ACProgramLogID");

            entity.Property(e => e.ACProgramLogTaskID).ValueGeneratedNever();
            entity.Property(e => e.ACClassMethodXAML).HasColumnType("text");
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLConfig).HasColumnType("text");

            entity.HasOne(d => d.ACProgramLog).WithMany(p => p.ACProgramLogTask_ACProgramLog)
                .HasForeignKey(d => d.ACProgramLogID)
                .HasConstraintName("FK_ACProgramLogTask_ACProgramLogID");
        });

        modelBuilder.Entity<ACProject>(entity =>
        {
            entity.ToTable("ACProject");

            entity.HasIndex(e => e.BasedOnACProjectID, "NCI_FK_ACProject_BasedOnACProjectID");

            entity.HasIndex(e => e.PAAppClassAssignmentACClassID, "NCI_FK_ACProject_PAAppClassAssignmentACClassID");

            entity.HasIndex(e => e.ACProjectNo, "UIX_ACProject_ACProjectNo").IsUnique();

            entity.Property(e => e.ACProjectID).ValueGeneratedNever();
            entity.Property(e => e.ACProjectName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.ACProjectNo)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Comment).IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLConfig).HasColumnType("text");

            entity.HasOne(d => d.ACProject1_BasedOnACProject).WithMany(p => p.ACProject_BasedOnACProject)
                .HasForeignKey(d => d.BasedOnACProjectID)
                .HasConstraintName("FK_ACProject_BasedOnACProjectID");

            entity.HasOne(d => d.PAAppClassAssignmentACClass).WithMany(p => p.ACProject_PAAppClassAssignmentACClass)
                .HasForeignKey(d => d.PAAppClassAssignmentACClassID)
                .HasConstraintName("FK_ACProject_PAAppClassAssignmentACClassID");
        });

        modelBuilder.Entity<ACPropertyLog>(entity =>
        {
            entity.ToTable("ACPropertyLog");

            entity.Property(e => e.ACPropertyLogID).ValueGeneratedNever();
            entity.Property(e => e.EventTime).HasColumnType("datetime");
            entity.Property(e => e.Value)
                .IsRequired()
                .IsUnicode(false);

            entity.HasOne(d => d.ACClass).WithMany(p => p.ACPropertyLog_ACClass)
                .HasForeignKey(d => d.ACClassID)
                .HasConstraintName("FK_ACPropertyLog_ACClass");

            entity.HasOne(d => d.ACClassProperty).WithMany(p => p.ACPropertyLog_ACClassProperty)
                .HasForeignKey(d => d.ACClassPropertyID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ACPropertyLog_ACClassProperty");
        });

        modelBuilder.Entity<ACPropertyLogRule>(entity =>
        {
            entity.ToTable("ACPropertyLogRule");

            entity.Property(e => e.ACPropertyLogRuleID).ValueGeneratedNever();
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.ACClass).WithMany(p => p.ACPropertyLogRule_ACClass)
                .HasForeignKey(d => d.ACClassID)
                .HasConstraintName("FK_ACPropertyLogRule_ACClass");
        });

        modelBuilder.Entity<MsgAlarmLog>(entity =>
        {
            entity.ToTable("MsgAlarmLog");

            entity.HasIndex(e => e.ACProgramLogID, "NCI_FK_MsgAlarmLog_ACProgramLogID");

            entity.HasIndex(e => new { e.ACClassID, e.TimeStampOccurred }, "UIX_MsgAlarmLog");

            entity.Property(e => e.MsgAlarmLogID).ValueGeneratedNever();
            entity.Property(e => e.ACIdentifier)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.AcknowledgedBy)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Message).IsUnicode(false);
            entity.Property(e => e.TimeStampAcknowledged).HasColumnType("datetime");
            entity.Property(e => e.TimeStampOccurred).HasColumnType("datetime");
            entity.Property(e => e.TranslID)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLConfig)
                .IsRequired()
                .HasColumnType("text");

            entity.HasOne(d => d.ACClass).WithMany(p => p.MsgAlarmLog_ACClass)
                .HasForeignKey(d => d.ACClassID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_MsgAlarmLog_ACClass");

            entity.HasOne(d => d.ACProgramLog).WithMany(p => p.MsgAlarmLog_ACProgramLog)
                .HasForeignKey(d => d.ACProgramLogID)
                .HasConstraintName("FK_MsgAlarmLog_ACProgramLogID");
        });

        modelBuilder.Entity<VBConfig>(entity =>
        {
            entity.ToTable("VBConfig");

            entity.HasIndex(e => e.ACClassID, "NCI_FK_VBConfig_ACClassID");

            entity.HasIndex(e => e.ACClassPropertyRelationID, "NCI_FK_VBConfig_ACClassPropertyRelationID");

            entity.HasIndex(e => e.ParentVBConfigID, "NCI_FK_VBConfig_ParentVBConfigID");

            entity.HasIndex(e => e.ValueTypeACClassID, "NCI_FK_VBConfig_ValueTypeACClassID");

            entity.Property(e => e.VBConfigID).ValueGeneratedNever();
            entity.Property(e => e.Comment).IsUnicode(false);
            entity.Property(e => e.Expression).HasColumnType("text");
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.KeyACUrl).IsUnicode(false);
            entity.Property(e => e.LocalConfigACUrl).IsUnicode(false);
            entity.Property(e => e.PreConfigACUrl).IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLConfig)
                .IsRequired()
                .HasColumnType("text");

            entity.HasOne(d => d.ACClass).WithMany(p => p.VBConfig_ACClass)
                .HasForeignKey(d => d.ACClassID)
                .HasConstraintName("FK_VBConfig_ACClassID");

            entity.HasOne(d => d.ACClassPropertyRelation).WithMany(p => p.VBConfig_ACClassPropertyRelation)
                .HasForeignKey(d => d.ACClassPropertyRelationID)
                .HasConstraintName("FK_VBConfig_ACClassPropertyRelationID");

            entity.HasOne(d => d.VBConfig1_ParentVBConfig).WithMany(p => p.VBConfig_ParentVBConfig)
                .HasForeignKey(d => d.ParentVBConfigID)
                .HasConstraintName("FK_VBConfig_ParentVBConfigID");

            entity.HasOne(d => d.ValueTypeACClass).WithMany(p => p.VBConfig_ValueTypeACClass)
                .HasForeignKey(d => d.ValueTypeACClassID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VBConfig_ValueTypeACClassID");
        });

        modelBuilder.Entity<VBGroup>(entity =>
        {
            entity.ToTable("VBGroup");

            entity.Property(e => e.VBGroupID).ValueGeneratedNever();
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.VBGroupName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VBGroupRight>(entity =>
        {
            entity.ToTable("VBGroupRight");

            entity.HasIndex(e => e.ACClassDesignID, "NCI_FK_VBGroupRight_ACClassDesignID");

            entity.HasIndex(e => e.ACClassID, "NCI_FK_VBGroupRight_ACClassID");

            entity.HasIndex(e => e.ACClassMethodID, "NCI_FK_VBGroupRight_ACClassMethodID");

            entity.HasIndex(e => e.ACClassPropertyID, "NCI_FK_VBGroupRight_ACClassPropertyID");

            entity.HasIndex(e => e.VBGroupID, "NCI_FK_VBGroupRight_VBGroupID");

            entity.Property(e => e.VBGroupRightID).ValueGeneratedNever();

            entity.HasOne(d => d.ACClassDesign).WithMany(p => p.VBGroupRight_ACClassDesign)
                .HasForeignKey(d => d.ACClassDesignID)
                .HasConstraintName("FK_VBGroupRight_ACClassDesignID");

            entity.HasOne(d => d.ACClass).WithMany(p => p.VBGroupRight_ACClass)
                .HasForeignKey(d => d.ACClassID)
                .HasConstraintName("FK_VBGroupRight_ACClassID");

            entity.HasOne(d => d.ACClassMethod).WithMany(p => p.VBGroupRight_ACClassMethod)
                .HasForeignKey(d => d.ACClassMethodID)
                .HasConstraintName("FK_VBGroupRight_ACClassMethodID");

            entity.HasOne(d => d.ACClassProperty).WithMany(p => p.VBGroupRight_ACClassProperty)
                .HasForeignKey(d => d.ACClassPropertyID)
                .HasConstraintName("FK_VBGroupRight_ACClassPropertyID");

            entity.HasOne(d => d.VBGroup).WithMany(p => p.VBGroupRight_VBGroup)
                .HasForeignKey(d => d.VBGroupID)
                .HasConstraintName("FK_VBGroupRight_VBGroupID");
        });

        modelBuilder.Entity<VBLanguage>(entity =>
        {
            entity.HasKey(e => e.VBLanguageID).HasName("PK_MDLanguage");

            entity.ToTable("VBLanguage");

            entity.HasIndex(e => e.VBKey, "UIX_MDLanguage").IsUnique();

            entity.Property(e => e.VBLanguageID).ValueGeneratedNever();
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.VBKey)
                .IsRequired()
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.VBLanguageCode)
                .IsRequired()
                .HasMaxLength(3)
                .IsUnicode(false);
            entity.Property(e => e.VBNameTrans)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.XMLConfig).HasColumnType("text");
        });

        modelBuilder.Entity<VBLicense>(entity =>
        {
            entity.ToTable("VBLicense");

            entity.Property(e => e.VBLicenseID).ValueGeneratedNever();
            entity.Property(e => e.CustomerName)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LicenseNo)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.PackageSystem).IsRequired();
            entity.Property(e => e.PackageSystem1)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.ProjectNo)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.SystemCommon)
                .IsRequired()
                .HasMaxLength(256);
            entity.Property(e => e.SystemCommon1).HasMaxLength(256);
            entity.Property(e => e.SystemDB)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.SystemDS)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.SystemKey).IsUnicode(false);
            entity.Property(e => e.SystemRemote)
                .IsRequired()
                .IsUnicode(false);
        });

        modelBuilder.Entity<VBNoConfiguration>(entity =>
        {
            entity.HasKey(e => e.VBNoConfigurationID).HasName("PK_MDNoConfiguration");

            entity.ToTable("VBNoConfiguration");

            entity.Property(e => e.VBNoConfigurationID).ValueGeneratedNever();
            entity.Property(e => e.CurrentDate).HasColumnType("datetime");
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.RowVersion)
                .IsRequired()
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UsedDelimiter)
                .IsRequired()
                .HasMaxLength(1)
                .IsUnicode(false);
            entity.Property(e => e.UsedPrefix)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.VBNoConfigurationName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.XMLConfig).HasColumnType("text");
        });

        modelBuilder.Entity<VBSystem>(entity =>
        {
            entity.ToTable("VBSystem");

            entity.Property(e => e.VBSystemID).ValueGeneratedNever();
            entity.Property(e => e.Company)
                .IsRequired()
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.CustomerName)
                .IsRequired()
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.ProjectNo)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.SystemCommon)
                .IsRequired()
                .HasMaxLength(256);
            entity.Property(e => e.SystemCommon1).HasMaxLength(256);
            entity.Property(e => e.SystemCommonPublic)
                .IsRequired()
                .IsUnicode(false);
            entity.Property(e => e.SystemInternal1).HasMaxLength(256);
            entity.Property(e => e.SystemInternal2).IsUnicode(false);
            entity.Property(e => e.SystemInternal3).HasMaxLength(256);
            entity.Property(e => e.SystemName)
                .IsRequired()
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.SystemPrivate).IsUnicode(false);
            entity.Property(e => e.SystemRemote).IsUnicode(false);
        });

        modelBuilder.Entity<VBSystemColumn>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VBSystemColumns");

            entity.Property(e => e.columnname)
                .IsRequired()
                .HasMaxLength(128);
            entity.Property(e => e.columntype).HasMaxLength(128);
            entity.Property(e => e.tablename)
                .IsRequired()
                .HasMaxLength(128);
        });

        modelBuilder.Entity<VBTranslationView>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VBTranslationView");

            entity.Property(e => e.ACIdentifier)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.ACProjectName)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.MandatoryACIdentifier)
                .IsRequired()
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.TableName)
                .IsRequired()
                .HasMaxLength(7)
                .IsUnicode(false);
            entity.Property(e => e.TranslationValue).IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(5)
                .IsUnicode(false);
        });

        modelBuilder.Entity<VBUser>(entity =>
        {
            entity.ToTable("VBUser");

            entity.HasIndex(e => e.MenuACClassDesignID, "NCI_FK_VBUser_MenuACClassDesignID");

            entity.HasIndex(e => e.VBLanguageID, "NCI_FK_VBUser_VBLanguageID");

            entity.Property(e => e.VBUserID).ValueGeneratedNever();
            entity.Property(e => e.Initials)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(32)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.VBUserName)
                .IsRequired()
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.VBUserNo)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLConfig).HasColumnType("text");

            entity.HasOne(d => d.MenuACClassDesign).WithMany(p => p.VBUser_MenuACClassDesign)
                .HasForeignKey(d => d.MenuACClassDesignID)
                .HasConstraintName("FK_VBUser_MenuACClassDesignID");

            entity.HasOne(d => d.VBLanguage).WithMany(p => p.VBUser_VBLanguage)
                .HasForeignKey(d => d.VBLanguageID)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_VBUser_MDLanguageID");
        });

        modelBuilder.Entity<VBUserACClassDesign>(entity =>
        {
            entity.ToTable("VBUserACClassDesign");

            entity.HasIndex(e => e.ACClassDesignID, "NCI_FK_VBUserACClassDesign_ACClassDesignID");

            entity.HasIndex(e => e.VBUserID, "NCI_FK_VBUserACClassDesign_VBUserID");

            entity.Property(e => e.VBUserACClassDesignID).ValueGeneratedNever();
            entity.Property(e => e.ACIdentifier)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.XMLDesign)
                .IsRequired()
                .HasColumnType("text");

            entity.HasOne(d => d.ACClassDesign).WithMany(p => p.VBUserACClassDesign_ACClassDesign)
                .HasForeignKey(d => d.ACClassDesignID)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_VBUserACClassDesign_ACClassDesignID");

            entity.HasOne(d => d.VBUser).WithMany(p => p.VBUserACClassDesign_VBUser)
                .HasForeignKey(d => d.VBUserID)
                .HasConstraintName("FK_VBUserACClassDesign_VBUserID");
        });

        modelBuilder.Entity<VBUserACProject>(entity =>
        {
            entity.ToTable("VBUserACProject");

            entity.HasIndex(e => e.ACProjectID, "NCI_FK_VBUserACProject_ACProjectID");

            entity.HasIndex(e => e.VBUserID, "NCI_FK_VBUserACProject_VBUserID");

            entity.Property(e => e.VBUserACProjectID).ValueGeneratedNever();
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.ACProject).WithMany(p => p.VBUserACProject_ACProject)
                .HasForeignKey(d => d.ACProjectID)
                .HasConstraintName("FK_VBUserACProject_ACProjectID");

            entity.HasOne(d => d.VBUser).WithMany(p => p.VBUserACProject_VBUser)
                .HasForeignKey(d => d.VBUserID)
                .HasConstraintName("FK_VBUserACProject_VBUserID");
        });

        modelBuilder.Entity<VBUserGroup>(entity =>
        {
            entity.ToTable("VBUserGroup");

            entity.HasIndex(e => e.VBGroupID, "NCI_FK_VBUserGroup_VBGroupID");

            entity.HasIndex(e => e.VBUserID, "NCI_FK_VBUserGroup_VBUserID");

            entity.Property(e => e.VBUserGroupID).ValueGeneratedNever();
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.VBGroup).WithMany(p => p.VBUserGroup_VBGroup)
                .HasForeignKey(d => d.VBGroupID)
                .HasConstraintName("FK_VBUserGroup_VBGroupID");

            entity.HasOne(d => d.VBUser).WithMany(p => p.VBUserGroup_VBUser)
                .HasForeignKey(d => d.VBUserID)
                .HasConstraintName("FK_VBUserGroup_VBUserID");
        });

        modelBuilder.Entity<VBUserInstance>(entity =>
        {
            entity.ToTable("VBUserInstance");

            entity.HasIndex(e => e.VBUserID, "NCI_FK_VBUserInstance_VBUserID");

            entity.Property(e => e.VBUserInstanceID).ValueGeneratedNever();
            entity.Property(e => e.Hostname)
                .IsRequired()
                .HasMaxLength(100);
            entity.Property(e => e.InsertDate).HasColumnType("datetime");
            entity.Property(e => e.InsertName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.LoginDate).HasColumnType("datetime");
            entity.Property(e => e.LogoutDate).HasColumnType("datetime");
            entity.Property(e => e.ServerIPV4)
                .IsRequired()
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.ServerIPV6)
                .IsRequired()
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.SessionInfo).HasColumnType("text");
            entity.Property(e => e.UpdateDate).HasColumnType("datetime");
            entity.Property(e => e.UpdateName)
                .IsRequired()
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.VBUser).WithMany(p => p.VBUserInstance_VBUser)
                .HasForeignKey(d => d.VBUserID)
                .HasConstraintName("FK_VBUserInstance_VBUserID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
