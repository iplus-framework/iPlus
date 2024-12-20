﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACProgram : VBEntityObject , IInsertInfo, IUpdateInfo
{

    public ACProgram()
    {
    }

    private ACProgram(ILazyLoader lazyLoader)
    {
        LazyLoader = lazyLoader;
    }

    private ILazyLoader LazyLoader { get; set; }
    Guid _ACProgramID;
    public Guid ACProgramID 
    {
        get { return _ACProgramID; }
        set { SetProperty<Guid>(ref _ACProgramID, value); }
    }

    Guid _WorkflowTypeACClassID;
    public Guid WorkflowTypeACClassID 
    {
        get { return _WorkflowTypeACClassID; }
        set { SetProperty<Guid>(ref _WorkflowTypeACClassID, value); }
    }

    Guid? _ProgramACClassMethodID;
    public Guid? ProgramACClassMethodID 
    {
        get { return _ProgramACClassMethodID; }
        set { SetProperty<Guid?>(ref _ProgramACClassMethodID, value); }
    }

    string _ProgramNo;
    public string ProgramNo 
    {
        get { return _ProgramNo; }
        set { SetProperty<string>(ref _ProgramNo, value); }
    }

    string _ProgramName;
    public string ProgramName 
    {
        get { return _ProgramName; }
        set { SetProperty<string>(ref _ProgramName, value); }
    }

    short _ACProgramTypeIndex;
    public short ACProgramTypeIndex 
    {
        get { return _ACProgramTypeIndex; }
        set { SetProperty<short>(ref _ACProgramTypeIndex, value); }
    }

    DateTime _PlannedStartDate;
    public DateTime PlannedStartDate 
    {
        get { return _PlannedStartDate; }
        set { SetProperty<DateTime>(ref _PlannedStartDate, value); }
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

    private ICollection<ACClassTask> _ACClassTask_ACProgram;
    public virtual ICollection<ACClassTask> ACClassTask_ACProgram
    {
        get { return LazyLoader.Load(this, ref _ACClassTask_ACProgram); }
        set { _ACClassTask_ACProgram = value; }
    }

    public bool ACClassTask_ACProgram_IsLoaded
    {
        get
        {
            return _ACClassTask_ACProgram != null;
        }
    }

    public virtual CollectionEntry ACClassTask_ACProgramReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassTask_ACProgram); }
    }

    private ICollection<ACProgramConfig> _ACProgramConfig_ACProgram;
    public virtual ICollection<ACProgramConfig> ACProgramConfig_ACProgram
    {
        get { return LazyLoader.Load(this, ref _ACProgramConfig_ACProgram); }
        set { _ACProgramConfig_ACProgram = value; }
    }

    public bool ACProgramConfig_ACProgram_IsLoaded
    {
        get
        {
            return _ACProgramConfig_ACProgram != null;
        }
    }

    public virtual CollectionEntry ACProgramConfig_ACProgramReference
    {
        get { return Context.Entry(this).Collection(c => c.ACProgramConfig_ACProgram); }
    }

    private ICollection<ACProgramLog> _ACProgramLog_ACProgram;
    public virtual ICollection<ACProgramLog> ACProgramLog_ACProgram
    {
        get { return LazyLoader.Load(this, ref _ACProgramLog_ACProgram); }
        set { _ACProgramLog_ACProgram = value; }
    }

    public bool ACProgramLog_ACProgram_IsLoaded
    {
        get
        {
            return _ACProgramLog_ACProgram != null;
        }
    }

    public virtual CollectionEntry ACProgramLog_ACProgramReference
    {
        get { return Context.Entry(this).Collection(c => c.ACProgramLog_ACProgram); }
    }

    private ACClassMethod _ProgramACClassMethod;
    public virtual ACClassMethod ProgramACClassMethod
    { 
        get { return LazyLoader.Load(this, ref _ProgramACClassMethod); }
        set { SetProperty<ACClassMethod>(ref _ProgramACClassMethod, value); }
    }

    public bool ProgramACClassMethod_IsLoaded
    {
        get
        {
            return _ProgramACClassMethod != null;
        }
    }

    public virtual ReferenceEntry ProgramACClassMethodReference 
    {
        get { return Context.Entry(this).Reference("ProgramACClassMethod"); }
    }
    
    private ACClass _WorkflowTypeACClass;
    public virtual ACClass WorkflowTypeACClass
    { 
        get { return LazyLoader.Load(this, ref _WorkflowTypeACClass); }
        set { SetProperty<ACClass>(ref _WorkflowTypeACClass, value); }
    }

    public bool WorkflowTypeACClass_IsLoaded
    {
        get
        {
            return _WorkflowTypeACClass != null;
        }
    }

    public virtual ReferenceEntry WorkflowTypeACClassReference 
    {
        get { return Context.Entry(this).Reference("WorkflowTypeACClass"); }
    }
    }
