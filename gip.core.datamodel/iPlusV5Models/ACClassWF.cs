using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClassWF : VBEntityObject
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
    public string ACIdentifier 
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

    private ACClassMethod _ACClassMethod;
    public virtual ACClassMethod ACClassMethod
    { 
        get => LazyLoader.Load(this, ref _ACClassMethod);
        set => _ACClassMethod = value;
    }

    public virtual ReferenceEntry ACClassMethodReference 
    {
        get { return Context.Entry(this).Reference("ACClassMethod"); }
    }
    
    private ICollection<ACClassMethodConfig> _ACClassMethodConfig_ACClassWF;
    public virtual ICollection<ACClassMethodConfig> ACClassMethodConfig_ACClassWF
    {
        get => LazyLoader.Load(this, ref _ACClassMethodConfig_ACClassWF);
        set => _ACClassMethodConfig_ACClassWF = value;
    }

    public virtual CollectionEntry ACClassMethodConfig_ACClassWFReference
    {
        get { return Context.Entry(this).Collection("ACClassMethodConfig_ACClassWF"); }
    }

    private ICollection<ACClassTask> _ACClassTask_ContentACClassWF;
    public virtual ICollection<ACClassTask> ACClassTask_ContentACClassWF
    {
        get => LazyLoader.Load(this, ref _ACClassTask_ContentACClassWF);
        set => _ACClassTask_ContentACClassWF = value;
    }

    public virtual CollectionEntry ACClassTask_ContentACClassWFReference
    {
        get { return Context.Entry(this).Collection("ACClassTask_ContentACClassWF"); }
    }

    private ICollection<ACClassWFEdge> _ACClassWFEdge_SourceACClassWF;
    public virtual ICollection<ACClassWFEdge> ACClassWFEdge_SourceACClassWF
    {
        get => LazyLoader.Load(this, ref _ACClassWFEdge_SourceACClassWF);
        set => _ACClassWFEdge_SourceACClassWF = value;
    }

    public virtual CollectionEntry ACClassWFEdge_SourceACClassWFReference
    {
        get { return Context.Entry(this).Collection("ACClassWFEdge_SourceACClassWF"); }
    }

    private ICollection<ACClassWFEdge> _ACClassWFEdge_TargetACClassWF;
    public virtual ICollection<ACClassWFEdge> ACClassWFEdge_TargetACClassWF
    {
        get => LazyLoader.Load(this, ref _ACClassWFEdge_TargetACClassWF);
        set => _ACClassWFEdge_TargetACClassWF = value;
    }

    public virtual CollectionEntry ACClassWFEdge_TargetACClassWFReference
    {
        get { return Context.Entry(this).Collection("ACClassWFEdge_TargetACClassWF"); }
    }

    private ICollection<ACClassWF> _ACClassWF_ParentACClassWF;
    public virtual ICollection<ACClassWF> ACClassWF_ParentACClassWF
    {
        get => LazyLoader.Load(this, ref _ACClassWF_ParentACClassWF);
        set => _ACClassWF_ParentACClassWF = value;
    }

    public virtual CollectionEntry ACClassWF_ParentACClassWFReference
    {
        get { return Context.Entry(this).Collection("ACClassWF_ParentACClassWF"); }
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
    
    private ACClassWF _ACClassWF1_ParentACClassWF;
    public virtual ACClassWF ACClassWF1_ParentACClassWF
    { 
        get => LazyLoader.Load(this, ref _ACClassWF1_ParentACClassWF);
        set => _ACClassWF1_ParentACClassWF = value;
    }

    public virtual ReferenceEntry ACClassWF1_ParentACClassWFReference 
    {
        get { return Context.Entry(this).Reference("ACClassWF1_ParentACClassWF"); }
    }
    
    private ACClass _RefPAACClass;
    public virtual ACClass RefPAACClass
    { 
        get => LazyLoader.Load(this, ref _RefPAACClass);
        set => _RefPAACClass = value;
    }

    public virtual ReferenceEntry RefPAACClassReference 
    {
        get { return Context.Entry(this).Reference("RefPAACClass"); }
    }
    
    private ACClassMethod _RefPAACClassMethod;
    public virtual ACClassMethod RefPAACClassMethod
    { 
        get => LazyLoader.Load(this, ref _RefPAACClassMethod);
        set => _RefPAACClassMethod = value;
    }

    public virtual ReferenceEntry RefPAACClassMethodReference 
    {
        get { return Context.Entry(this).Reference("RefPAACClassMethod"); }
    }
    }
