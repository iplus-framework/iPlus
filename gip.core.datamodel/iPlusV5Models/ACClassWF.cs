using System;
using System.Collections.Generic;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClassWF : VBEntityObject
{
    Guid _ACClassWFID;
    public Guid ACClassWFID 
    { 
        get { return _ACClassWFID; }
        set { SetProperty<Guid>(ref _ACClassWFID, value); } 
    }

    Guid _ACClassMethodID;
    public Guid ACClassMethodID 
    { 
        get { return _ACClassMethodID; }
        set { SetProperty<Guid>(ref _ACClassMethodID, value); } 
    }

    string _XName;
    public string XName 
    { 
        get { return _XName; }
        set { SetProperty<string>(ref _XName, value); } 
    }

    string _ACIdentifier;
    public string ACIdentifier 
    { 
        get { return _ACIdentifier; }
        set { SetProperty<string>(ref _ACIdentifier, value); } 
    }

    Guid? _ParentACClassWFID;
    public Guid? ParentACClassWFID 
    { 
        get { return _ParentACClassWFID; }
        set { SetProperty<Guid?>(ref _ParentACClassWFID, value); } 
    }

    Guid _PWACClassID;
    public Guid PWACClassID 
    { 
        get { return _PWACClassID; }
        set { SetProperty<Guid>(ref _PWACClassID, value); } 
    }

    Guid? _RefPAACClassID;
    public Guid? RefPAACClassID 
    { 
        get { return _RefPAACClassID; }
        set { SetProperty<Guid?>(ref _RefPAACClassID, value); } 
    }

    Guid? _RefPAACClassMethodID;
    public Guid? RefPAACClassMethodID 
    { 
        get { return _RefPAACClassMethodID; }
        set { SetProperty<Guid?>(ref _RefPAACClassMethodID, value); } 
    }

    string _PhaseIdentifier;
    public string PhaseIdentifier 
    { 
        get { return _PhaseIdentifier; }
        set { SetProperty<string>(ref _PhaseIdentifier, value); } 
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

    int _BranchNo;
    public int BranchNo 
    { 
        get { return _BranchNo; }
        set { SetProperty<int>(ref _BranchNo, value); } 
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

    public virtual ACClassMethod ACClassMethod { get; set; }

    public virtual ICollection<ACClassMethodConfig> ACClassMethodConfig_ACClassWF { get; } = new List<ACClassMethodConfig>();

    public virtual ICollection<ACClassTask> ACClassTask_ContentACClassWF { get; } = new List<ACClassTask>();

    public virtual ICollection<ACClassWFEdge> ACClassWFEdge_SourceACClassWF { get; } = new List<ACClassWFEdge>();

    public virtual ICollection<ACClassWFEdge> ACClassWFEdge_TargetACClassWF { get; } = new List<ACClassWFEdge>();

    public virtual ICollection<ACClassWF> ACClassWF_ParentACClassWF { get; } = new List<ACClassWF>();

    public virtual ACClass PWACClass { get; set; }

    public virtual ACClassWF ACClassWF1_ParentACClassWF { get; set; }

    public virtual ACClass RefPAACClass { get; set; }

    public virtual ACClassMethod RefPAACClassMethod { get; set; }
}
