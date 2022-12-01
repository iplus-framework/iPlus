using System;
using System.Collections.Generic;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACProgramLog : VBEntityObject
{
    Guid _ACProgramLogID;
    public Guid ACProgramLogID 
    { 
        get { return _ACProgramLogID; }
        set { SetProperty<Guid>(ref _ACProgramLogID, value); } 
    }

    Guid _ACProgramID;
    public Guid ACProgramID 
    { 
        get { return _ACProgramID; }
        set { SetProperty<Guid>(ref _ACProgramID, value); } 
    }

    Guid? _ParentACProgramLogID;
    public Guid? ParentACProgramLogID 
    { 
        get { return _ParentACProgramLogID; }
        set { SetProperty<Guid?>(ref _ParentACProgramLogID, value); } 
    }

    string _ACUrl;
    public string ACUrl 
    { 
        get { return _ACUrl; }
        set { SetProperty<string>(ref _ACUrl, value); } 
    }

    DateTime? _StartDate;
    public DateTime? StartDate 
    { 
        get { return _StartDate; }
        set { SetProperty<DateTime?>(ref _StartDate, value); } 
    }

    DateTime? _EndDate;
    public DateTime? EndDate 
    { 
        get { return _EndDate; }
        set { SetProperty<DateTime?>(ref _EndDate, value); } 
    }

    double _DurationSec;
    public double DurationSec 
    { 
        get { return _DurationSec; }
        set { SetProperty<double>(ref _DurationSec, value); } 
    }

    DateTime _StartDatePlan;
    public DateTime StartDatePlan 
    { 
        get { return _StartDatePlan; }
        set { SetProperty<DateTime>(ref _StartDatePlan, value); } 
    }

    DateTime _EndDatePlan;
    public DateTime EndDatePlan 
    { 
        get { return _EndDatePlan; }
        set { SetProperty<DateTime>(ref _EndDatePlan, value); } 
    }

    double _DurationSecPlan;
    public double DurationSecPlan 
    { 
        get { return _DurationSecPlan; }
        set { SetProperty<double>(ref _DurationSecPlan, value); } 
    }

    string _Message;
    public string Message 
    { 
        get { return _Message; }
        set { SetProperty<string>(ref _Message, value); } 
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

    Guid? _ACClassID;
    public Guid? ACClassID 
    { 
        get { return _ACClassID; }
        set { SetProperty<Guid?>(ref _ACClassID, value); } 
    }

    Guid? _RefACClassID;
    public Guid? RefACClassID 
    { 
        get { return _RefACClassID; }
        set { SetProperty<Guid?>(ref _RefACClassID, value); } 
    }

    public virtual ACProgram ACProgram { get; set; }

    public virtual ICollection<ACProgramLogTask> ACProgramLogTask_ACProgramLog { get; } = new List<ACProgramLogTask>();

    public virtual ICollection<ACProgramLog> ACProgramLog_ParentACProgramLog { get; } = new List<ACProgramLog>();

    public virtual ICollection<MsgAlarmLog> MsgAlarmLog_ACProgramLog { get; } = new List<MsgAlarmLog>();

    public virtual ACProgramLog ACProgramLog1_ParentACProgramLog { get; set; }
}
