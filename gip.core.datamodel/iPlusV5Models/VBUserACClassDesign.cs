using System;
using System.Collections.Generic;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class VBUserACClassDesign : VBEntityObject
{
    Guid _VBUserACClassDesignID;
    public Guid VBUserACClassDesignID 
    { 
        get { return _VBUserACClassDesignID; }
        set { SetProperty<Guid>(ref _VBUserACClassDesignID, value); } 
    }

    Guid _VBUserID;
    public Guid VBUserID 
    { 
        get { return _VBUserID; }
        set { SetProperty<Guid>(ref _VBUserID, value); } 
    }

    Guid? _ACClassDesignID;
    public Guid? ACClassDesignID 
    { 
        get { return _ACClassDesignID; }
        set { SetProperty<Guid?>(ref _ACClassDesignID, value); } 
    }

    string _XMLDesign;
    public string XMLDesign 
    { 
        get { return _XMLDesign; }
        set { SetProperty<string>(ref _XMLDesign, value); } 
    }

    string _ACIdentifier;
    public string ACIdentifier 
    { 
        get { return _ACIdentifier; }
        set { SetProperty<string>(ref _ACIdentifier, value); } 
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

    public virtual ACClassDesign ACClassDesign { get; set; }

    public virtual VBUser VBUser { get; set; }
}
