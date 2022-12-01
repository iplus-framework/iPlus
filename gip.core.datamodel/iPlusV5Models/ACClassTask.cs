using System;
using System.Collections.Generic;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClassTask : VBEntityObject
{
    Guid _ACClassTaskID;
    public Guid ACClassTaskID 
    { 
        get { return _ACClassTaskID; }
        set { SetProperty<Guid>(ref _ACClassTaskID, value); } 
    }

    Guid? _ParentACClassTaskID;
    public Guid? ParentACClassTaskID 
    { 
        get { return _ParentACClassTaskID; }
        set { SetProperty<Guid?>(ref _ParentACClassTaskID, value); } 
    }

    Guid _TaskTypeACClassID;
    public Guid TaskTypeACClassID 
    { 
        get { return _TaskTypeACClassID; }
        set { SetProperty<Guid>(ref _TaskTypeACClassID, value); } 
    }

    Guid? _ContentACClassWFID;
    public Guid? ContentACClassWFID 
    { 
        get { return _ContentACClassWFID; }
        set { SetProperty<Guid?>(ref _ContentACClassWFID, value); } 
    }

    Guid? _ACProgramID;
    public Guid? ACProgramID 
    { 
        get { return _ACProgramID; }
        set { SetProperty<Guid?>(ref _ACProgramID, value); } 
    }

    string _ACIdentifier;
    public string ACIdentifier 
    { 
        get { return _ACIdentifier; }
        set { SetProperty<string>(ref _ACIdentifier, value); } 
    }

    short _ACTaskTypeIndex;
    public short ACTaskTypeIndex 
    { 
        get { return _ACTaskTypeIndex; }
        set { SetProperty<short>(ref _ACTaskTypeIndex, value); } 
    }

    bool _IsDynamic;
    public bool IsDynamic 
    { 
        get { return _IsDynamic; }
        set { SetProperty<bool>(ref _IsDynamic, value); } 
    }

    bool _IsTestmode;
    public bool IsTestmode 
    { 
        get { return _IsTestmode; }
        set { SetProperty<bool>(ref _IsTestmode, value); } 
    }

    string _XMLACMethod;
    public string XMLACMethod 
    { 
        get { return _XMLACMethod; }
        set { SetProperty<string>(ref _XMLACMethod, value); } 
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

    public virtual ICollection<ACClassTaskValue> ACClassTaskValue_ACClassTask { get; } = new List<ACClassTaskValue>();

    public virtual ACProgram ACProgram { get; set; }

    public virtual ACClassWF ContentACClassWF { get; set; }

    public virtual ICollection<ACClassTask> ACClassTask_ParentACClassTask { get; } = new List<ACClassTask>();

    public virtual ACClassTask ACClassTask1_ParentACClassTask { get; set; }

    public virtual ACClass TaskTypeACClass { get; set; }
}
