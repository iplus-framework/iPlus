﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClassConfig : VBEntityObject
{
    Guid _ACClassConfigID;
    public Guid ACClassConfigID 
    { 
        get { return _ACClassConfigID; }
        set { SetProperty<Guid>(ref _ACClassConfigID, value); } 
    }

    Guid _ACClassID;
    public Guid ACClassID 
    { 
        get { return _ACClassID; }
        set { SetProperty<Guid>(ref _ACClassID, value); } 
    }

    Guid? _ACClassPropertyRelationID;
    public Guid? ACClassPropertyRelationID 
    { 
        get { return _ACClassPropertyRelationID; }
        set { SetProperty<Guid?>(ref _ACClassPropertyRelationID, value); } 
    }

    Guid? _ParentACClassConfigID;
    public Guid? ParentACClassConfigID 
    { 
        get { return _ParentACClassConfigID; }
        set { SetProperty<Guid?>(ref _ParentACClassConfigID, value); } 
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

    public virtual ACClass ACClass { get; set; }

    public virtual ReferenceEntry ACClassReference 
    { 
        get { return Context.Entry(this).Reference("ACClass"); }
    }
    
    public virtual ACClassPropertyRelation ACClassPropertyRelation { get; set; }

    public virtual ReferenceEntry ACClassPropertyRelationReference 
    { 
        get { return Context.Entry(this).Reference("ACClassPropertyRelation"); }
    }
    
    public virtual ICollection<ACClassConfig> ACClassConfig_ParentACClassConfig { get; } = new List<ACClassConfig>();

    public virtual CollectionEntry ACClassConfig_ParentACClassConfigReference
    {
        get { return Context.Entry(this).Collection("ACClassConfig_ParentACClassConfig"); }
    }

    public virtual ACClassConfig ACClassConfig1_ParentACClassConfig { get; set; }

    public virtual ReferenceEntry ACClassConfig1_ParentACClassConfigReference 
    { 
        get { return Context.Entry(this).Reference("ACClassConfig1_ParentACClassConfig"); }
    }
    
    public virtual ACClass ValueTypeACClass { get; set; }

    public virtual ReferenceEntry ValueTypeACClassReference 
    { 
        get { return Context.Entry(this).Reference("ValueTypeACClass"); }
    }
    }