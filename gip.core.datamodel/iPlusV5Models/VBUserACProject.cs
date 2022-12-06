using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class VBUserACProject : VBEntityObject
{
    Guid _VBUserACProjectID;
    public Guid VBUserACProjectID 
    { 
        get { return _VBUserACProjectID; }
        set { SetProperty<Guid>(ref _VBUserACProjectID, value); } 
    }

    Guid _ACProjectID;
    public Guid ACProjectID 
    { 
        get { return _ACProjectID; }
        set { SetProperty<Guid>(ref _ACProjectID, value); } 
    }

    Guid _VBUserID;
    public Guid VBUserID 
    { 
        get { return _VBUserID; }
        set { SetProperty<Guid>(ref _VBUserID, value); } 
    }

    bool _IsClient;
    public bool IsClient 
    { 
        get { return _IsClient; }
        set { SetProperty<bool>(ref _IsClient, value); } 
    }

    bool _IsServer;
    public bool IsServer 
    { 
        get { return _IsServer; }
        set { SetProperty<bool>(ref _IsServer, value); } 
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

    public virtual ACProject ACProject { get; set; }

    public virtual ReferenceEntry ACProjectReference 
    { 
        get { return Context.Entry(this).Reference("ACProject"); }
    }
    
    public virtual VBUser VBUser { get; set; }

    public virtual ReferenceEntry VBUserReference 
    { 
        get { return Context.Entry(this).Reference("VBUser"); }
    }
    }
