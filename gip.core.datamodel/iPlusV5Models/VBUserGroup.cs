using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class VBUserGroup : VBEntityObject
{
    Guid _VBUserGroupID;
    public Guid VBUserGroupID 
    { 
        get { return _VBUserGroupID; }
        set { SetProperty<Guid>(ref _VBUserGroupID, value); } 
    }

    Guid _VBUserID;
    public Guid VBUserID 
    { 
        get { return _VBUserID; }
        set { SetProperty<Guid>(ref _VBUserID, value); } 
    }

    Guid _VBGroupID;
    public Guid VBGroupID 
    { 
        get { return _VBGroupID; }
        set { SetProperty<Guid>(ref _VBGroupID, value); } 
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

    public virtual VBGroup VBGroup { get; set; }

    public virtual ReferenceEntry VBGroupReference 
    { 
        get { return Context.Entry(this).Reference("VBGroup"); }
    }
    
    public virtual VBUser VBUser { get; set; }

    public virtual ReferenceEntry VBUserReference 
    { 
        get { return Context.Entry(this).Reference("VBUser"); }
    }
    }
