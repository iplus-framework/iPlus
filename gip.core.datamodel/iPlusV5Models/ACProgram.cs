using System;
using System.Collections.Generic;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACProgram : VBEntityObject
{
    Guid _ACProgramID;
    public Guid ACProgramID 
    { 
        get { return _ACProgramID; }
        set { SetProperty<Guid>(ref _ACProgramID, value); } 
    }

    Guid _WorkflowTypeACClassID;
    public Guid WorkflowTypeACClassID 
    { 
        get { return _WorkflowTypeACClassID; }
        set { SetProperty<Guid>(ref _WorkflowTypeACClassID, value); } 
    }

    Guid? _ProgramACClassMethodID;
    public Guid? ProgramACClassMethodID 
    { 
        get { return _ProgramACClassMethodID; }
        set { SetProperty<Guid?>(ref _ProgramACClassMethodID, value); } 
    }

    string _ProgramNo;
    public string ProgramNo 
    { 
        get { return _ProgramNo; }
        set { SetProperty<string>(ref _ProgramNo, value); } 
    }

    string _ProgramName;
    public string ProgramName 
    { 
        get { return _ProgramName; }
        set { SetProperty<string>(ref _ProgramName, value); } 
    }

    short _ACProgramTypeIndex;
    public short ACProgramTypeIndex 
    { 
        get { return _ACProgramTypeIndex; }
        set { SetProperty<short>(ref _ACProgramTypeIndex, value); } 
    }

    DateTime _PlannedStartDate;
    public DateTime PlannedStartDate 
    { 
        get { return _PlannedStartDate; }
        set { SetProperty<DateTime>(ref _PlannedStartDate, value); } 
    }

    string _Comment;
    public string Comment 
    { 
        get { return _Comment; }
        set { SetProperty<string>(ref _Comment, value); } 
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

    public virtual ICollection<ACClassTask> ACClassTask_ACProgram { get; } = new List<ACClassTask>();

    public virtual ICollection<ACProgramConfig> ACProgramConfig_ACProgram { get; } = new List<ACProgramConfig>();

    public virtual ICollection<ACProgramLog> ACProgramLog_ACProgram { get; } = new List<ACProgramLog>();

    public virtual ACClassMethod ProgramACClassMethod { get; set; }

    public virtual ACClass WorkflowTypeACClass { get; set; }
}
