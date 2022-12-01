using System;
using System.Collections.Generic;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACPropertyLog : VBEntityObject
{
    Guid _ACPropertyLogID;
    public Guid ACPropertyLogID 
    { 
        get { return _ACPropertyLogID; }
        set { SetProperty<Guid>(ref _ACPropertyLogID, value); } 
    }

    Guid _ACClassID;
    public Guid ACClassID 
    { 
        get { return _ACClassID; }
        set { SetProperty<Guid>(ref _ACClassID, value); } 
    }

    Guid _ACClassPropertyID;
    public Guid ACClassPropertyID 
    { 
        get { return _ACClassPropertyID; }
        set { SetProperty<Guid>(ref _ACClassPropertyID, value); } 
    }

    Guid? _ACProgramLogID;
    public Guid? ACProgramLogID 
    { 
        get { return _ACProgramLogID; }
        set { SetProperty<Guid?>(ref _ACProgramLogID, value); } 
    }

    DateTime _EventTime;
    public DateTime EventTime 
    { 
        get { return _EventTime; }
        set { SetProperty<DateTime>(ref _EventTime, value); } 
    }

    string _Value;
    public string Value 
    { 
        get { return _Value; }
        set { SetProperty<string>(ref _Value, value); } 
    }

    public virtual ACClass ACClass { get; set; }

    public virtual ACClassProperty ACClassProperty { get; set; }
}
