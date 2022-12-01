using System;
using System.Collections.Generic;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClassMethod : VBEntityObject
{
    Guid _ACClassMethodID;
    public Guid ACClassMethodID 
    { 
        get { return _ACClassMethodID; }
        set { SetProperty<Guid>(ref _ACClassMethodID, value); } 
    }

    Guid _ACClassID;
    public Guid ACClassID 
    { 
        get { return _ACClassID; }
        set { SetProperty<Guid>(ref _ACClassID, value); } 
    }

    string _ACIdentifier;
    public string ACIdentifier 
    { 
        get { return _ACIdentifier; }
        set { SetProperty<string>(ref _ACIdentifier, value); } 
    }

    int? _ACIdentifierKey;
    public int? ACIdentifierKey 
    { 
        get { return _ACIdentifierKey; }
        set { SetProperty<int?>(ref _ACIdentifierKey, value); } 
    }

    string _ACCaptionTranslation;
    public string ACCaptionTranslation 
    { 
        get { return _ACCaptionTranslation; }
        set { SetProperty<string>(ref _ACCaptionTranslation, value); } 
    }

    string _ACGroup;
    public string ACGroup 
    { 
        get { return _ACGroup; }
        set { SetProperty<string>(ref _ACGroup, value); } 
    }

    string _Sourcecode;
    public string Sourcecode 
    { 
        get { return _Sourcecode; }
        set { SetProperty<string>(ref _Sourcecode, value); } 
    }

    short _ACKindIndex;
    public short ACKindIndex 
    { 
        get { return _ACKindIndex; }
        set { SetProperty<short>(ref _ACKindIndex, value); } 
    }

    short _SortIndex;
    public short SortIndex 
    { 
        get { return _SortIndex; }
        set { SetProperty<short>(ref _SortIndex, value); } 
    }

    bool _IsRightmanagement;
    public bool IsRightmanagement 
    { 
        get { return _IsRightmanagement; }
        set { SetProperty<bool>(ref _IsRightmanagement, value); } 
    }

    string _Comment;
    public string Comment 
    { 
        get { return _Comment; }
        set { SetProperty<string>(ref _Comment, value); } 
    }

    bool _IsCommand;
    public bool IsCommand 
    { 
        get { return _IsCommand; }
        set { SetProperty<bool>(ref _IsCommand, value); } 
    }

    bool _IsInteraction;
    public bool IsInteraction 
    { 
        get { return _IsInteraction; }
        set { SetProperty<bool>(ref _IsInteraction, value); } 
    }

    bool _IsAsyncProcess;
    public bool IsAsyncProcess 
    { 
        get { return _IsAsyncProcess; }
        set { SetProperty<bool>(ref _IsAsyncProcess, value); } 
    }

    bool _IsPeriodic;
    public bool IsPeriodic 
    { 
        get { return _IsPeriodic; }
        set { SetProperty<bool>(ref _IsPeriodic, value); } 
    }

    bool _IsParameterACMethod;
    public bool IsParameterACMethod 
    { 
        get { return _IsParameterACMethod; }
        set { SetProperty<bool>(ref _IsParameterACMethod, value); } 
    }

    bool _IsSubMethod;
    public bool IsSubMethod 
    { 
        get { return _IsSubMethod; }
        set { SetProperty<bool>(ref _IsSubMethod, value); } 
    }

    string _InteractionVBContent;
    public string InteractionVBContent 
    { 
        get { return _InteractionVBContent; }
        set { SetProperty<string>(ref _InteractionVBContent, value); } 
    }

    bool _IsAutoenabled;
    public bool IsAutoenabled 
    { 
        get { return _IsAutoenabled; }
        set { SetProperty<bool>(ref _IsAutoenabled, value); } 
    }

    bool _IsPersistable;
    public bool IsPersistable 
    { 
        get { return _IsPersistable; }
        set { SetProperty<bool>(ref _IsPersistable, value); } 
    }

    Guid? _PWACClassID;
    public Guid? PWACClassID 
    { 
        get { return _PWACClassID; }
        set { SetProperty<Guid?>(ref _PWACClassID, value); } 
    }

    bool _ContinueByError;
    public bool ContinueByError 
    { 
        get { return _ContinueByError; }
        set { SetProperty<bool>(ref _ContinueByError, value); } 
    }

    Guid? _ValueTypeACClassID;
    public Guid? ValueTypeACClassID 
    { 
        get { return _ValueTypeACClassID; }
        set { SetProperty<Guid?>(ref _ValueTypeACClassID, value); } 
    }

    string _GenericType;
    public string GenericType 
    { 
        get { return _GenericType; }
        set { SetProperty<string>(ref _GenericType, value); } 
    }

    Guid? _ParentACClassMethodID;
    public Guid? ParentACClassMethodID 
    { 
        get { return _ParentACClassMethodID; }
        set { SetProperty<Guid?>(ref _ParentACClassMethodID, value); } 
    }

    string _XMLACMethod;
    public string XMLACMethod 
    { 
        get { return _XMLACMethod; }
        set { SetProperty<string>(ref _XMLACMethod, value); } 
    }

    string _XMLDesign;
    public string XMLDesign 
    { 
        get { return _XMLDesign; }
        set { SetProperty<string>(ref _XMLDesign, value); } 
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

    short? _ContextMenuCategoryIndex;
    public short? ContextMenuCategoryIndex 
    { 
        get { return _ContextMenuCategoryIndex; }
        set { SetProperty<short?>(ref _ContextMenuCategoryIndex, value); } 
    }

    bool _IsRPCEnabled;
    public bool IsRPCEnabled 
    { 
        get { return _IsRPCEnabled; }
        set { SetProperty<bool>(ref _IsRPCEnabled, value); } 
    }

    Guid? _AttachedFromACClassID;
    public Guid? AttachedFromACClassID 
    { 
        get { return _AttachedFromACClassID; }
        set { SetProperty<Guid?>(ref _AttachedFromACClassID, value); } 
    }

    bool _IsStatic;
    public bool IsStatic 
    { 
        get { return _IsStatic; }
        set { SetProperty<bool>(ref _IsStatic, value); } 
    }

    bool _ExecuteByDoubleClick;
    public bool ExecuteByDoubleClick 
    { 
        get { return _ExecuteByDoubleClick; }
        set { SetProperty<bool>(ref _ExecuteByDoubleClick, value); } 
    }

    public virtual ACClass ACClass { get; set; }

    public virtual ICollection<ACClassMethodConfig> ACClassMethodConfig_ACClassMethod { get; } = new List<ACClassMethodConfig>();

    public virtual ICollection<ACClassWF> ACClassWF_ACClassMethod { get; } = new List<ACClassWF>();

    public virtual ICollection<ACClassWFEdge> ACClassWFEdge_ACClassMethod { get; } = new List<ACClassWFEdge>();

    public virtual ICollection<ACClassWFEdge> ACClassWFEdge_SourceACClassMethod { get; } = new List<ACClassWFEdge>();

    public virtual ICollection<ACClassWFEdge> ACClassWFEdge_TargetACClassMethod { get; } = new List<ACClassWFEdge>();

    public virtual ICollection<ACClassWF> ACClassWF_RefPAACClassMethod { get; } = new List<ACClassWF>();

    public virtual ICollection<ACProgram> ACProgram_ProgramACClassMethod { get; } = new List<ACProgram>();

    public virtual ACClass AttachedFromACClass { get; set; }

    public virtual ICollection<ACClassMethod> ACClassMethod_ParentACClassMethod { get; } = new List<ACClassMethod>();

    public virtual ACClass PWACClass { get; set; }

    public virtual ACClassMethod ACClassMethod1_ParentACClassMethod { get; set; }

    public virtual ICollection<VBGroupRight> VBGroupRight_ACClassMethod { get; } = new List<VBGroupRight>();

    public virtual ACClass ValueTypeACClass { get; set; }
}
