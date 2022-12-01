using System;
using System.Collections.Generic;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACProgramLogTask : VBEntityObject
{
    Guid _ACProgramLogTaskID;
    public Guid ACProgramLogTaskID 
    { 
        get { return _ACProgramLogTaskID; }
        set { SetProperty<Guid>(ref _ACProgramLogTaskID, value); } 
    }

    Guid _ACProgramLogID;
    public Guid ACProgramLogID 
    { 
        get { return _ACProgramLogID; }
        set { SetProperty<Guid>(ref _ACProgramLogID, value); } 
    }

    string _ACClassMethodXAML;
    public string ACClassMethodXAML 
    { 
        get { return _ACClassMethodXAML; }
        set { SetProperty<string>(ref _ACClassMethodXAML, value); } 
    }

    int _LoopNo;
    public int LoopNo 
    { 
        get { return _LoopNo; }
        set { SetProperty<int>(ref _LoopNo, value); } 
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

    public virtual ACProgramLog ACProgramLog { get; set; }
}
