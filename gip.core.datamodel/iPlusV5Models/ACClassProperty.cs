using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClassProperty : VBEntityObject
{
    Guid _ACClassPropertyID;
    public Guid ACClassPropertyID 
    { 
        get { return _ACClassPropertyID; }
        set { SetProperty<Guid>(ref _ACClassPropertyID, value); } 
    }

    Guid _ACClassID;
    public Guid ACClassID 
    { 
        get { return _ACClassID; }
        set { SetProperty<Guid>(ref _ACClassID, value); } 
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

    string _ACGroup;
    public string ACGroup 
    { 
        get { return _ACGroup; }
        set { SetProperty<string>(ref _ACGroup, value); } 
    }

    Guid _BasedOnACClassPropertyID;
    public Guid BasedOnACClassPropertyID 
    { 
        get { return _BasedOnACClassPropertyID; }
        set { SetProperty<Guid>(ref _BasedOnACClassPropertyID, value); } 
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

    string _ACSource;
    public string ACSource 
    { 
        get { return _ACSource; }
        set { SetProperty<string>(ref _ACSource, value); } 
    }

    string _Comment;
    public string Comment 
    { 
        get { return _Comment; }
        set { SetProperty<string>(ref _Comment, value); } 
    }

    bool _IsInteraction;
    public bool IsInteraction 
    { 
        get { return _IsInteraction; }
        set { SetProperty<bool>(ref _IsInteraction, value); } 
    }

    Guid _ValueTypeACClassID;
    public Guid ValueTypeACClassID 
    { 
        get { return _ValueTypeACClassID; }
        set { SetProperty<Guid>(ref _ValueTypeACClassID, value); } 
    }

    string _GenericType;
    public string GenericType 
    { 
        get { return _GenericType; }
        set { SetProperty<string>(ref _GenericType, value); } 
    }

    Guid? _ConfigACClassID;
    public Guid? ConfigACClassID 
    { 
        get { return _ConfigACClassID; }
        set { SetProperty<Guid?>(ref _ConfigACClassID, value); } 
    }

    short _ACPropUsageIndex;
    public short ACPropUsageIndex 
    { 
        get { return _ACPropUsageIndex; }
        set { SetProperty<short>(ref _ACPropUsageIndex, value); } 
    }

    short _DeleteActionIndex;
    public short DeleteActionIndex 
    { 
        get { return _DeleteActionIndex; }
        set { SetProperty<short>(ref _DeleteActionIndex, value); } 
    }

    bool _IsBroadcast;
    public bool IsBroadcast 
    { 
        get { return _IsBroadcast; }
        set { SetProperty<bool>(ref _IsBroadcast, value); } 
    }

    bool _ForceBroadcast;
    public bool ForceBroadcast 
    { 
        get { return _ForceBroadcast; }
        set { SetProperty<bool>(ref _ForceBroadcast, value); } 
    }

    bool _IsProxyProperty;
    public bool IsProxyProperty 
    { 
        get { return _IsProxyProperty; }
        set { SetProperty<bool>(ref _IsProxyProperty, value); } 
    }

    bool _IsInput;
    public bool IsInput 
    { 
        get { return _IsInput; }
        set { SetProperty<bool>(ref _IsInput, value); } 
    }

    bool _IsOutput;
    public bool IsOutput 
    { 
        get { return _IsOutput; }
        set { SetProperty<bool>(ref _IsOutput, value); } 
    }

    bool _IsContent;
    public bool IsContent 
    { 
        get { return _IsContent; }
        set { SetProperty<bool>(ref _IsContent, value); } 
    }

    bool _IsPersistable;
    public bool IsPersistable 
    { 
        get { return _IsPersistable; }
        set { SetProperty<bool>(ref _IsPersistable, value); } 
    }

    bool _IsSerializable;
    public bool IsSerializable 
    { 
        get { return _IsSerializable; }
        set { SetProperty<bool>(ref _IsSerializable, value); } 
    }

    bool _IsEnumerable;
    public bool IsEnumerable 
    { 
        get { return _IsEnumerable; }
        set { SetProperty<bool>(ref _IsEnumerable, value); } 
    }

    int _ACPointCapacity;
    public int ACPointCapacity 
    { 
        get { return _ACPointCapacity; }
        set { SetProperty<int>(ref _ACPointCapacity, value); } 
    }

    string _CallbackMethodName;
    public string CallbackMethodName 
    { 
        get { return _CallbackMethodName; }
        set { SetProperty<string>(ref _CallbackMethodName, value); } 
    }

    Guid? _ParentACClassPropertyID;
    public Guid? ParentACClassPropertyID 
    { 
        get { return _ParentACClassPropertyID; }
        set { SetProperty<Guid?>(ref _ParentACClassPropertyID, value); } 
    }

    int _DataTypeLength;
    public int DataTypeLength 
    { 
        get { return _DataTypeLength; }
        set { SetProperty<int>(ref _DataTypeLength, value); } 
    }

    bool _IsNullable;
    public bool IsNullable 
    { 
        get { return _IsNullable; }
        set { SetProperty<bool>(ref _IsNullable, value); } 
    }

    string _InputMask;
    public string InputMask 
    { 
        get { return _InputMask; }
        set { SetProperty<string>(ref _InputMask, value); } 
    }

    int? _MinLength;
    public int? MinLength 
    { 
        get { return _MinLength; }
        set { SetProperty<int?>(ref _MinLength, value); } 
    }

    int? _MaxLength;
    public int? MaxLength 
    { 
        get { return _MaxLength; }
        set { SetProperty<int?>(ref _MaxLength, value); } 
    }

    double? _MinValue;
    public double? MinValue 
    { 
        get { return _MinValue; }
        set { SetProperty<double?>(ref _MinValue, value); } 
    }

    double? _MaxValue;
    public double? MaxValue 
    { 
        get { return _MaxValue; }
        set { SetProperty<double?>(ref _MaxValue, value); } 
    }

    string _XMLValue;
    public string XMLValue 
    { 
        get { return _XMLValue; }
        set { SetProperty<string>(ref _XMLValue, value); } 
    }

    short _LogRefreshRateIndex;
    public short LogRefreshRateIndex 
    { 
        get { return _LogRefreshRateIndex; }
        set { SetProperty<short>(ref _LogRefreshRateIndex, value); } 
    }

    double? _LogFilter;
    public double? LogFilter 
    { 
        get { return _LogFilter; }
        set { SetProperty<double?>(ref _LogFilter, value); } 
    }

    short? _Precision;
    public short? Precision 
    { 
        get { return _Precision; }
        set { SetProperty<short?>(ref _Precision, value); } 
    }

    string _XMLACEventArgs;
    public string XMLACEventArgs 
    { 
        get { return _XMLACEventArgs; }
        set { SetProperty<string>(ref _XMLACEventArgs, value); } 
    }

    string _XMLConfig;
    public string XMLConfig 
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

    bool _IsRPCEnabled;
    public bool IsRPCEnabled 
    { 
        get { return _IsRPCEnabled; }
        set { SetProperty<bool>(ref _IsRPCEnabled, value); } 
    }

    int _RemotePropID;
    public int RemotePropID 
    { 
        get { return _RemotePropID; }
        set { SetProperty<int>(ref _RemotePropID, value); } 
    }

    int? _ChangeLogMax;
    public int? ChangeLogMax 
    { 
        get { return _ChangeLogMax; }
        set { SetProperty<int?>(ref _ChangeLogMax, value); } 
    }

    int _LogBufferSize;
    public int LogBufferSize 
    { 
        get { return _LogBufferSize; }
        set { SetProperty<int>(ref _LogBufferSize, value); } 
    }

    bool _IsStatic;
    public bool IsStatic 
    { 
        get { return _IsStatic; }
        set { SetProperty<bool>(ref _IsStatic, value); } 
    }

    public virtual ICollection<ACChangeLog> ACChangeLog_ACClassProperty { get; } = new List<ACChangeLog>();

    public virtual CollectionEntry ACChangeLog_ACClassPropertyReference
    {
        get { return Context.Entry(this).Collection("ACChangeLog_ACClassProperty"); }
    }

    public virtual ACClass ACClass { get; set; }

    public virtual ReferenceEntry ACClassReference 
    { 
        get { return Context.Entry(this).Reference("ACClass"); }
    }
    
    public virtual ICollection<ACClassPropertyRelation> ACClassPropertyRelation_SourceACClassProperty { get; } = new List<ACClassPropertyRelation>();

    public virtual CollectionEntry ACClassPropertyRelation_SourceACClassPropertyReference
    {
        get { return Context.Entry(this).Collection("ACClassPropertyRelation_SourceACClassProperty"); }
    }

    public virtual ICollection<ACClassPropertyRelation> ACClassPropertyRelation_TargetACClassProperty { get; } = new List<ACClassPropertyRelation>();

    public virtual CollectionEntry ACClassPropertyRelation_TargetACClassPropertyReference
    {
        get { return Context.Entry(this).Collection("ACClassPropertyRelation_TargetACClassProperty"); }
    }

    public virtual ICollection<ACClassTaskValue> ACClassTaskValue_ACClassProperty { get; } = new List<ACClassTaskValue>();

    public virtual CollectionEntry ACClassTaskValue_ACClassPropertyReference
    {
        get { return Context.Entry(this).Collection("ACClassTaskValue_ACClassProperty"); }
    }

    public virtual ICollection<ACClassWFEdge> ACClassWFEdge_SourceACClassProperty { get; } = new List<ACClassWFEdge>();

    public virtual CollectionEntry ACClassWFEdge_SourceACClassPropertyReference
    {
        get { return Context.Entry(this).Collection("ACClassWFEdge_SourceACClassProperty"); }
    }

    public virtual ICollection<ACClassWFEdge> ACClassWFEdge_TargetACClassProperty { get; } = new List<ACClassWFEdge>();

    public virtual CollectionEntry ACClassWFEdge_TargetACClassPropertyReference
    {
        get { return Context.Entry(this).Collection("ACClassWFEdge_TargetACClassProperty"); }
    }

    public virtual ICollection<ACPropertyLog> ACPropertyLog_ACClassProperty { get; } = new List<ACPropertyLog>();

    public virtual CollectionEntry ACPropertyLog_ACClassPropertyReference
    {
        get { return Context.Entry(this).Collection("ACPropertyLog_ACClassProperty"); }
    }

    public virtual ACClassProperty ACClassProperty1_BasedOnACClassProperty { get; set; }

    public virtual ReferenceEntry ACClassProperty1_BasedOnACClassPropertyReference 
    { 
        get { return Context.Entry(this).Reference("ACClassProperty1_BasedOnACClassProperty"); }
    }
    
    public virtual ACClass ConfigACClass { get; set; }

    public virtual ReferenceEntry ConfigACClassReference 
    { 
        get { return Context.Entry(this).Reference("ConfigACClass"); }
    }
    
    public virtual ICollection<ACClassProperty> ACClassProperty_BasedOnACClassProperty { get; } = new List<ACClassProperty>();

    public virtual CollectionEntry ACClassProperty_BasedOnACClassPropertyReference
    {
        get { return Context.Entry(this).Collection("ACClassProperty_BasedOnACClassProperty"); }
    }

    public virtual ICollection<ACClassProperty> ACClassProperty_ParentACClassProperty { get; } = new List<ACClassProperty>();

    public virtual CollectionEntry ACClassProperty_ParentACClassPropertyReference
    {
        get { return Context.Entry(this).Collection("ACClassProperty_ParentACClassProperty"); }
    }

    public virtual ACClassProperty ACClassProperty1_ParentACClassProperty { get; set; }

    public virtual ReferenceEntry ACClassProperty1_ParentACClassPropertyReference 
    { 
        get { return Context.Entry(this).Reference("ACClassProperty1_ParentACClassProperty"); }
    }
    
    public virtual ICollection<VBGroupRight> VBGroupRight_ACClassProperty { get; } = new List<VBGroupRight>();

    public virtual CollectionEntry VBGroupRight_ACClassPropertyReference
    {
        get { return Context.Entry(this).Collection("VBGroupRight_ACClassProperty"); }
    }

    public virtual ACClass ValueTypeACClass { get; set; }

    public virtual ReferenceEntry ValueTypeACClassReference 
    { 
        get { return Context.Entry(this).Reference("ValueTypeACClass"); }
    }
    }
