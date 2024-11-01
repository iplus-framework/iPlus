﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClassPropertyRelation : VBEntityObject , IInsertInfo, IUpdateInfo
{

    public ACClassPropertyRelation()
    {
    }

    private ACClassPropertyRelation(ILazyLoader lazyLoader)
    {
        LazyLoader = lazyLoader;
    }

    private ILazyLoader LazyLoader { get; set; }
    Guid _ACClassPropertyRelationID;
    public Guid ACClassPropertyRelationID 
    {
        get { return _ACClassPropertyRelationID; }
        set { SetProperty<Guid>(ref _ACClassPropertyRelationID, value); }
    }

    Guid _SourceACClassID;
    public Guid SourceACClassID 
    {
        get { return _SourceACClassID; }
        set { SetProperty<Guid>(ref _SourceACClassID, value); }
    }

    Guid? _SourceACClassPropertyID;
    public Guid? SourceACClassPropertyID 
    {
        get { return _SourceACClassPropertyID; }
        set { SetProperty<Guid?>(ref _SourceACClassPropertyID, value); }
    }

    Guid _TargetACClassID;
    public Guid TargetACClassID 
    {
        get { return _TargetACClassID; }
        set { SetProperty<Guid>(ref _TargetACClassID, value); }
    }

    Guid? _TargetACClassPropertyID;
    public Guid? TargetACClassPropertyID 
    {
        get { return _TargetACClassPropertyID; }
        set { SetProperty<Guid?>(ref _TargetACClassPropertyID, value); }
    }

    short _ConnectionTypeIndex;
    public short ConnectionTypeIndex 
    {
        get { return _ConnectionTypeIndex; }
        set { SetProperty<short>(ref _ConnectionTypeIndex, value); }
    }

    short _DirectionIndex;
    public short DirectionIndex 
    {
        get { return _DirectionIndex; }
        set { SetProperty<short>(ref _DirectionIndex, value); }
    }

    string _XMLValue;
    public string XMLValue 
    {
        get { return _XMLValue; }
        set { SetProperty<string>(ref _XMLValue, value); }
    }

    short _LogicalOperationIndex;
    public short LogicalOperationIndex 
    {
        get { return _LogicalOperationIndex; }
        set { SetProperty<short>(ref _LogicalOperationIndex, value); }
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

    double? _Multiplier;
    public double? Multiplier 
    {
        get { return _Multiplier; }
        set { SetProperty<double?>(ref _Multiplier, value); }
    }

    double? _Divisor;
    public double? Divisor 
    {
        get { return _Divisor; }
        set { SetProperty<double?>(ref _Divisor, value); }
    }

    string _ConvExpressionT;
    public string ConvExpressionT 
    {
        get { return _ConvExpressionT; }
        set { SetProperty<string>(ref _ConvExpressionT, value); }
    }

    string _ConvExpressionS;
    public string ConvExpressionS 
    {
        get { return _ConvExpressionS; }
        set { SetProperty<string>(ref _ConvExpressionS, value); }
    }

    short _RelationWeight;
    public short RelationWeight 
    {
        get { return _RelationWeight; }
        set { SetProperty<short>(ref _RelationWeight, value); }
    }

    short _UseFactor;
    public short UseFactor 
    {
        get { return _UseFactor; }
        set { SetProperty<short>(ref _UseFactor, value); }
    }

    DateTime _LastManipulationDT;
    public DateTime LastManipulationDT 
    {
        get { return _LastManipulationDT; }
        set { SetProperty<DateTime>(ref _LastManipulationDT, value); }
    }

    bool _IsDeactivated;
    public bool IsDeactivated 
    {
        get { return _IsDeactivated; }
        set { SetProperty<bool>(ref _IsDeactivated, value); }
    }

    short? _DisplayGroup;
    public short? DisplayGroup 
    {
        get { return _DisplayGroup; }
        set { SetProperty<short?>(ref _DisplayGroup, value); }
    }

    string _GroupName;
    public string GroupName 
    {
        get { return _GroupName; }
        set { SetProperty<string>(ref _GroupName, value); }
    }

    string _StateName;
    public string StateName 
    {
        get { return _StateName; }
        set { SetProperty<string>(ref _StateName, value); }
    }

    private ICollection<ACClassConfig> _ACClassConfig_ACClassPropertyRelation;
    public virtual ICollection<ACClassConfig> ACClassConfig_ACClassPropertyRelation
    {
        get { return LazyLoader.Load(this, ref _ACClassConfig_ACClassPropertyRelation); }
        set { _ACClassConfig_ACClassPropertyRelation = value; }
    }

    public bool ACClassConfig_ACClassPropertyRelation_IsLoaded
    {
        get
        {
            return ACClassConfig_ACClassPropertyRelation != null;
        }
    }

    public virtual CollectionEntry ACClassConfig_ACClassPropertyRelationReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassConfig_ACClassPropertyRelation); }
    }

    private ICollection<ACClassMethodConfig> _ACClassMethodConfig_VBiACClassPropertyRelation;
    public virtual ICollection<ACClassMethodConfig> ACClassMethodConfig_VBiACClassPropertyRelation
    {
        get { return LazyLoader.Load(this, ref _ACClassMethodConfig_VBiACClassPropertyRelation); }
        set { _ACClassMethodConfig_VBiACClassPropertyRelation = value; }
    }

    public bool ACClassMethodConfig_VBiACClassPropertyRelation_IsLoaded
    {
        get
        {
            return ACClassMethodConfig_VBiACClassPropertyRelation != null;
        }
    }

    public virtual CollectionEntry ACClassMethodConfig_VBiACClassPropertyRelationReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassMethodConfig_VBiACClassPropertyRelation); }
    }

    private ICollection<ACProgramConfig> _ACProgramConfig_ACClassPropertyRelation;
    public virtual ICollection<ACProgramConfig> ACProgramConfig_ACClassPropertyRelation
    {
        get { return LazyLoader.Load(this, ref _ACProgramConfig_ACClassPropertyRelation); }
        set { _ACProgramConfig_ACClassPropertyRelation = value; }
    }

    public bool ACProgramConfig_ACClassPropertyRelation_IsLoaded
    {
        get
        {
            return ACProgramConfig_ACClassPropertyRelation != null;
        }
    }

    public virtual CollectionEntry ACProgramConfig_ACClassPropertyRelationReference
    {
        get { return Context.Entry(this).Collection(c => c.ACProgramConfig_ACClassPropertyRelation); }
    }

    private ACClass _SourceACClass;
    public virtual ACClass SourceACClass
    { 
        get { return LazyLoader.Load(this, ref _SourceACClass); }
        set { SetProperty<ACClass>(ref _SourceACClass, value); }
    }

    public bool SourceACClass_IsLoaded
    {
        get
        {
            return SourceACClass != null;
        }
    }

    public virtual ReferenceEntry SourceACClassReference 
    {
        get { return Context.Entry(this).Reference("SourceACClass"); }
    }
    
    private ACClassProperty _SourceACClassProperty;
    public virtual ACClassProperty SourceACClassProperty
    { 
        get { return LazyLoader.Load(this, ref _SourceACClassProperty); }
        set { SetProperty<ACClassProperty>(ref _SourceACClassProperty, value); }
    }

    public bool SourceACClassProperty_IsLoaded
    {
        get
        {
            return SourceACClassProperty != null;
        }
    }

    public virtual ReferenceEntry SourceACClassPropertyReference 
    {
        get { return Context.Entry(this).Reference("SourceACClassProperty"); }
    }
    
    private ACClass _TargetACClass;
    public virtual ACClass TargetACClass
    { 
        get { return LazyLoader.Load(this, ref _TargetACClass); }
        set { SetProperty<ACClass>(ref _TargetACClass, value); }
    }

    public bool TargetACClass_IsLoaded
    {
        get
        {
            return TargetACClass != null;
        }
    }

    public virtual ReferenceEntry TargetACClassReference 
    {
        get { return Context.Entry(this).Reference("TargetACClass"); }
    }
    
    private ACClassProperty _TargetACClassProperty;
    public virtual ACClassProperty TargetACClassProperty
    { 
        get { return LazyLoader.Load(this, ref _TargetACClassProperty); }
        set { SetProperty<ACClassProperty>(ref _TargetACClassProperty, value); }
    }

    public bool TargetACClassProperty_IsLoaded
    {
        get
        {
            return TargetACClassProperty != null;
        }
    }

    public virtual ReferenceEntry TargetACClassPropertyReference 
    {
        get { return Context.Entry(this).Reference("TargetACClassProperty"); }
    }
    
    private ICollection<VBConfig> _VBConfig_ACClassPropertyRelation;
    public virtual ICollection<VBConfig> VBConfig_ACClassPropertyRelation
    {
        get { return LazyLoader.Load(this, ref _VBConfig_ACClassPropertyRelation); }
        set { _VBConfig_ACClassPropertyRelation = value; }
    }

    public bool VBConfig_ACClassPropertyRelation_IsLoaded
    {
        get
        {
            return VBConfig_ACClassPropertyRelation != null;
        }
    }

    public virtual CollectionEntry VBConfig_ACClassPropertyRelationReference
    {
        get { return Context.Entry(this).Collection(c => c.VBConfig_ACClassPropertyRelation); }
    }
}