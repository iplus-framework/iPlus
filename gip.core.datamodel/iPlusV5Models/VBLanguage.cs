using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class VBLanguage : VBEntityObject
{
    Guid _VBLanguageID;
    public Guid VBLanguageID 
    { 
        get { return _VBLanguageID; }
        set { SetProperty<Guid>(ref _VBLanguageID, value); } 
    }

    string _VBLanguageCode;
    public string VBLanguageCode 
    { 
        get { return _VBLanguageCode; }
        set { SetProperty<string>(ref _VBLanguageCode, value); } 
    }

    string _VBNameTrans;
    public string VBNameTrans 
    { 
        get { return _VBNameTrans; }
        set { SetProperty<string>(ref _VBNameTrans, value); } 
    }

    string _VBKey;
    public string VBKey 
    { 
        get { return _VBKey; }
        set { SetProperty<string>(ref _VBKey, value); } 
    }

    short _SortIndex;
    public short SortIndex 
    { 
        get { return _SortIndex; }
        set { SetProperty<short>(ref _SortIndex, value); } 
    }

    string _XMLConfig;
    public string XMLConfig 
    { 
        get { return _XMLConfig; }
        set { SetProperty<string>(ref _XMLConfig, value); } 
    }

    bool _IsDefault;
    public bool IsDefault 
    { 
        get { return _IsDefault; }
        set { SetProperty<bool>(ref _IsDefault, value); } 
    }

    bool _IsTranslation;
    public bool IsTranslation 
    { 
        get { return _IsTranslation; }
        set { SetProperty<bool>(ref _IsTranslation, value); } 
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

    public virtual ICollection<VBUser> VBUser_VBLanguage { get; } = new List<VBUser>();

    public virtual CollectionEntry VBUser_VBLanguageReference
    {
        get { return Context.Entry(this).Collection("VBUser_VBLanguage"); }
    }
}
