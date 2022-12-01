using System;
using System.Collections.Generic;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACProject : VBEntityObject
{
    Guid _ACProjectID;
    public Guid ACProjectID 
    { 
        get { return _ACProjectID; }
        set { SetProperty<Guid>(ref _ACProjectID, value); } 
    }

    string _ACProjectNo;
    public string ACProjectNo 
    { 
        get { return _ACProjectNo; }
        set { SetProperty<string>(ref _ACProjectNo, value); } 
    }

    string _ACProjectName;
    public string ACProjectName 
    { 
        get { return _ACProjectName; }
        set { SetProperty<string>(ref _ACProjectName, value); } 
    }

    short _ACProjectTypeIndex;
    public short ACProjectTypeIndex 
    { 
        get { return _ACProjectTypeIndex; }
        set { SetProperty<short>(ref _ACProjectTypeIndex, value); } 
    }

    Guid? _BasedOnACProjectID;
    public Guid? BasedOnACProjectID 
    { 
        get { return _BasedOnACProjectID; }
        set { SetProperty<Guid?>(ref _BasedOnACProjectID, value); } 
    }

    Guid? _PAAppClassAssignmentACClassID;
    public Guid? PAAppClassAssignmentACClassID 
    { 
        get { return _PAAppClassAssignmentACClassID; }
        set { SetProperty<Guid?>(ref _PAAppClassAssignmentACClassID, value); } 
    }

    bool _IsEnabled;
    public bool IsEnabled 
    { 
        get { return _IsEnabled; }
        set { SetProperty<bool>(ref _IsEnabled, value); } 
    }

    bool _IsGlobal;
    public bool IsGlobal 
    { 
        get { return _IsGlobal; }
        set { SetProperty<bool>(ref _IsGlobal, value); } 
    }

    bool _IsWorkflowEnabled;
    public bool IsWorkflowEnabled 
    { 
        get { return _IsWorkflowEnabled; }
        set { SetProperty<bool>(ref _IsWorkflowEnabled, value); } 
    }

    bool _IsControlCenterEnabled;
    public bool IsControlCenterEnabled 
    { 
        get { return _IsControlCenterEnabled; }
        set { SetProperty<bool>(ref _IsControlCenterEnabled, value); } 
    }

    bool _IsVisualisationEnabled;
    public bool IsVisualisationEnabled 
    { 
        get { return _IsVisualisationEnabled; }
        set { SetProperty<bool>(ref _IsVisualisationEnabled, value); } 
    }

    bool _IsProduction;
    public bool IsProduction 
    { 
        get { return _IsProduction; }
        set { SetProperty<bool>(ref _IsProduction, value); } 
    }

    bool _IsDataAccess;
    public bool IsDataAccess 
    { 
        get { return _IsDataAccess; }
        set { SetProperty<bool>(ref _IsDataAccess, value); } 
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

    public virtual ICollection<ACClass> ACClass_ACProject { get; } = new List<ACClass>();

    public virtual ACProject ACProject1_BasedOnACProject { get; set; }

    public virtual ICollection<ACProject> ACProject_BasedOnACProject { get; } = new List<ACProject>();

    public virtual ACClass PAAppClassAssignmentACClass { get; set; }

    public virtual ICollection<VBUserACProject> VBUserACProject_ACProject { get; } = new List<VBUserACProject>();
}
