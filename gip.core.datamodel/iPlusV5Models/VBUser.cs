using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class VBUser : VBEntityObject
{
    Guid _VBUserID;
    public Guid VBUserID 
    { 
        get { return _VBUserID; }
        set { SetProperty<Guid>(ref _VBUserID, value); } 
    }

    string _VBUserNo;
    public string VBUserNo 
    { 
        get { return _VBUserNo; }
        set { SetProperty<string>(ref _VBUserNo, value); } 
    }

    string _VBUserName;
    public string VBUserName 
    { 
        get { return _VBUserName; }
        set { SetProperty<string>(ref _VBUserName, value); } 
    }

    string _Initials;
    public string Initials 
    { 
        get { return _Initials; }
        set { SetProperty<string>(ref _Initials, value); } 
    }

    string _Password;
    public string Password 
    { 
        get { return _Password; }
        set { SetProperty<string>(ref _Password, value); } 
    }

    bool _AllowChangePW;
    public bool AllowChangePW 
    { 
        get { return _AllowChangePW; }
        set { SetProperty<bool>(ref _AllowChangePW, value); } 
    }

    Guid? _MenuACClassDesignID;
    public Guid? MenuACClassDesignID 
    { 
        get { return _MenuACClassDesignID; }
        set { SetProperty<Guid?>(ref _MenuACClassDesignID, value); } 
    }

    Guid _VBLanguageID;
    public Guid VBLanguageID 
    { 
        get { return _VBLanguageID; }
        set { SetProperty<Guid>(ref _VBLanguageID, value); } 
    }

    bool _IsSuperuser;
    public bool IsSuperuser 
    { 
        get { return _IsSuperuser; }
        set { SetProperty<bool>(ref _IsSuperuser, value); } 
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

    public virtual ICollection<ACChangeLog> ACChangeLog_VBUser { get; } = new List<ACChangeLog>();

    public virtual CollectionEntry ACChangeLog_VBUserReference
    {
        get { return Context.Entry(this).Collection("ACChangeLog_VBUser"); }
    }

    public virtual ICollection<ACClassTaskValue> ACClassTaskValue_VBUser { get; } = new List<ACClassTaskValue>();

    public virtual CollectionEntry ACClassTaskValue_VBUserReference
    {
        get { return Context.Entry(this).Collection("ACClassTaskValue_VBUser"); }
    }

    public virtual ACClassDesign MenuACClassDesign { get; set; }

    public virtual ReferenceEntry MenuACClassDesignReference 
    { 
        get { return Context.Entry(this).Reference("MenuACClassDesign"); }
    }
    
    public virtual VBLanguage VBLanguage { get; set; }

    public virtual ReferenceEntry VBLanguageReference 
    { 
        get { return Context.Entry(this).Reference("VBLanguage"); }
    }
    
    public virtual ICollection<VBUserACClassDesign> VBUserACClassDesign_VBUser { get; } = new List<VBUserACClassDesign>();

    public virtual CollectionEntry VBUserACClassDesign_VBUserReference
    {
        get { return Context.Entry(this).Collection("VBUserACClassDesign_VBUser"); }
    }

    public virtual ICollection<VBUserACProject> VBUserACProject_VBUser { get; } = new List<VBUserACProject>();

    public virtual CollectionEntry VBUserACProject_VBUserReference
    {
        get { return Context.Entry(this).Collection("VBUserACProject_VBUser"); }
    }

    public virtual ICollection<VBUserGroup> VBUserGroup_VBUser { get; } = new List<VBUserGroup>();

    public virtual CollectionEntry VBUserGroup_VBUserReference
    {
        get { return Context.Entry(this).Collection("VBUserGroup_VBUser"); }
    }

    public virtual ICollection<VBUserInstance> VBUserInstance_VBUser { get; } = new List<VBUserInstance>();

    public virtual CollectionEntry VBUserInstance_VBUserReference
    {
        get { return Context.Entry(this).Collection("VBUserInstance_VBUser"); }
    }
}
