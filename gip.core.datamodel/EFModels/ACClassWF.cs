﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClassWF : VBEntityObject , IInsertInfo, IUpdateInfo
{

    public ACClassWF()
    {
    }

    private ACClassWF(ILazyLoader lazyLoader)
    {
        LazyLoader = lazyLoader;
    }

    private ILazyLoader LazyLoader { get; set; }
    Guid _ACClassWFID;
    public Guid ACClassWFID 
    {
        get { return _ACClassWFID; }
        set { SetProperty<Guid>(ref _ACClassWFID, value); }
    }

    Guid _ACClassMethodID;
    public Guid ACClassMethodID 
    {
        get { return _ACClassMethodID; }
        set { SetProperty<Guid>(ref _ACClassMethodID, value); }
    }

    string _XName;
    public string XName 
    {
        get { return _XName; }
        set { SetProperty<string>(ref _XName, value); }
    }

    string _ACIdentifier;
    public override string ACIdentifier 
    {
        get { return _ACIdentifier; }
        set { SetProperty<string>(ref _ACIdentifier, value); }
    }

    Guid? _ParentACClassWFID;
    public Guid? ParentACClassWFID 
    {
        get { return _ParentACClassWFID; }
        set { SetProperty<Guid?>(ref _ParentACClassWFID, value); }
    }

    Guid _PWACClassID;
    public Guid PWACClassID 
    {
        get { return _PWACClassID; }
        set { SetProperty<Guid>(ref _PWACClassID, value); }
    }

    Guid? _RefPAACClassID;
    public Guid? RefPAACClassID 
    {
        get { return _RefPAACClassID; }
        set { SetProperty<Guid?>(ref _RefPAACClassID, value); }
    }

    Guid? _RefPAACClassMethodID;
    public Guid? RefPAACClassMethodID 
    {
        get { return _RefPAACClassMethodID; }
        set { SetProperty<Guid?>(ref _RefPAACClassMethodID, value); }
    }

    string _PhaseIdentifier;
    public string PhaseIdentifier 
    {
        get { return _PhaseIdentifier; }
        set { SetProperty<string>(ref _PhaseIdentifier, value); }
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

    private ACClassMethod _ACClassMethod;
    public virtual ACClassMethod ACClassMethod
    { 
        get { return LazyLoader.Load(this, ref _ACClassMethod); }
        set { SetProperty<ACClassMethod>(ref _ACClassMethod, value); }
    }

    public bool ACClassMethod_IsLoaded
    {
        get
        {
            return _ACClassMethod != null;
        }
    }

    public virtual ReferenceEntry ACClassMethodReference 
    {
        get { return Context.Entry(this).Reference("ACClassMethod"); }
    }
    
    private ICollection<ACClassMethodConfig> _ACClassMethodConfig_ACClassWF;
    public virtual ICollection<ACClassMethodConfig> ACClassMethodConfig_ACClassWF
    {
        get { return LazyLoader.Load(this, ref _ACClassMethodConfig_ACClassWF); }
        set { _ACClassMethodConfig_ACClassWF = value; }
    }

    public bool ACClassMethodConfig_ACClassWF_IsLoaded
    {
        get
        {
            return _ACClassMethodConfig_ACClassWF != null;
        }
    }

    public virtual CollectionEntry ACClassMethodConfig_ACClassWFReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassMethodConfig_ACClassWF); }
    }

    private ICollection<ACClassTask> _ACClassTask_ContentACClassWF;
    public virtual ICollection<ACClassTask> ACClassTask_ContentACClassWF
    {
        get { return LazyLoader.Load(this, ref _ACClassTask_ContentACClassWF); }
        set { _ACClassTask_ContentACClassWF = value; }
    }

    public bool ACClassTask_ContentACClassWF_IsLoaded
    {
        get
        {
            return _ACClassTask_ContentACClassWF != null;
        }
    }

    public virtual CollectionEntry ACClassTask_ContentACClassWFReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassTask_ContentACClassWF); }
    }

    private ICollection<ACClassWFEdge> _ACClassWFEdge_SourceACClassWF;
    public virtual ICollection<ACClassWFEdge> ACClassWFEdge_SourceACClassWF
    {
        get { return LazyLoader.Load(this, ref _ACClassWFEdge_SourceACClassWF); }
        set { _ACClassWFEdge_SourceACClassWF = value; }
    }

    public bool ACClassWFEdge_SourceACClassWF_IsLoaded
    {
        get
        {
            return _ACClassWFEdge_SourceACClassWF != null;
        }
    }

    public virtual CollectionEntry ACClassWFEdge_SourceACClassWFReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassWFEdge_SourceACClassWF); }
    }

    private ICollection<ACClassWFEdge> _ACClassWFEdge_TargetACClassWF;
    public virtual ICollection<ACClassWFEdge> ACClassWFEdge_TargetACClassWF
    {
        get { return LazyLoader.Load(this, ref _ACClassWFEdge_TargetACClassWF); }
        set { _ACClassWFEdge_TargetACClassWF = value; }
    }

    public bool ACClassWFEdge_TargetACClassWF_IsLoaded
    {
        get
        {
            return _ACClassWFEdge_TargetACClassWF != null;
        }
    }

    public virtual CollectionEntry ACClassWFEdge_TargetACClassWFReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassWFEdge_TargetACClassWF); }
    }

    private ICollection<ACClassWF> _ACClassWF_ParentACClassWF;
    public virtual ICollection<ACClassWF> ACClassWF_ParentACClassWF
    {
        get { return LazyLoader.Load(this, ref _ACClassWF_ParentACClassWF); }
        set { _ACClassWF_ParentACClassWF = value; }
    }

    public bool ACClassWF_ParentACClassWF_IsLoaded
    {
        get
        {
            return _ACClassWF_ParentACClassWF != null;
        }
    }

    public virtual CollectionEntry ACClassWF_ParentACClassWFReference
    {
        get { return Context.Entry(this).Collection(c => c.ACClassWF_ParentACClassWF); }
    }

    private ACClass _PWACClass;
    public virtual ACClass PWACClass
    { 
        get { return LazyLoader.Load(this, ref _PWACClass); }
        set { SetProperty<ACClass>(ref _PWACClass, value); }
    }

    public bool PWACClass_IsLoaded
    {
        get
        {
            return _PWACClass != null;
        }
    }

    public virtual ReferenceEntry PWACClassReference 
    {
        get { return Context.Entry(this).Reference("PWACClass"); }
    }
    
    private ACClassWF _ACClassWF1_ParentACClassWF;
    public virtual ACClassWF ACClassWF1_ParentACClassWF
    { 
        get { return LazyLoader.Load(this, ref _ACClassWF1_ParentACClassWF); }
        set { SetProperty<ACClassWF>(ref _ACClassWF1_ParentACClassWF, value); }
    }

    public bool ACClassWF1_ParentACClassWF_IsLoaded
    {
        get
        {
            return _ACClassWF1_ParentACClassWF != null;
        }
    }

    public virtual ReferenceEntry ACClassWF1_ParentACClassWFReference 
    {
        get { return Context.Entry(this).Reference("ACClassWF1_ParentACClassWF"); }
    }
    
    private ACClass _RefPAACClass;
    public virtual ACClass RefPAACClass
    { 
        get { return LazyLoader.Load(this, ref _RefPAACClass); }
        set { SetProperty<ACClass>(ref _RefPAACClass, value); }
    }

    public bool RefPAACClass_IsLoaded
    {
        get
        {
            return _RefPAACClass != null;
        }
    }

    public virtual ReferenceEntry RefPAACClassReference 
    {
        get { return Context.Entry(this).Reference("RefPAACClass"); }
    }
    
    private ACClassMethod _RefPAACClassMethod;
    public virtual ACClassMethod RefPAACClassMethod
    { 
        get { return LazyLoader.Load(this, ref _RefPAACClassMethod); }
        set { SetProperty<ACClassMethod>(ref _RefPAACClassMethod, value); }
    }

    public bool RefPAACClassMethod_IsLoaded
    {
        get
        {
            return _RefPAACClassMethod != null;
        }
    }

    public virtual ReferenceEntry RefPAACClassMethodReference 
    {
        get { return Context.Entry(this).Reference("RefPAACClassMethod"); }
    }
    }
