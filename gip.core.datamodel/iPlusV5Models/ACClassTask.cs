﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClassTask : VBEntityObject , IInsertInfo, IUpdateInfo
{

    public ACClassTask()
    {
    }

    private ACClassTask(ILazyLoader lazyLoader)
    {
        LazyLoader = lazyLoader;
    }

    private ILazyLoader LazyLoader { get; set; }
    Guid _ACClassTaskID;
    public Guid ACClassTaskID 
    {
        get { return _ACClassTaskID; }
        set { SetProperty<Guid>(ref _ACClassTaskID, value); }
    }

    Guid? _ParentACClassTaskID;
    public Guid? ParentACClassTaskID 
    {
        get { return _ParentACClassTaskID; }
        set { SetProperty<Guid?>(ref _ParentACClassTaskID, value); }
    }

    Guid _TaskTypeACClassID;
    public Guid TaskTypeACClassID 
    {
        get { return _TaskTypeACClassID; }
        set { SetProperty<Guid>(ref _TaskTypeACClassID, value); }
    }

    Guid? _ContentACClassWFID;
    public Guid? ContentACClassWFID 
    {
        get { return _ContentACClassWFID; }
        set { SetProperty<Guid?>(ref _ContentACClassWFID, value); }
    }

    Guid? _ACProgramID;
    public Guid? ACProgramID 
    {
        get { return _ACProgramID; }
        set { SetProperty<Guid?>(ref _ACProgramID, value); }
    }

    string _ACIdentifier;
    public override string ACIdentifier 
    {
        get { return _ACIdentifier; }
        set { SetProperty<string>(ref _ACIdentifier, value); }
    }

    short _ACTaskTypeIndex;
    public short ACTaskTypeIndex 
    {
        get { return _ACTaskTypeIndex; }
        set { SetProperty<short>(ref _ACTaskTypeIndex, value); }
    }

    bool _IsDynamic;
    public bool IsDynamic 
    {
        get { return _IsDynamic; }
        set { SetProperty<bool>(ref _IsDynamic, value); }
    }

    bool _IsTestmode;
    public bool IsTestmode 
    {
        get { return _IsTestmode; }
        set { SetProperty<bool>(ref _IsTestmode, value); }
    }

    string _XMLACMethod;
    public string XMLACMethod 
    {
        get { return _XMLACMethod; }
        set { SetProperty<string>(ref _XMLACMethod, value); }
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

    private ICollection<ACClassTaskValue> _ACClassTaskValue_ACClassTask;
    public virtual ICollection<ACClassTaskValue> ACClassTaskValue_ACClassTask
    {
        get => LazyLoader.Load(this, ref _ACClassTaskValue_ACClassTask);
        set => _ACClassTaskValue_ACClassTask = value;
    }

    public virtual CollectionEntry ACClassTaskValue_ACClassTaskReference
    {
        get { return Context.Entry(this).Collection("ACClassTaskValue_ACClassTask"); }
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
    
    private ACClassWF _ContentACClassWF;
    public virtual ACClassWF ContentACClassWF
    { 
        get => LazyLoader.Load(this, ref _ContentACClassWF);
        set => _ContentACClassWF = value;
    }

    public virtual ReferenceEntry ContentACClassWFReference 
    {
        get { return Context.Entry(this).Reference("ContentACClassWF"); }
    }
    
    private ICollection<ACClassTask> _ACClassTask_ParentACClassTask;
    public virtual ICollection<ACClassTask> ACClassTask_ParentACClassTask
    {
        get => LazyLoader.Load(this, ref _ACClassTask_ParentACClassTask);
        set => _ACClassTask_ParentACClassTask = value;
    }

    public virtual CollectionEntry ACClassTask_ParentACClassTaskReference
    {
        get { return Context.Entry(this).Collection("ACClassTask_ParentACClassTask"); }
    }

    private ACClassTask _ACClassTask1_ParentACClassTask;
    public virtual ACClassTask ACClassTask1_ParentACClassTask
    { 
        get => LazyLoader.Load(this, ref _ACClassTask1_ParentACClassTask);
        set => _ACClassTask1_ParentACClassTask = value;
    }

    public virtual ReferenceEntry ACClassTask1_ParentACClassTaskReference 
    {
        get { return Context.Entry(this).Reference("ACClassTask1_ParentACClassTask"); }
    }
    
    private ACClass _TaskTypeACClass;
    public virtual ACClass TaskTypeACClass
    { 
        get => LazyLoader.Load(this, ref _TaskTypeACClass);
        set => _TaskTypeACClass = value;
    }

    public virtual ReferenceEntry TaskTypeACClassReference 
    {
        get { return Context.Entry(this).Reference("TaskTypeACClass"); }
    }
    }
