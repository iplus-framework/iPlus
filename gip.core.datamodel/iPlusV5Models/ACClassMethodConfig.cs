using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClassMethodConfig : VBEntityObject
{
    Guid _ACClassMethodConfigID;
    public Guid ACClassMethodConfigID 
    { 
        get { return _ACClassMethodConfigID; }
        set { SetProperty<Guid>(ref _ACClassMethodConfigID, value); } 
    }

    Guid? _ParentACClassMethodConfigID;
    public Guid? ParentACClassMethodConfigID 
    { 
        get { return _ParentACClassMethodConfigID; }
        set { SetProperty<Guid?>(ref _ParentACClassMethodConfigID, value); } 
    }

    Guid _ACClassMethodID;
    public Guid ACClassMethodID 
    { 
        get { return _ACClassMethodID; }
        set { SetProperty<Guid>(ref _ACClassMethodID, value); } 
    }

    Guid? _ACClassWFID;
    public Guid? ACClassWFID 
    { 
        get { return _ACClassWFID; }
        set { SetProperty<Guid?>(ref _ACClassWFID, value); } 
    }

    Guid? _VBiACClassID;
    public Guid? VBiACClassID 
    { 
        get { return _VBiACClassID; }
        set { SetProperty<Guid?>(ref _VBiACClassID, value); } 
    }

    Guid? _VBiACClassPropertyRelationID;
    public Guid? VBiACClassPropertyRelationID 
    { 
        get { return _VBiACClassPropertyRelationID; }
        set { SetProperty<Guid?>(ref _VBiACClassPropertyRelationID, value); } 
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

    public virtual ACClassMethod ACClassMethod { get; set; }

    public virtual ReferenceEntry ACClassMethodReference 
    { 
        get { return Context.Entry(this).Reference("ACClassMethod"); }
    }
    
    public virtual ACClassWF ACClassWF { get; set; }

    public virtual ReferenceEntry ACClassWFReference 
    { 
        get { return Context.Entry(this).Reference("ACClassWF"); }
    }
    
    public virtual ICollection<ACClassMethodConfig> ACClassMethodConfig_ParentACClassMethodConfig { get; } = new List<ACClassMethodConfig>();

    public virtual CollectionEntry ACClassMethodConfig_ParentACClassMethodConfigReference
    {
        get { return Context.Entry(this).Collection("ACClassMethodConfig_ParentACClassMethodConfig"); }
    }

    public virtual ACClassMethodConfig ACClassMethodConfig1_ParentACClassMethodConfig { get; set; }

    public virtual ReferenceEntry ACClassMethodConfig1_ParentACClassMethodConfigReference 
    { 
        get { return Context.Entry(this).Reference("ACClassMethodConfig1_ParentACClassMethodConfig"); }
    }
    
    public virtual ACClass VBiACClass { get; set; }

    public virtual ReferenceEntry VBiACClassReference 
    { 
        get { return Context.Entry(this).Reference("VBiACClass"); }
    }
    
    public virtual ACClassPropertyRelation VBiACClassPropertyRelation { get; set; }

    public virtual ReferenceEntry VBiACClassPropertyRelationReference 
    { 
        get { return Context.Entry(this).Reference("VBiACClassPropertyRelation"); }
    }
    
    public virtual ACClass ValueTypeACClass { get; set; }

    public virtual ReferenceEntry ValueTypeACClassReference 
    { 
        get { return Context.Entry(this).Reference("ValueTypeACClass"); }
    }
    }
