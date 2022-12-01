using System;
using System.Collections.Generic;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClassTaskValue : VBEntityObject
{
    Guid _ACClassTaskValueID;
    public Guid ACClassTaskValueID 
    { 
        get { return _ACClassTaskValueID; }
        set { SetProperty<Guid>(ref _ACClassTaskValueID, value); } 
    }

    Guid _ACClassTaskID;
    public Guid ACClassTaskID 
    { 
        get { return _ACClassTaskID; }
        set { SetProperty<Guid>(ref _ACClassTaskID, value); } 
    }

    Guid _ACClassPropertyID;
    public Guid ACClassPropertyID 
    { 
        get { return _ACClassPropertyID; }
        set { SetProperty<Guid>(ref _ACClassPropertyID, value); } 
    }

    Guid? _VBUserID;
    public Guid? VBUserID 
    { 
        get { return _VBUserID; }
        set { SetProperty<Guid?>(ref _VBUserID, value); } 
    }

    string _XMLValue;
    public string XMLValue 
    { 
        get { return _XMLValue; }
        set { SetProperty<string>(ref _XMLValue, value); } 
    }

    string _XMLValue2;
    public string XMLValue2 
    { 
        get { return _XMLValue2; }
        set { SetProperty<string>(ref _XMLValue2, value); } 
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

    public virtual ACClassProperty ACClassProperty { get; set; }

    public virtual ACClassTask ACClassTask { get; set; }

    public virtual ICollection<ACClassTaskValuePo> ACClassTaskValuePo_ACClassTaskValue { get; } = new List<ACClassTaskValuePo>();

    public virtual VBUser VBUser { get; set; }
}
