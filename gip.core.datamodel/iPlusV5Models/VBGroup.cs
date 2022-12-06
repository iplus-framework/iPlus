using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class VBGroup : VBEntityObject
{
    Guid _VBGroupID;
    public Guid VBGroupID 
    { 
        get { return _VBGroupID; }
        set { SetProperty<Guid>(ref _VBGroupID, value); } 
    }

    string _VBGroupName;
    public string VBGroupName 
    { 
        get { return _VBGroupName; }
        set { SetProperty<string>(ref _VBGroupName, value); } 
    }

    string _Description;
    public string Description 
    { 
        get { return _Description; }
        set { SetProperty<string>(ref _Description, value); } 
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

    public virtual ICollection<VBGroupRight> VBGroupRight_VBGroup { get; } = new List<VBGroupRight>();

    public virtual CollectionEntry VBGroupRight_VBGroupReference
    {
        get { return Context.Entry(this).Collection("VBGroupRight_VBGroup"); }
    }

    public virtual ICollection<VBUserGroup> VBUserGroup_VBGroup { get; } = new List<VBUserGroup>();

    public virtual CollectionEntry VBUserGroup_VBGroupReference
    {
        get { return Context.Entry(this).Collection("VBUserGroup_VBGroup"); }
    }
}
