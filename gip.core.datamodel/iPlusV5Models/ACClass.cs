using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClass : VBEntityObject
{
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

    public virtual ICollection<ACChangeLog> ACChangeLog_ACClass { get; } = new List<ACChangeLog>();

    public virtual CollectionEntry ACChangeLog_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACChangeLog_ACClass"); }
    }

    public virtual ICollection<ACClassConfig> ACClassConfig_ACClass { get; } = new List<ACClassConfig>();

    public virtual CollectionEntry ACClassConfig_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassConfig_ACClass"); }
    }

    public virtual ICollection<ACClassConfig> ACClassConfig_ValueTypeACClass { get; } = new List<ACClassConfig>();

    public virtual CollectionEntry ACClassConfig_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassConfig_ValueTypeACClass"); }
    }

    public virtual ICollection<ACClassDesign> ACClassDesign_ACClass { get; } = new List<ACClassDesign>();

    public virtual CollectionEntry ACClassDesign_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassDesign_ACClass"); }
    }

    public virtual ICollection<ACClassDesign> ACClassDesign_ValueTypeACClass { get; } = new List<ACClassDesign>();

    public virtual CollectionEntry ACClassDesign_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassDesign_ValueTypeACClass"); }
    }

    public virtual ICollection<ACClassMessage> ACClassMessage_ACClass { get; } = new List<ACClassMessage>();

    public virtual CollectionEntry ACClassMessage_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassMessage_ACClass"); }
    }

    public virtual ICollection<ACClassMethod> ACClassMethod_ACClass { get; } = new List<ACClassMethod>();

    public virtual CollectionEntry ACClassMethod_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassMethod_ACClass"); }
    }

    public virtual ICollection<ACClassMethod> ACClassMethod_AttachedFromACClass { get; } = new List<ACClassMethod>();

    public virtual CollectionEntry ACClassMethod_AttachedFromACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassMethod_AttachedFromACClass"); }
    }

    public virtual ICollection<ACClassMethodConfig> ACClassMethodConfig_VBiACClass { get; } = new List<ACClassMethodConfig>();

    public virtual CollectionEntry ACClassMethodConfig_VBiACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassMethodConfig_VBiACClass"); }
    }

    public virtual ICollection<ACClassMethodConfig> ACClassMethodConfig_ValueTypeACClass { get; } = new List<ACClassMethodConfig>();

    public virtual CollectionEntry ACClassMethodConfig_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassMethodConfig_ValueTypeACClass"); }
    }

    public virtual ICollection<ACClassMethod> ACClassMethod_PWACClass { get; } = new List<ACClassMethod>();

    public virtual CollectionEntry ACClassMethod_PWACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassMethod_PWACClass"); }
    }

    public virtual ICollection<ACClassMethod> ACClassMethod_ValueTypeACClass { get; } = new List<ACClassMethod>();

    public virtual CollectionEntry ACClassMethod_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassMethod_ValueTypeACClass"); }
    }

    public virtual ICollection<ACClassProperty> ACClassProperty_ACClass { get; } = new List<ACClassProperty>();

    public virtual CollectionEntry ACClassProperty_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassProperty_ACClass"); }
    }

    public virtual ICollection<ACClassProperty> ACClassProperty_ConfigACClass { get; } = new List<ACClassProperty>();

    public virtual CollectionEntry ACClassProperty_ConfigACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassProperty_ConfigACClass"); }
    }

    public virtual ICollection<ACClassPropertyRelation> ACClassPropertyRelation_SourceACClass { get; } = new List<ACClassPropertyRelation>();

    public virtual CollectionEntry ACClassPropertyRelation_SourceACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassPropertyRelation_SourceACClass"); }
    }

    public virtual ICollection<ACClassPropertyRelation> ACClassPropertyRelation_TargetACClass { get; } = new List<ACClassPropertyRelation>();

    public virtual CollectionEntry ACClassPropertyRelation_TargetACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassPropertyRelation_TargetACClass"); }
    }

    public virtual ICollection<ACClassProperty> ACClassProperty_ValueTypeACClass { get; } = new List<ACClassProperty>();

    public virtual CollectionEntry ACClassProperty_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassProperty_ValueTypeACClass"); }
    }

    public virtual ICollection<ACClassTask> ACClassTask_TaskTypeACClass { get; } = new List<ACClassTask>();

    public virtual CollectionEntry ACClassTask_TaskTypeACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassTask_TaskTypeACClass"); }
    }

    public virtual ICollection<ACClassText> ACClassText_ACClass { get; } = new List<ACClassText>();

    public virtual CollectionEntry ACClassText_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassText_ACClass"); }
    }

    public virtual ICollection<ACClassWF> ACClassWF_PWACClass { get; } = new List<ACClassWF>();

    public virtual CollectionEntry ACClassWF_PWACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassWF_PWACClass"); }
    }

    public virtual ICollection<ACClassWF> ACClassWF_RefPAACClass { get; } = new List<ACClassWF>();

    public virtual CollectionEntry ACClassWF_RefPAACClassReference
    {
        get { return Context.Entry(this).Collection("ACClassWF_RefPAACClass"); }
    }

    public virtual ACPackage ACPackage { get; set; }

    public virtual ReferenceEntry ACPackageReference 
    { 
        get { return Context.Entry(this).Reference("ACPackage"); }
    }
    
    public virtual ICollection<ACProgramConfig> ACProgramConfig_ACClass { get; } = new List<ACProgramConfig>();

    public virtual CollectionEntry ACProgramConfig_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACProgramConfig_ACClass"); }
    }

    public virtual ICollection<ACProgramConfig> ACProgramConfig_ValueTypeACClass { get; } = new List<ACProgramConfig>();

    public virtual CollectionEntry ACProgramConfig_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection("ACProgramConfig_ValueTypeACClass"); }
    }

    public virtual ICollection<ACProgram> ACProgram_WorkflowTypeACClass { get; } = new List<ACProgram>();

    public virtual CollectionEntry ACProgram_WorkflowTypeACClassReference
    {
        get { return Context.Entry(this).Collection("ACProgram_WorkflowTypeACClass"); }
    }

    public virtual ACProject ACProject { get; set; }

    public virtual ReferenceEntry ACProjectReference 
    { 
        get { return Context.Entry(this).Reference("ACProject"); }
    }
    
    public virtual ICollection<ACProject> ACProject_PAAppClassAssignmentACClass { get; } = new List<ACProject>();

    public virtual CollectionEntry ACProject_PAAppClassAssignmentACClassReference
    {
        get { return Context.Entry(this).Collection("ACProject_PAAppClassAssignmentACClass"); }
    }

    public virtual ICollection<ACPropertyLogRule> ACPropertyLogRule_ACClass { get; } = new List<ACPropertyLogRule>();

    public virtual CollectionEntry ACPropertyLogRule_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACPropertyLogRule_ACClass"); }
    }

    public virtual ICollection<ACPropertyLog> ACPropertyLog_ACClass { get; } = new List<ACPropertyLog>();

    public virtual CollectionEntry ACPropertyLog_ACClassReference
    {
        get { return Context.Entry(this).Collection("ACPropertyLog_ACClass"); }
    }

    public virtual ACClass ACClass1_BasedOnACClass { get; set; }

    public virtual ReferenceEntry ACClass1_BasedOnACClassReference 
    { 
        get { return Context.Entry(this).Reference("ACClass1_BasedOnACClass"); }
    }
    
    public virtual ICollection<ACClass> ACClass_BasedOnACClass { get; } = new List<ACClass>();

    public virtual CollectionEntry ACClass_BasedOnACClassReference
    {
        get { return Context.Entry(this).Collection("ACClass_BasedOnACClass"); }
    }

    public virtual ICollection<ACClass> ACClass_PWACClass { get; } = new List<ACClass>();

    public virtual CollectionEntry ACClass_PWACClassReference
    {
        get { return Context.Entry(this).Collection("ACClass_PWACClass"); }
    }

    public virtual ICollection<ACClass> ACClass_PWMethodACClass { get; } = new List<ACClass>();

    public virtual CollectionEntry ACClass_PWMethodACClassReference
    {
        get { return Context.Entry(this).Collection("ACClass_PWMethodACClass"); }
    }

    public virtual ICollection<ACClass> ACClass_ParentACClass { get; } = new List<ACClass>();

    public virtual CollectionEntry ACClass_ParentACClassReference
    {
        get { return Context.Entry(this).Collection("ACClass_ParentACClass"); }
    }

    public virtual ICollection<MsgAlarmLog> MsgAlarmLog_ACClass { get; } = new List<MsgAlarmLog>();

    public virtual CollectionEntry MsgAlarmLog_ACClassReference
    {
        get { return Context.Entry(this).Collection("MsgAlarmLog_ACClass"); }
    }

    public virtual ACClass ACClass1_PWACClass { get; set; }

    public virtual ReferenceEntry ACClass1_PWACClassReference 
    { 
        get { return Context.Entry(this).Reference("ACClass1_PWACClass"); }
    }
    
    public virtual ACClass ACClass1_PWMethodACClass { get; set; }

    public virtual ReferenceEntry ACClass1_PWMethodACClassReference 
    { 
        get { return Context.Entry(this).Reference("ACClass1_PWMethodACClass"); }
    }
    
    public virtual ACClass ACClass1_ParentACClass { get; set; }

    public virtual ReferenceEntry ACClass1_ParentACClassReference 
    { 
        get { return Context.Entry(this).Reference("ACClass1_ParentACClass"); }
    }
    
    public virtual ICollection<VBConfig> VBConfig_ACClass { get; } = new List<VBConfig>();

    public virtual CollectionEntry VBConfig_ACClassReference
    {
        get { return Context.Entry(this).Collection("VBConfig_ACClass"); }
    }

    public virtual ICollection<VBConfig> VBConfig_ValueTypeACClass { get; } = new List<VBConfig>();

    public virtual CollectionEntry VBConfig_ValueTypeACClassReference
    {
        get { return Context.Entry(this).Collection("VBConfig_ValueTypeACClass"); }
    }

    public virtual ICollection<VBGroupRight> VBGroupRight_ACClass { get; } = new List<VBGroupRight>();

    public virtual CollectionEntry VBGroupRight_ACClassReference
    {
        get { return Context.Entry(this).Collection("VBGroupRight_ACClass"); }
    }
}
