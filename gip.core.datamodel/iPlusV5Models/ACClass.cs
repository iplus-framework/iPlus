using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClass : VBEntityObject
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
    public string ACIdentifier 
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
    public string XMLConfig 
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
        get => LazyLoader.Load(this, ref _ACChangeLog_ACClass);
        set => _ACChangeLog_ACClass = value;
    }

    public virtual CollectionEntry ACChangeLog_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACChangeLog_ACClass"); }
    }

    private ICollection<ACClassConfig> _ACClassConfig_ACClass;
    public virtual ICollection<ACClassConfig> ACClassConfig_ACClass
    {
        get => LazyLoader.Load(this, ref _ACClassConfig_ACClass);
        set => _ACClassConfig_ACClass = value;
    }

    public virtual CollectionEntry ACClassConfig_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassConfig_ACClass"); }
    }

    private ICollection<ACClassConfig> _ACClassConfig_ValueTypeACClass;
    public virtual ICollection<ACClassConfig> ACClassConfig_ValueTypeACClass
    {
        get => LazyLoader.Load(this, ref _ACClassConfig_ValueTypeACClass);
        set => _ACClassConfig_ValueTypeACClass = value;
    }

    public virtual CollectionEntry ACClassConfig_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassConfig_ValueTypeACClass"); }
    }

    private ICollection<ACClassDesign> _ACClassDesign_ACClass;
    public virtual ICollection<ACClassDesign> ACClassDesign_ACClass
    {
        get => LazyLoader.Load(this, ref _ACClassDesign_ACClass);
        set => _ACClassDesign_ACClass = value;
    }

    public virtual CollectionEntry ACClassDesign_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassDesign_ACClass"); }
    }

    private ICollection<ACClassDesign> _ACClassDesign_ValueTypeACClass;
    public virtual ICollection<ACClassDesign> ACClassDesign_ValueTypeACClass
    {
        get => LazyLoader.Load(this, ref _ACClassDesign_ValueTypeACClass);
        set => _ACClassDesign_ValueTypeACClass = value;
    }

    public virtual CollectionEntry ACClassDesign_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassDesign_ValueTypeACClass"); }
    }

    private ICollection<ACClassMessage> _ACClassMessage_ACClass;
    public virtual ICollection<ACClassMessage> ACClassMessage_ACClass
    {
        get => LazyLoader.Load(this, ref _ACClassMessage_ACClass);
        set => _ACClassMessage_ACClass = value;
    }

    public virtual CollectionEntry ACClassMessage_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassMessage_ACClass"); }
    }

    private ICollection<ACClassMethod> _ACClassMethod_ACClass;
    public virtual ICollection<ACClassMethod> ACClassMethod_ACClass
    {
        get => LazyLoader.Load(this, ref _ACClassMethod_ACClass);
        set => _ACClassMethod_ACClass = value;
    }

    public virtual CollectionEntry ACClassMethod_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassMethod_ACClass"); }
    }

    private ICollection<ACClassMethod> _ACClassMethod_AttachedFromACClass;
    public virtual ICollection<ACClassMethod> ACClassMethod_AttachedFromACClass
    {
        get => LazyLoader.Load(this, ref _ACClassMethod_AttachedFromACClass);
        set => _ACClassMethod_AttachedFromACClass = value;
    }

    public virtual CollectionEntry ACClassMethod_AttachedFromACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassMethod_AttachedFromACClass"); }
    }

    private ICollection<ACClassMethodConfig> _ACClassMethodConfig_VBiACClass;
    public virtual ICollection<ACClassMethodConfig> ACClassMethodConfig_VBiACClass
    {
        get => LazyLoader.Load(this, ref _ACClassMethodConfig_VBiACClass);
        set => _ACClassMethodConfig_VBiACClass = value;
    }

    public virtual CollectionEntry ACClassMethodConfig_VBiACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassMethodConfig_VBiACClass"); }
    }

    private ICollection<ACClassMethodConfig> _ACClassMethodConfig_ValueTypeACClass;
    public virtual ICollection<ACClassMethodConfig> ACClassMethodConfig_ValueTypeACClass
    {
        get => LazyLoader.Load(this, ref _ACClassMethodConfig_ValueTypeACClass);
        set => _ACClassMethodConfig_ValueTypeACClass = value;
    }

    public virtual CollectionEntry ACClassMethodConfig_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassMethodConfig_ValueTypeACClass"); }
    }

    private ICollection<ACClassMethod> _ACClassMethod_PWACClass;
    public virtual ICollection<ACClassMethod> ACClassMethod_PWACClass
    {
        get => LazyLoader.Load(this, ref _ACClassMethod_PWACClass);
        set => _ACClassMethod_PWACClass = value;
    }

    public virtual CollectionEntry ACClassMethod_PWACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassMethod_PWACClass"); }
    }

    private ICollection<ACClassMethod> _ACClassMethod_ValueTypeACClass;
    public virtual ICollection<ACClassMethod> ACClassMethod_ValueTypeACClass
    {
        get => LazyLoader.Load(this, ref _ACClassMethod_ValueTypeACClass);
        set => _ACClassMethod_ValueTypeACClass = value;
    }

    public virtual CollectionEntry ACClassMethod_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassMethod_ValueTypeACClass"); }
    }

    private ICollection<ACClassProperty> _ACClassProperty_ACClass;
    public virtual ICollection<ACClassProperty> ACClassProperty_ACClass
    {
        get => LazyLoader.Load(this, ref _ACClassProperty_ACClass);
        set => _ACClassProperty_ACClass = value;
    }

    public virtual CollectionEntry ACClassProperty_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassProperty_ACClass"); }
    }

    private ICollection<ACClassProperty> _ACClassProperty_ConfigACClass;
    public virtual ICollection<ACClassProperty> ACClassProperty_ConfigACClass
    {
        get => LazyLoader.Load(this, ref _ACClassProperty_ConfigACClass);
        set => _ACClassProperty_ConfigACClass = value;
    }

    public virtual CollectionEntry ACClassProperty_ConfigACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassProperty_ConfigACClass"); }
    }

    private ICollection<ACClassPropertyRelation> _ACClassPropertyRelation_SourceACClass;
    public virtual ICollection<ACClassPropertyRelation> ACClassPropertyRelation_SourceACClass
    {
        get => LazyLoader.Load(this, ref _ACClassPropertyRelation_SourceACClass);
        set => _ACClassPropertyRelation_SourceACClass = value;
    }

    public virtual CollectionEntry ACClassPropertyRelation_SourceACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassPropertyRelation_SourceACClass"); }
    }

    private ICollection<ACClassPropertyRelation> _ACClassPropertyRelation_TargetACClass;
    public virtual ICollection<ACClassPropertyRelation> ACClassPropertyRelation_TargetACClass
    {
        get => LazyLoader.Load(this, ref _ACClassPropertyRelation_TargetACClass);
        set => _ACClassPropertyRelation_TargetACClass = value;
    }

    public virtual CollectionEntry ACClassPropertyRelation_TargetACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassPropertyRelation_TargetACClass"); }
    }

    private ICollection<ACClassProperty> _ACClassProperty_ValueTypeACClass;
    public virtual ICollection<ACClassProperty> ACClassProperty_ValueTypeACClass
    {
        get => LazyLoader.Load(this, ref _ACClassProperty_ValueTypeACClass);
        set => _ACClassProperty_ValueTypeACClass = value;
    }

    public virtual CollectionEntry ACClassProperty_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassProperty_ValueTypeACClass"); }
    }

    private ICollection<ACClassTask> _ACClassTask_TaskTypeACClass;
    public virtual ICollection<ACClassTask> ACClassTask_TaskTypeACClass
    {
        get => LazyLoader.Load(this, ref _ACClassTask_TaskTypeACClass);
        set => _ACClassTask_TaskTypeACClass = value;
    }

    public virtual CollectionEntry ACClassTask_TaskTypeACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassTask_TaskTypeACClass"); }
    }

    private ICollection<ACClassText> _ACClassText_ACClass;
    public virtual ICollection<ACClassText> ACClassText_ACClass
    {
        get => LazyLoader.Load(this, ref _ACClassText_ACClass);
        set => _ACClassText_ACClass = value;
    }

    public virtual CollectionEntry ACClassText_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassText_ACClass"); }
    }

    private ICollection<ACClassWF> _ACClassWF_PWACClass;
    public virtual ICollection<ACClassWF> ACClassWF_PWACClass
    {
        get => LazyLoader.Load(this, ref _ACClassWF_PWACClass);
        set => _ACClassWF_PWACClass = value;
    }

    public virtual CollectionEntry ACClassWF_PWACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassWF_PWACClass"); }
    }

    private ICollection<ACClassWF> _ACClassWF_RefPAACClass;
    public virtual ICollection<ACClassWF> ACClassWF_RefPAACClass
    {
        get => LazyLoader.Load(this, ref _ACClassWF_RefPAACClass);
        set => _ACClassWF_RefPAACClass = value;
    }

    public virtual CollectionEntry ACClassWF_RefPAACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassWF_RefPAACClass"); }
    }

    private ACPackage _ACPackage;
    public virtual ACPackage ACPackage
    { 
        get => LazyLoader.Load(this, ref _ACPackage);
        set => _ACPackage = value;
    }

    public virtual ReferenceEntry ACPackageReference 
    {
        get { return Context.Entry(this).Reference("ACPackage"); }
    }
    
    private ICollection<ACProgramConfig> _ACProgramConfig_ACClass;
    public virtual ICollection<ACProgramConfig> ACProgramConfig_ACClass
    {
        get => LazyLoader.Load(this, ref _ACProgramConfig_ACClass);
        set => _ACProgramConfig_ACClass = value;
    }

    public virtual CollectionEntry ACProgramConfig_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACProgramConfig_ACClass"); }
    }

    private ICollection<ACProgramConfig> _ACProgramConfig_ValueTypeACClass;
    public virtual ICollection<ACProgramConfig> ACProgramConfig_ValueTypeACClass
    {
        get => LazyLoader.Load(this, ref _ACProgramConfig_ValueTypeACClass);
        set => _ACProgramConfig_ValueTypeACClass = value;
    }

    public virtual CollectionEntry ACProgramConfig_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection("ACProgramConfig_ValueTypeACClass"); }
    }

    private ICollection<ACProgram> _ACProgram_WorkflowTypeACClass;
    public virtual ICollection<ACProgram> ACProgram_WorkflowTypeACClass
    {
        get => LazyLoader.Load(this, ref _ACProgram_WorkflowTypeACClass);
        set => _ACProgram_WorkflowTypeACClass = value;
    }

    public virtual CollectionEntry ACProgram_WorkflowTypeACClassReference
    {
        get { return Context.Entry(this).Collection("ACProgram_WorkflowTypeACClass"); }
    }

    private ACProject _ACProject;
    public virtual ACProject ACProject
    { 
        get => LazyLoader.Load(this, ref _ACProject);
        set => _ACProject = value;
    }

    public virtual ReferenceEntry ACProjectReference 
    {
        get { return Context.Entry(this).Reference("ACProject"); }
    }
    
    private ICollection<ACProject> _ACProject_PAAppClassAssignmentACClass;
    public virtual ICollection<ACProject> ACProject_PAAppClassAssignmentACClass
    {
        get => LazyLoader.Load(this, ref _ACProject_PAAppClassAssignmentACClass);
        set => _ACProject_PAAppClassAssignmentACClass = value;
    }

    public virtual CollectionEntry ACProject_PAAppClassAssignmentACClassReference
    {
        get { return Context.Entry(this).Collection("ACProject_PAAppClassAssignmentACClass"); }
    }

    private ICollection<ACPropertyLogRule> _ACPropertyLogRule_ACClass;
    public virtual ICollection<ACPropertyLogRule> ACPropertyLogRule_ACClass
    {
        get => LazyLoader.Load(this, ref _ACPropertyLogRule_ACClass);
        set => _ACPropertyLogRule_ACClass = value;
    }

    public virtual CollectionEntry ACPropertyLogRule_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACPropertyLogRule_ACClass"); }
    }

    private ICollection<ACPropertyLog> _ACPropertyLog_ACClass;
    public virtual ICollection<ACPropertyLog> ACPropertyLog_ACClass
    {
        get => LazyLoader.Load(this, ref _ACPropertyLog_ACClass);
        set => _ACPropertyLog_ACClass = value;
    }

    public virtual CollectionEntry ACPropertyLog_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACPropertyLog_ACClass"); }
    }

    private ACClass _ACClass1_BasedOnACClass;
    public virtual ACClass ACClass1_BasedOnACClass
    { 
        get => LazyLoader.Load(this, ref _ACClass1_BasedOnACClass);
        set => _ACClass1_BasedOnACClass = value;
    }

    public virtual ReferenceEntry ACClass1_BasedOnACClassReference 
    {
        get { return Context.Entry(this).Reference("ACClass1_BasedOnACClass"); }
    }
    
    private ICollection<ACClass> _ACClass_BasedOnACClass;
    public virtual ICollection<ACClass> ACClass_BasedOnACClass
    {
        get => LazyLoader.Load(this, ref _ACClass_BasedOnACClass);
        set => _ACClass_BasedOnACClass = value;
    }

    public virtual CollectionEntry ACClass_BasedOnACClassReference
    {
        get { return Context.Entry(this).Collection("ACClass_BasedOnACClass"); }
    }

    private ICollection<ACClass> _ACClass_PWACClass;
    public virtual ICollection<ACClass> ACClass_PWACClass
    {
        get => LazyLoader.Load(this, ref _ACClass_PWACClass);
        set => _ACClass_PWACClass = value;
    }

    public virtual CollectionEntry ACClass_PWACClassReference
    {
        get { return Context.Entry(this).Collection("ACClass_PWACClass"); }
    }

    private ICollection<ACClass> _ACClass_PWMethodACClass;
    public virtual ICollection<ACClass> ACClass_PWMethodACClass
    {
        get => LazyLoader.Load(this, ref _ACClass_PWMethodACClass);
        set => _ACClass_PWMethodACClass = value;
    }

    public virtual CollectionEntry ACClass_PWMethodACClassReference
    {
        get { return Context.Entry(this).Collection("ACClass_PWMethodACClass"); }
    }

    private ICollection<ACClass> _ACClass_ParentACClass;
    public virtual ICollection<ACClass> ACClass_ParentACClass
    {
        get => LazyLoader.Load(this, ref _ACClass_ParentACClass);
        set => _ACClass_ParentACClass = value;
    }

    public virtual CollectionEntry ACClass_ParentACClassReference
    {
        get { return Context.Entry(this).Collection("ACClass_ParentACClass"); }
    }

    private ICollection<MsgAlarmLog> _MsgAlarmLog_ACClass;
    public virtual ICollection<MsgAlarmLog> MsgAlarmLog_ACClass
    {
        get => LazyLoader.Load(this, ref _MsgAlarmLog_ACClass);
        set => _MsgAlarmLog_ACClass = value;
    }

    public virtual CollectionEntry MsgAlarmLog_ACClassReference
    {
        get { return Context.Entry(this).Collection("MsgAlarmLog_ACClass"); }
    }

    private ACClass _ACClass1_PWACClass;
    public virtual ACClass ACClass1_PWACClass
    { 
        get => LazyLoader.Load(this, ref _ACClass1_PWACClass);
        set => _ACClass1_PWACClass = value;
    }

    public virtual ReferenceEntry ACClass1_PWACClassReference 
    {
        get { return Context.Entry(this).Reference("ACClass1_PWACClass"); }
    }
    
    private ACClass _ACClass1_PWMethodACClass;
    public virtual ACClass ACClass1_PWMethodACClass
    { 
        get => LazyLoader.Load(this, ref _ACClass1_PWMethodACClass);
        set => _ACClass1_PWMethodACClass = value;
    }

    public virtual ReferenceEntry ACClass1_PWMethodACClassReference 
    {
        get { return Context.Entry(this).Reference("ACClass1_PWMethodACClass"); }
    }
    
    private ACClass _ACClass1_ParentACClass;
    public virtual ACClass ACClass1_ParentACClass
    { 
        get => LazyLoader.Load(this, ref _ACClass1_ParentACClass);
        set => _ACClass1_ParentACClass = value;
    }

    public virtual ReferenceEntry ACClass1_ParentACClassReference 
    {
        get { return Context.Entry(this).Reference("ACClass1_ParentACClass"); }
    }
    
    private ICollection<VBConfig> _VBConfig_ACClass;
    public virtual ICollection<VBConfig> VBConfig_ACClass
    {
        get => LazyLoader.Load(this, ref _VBConfig_ACClass);
        set => _VBConfig_ACClass = value;
    }

    public virtual CollectionEntry VBConfig_ACClassReference
    {
        get { return Context.Entry(this).Collection("VBConfig_ACClass"); }
    }

    private ICollection<VBConfig> _VBConfig_ValueTypeACClass;
    public virtual ICollection<VBConfig> VBConfig_ValueTypeACClass
    {
        get => LazyLoader.Load(this, ref _VBConfig_ValueTypeACClass);
        set => _VBConfig_ValueTypeACClass = value;
    }

    public virtual CollectionEntry VBConfig_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection("VBConfig_ValueTypeACClass"); }
    }

    private ICollection<VBGroupRight> _VBGroupRight_ACClass;
    public virtual ICollection<VBGroupRight> VBGroupRight_ACClass
    {
        get => LazyLoader.Load(this, ref _VBGroupRight_ACClass);
        set => _VBGroupRight_ACClass = value;
    }

    public virtual CollectionEntry VBGroupRight_ACClassReference
    {
        get { return Context.Entry(this).Collection("VBGroupRight_ACClass"); }
    }
}
