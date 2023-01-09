﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACProgramConfig : VBEntityObject
{

    public ACProgramConfig()
    {
    }

    private ACProgramConfig(ILazyLoader lazyLoader)
    {
        LazyLoader = lazyLoader;
    }

    private ILazyLoader LazyLoader { get; set; }
    Guid _ACProgramConfigID;
    public Guid ACProgramConfigID 
    {
        get { return _ACProgramConfigID; }
        set { SetProperty<Guid>(ref _ACProgramConfigID, value); }
    }

    Guid _ACProgramID;
    public Guid ACProgramID 
    {
        get { return _ACProgramID; }
        set { SetProperty<Guid>(ref _ACProgramID, value); }
    }

    Guid? _ParentACProgramConfigID;
    public Guid? ParentACProgramConfigID 
    {
        get { return _ParentACProgramConfigID; }
        set { SetProperty<Guid?>(ref _ParentACProgramConfigID, value); }
    }

    Guid? _ACClassID;
    public Guid? ACClassID 
    {
        get { return _ACClassID; }
        set { SetProperty<Guid?>(ref _ACClassID, value); }
    }

    Guid? _ACClassPropertyRelationID;
    public Guid? ACClassPropertyRelationID 
    {
        get { return _ACClassPropertyRelationID; }
        set { SetProperty<Guid?>(ref _ACClassPropertyRelationID, value); }
    }

    Guid _ValueTypeACClassID;
    public Guid ValueTypeACClassID 
    {
        get { return _ValueTypeACClassID; }
        set { SetProperty<Guid>(ref _ValueTypeACClassID, value); }
    }

    string _KeyACUrl;
    public string KeyACUrl 
    {
        get { return _KeyACUrl; }
        set { SetProperty<string>(ref _KeyACUrl, value); }
    }

    string _PreConfigACUrl;
    public string PreConfigACUrl 
    {
        get { return _PreConfigACUrl; }
        set { SetProperty<string>(ref _PreConfigACUrl, value); }
    }

    string _LocalConfigACUrl;
    public string LocalConfigACUrl 
    {
        get { return _LocalConfigACUrl; }
        set { SetProperty<string>(ref _LocalConfigACUrl, value); }
    }

    string _Expression;
    public string Expression 
    {
        get { return _Expression; }
        set { SetProperty<string>(ref _Expression, value); }
    }

    string _Comment;
    public string Comment 
    {
        get { return _Comment; }
        set { SetProperty<string>(ref _Comment, value); }
    }

    string _XMLConfig;
    public override string XMLConfig 
    {
        get { return _XMLConfig; }
        set { SetProperty<string>(ref _XMLConfig, value); }
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
    
    private ACClassPropertyRelation _ACClassPropertyRelation;
    public virtual ACClassPropertyRelation ACClassPropertyRelation
    { 
        get => LazyLoader.Load(this, ref _ACClassPropertyRelation);
        set => _ACClassPropertyRelation = value;
    }

    public virtual ReferenceEntry ACClassPropertyRelationReference 
    {
        get { return Context.Entry(this).Reference("ACClassPropertyRelation"); }
    }
    
    private ACProgram _ACProgram;
    public virtual ACProgram ACProgram
    { 
        get => LazyLoader.Load(this, ref _ACProgram);
        set => _ACProgram = value;
    }

    public virtual ReferenceEntry ACProgramReference 
    {
        get { return Context.Entry(this).Reference("ACProgram"); }
    }
    
    private ICollection<ACProgramConfig> _ACProgramConfig_ParentACProgramConfig;
    public virtual ICollection<ACProgramConfig> ACProgramConfig_ParentACProgramConfig
    {
        get => LazyLoader.Load(this, ref _ACProgramConfig_ParentACProgramConfig);
        set => _ACProgramConfig_ParentACProgramConfig = value;
    }

    public virtual CollectionEntry ACProgramConfig_ParentACProgramConfigReference
    {
        get { return Context.Entry(this).Collection("ACProgramConfig_ParentACProgramConfig"); }
    }

    private ACProgramConfig _ACProgramConfig1_ParentACProgramConfig;
    public virtual ACProgramConfig ACProgramConfig1_ParentACProgramConfig
    { 
        get => LazyLoader.Load(this, ref _ACProgramConfig1_ParentACProgramConfig);
        set => _ACProgramConfig1_ParentACProgramConfig = value;
    }

    public virtual ReferenceEntry ACProgramConfig1_ParentACProgramConfigReference 
    {
        get { return Context.Entry(this).Reference("ACProgramConfig1_ParentACProgramConfig"); }
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
