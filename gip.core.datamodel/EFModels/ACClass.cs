﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClass : VBEntityObject , IInsertInfo, IUpdateInfo
{

    public ACClass()
    {
    }

    private ACClass(ILazyLoader lazyLoader)
    {
        LazyLoader = lazyLoader;
    }

    private ILazyLoader LazyLoader { get; set; }
    Guid _ACClassID;
    public Guid ACClassID 
    {
        get { return _ACClassID; }
        set { SetProperty<Guid>(ref _ACClassID, value); }
    }

    Guid _ACProjectID;
    public Guid ACProjectID 
    {
        get { return _ACProjectID; }
        set { SetProperty<Guid>(ref _ACProjectID, value); }
    }

    Guid? _BasedOnACClassID;
    public Guid? BasedOnACClassID 
    {
        get { return _BasedOnACClassID; }
        set { SetProperty<Guid?>(ref _BasedOnACClassID, value); }
    }

    Guid? _ParentACClassID;
    public Guid? ParentACClassID 
    {
        get { return _ParentACClassID; }
        set { SetProperty<Guid?>(ref _ParentACClassID, value); }
    }

    string _ACIdentifier;
    public override string ACIdentifier 
    {
        get { return _ACIdentifier; }
        set { SetProperty<string>(ref _ACIdentifier, value); }
    }

    int? _ACIdentifierKey;
    public int? ACIdentifierKey 
    {
        get { return _ACIdentifierKey; }
        set { SetProperty<int?>(ref _ACIdentifierKey, value); }
    }

    string _ACCaptionTranslation;
    public string ACCaptionTranslation 
    {
        get { return _ACCaptionTranslation; }
        set { SetProperty<string>(ref _ACCaptionTranslation, value); }
    }

    short _ACKindIndex;
    public short ACKindIndex 
    {
        get { return _ACKindIndex; }
        set { SetProperty<short>(ref _ACKindIndex, value); }
    }

    short _SortIndex;
    public short SortIndex 
    {
        get { return _SortIndex; }
        set { SetProperty<short>(ref _SortIndex, value); }
    }

    Guid _ACPackageID;
    public Guid ACPackageID 
    {
        get { return _ACPackageID; }
        set { SetProperty<Guid>(ref _ACPackageID, value); }
    }

    string _AssemblyQualifiedName;
    public string AssemblyQualifiedName 
    {
        get { return _AssemblyQualifiedName; }
        set { SetProperty<string>(ref _AssemblyQualifiedName, value); }
    }

    Guid? _PWACClassID;
    public Guid? PWACClassID 
    {
        get { return _PWACClassID; }
        set { SetProperty<Guid?>(ref _PWACClassID, value); }
    }

    Guid? _PWMethodACClassID;
    public Guid? PWMethodACClassID 
    {
        get { return _PWMethodACClassID; }
        set { SetProperty<Guid?>(ref _PWMethodACClassID, value); }
    }

    string _Comment;
    public string Comment 
    {
        get { return _Comment; }
        set { SetProperty<string>(ref _Comment, value); }
    }

    bool _IsAutostart;
    public bool IsAutostart 
    {
        get { return _IsAutostart; }
        set { SetProperty<bool>(ref _IsAutostart, value); }
    }

    bool _IsAbstract;
    public bool IsAbstract 
    {
        get { return _IsAbstract; }
        set { SetProperty<bool>(ref _IsAbstract, value); }
    }

    short _ACStartTypeIndex;
    public short ACStartTypeIndex 
    {
        get { return _ACStartTypeIndex; }
        set { SetProperty<short>(ref _ACStartTypeIndex, value); }
    }

    short _ACStorableTypeIndex;
    public short ACStorableTypeIndex 
    {
        get { return _ACStorableTypeIndex; }
        set { SetProperty<short>(ref _ACStorableTypeIndex, value); }
    }

    bool _IsAssembly;
    public bool IsAssembly 
    {
        get { return _IsAssembly; }
        set { SetProperty<bool>(ref _IsAssembly, value); }
    }

    bool _IsMultiInstance;
    public bool IsMultiInstance 
    {
        get { return _IsMultiInstance; }
        set { SetProperty<bool>(ref _IsMultiInstance, value); }
    }

    bool _IsRightmanagement;
    public bool IsRightmanagement 
    {
        get { return _IsRightmanagement; }
        set { SetProperty<bool>(ref _IsRightmanagement, value); }
    }

    string _ACSortColumns;
    public string ACSortColumns 
    {
        get { return _ACSortColumns; }
        set { SetProperty<string>(ref _ACSortColumns, value); }
    }

    string _ACFilterColumns;
    public string ACFilterColumns 
    {
        get { return _ACFilterColumns; }
        set { SetProperty<string>(ref _ACFilterColumns, value); }
    }

    string _XMLConfig;
    public override string XMLConfig 
    {
        get { return _XMLConfig; }
        set { SetProperty<string>(ref _XMLConfig, value); }
    }

    string _XMLACClass;
    public string XMLACClass 
    {
        get { return _XMLACClass; }
        set { SetProperty<string>(ref _XMLACClass, value); }
    }

    int _BranchNo;
    public int BranchNo 
    {
        get { return _BranchNo; }
        set { SetProperty<int>(ref _BranchNo, value); }
    }

    string _InsertName;
    public string InsertName 
    {
        get { return _InsertName; }
        set { SetProperty<string>(ref _InsertName, value); }
    }

    DateTime _InsertDate;
    public DateTime InsertDate 
    {
        get { return _InsertDate; }
        set { SetProperty<DateTime>(ref _InsertDate, value); }
    }

    string _UpdateName;
    public string UpdateName 
    {
        get { return _UpdateName; }
        set { SetProperty<string>(ref _UpdateName, value); }
    }

    DateTime _UpdateDate;
    public DateTime UpdateDate 
    {
        get { return _UpdateDate; }
        set { SetProperty<DateTime>(ref _UpdateDate, value); }
    }

    int? _ChangeLogMax;
    public int? ChangeLogMax 
    {
        get { return _ChangeLogMax; }
        set { SetProperty<int?>(ref _ChangeLogMax, value); }
    }

    string _ACURLCached;
    public string ACURLCached 
    {
        get { return _ACURLCached; }
        set { SetProperty<string>(ref _ACURLCached, value); }
    }

    string _ACURLComponentCached;
    public string ACURLComponentCached 
    {
        get { return _ACURLComponentCached; }
        set { SetProperty<string>(ref _ACURLComponentCached, value); }
    }

    bool _IsStatic;
    public bool IsStatic 
    {
        get { return _IsStatic; }
        set { SetProperty<bool>(ref _IsStatic, value); }
    }

    private ICollection<ACChangeLog> _ACChangeLog_ACClass;
    public virtual ICollection<ACChangeLog> ACChangeLog_ACClass
    {
        get { return LazyLoader.Load(this, ref _ACChangeLog_ACClass); }
        set { _ACChangeLog_ACClass = value; }
    }

    public bool ACChangeLog_ACClass_IsLoaded
    {
        get
        {
            return _ACChangeLog_ACClass != null;
        }
    }

    public virtual CollectionEntry ACChangeLog_ACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACChangeLog_ACClass); }
    }

    private ICollection<ACClassConfig> _ACClassConfig_ACClass;
    public virtual ICollection<ACClassConfig> ACClassConfig_ACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassConfig_ACClass); }
        set { _ACClassConfig_ACClass = value; }
    }

    public bool ACClassConfig_ACClass_IsLoaded
    {
        get
        {
            return _ACClassConfig_ACClass != null;
        }
    }

    public virtual CollectionEntry ACClassConfig_ACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassConfig_ACClass); }
    }

    private ICollection<ACClassConfig> _ACClassConfig_ValueTypeACClass;
    public virtual ICollection<ACClassConfig> ACClassConfig_ValueTypeACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassConfig_ValueTypeACClass); }
        set { _ACClassConfig_ValueTypeACClass = value; }
    }

    public bool ACClassConfig_ValueTypeACClass_IsLoaded
    {
        get
        {
            return _ACClassConfig_ValueTypeACClass != null;
        }
    }

    public virtual CollectionEntry ACClassConfig_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassConfig_ValueTypeACClass); }
    }

    private ICollection<ACClassDesign> _ACClassDesign_ACClass;
    public virtual ICollection<ACClassDesign> ACClassDesign_ACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassDesign_ACClass); }
        set { _ACClassDesign_ACClass = value; }
    }

    public bool ACClassDesign_ACClass_IsLoaded
    {
        get
        {
            return _ACClassDesign_ACClass != null;
        }
    }

    public virtual CollectionEntry ACClassDesign_ACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassDesign_ACClass); }
    }

    private ICollection<ACClassDesign> _ACClassDesign_ValueTypeACClass;
    public virtual ICollection<ACClassDesign> ACClassDesign_ValueTypeACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassDesign_ValueTypeACClass); }
        set { _ACClassDesign_ValueTypeACClass = value; }
    }

    public bool ACClassDesign_ValueTypeACClass_IsLoaded
    {
        get
        {
            return _ACClassDesign_ValueTypeACClass != null;
        }
    }

    public virtual CollectionEntry ACClassDesign_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassDesign_ValueTypeACClass); }
    }

    private ICollection<ACClassMessage> _ACClassMessage_ACClass;
    public virtual ICollection<ACClassMessage> ACClassMessage_ACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassMessage_ACClass); }
        set { _ACClassMessage_ACClass = value; }
    }

    public bool ACClassMessage_ACClass_IsLoaded
    {
        get
        {
            return _ACClassMessage_ACClass != null;
        }
    }

    public virtual CollectionEntry ACClassMessage_ACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassMessage_ACClass); }
    }

    private ICollection<ACClassMethod> _ACClassMethod_ACClass;
    public virtual ICollection<ACClassMethod> ACClassMethod_ACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassMethod_ACClass); }
        set { _ACClassMethod_ACClass = value; }
    }

    public bool ACClassMethod_ACClass_IsLoaded
    {
        get
        {
            return _ACClassMethod_ACClass != null;
        }
    }

    public virtual CollectionEntry ACClassMethod_ACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassMethod_ACClass); }
    }

    private ICollection<ACClassMethod> _ACClassMethod_AttachedFromACClass;
    public virtual ICollection<ACClassMethod> ACClassMethod_AttachedFromACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassMethod_AttachedFromACClass); }
        set { _ACClassMethod_AttachedFromACClass = value; }
    }

    public bool ACClassMethod_AttachedFromACClass_IsLoaded
    {
        get
        {
            return _ACClassMethod_AttachedFromACClass != null;
        }
    }

    public virtual CollectionEntry ACClassMethod_AttachedFromACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassMethod_AttachedFromACClass); }
    }

    private ICollection<ACClassMethodConfig> _ACClassMethodConfig_VBiACClass;
    public virtual ICollection<ACClassMethodConfig> ACClassMethodConfig_VBiACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassMethodConfig_VBiACClass); }
        set { _ACClassMethodConfig_VBiACClass = value; }
    }

    public bool ACClassMethodConfig_VBiACClass_IsLoaded
    {
        get
        {
            return _ACClassMethodConfig_VBiACClass != null;
        }
    }

    public virtual CollectionEntry ACClassMethodConfig_VBiACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassMethodConfig_VBiACClass); }
    }

    private ICollection<ACClassMethodConfig> _ACClassMethodConfig_ValueTypeACClass;
    public virtual ICollection<ACClassMethodConfig> ACClassMethodConfig_ValueTypeACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassMethodConfig_ValueTypeACClass); }
        set { _ACClassMethodConfig_ValueTypeACClass = value; }
    }

    public bool ACClassMethodConfig_ValueTypeACClass_IsLoaded
    {
        get
        {
            return _ACClassMethodConfig_ValueTypeACClass != null;
        }
    }

    public virtual CollectionEntry ACClassMethodConfig_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassMethodConfig_ValueTypeACClass); }
    }

    private ICollection<ACClassMethod> _ACClassMethod_PWACClass;
    public virtual ICollection<ACClassMethod> ACClassMethod_PWACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassMethod_PWACClass); }
        set { _ACClassMethod_PWACClass = value; }
    }

    public bool ACClassMethod_PWACClass_IsLoaded
    {
        get
        {
            return _ACClassMethod_PWACClass != null;
        }
    }

    public virtual CollectionEntry ACClassMethod_PWACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassMethod_PWACClass); }
    }

    private ICollection<ACClassMethod> _ACClassMethod_ValueTypeACClass;
    public virtual ICollection<ACClassMethod> ACClassMethod_ValueTypeACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassMethod_ValueTypeACClass); }
        set { _ACClassMethod_ValueTypeACClass = value; }
    }

    public bool ACClassMethod_ValueTypeACClass_IsLoaded
    {
        get
        {
            return _ACClassMethod_ValueTypeACClass != null;
        }
    }

    public virtual CollectionEntry ACClassMethod_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassMethod_ValueTypeACClass); }
    }

    private ICollection<ACClassProperty> _ACClassProperty_ACClass;
    public virtual ICollection<ACClassProperty> ACClassProperty_ACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassProperty_ACClass); }
        set { _ACClassProperty_ACClass = value; }
    }

    public bool ACClassProperty_ACClass_IsLoaded
    {
        get
        {
            return _ACClassProperty_ACClass != null;
        }
    }

    public virtual CollectionEntry ACClassProperty_ACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassProperty_ACClass); }
    }

    private ICollection<ACClassProperty> _ACClassProperty_ConfigACClass;
    public virtual ICollection<ACClassProperty> ACClassProperty_ConfigACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassProperty_ConfigACClass); }
        set { _ACClassProperty_ConfigACClass = value; }
    }

    public bool ACClassProperty_ConfigACClass_IsLoaded
    {
        get
        {
            return _ACClassProperty_ConfigACClass != null;
        }
    }

    public virtual CollectionEntry ACClassProperty_ConfigACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassProperty_ConfigACClass); }
    }

    private ICollection<ACClassPropertyRelation> _ACClassPropertyRelation_SourceACClass;
    public virtual ICollection<ACClassPropertyRelation> ACClassPropertyRelation_SourceACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassPropertyRelation_SourceACClass); }
        set { _ACClassPropertyRelation_SourceACClass = value; }
    }

    public bool ACClassPropertyRelation_SourceACClass_IsLoaded
    {
        get
        {
            return _ACClassPropertyRelation_SourceACClass != null;
        }
    }

    public virtual CollectionEntry ACClassPropertyRelation_SourceACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassPropertyRelation_SourceACClass); }
    }

    private ICollection<ACClassPropertyRelation> _ACClassPropertyRelation_TargetACClass;
    public virtual ICollection<ACClassPropertyRelation> ACClassPropertyRelation_TargetACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassPropertyRelation_TargetACClass); }
        set { _ACClassPropertyRelation_TargetACClass = value; }
    }

    public bool ACClassPropertyRelation_TargetACClass_IsLoaded
    {
        get
        {
            return _ACClassPropertyRelation_TargetACClass != null;
        }
    }

    public virtual CollectionEntry ACClassPropertyRelation_TargetACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassPropertyRelation_TargetACClass); }
    }

    private ICollection<ACClassProperty> _ACClassProperty_ValueTypeACClass;
    public virtual ICollection<ACClassProperty> ACClassProperty_ValueTypeACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassProperty_ValueTypeACClass); }
        set { _ACClassProperty_ValueTypeACClass = value; }
    }

    public bool ACClassProperty_ValueTypeACClass_IsLoaded
    {
        get
        {
            return _ACClassProperty_ValueTypeACClass != null;
        }
    }

    public virtual CollectionEntry ACClassProperty_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassProperty_ValueTypeACClass); }
    }

    private ICollection<ACClassTask> _ACClassTask_TaskTypeACClass;
    public virtual ICollection<ACClassTask> ACClassTask_TaskTypeACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassTask_TaskTypeACClass); }
        set { _ACClassTask_TaskTypeACClass = value; }
    }

    public bool ACClassTask_TaskTypeACClass_IsLoaded
    {
        get
        {
            return _ACClassTask_TaskTypeACClass != null;
        }
    }

    public virtual CollectionEntry ACClassTask_TaskTypeACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassTask_TaskTypeACClass); }
    }

    private ICollection<ACClassText> _ACClassText_ACClass;
    public virtual ICollection<ACClassText> ACClassText_ACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassText_ACClass); }
        set { _ACClassText_ACClass = value; }
    }

    public bool ACClassText_ACClass_IsLoaded
    {
        get
        {
            return _ACClassText_ACClass != null;
        }
    }

    public virtual CollectionEntry ACClassText_ACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassText_ACClass); }
    }

    private ICollection<ACClassWF> _ACClassWF_PWACClass;
    public virtual ICollection<ACClassWF> ACClassWF_PWACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassWF_PWACClass); }
        set { _ACClassWF_PWACClass = value; }
    }

    public bool ACClassWF_PWACClass_IsLoaded
    {
        get
        {
            return _ACClassWF_PWACClass != null;
        }
    }

    public virtual CollectionEntry ACClassWF_PWACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassWF_PWACClass); }
    }

    private ICollection<ACClassWF> _ACClassWF_RefPAACClass;
    public virtual ICollection<ACClassWF> ACClassWF_RefPAACClass
    {
        get { return LazyLoader.Load(this, ref _ACClassWF_RefPAACClass); }
        set { _ACClassWF_RefPAACClass = value; }
    }

    public bool ACClassWF_RefPAACClass_IsLoaded
    {
        get
        {
            return _ACClassWF_RefPAACClass != null;
        }
    }

    public virtual CollectionEntry ACClassWF_RefPAACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassWF_RefPAACClass); }
    }

    private ACPackage _ACPackage;
    public virtual ACPackage ACPackage
    { 
        get { return LazyLoader.Load(this, ref _ACPackage); }
        set { SetProperty<ACPackage>(ref _ACPackage, value); }
    }

    public bool ACPackage_IsLoaded
    {
        get
        {
            return _ACPackage != null;
        }
    }

    public virtual ReferenceEntry ACPackageReference 
    {
        get { return Context.Entry(this).Reference("ACPackage"); }
    }
    
    private ICollection<ACProgramConfig> _ACProgramConfig_ACClass;
    public virtual ICollection<ACProgramConfig> ACProgramConfig_ACClass
    {
        get { return LazyLoader.Load(this, ref _ACProgramConfig_ACClass); }
        set { _ACProgramConfig_ACClass = value; }
    }

    public bool ACProgramConfig_ACClass_IsLoaded
    {
        get
        {
            return _ACProgramConfig_ACClass != null;
        }
    }

    public virtual CollectionEntry ACProgramConfig_ACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACProgramConfig_ACClass); }
    }

    private ICollection<ACProgramConfig> _ACProgramConfig_ValueTypeACClass;
    public virtual ICollection<ACProgramConfig> ACProgramConfig_ValueTypeACClass
    {
        get { return LazyLoader.Load(this, ref _ACProgramConfig_ValueTypeACClass); }
        set { _ACProgramConfig_ValueTypeACClass = value; }
    }

    public bool ACProgramConfig_ValueTypeACClass_IsLoaded
    {
        get
        {
            return _ACProgramConfig_ValueTypeACClass != null;
        }
    }

    public virtual CollectionEntry ACProgramConfig_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACProgramConfig_ValueTypeACClass); }
    }

    private ICollection<ACProgram> _ACProgram_WorkflowTypeACClass;
    public virtual ICollection<ACProgram> ACProgram_WorkflowTypeACClass
    {
        get { return LazyLoader.Load(this, ref _ACProgram_WorkflowTypeACClass); }
        set { _ACProgram_WorkflowTypeACClass = value; }
    }

    public bool ACProgram_WorkflowTypeACClass_IsLoaded
    {
        get
        {
            return _ACProgram_WorkflowTypeACClass != null;
        }
    }

    public virtual CollectionEntry ACProgram_WorkflowTypeACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACProgram_WorkflowTypeACClass); }
    }

    private ACProject _ACProject;
    public virtual ACProject ACProject
    { 
        get { return LazyLoader.Load(this, ref _ACProject); }
        set { SetProperty<ACProject>(ref _ACProject, value); }
    }

    public bool ACProject_IsLoaded
    {
        get
        {
            return _ACProject != null;
        }
    }

    public virtual ReferenceEntry ACProjectReference 
    {
        get { return Context.Entry(this).Reference("ACProject"); }
    }
    
    private ICollection<ACProject> _ACProject_PAAppClassAssignmentACClass;
    public virtual ICollection<ACProject> ACProject_PAAppClassAssignmentACClass
    {
        get { return LazyLoader.Load(this, ref _ACProject_PAAppClassAssignmentACClass); }
        set { _ACProject_PAAppClassAssignmentACClass = value; }
    }

    public bool ACProject_PAAppClassAssignmentACClass_IsLoaded
    {
        get
        {
            return _ACProject_PAAppClassAssignmentACClass != null;
        }
    }

    public virtual CollectionEntry ACProject_PAAppClassAssignmentACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACProject_PAAppClassAssignmentACClass); }
    }

    private ICollection<ACPropertyLogRule> _ACPropertyLogRule_ACClass;
    public virtual ICollection<ACPropertyLogRule> ACPropertyLogRule_ACClass
    {
        get { return LazyLoader.Load(this, ref _ACPropertyLogRule_ACClass); }
        set { _ACPropertyLogRule_ACClass = value; }
    }

    public bool ACPropertyLogRule_ACClass_IsLoaded
    {
        get
        {
            return _ACPropertyLogRule_ACClass != null;
        }
    }

    public virtual CollectionEntry ACPropertyLogRule_ACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACPropertyLogRule_ACClass); }
    }

    private ICollection<ACPropertyLog> _ACPropertyLog_ACClass;
    public virtual ICollection<ACPropertyLog> ACPropertyLog_ACClass
    {
        get { return LazyLoader.Load(this, ref _ACPropertyLog_ACClass); }
        set { _ACPropertyLog_ACClass = value; }
    }

    public bool ACPropertyLog_ACClass_IsLoaded
    {
        get
        {
            return _ACPropertyLog_ACClass != null;
        }
    }

    public virtual CollectionEntry ACPropertyLog_ACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACPropertyLog_ACClass); }
    }

    private ACClass _ACClass1_BasedOnACClass;
    public virtual ACClass ACClass1_BasedOnACClass
    { 
        get { return LazyLoader.Load(this, ref _ACClass1_BasedOnACClass); }
        set { SetProperty<ACClass>(ref _ACClass1_BasedOnACClass, value); }
    }

    public bool ACClass1_BasedOnACClass_IsLoaded
    {
        get
        {
            return _ACClass1_BasedOnACClass != null;
        }
    }

    public virtual ReferenceEntry ACClass1_BasedOnACClassReference 
    {
        get { return Context.Entry(this).Reference("ACClass1_BasedOnACClass"); }
    }
    
    private ICollection<ACClass> _ACClass_BasedOnACClass;
    public virtual ICollection<ACClass> ACClass_BasedOnACClass
    {
        get { return LazyLoader.Load(this, ref _ACClass_BasedOnACClass); }
        set { _ACClass_BasedOnACClass = value; }
    }

    public bool ACClass_BasedOnACClass_IsLoaded
    {
        get
        {
            return _ACClass_BasedOnACClass != null;
        }
    }

    public virtual CollectionEntry ACClass_BasedOnACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClass_BasedOnACClass); }
    }

    private ICollection<ACClass> _ACClass_PWACClass;
    public virtual ICollection<ACClass> ACClass_PWACClass
    {
        get { return LazyLoader.Load(this, ref _ACClass_PWACClass); }
        set { _ACClass_PWACClass = value; }
    }

    public bool ACClass_PWACClass_IsLoaded
    {
        get
        {
            return _ACClass_PWACClass != null;
        }
    }

    public virtual CollectionEntry ACClass_PWACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClass_PWACClass); }
    }

    private ICollection<ACClass> _ACClass_PWMethodACClass;
    public virtual ICollection<ACClass> ACClass_PWMethodACClass
    {
        get { return LazyLoader.Load(this, ref _ACClass_PWMethodACClass); }
        set { _ACClass_PWMethodACClass = value; }
    }

    public bool ACClass_PWMethodACClass_IsLoaded
    {
        get
        {
            return _ACClass_PWMethodACClass != null;
        }
    }

    public virtual CollectionEntry ACClass_PWMethodACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClass_PWMethodACClass); }
    }

    private ICollection<ACClass> _ACClass_ParentACClass;
    public virtual ICollection<ACClass> ACClass_ParentACClass
    {
        get { return LazyLoader.Load(this, ref _ACClass_ParentACClass); }
        set { _ACClass_ParentACClass = value; }
    }

    public bool ACClass_ParentACClass_IsLoaded
    {
        get
        {
            return _ACClass_ParentACClass != null;
        }
    }

    public virtual CollectionEntry ACClass_ParentACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClass_ParentACClass); }
    }

    private ICollection<MsgAlarmLog> _MsgAlarmLog_ACClass;
    public virtual ICollection<MsgAlarmLog> MsgAlarmLog_ACClass
    {
        get { return LazyLoader.Load(this, ref _MsgAlarmLog_ACClass); }
        set { _MsgAlarmLog_ACClass = value; }
    }

    public bool MsgAlarmLog_ACClass_IsLoaded
    {
        get
        {
            return _MsgAlarmLog_ACClass != null;
        }
    }

    public virtual CollectionEntry MsgAlarmLog_ACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.MsgAlarmLog_ACClass); }
    }

    private ACClass _ACClass1_PWACClass;
    public virtual ACClass ACClass1_PWACClass
    { 
        get { return LazyLoader.Load(this, ref _ACClass1_PWACClass); }
        set { SetProperty<ACClass>(ref _ACClass1_PWACClass, value); }
    }

    public bool ACClass1_PWACClass_IsLoaded
    {
        get
        {
            return _ACClass1_PWACClass != null;
        }
    }

    public virtual ReferenceEntry ACClass1_PWACClassReference 
    {
        get { return Context.Entry(this).Reference("ACClass1_PWACClass"); }
    }
    
    private ACClass _ACClass1_PWMethodACClass;
    public virtual ACClass ACClass1_PWMethodACClass
    { 
        get { return LazyLoader.Load(this, ref _ACClass1_PWMethodACClass); }
        set { SetProperty<ACClass>(ref _ACClass1_PWMethodACClass, value); }
    }

    public bool ACClass1_PWMethodACClass_IsLoaded
    {
        get
        {
            return _ACClass1_PWMethodACClass != null;
        }
    }

    public virtual ReferenceEntry ACClass1_PWMethodACClassReference 
    {
        get { return Context.Entry(this).Reference("ACClass1_PWMethodACClass"); }
    }
    
    private ACClass _ACClass1_ParentACClass;
    public virtual ACClass ACClass1_ParentACClass
    { 
        get { return LazyLoader.Load(this, ref _ACClass1_ParentACClass); }
        set { SetProperty<ACClass>(ref _ACClass1_ParentACClass, value); }
    }

    public bool ACClass1_ParentACClass_IsLoaded
    {
        get
        {
            return _ACClass1_ParentACClass != null;
        }
    }

    public virtual ReferenceEntry ACClass1_ParentACClassReference 
    {
        get { return Context.Entry(this).Reference("ACClass1_ParentACClass"); }
    }
    
    private ICollection<VBConfig> _VBConfig_ACClass;
    public virtual ICollection<VBConfig> VBConfig_ACClass
    {
        get { return LazyLoader.Load(this, ref _VBConfig_ACClass); }
        set { _VBConfig_ACClass = value; }
    }

    public bool VBConfig_ACClass_IsLoaded
    {
        get
        {
            return _VBConfig_ACClass != null;
        }
    }

    public virtual CollectionEntry VBConfig_ACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.VBConfig_ACClass); }
    }

    private ICollection<VBConfig> _VBConfig_ValueTypeACClass;
    public virtual ICollection<VBConfig> VBConfig_ValueTypeACClass
    {
        get { return LazyLoader.Load(this, ref _VBConfig_ValueTypeACClass); }
        set { _VBConfig_ValueTypeACClass = value; }
    }

    public bool VBConfig_ValueTypeACClass_IsLoaded
    {
        get
        {
            return _VBConfig_ValueTypeACClass != null;
        }
    }

    public virtual CollectionEntry VBConfig_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.VBConfig_ValueTypeACClass); }
    }

    private ICollection<VBGroupRight> _VBGroupRight_ACClass;
    public virtual ICollection<VBGroupRight> VBGroupRight_ACClass
    {
        get { return LazyLoader.Load(this, ref _VBGroupRight_ACClass); }
        set { _VBGroupRight_ACClass = value; }
    }

    public bool VBGroupRight_ACClass_IsLoaded
    {
        get
        {
            return _VBGroupRight_ACClass != null;
        }
    }

    public virtual CollectionEntry VBGroupRight_ACClassReference
    {
        get { return Context.Entry(this).Collection(c => c.VBGroupRight_ACClass); }
    }
}
