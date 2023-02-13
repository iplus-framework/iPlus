using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClassMethod : VBEntityObject , IInsertInfo, IUpdateInfo
{

    public ACClassMethod()
    {
    }

    private ACClassMethod(ILazyLoader lazyLoader)
    {
        LazyLoader = lazyLoader;
    }

    private ILazyLoader LazyLoader { get; set; }
    Guid _ACClassMethodID;
    public Guid ACClassMethodID 
    {
        get { return _ACClassMethodID; }
        set { SetProperty<Guid>(ref _ACClassMethodID, value); }
    }

    Guid _ACClassID;
    public Guid ACClassID 
    {
        get { return _ACClassID; }
        set { SetProperty<Guid>(ref _ACClassID, value); }
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

    string _ACGroup;
    public string ACGroup 
    {
        get { return _ACGroup; }
        set { SetProperty<string>(ref _ACGroup, value); }
    }

    string _Sourcecode;
    public string Sourcecode 
    {
        get { return _Sourcecode; }
        set { SetProperty<string>(ref _Sourcecode, value); }
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

    bool _IsRightmanagement;
    public bool IsRightmanagement 
    {
        get { return _IsRightmanagement; }
        set { SetProperty<bool>(ref _IsRightmanagement, value); }
    }

    string _Comment;
    public string Comment 
    {
        get { return _Comment; }
        set { SetProperty<string>(ref _Comment, value); }
    }

    bool _IsCommand;
    public bool IsCommand 
    {
        get { return _IsCommand; }
        set { SetProperty<bool>(ref _IsCommand, value); }
    }

    bool _IsInteraction;
    public bool IsInteraction 
    {
        get { return _IsInteraction; }
        set { SetProperty<bool>(ref _IsInteraction, value); }
    }

    bool _IsAsyncProcess;
    public bool IsAsyncProcess 
    {
        get { return _IsAsyncProcess; }
        set { SetProperty<bool>(ref _IsAsyncProcess, value); }
    }

    bool _IsPeriodic;
    public bool IsPeriodic 
    {
        get { return _IsPeriodic; }
        set { SetProperty<bool>(ref _IsPeriodic, value); }
    }

    bool _IsParameterACMethod;
    public bool IsParameterACMethod 
    {
        get { return _IsParameterACMethod; }
        set { SetProperty<bool>(ref _IsParameterACMethod, value); }
    }

    bool _IsSubMethod;
    public bool IsSubMethod 
    {
        get { return _IsSubMethod; }
        set { SetProperty<bool>(ref _IsSubMethod, value); }
    }

    string _InteractionVBContent;
    public string InteractionVBContent 
    {
        get { return _InteractionVBContent; }
        set { SetProperty<string>(ref _InteractionVBContent, value); }
    }

    bool _IsAutoenabled;
    public bool IsAutoenabled 
    {
        get { return _IsAutoenabled; }
        set { SetProperty<bool>(ref _IsAutoenabled, value); }
    }

    bool _IsPersistable;
    public bool IsPersistable 
    {
        get { return _IsPersistable; }
        set { SetProperty<bool>(ref _IsPersistable, value); }
    }

    Guid? _PWACClassID;
    public Guid? PWACClassID 
    {
        get { return _PWACClassID; }
        set { SetProperty<Guid?>(ref _PWACClassID, value); }
    }

    bool _ContinueByError;
    public bool ContinueByError 
    {
        get { return _ContinueByError; }
        set { SetProperty<bool>(ref _ContinueByError, value); }
    }

    Guid? _ValueTypeACClassID;
    public Guid? ValueTypeACClassID 
    {
        get { return _ValueTypeACClassID; }
        set { SetProperty<Guid?>(ref _ValueTypeACClassID, value); }
    }

    string _GenericType;
    public string GenericType 
    {
        get { return _GenericType; }
        set { SetProperty<string>(ref _GenericType, value); }
    }

    Guid? _ParentACClassMethodID;
    public Guid? ParentACClassMethodID 
    {
        get { return _ParentACClassMethodID; }
        set { SetProperty<Guid?>(ref _ParentACClassMethodID, value); }
    }

    string _XMLACMethod;
    public string XMLACMethod 
    {
        get { return _XMLACMethod; }
        set { SetProperty<string>(ref _XMLACMethod, value); }
    }

    string _XMLDesign;
    public string XMLDesign 
    {
        get { return _XMLDesign; }
        set { SetProperty<string>(ref _XMLDesign, value); }
    }

    string _XMLConfig;
    public override string XMLConfig 
    {
        get { return _XMLConfig; }
        set { SetProperty<string>(ref _XMLConfig, value); }
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

    short? _ContextMenuCategoryIndex;
    public short? ContextMenuCategoryIndex 
    {
        get { return _ContextMenuCategoryIndex; }
        set { SetProperty<short?>(ref _ContextMenuCategoryIndex, value); }
    }

    bool _IsRPCEnabled;
    public bool IsRPCEnabled 
    {
        get { return _IsRPCEnabled; }
        set { SetProperty<bool>(ref _IsRPCEnabled, value); }
    }

    Guid? _AttachedFromACClassID;
    public Guid? AttachedFromACClassID 
    {
        get { return _AttachedFromACClassID; }
        set { SetProperty<Guid?>(ref _AttachedFromACClassID, value); }
    }

    bool _IsStatic;
    public bool IsStatic 
    {
        get { return _IsStatic; }
        set { SetProperty<bool>(ref _IsStatic, value); }
    }

    bool _ExecuteByDoubleClick;
    public bool ExecuteByDoubleClick 
    {
        get { return _ExecuteByDoubleClick; }
        set { SetProperty<bool>(ref _ExecuteByDoubleClick, value); }
    }

    private ACClass _ACClass;
    public virtual ACClass ACClass
    { 
        get => LazyLoader.Load(this, ref _ACClass);
        set => _ACClass = value;
    }

    public virtual ReferenceEntry ACClassReference 
    {
        get { return Context.Entry(this).Reference("ACClass"); }
    }
    
    private ICollection<ACClassMethodConfig> _ACClassMethodConfig_ACClassMethod;
    public virtual ICollection<ACClassMethodConfig> ACClassMethodConfig_ACClassMethod
    {
        get => LazyLoader.Load(this, ref _ACClassMethodConfig_ACClassMethod);
        set => _ACClassMethodConfig_ACClassMethod = value;
    }

    public virtual CollectionEntry ACClassMethodConfig_ACClassMethodReference
    {
        get { return Context.Entry(this).Collection("ACClassMethodConfig_ACClassMethod"); }
    }

    private ICollection<ACClassWF> _ACClassWF_ACClassMethod;
    public virtual ICollection<ACClassWF> ACClassWF_ACClassMethod
    {
        get => LazyLoader.Load(this, ref _ACClassWF_ACClassMethod);
        set => _ACClassWF_ACClassMethod = value;
    }

    public virtual CollectionEntry ACClassWF_ACClassMethodReference
    {
        get { return Context.Entry(this).Collection("ACClassWF_ACClassMethod"); }
    }

    private ICollection<ACClassWFEdge> _ACClassWFEdge_ACClassMethod;
    public virtual ICollection<ACClassWFEdge> ACClassWFEdge_ACClassMethod
    {
        get => LazyLoader.Load(this, ref _ACClassWFEdge_ACClassMethod);
        set => _ACClassWFEdge_ACClassMethod = value;
    }

    public virtual CollectionEntry ACClassWFEdge_ACClassMethodReference
    {
        get { return Context.Entry(this).Collection("ACClassWFEdge_ACClassMethod"); }
    }

    private ICollection<ACClassWFEdge> _ACClassWFEdge_SourceACClassMethod;
    public virtual ICollection<ACClassWFEdge> ACClassWFEdge_SourceACClassMethod
    {
        get => LazyLoader.Load(this, ref _ACClassWFEdge_SourceACClassMethod);
        set => _ACClassWFEdge_SourceACClassMethod = value;
    }

    public virtual CollectionEntry ACClassWFEdge_SourceACClassMethodReference
    {
        get { return Context.Entry(this).Collection("ACClassWFEdge_SourceACClassMethod"); }
    }

    private ICollection<ACClassWFEdge> _ACClassWFEdge_TargetACClassMethod;
    public virtual ICollection<ACClassWFEdge> ACClassWFEdge_TargetACClassMethod
    {
        get => LazyLoader.Load(this, ref _ACClassWFEdge_TargetACClassMethod);
        set => _ACClassWFEdge_TargetACClassMethod = value;
    }

    public virtual CollectionEntry ACClassWFEdge_TargetACClassMethodReference
    {
        get { return Context.Entry(this).Collection("ACClassWFEdge_TargetACClassMethod"); }
    }

    private ICollection<ACClassWF> _ACClassWF_RefPAACClassMethod;
    public virtual ICollection<ACClassWF> ACClassWF_RefPAACClassMethod
    {
        get => LazyLoader.Load(this, ref _ACClassWF_RefPAACClassMethod);
        set => _ACClassWF_RefPAACClassMethod = value;
    }

    public virtual CollectionEntry ACClassWF_RefPAACClassMethodReference
    {
        get { return Context.Entry(this).Collection("ACClassWF_RefPAACClassMethod"); }
    }

    private ICollection<ACProgram> _ACProgram_ProgramACClassMethod;
    public virtual ICollection<ACProgram> ACProgram_ProgramACClassMethod
    {
        get => LazyLoader.Load(this, ref _ACProgram_ProgramACClassMethod);
        set => _ACProgram_ProgramACClassMethod = value;
    }

    public virtual CollectionEntry ACProgram_ProgramACClassMethodReference
    {
        get { return Context.Entry(this).Collection("ACProgram_ProgramACClassMethod"); }
    }

    private ACClass _AttachedFromACClass;
    public virtual ACClass AttachedFromACClass
    { 
        get => LazyLoader.Load(this, ref _AttachedFromACClass);
        set => _AttachedFromACClass = value;
    }

    public virtual ReferenceEntry AttachedFromACClassReference 
    {
        get { return Context.Entry(this).Reference("AttachedFromACClass"); }
    }
    
    private ICollection<ACClassMethod> _ACClassMethod_ParentACClassMethod;
    public virtual ICollection<ACClassMethod> ACClassMethod_ParentACClassMethod
    {
        get => LazyLoader.Load(this, ref _ACClassMethod_ParentACClassMethod);
        set => _ACClassMethod_ParentACClassMethod = value;
    }

    public virtual CollectionEntry ACClassMethod_ParentACClassMethodReference
    {
        get { return Context.Entry(this).Collection("ACClassMethod_ParentACClassMethod"); }
    }

    private ACClass _PWACClass;
    public virtual ACClass PWACClass
    { 
        get => LazyLoader.Load(this, ref _PWACClass);
        set => _PWACClass = value;
    }

    public virtual ReferenceEntry PWACClassReference 
    {
        get { return Context.Entry(this).Reference("PWACClass"); }
    }
    
    private ACClassMethod _ACClassMethod1_ParentACClassMethod;
    public virtual ACClassMethod ACClassMethod1_ParentACClassMethod
    { 
        get => LazyLoader.Load(this, ref _ACClassMethod1_ParentACClassMethod);
        set => _ACClassMethod1_ParentACClassMethod = value;
    }

    public virtual ReferenceEntry ACClassMethod1_ParentACClassMethodReference 
    {
        get { return Context.Entry(this).Reference("ACClassMethod1_ParentACClassMethod"); }
    }
    
    private ICollection<VBGroupRight> _VBGroupRight_ACClassMethod;
    public virtual ICollection<VBGroupRight> VBGroupRight_ACClassMethod
    {
        get => LazyLoader.Load(this, ref _VBGroupRight_ACClassMethod);
        set => _VBGroupRight_ACClassMethod = value;
    }

    public virtual CollectionEntry VBGroupRight_ACClassMethodReference
    {
        get { return Context.Entry(this).Collection("VBGroupRight_ACClassMethod"); }
    }

    private ACClass _ValueTypeACClass;
    public virtual ACClass ValueTypeACClass
    { 
        get => LazyLoader.Load(this, ref _ValueTypeACClass);
        set => _ValueTypeACClass = value;
    }

    public virtual ReferenceEntry ValueTypeACClassReference 
    {
        get { return Context.Entry(this).Reference("ValueTypeACClass"); }
    }
    }
