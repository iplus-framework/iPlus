using System;
using System.Collections.Generic;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACPropertyLogRule : VBEntityObject
{
    Guid _ACPropertyLogRuleID;
    public Guid ACPropertyLogRuleID 
    { 
        get { return _ACPropertyLogRuleID; }
        set { SetProperty<Guid>(ref _ACPropertyLogRuleID, value); } 
    }

    Guid _ACClassID;
    public Guid ACClassID 
    { 
        get { return _ACClassID; }
        set { SetProperty<Guid>(ref _ACClassID, value); } 
    }

    short _RuleType;
    public short RuleType 
    { 
        get { return _RuleType; }
        set { SetProperty<short>(ref _RuleType, value); } 
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
}
