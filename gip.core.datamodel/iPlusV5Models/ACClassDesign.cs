using System;
using System.Collections.Generic;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClassDesign : VBEntityObject
{
    Guid _ACClassDesignID;
    public Guid ACClassDesignID 
    { 
        get { return _ACClassDesignID; }
        set { SetProperty<Guid>(ref _ACClassDesignID, value); } 
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

    string _XMLDesign;
    public string XMLDesign 
    { 
        get { return _XMLDesign; }
        set { SetProperty<string>(ref _XMLDesign, value); } 
    }

    byte[] _DesignBinary;
    public byte[] DesignBinary 
    { 
        get { return _DesignBinary; }
        set { SetProperty<byte[]>(ref _DesignBinary, value); } 
    }

    string _DesignNo;
    public string DesignNo 
    { 
        get { return _DesignNo; }
        set { SetProperty<string>(ref _DesignNo, value); } 
    }

    Guid? _ValueTypeACClassID;
    public Guid? ValueTypeACClassID 
    { 
        get { return _ValueTypeACClassID; }
        set { SetProperty<Guid?>(ref _ValueTypeACClassID, value); } 
    }

    short _ACKindIndex;
    public short ACKindIndex 
    { 
        get { return _ACKindIndex; }
        set { SetProperty<short>(ref _ACKindIndex, value); } 
    }

    short _ACUsageIndex;
    public short ACUsageIndex 
    { 
        get { return _ACUsageIndex; }
        set { SetProperty<short>(ref _ACUsageIndex, value); } 
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

    bool _IsDefault;
    public bool IsDefault 
    { 
        get { return _IsDefault; }
        set { SetProperty<bool>(ref _IsDefault, value); } 
    }

    bool _IsResourceStyle;
    public bool IsResourceStyle 
    { 
        get { return _IsResourceStyle; }
        set { SetProperty<bool>(ref _IsResourceStyle, value); } 
    }

    double _VisualHeight;
    public double VisualHeight 
    { 
        get { return _VisualHeight; }
        set { SetProperty<double>(ref _VisualHeight, value); } 
    }

    double _VisualWidth;
    public double VisualWidth 
    { 
        get { return _VisualWidth; }
        set { SetProperty<double>(ref _VisualWidth, value); } 
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

    short? _DesignerMaxRecursion;
    public short? DesignerMaxRecursion 
    { 
        get { return _DesignerMaxRecursion; }
        set { SetProperty<short?>(ref _DesignerMaxRecursion, value); } 
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

    byte[] _BAMLDesign;
    public byte[] BAMLDesign 
    { 
        get { return _BAMLDesign; }
        set { SetProperty<byte[]>(ref _BAMLDesign, value); } 
    }

    DateTime? _BAMLDate;
    public DateTime? BAMLDate 
    { 
        get { return _BAMLDate; }
        set { SetProperty<DateTime?>(ref _BAMLDate, value); } 
    }

    public virtual ACClass ACClass { get; set; }

    public virtual ICollection<VBGroupRight> VBGroupRight_ACClassDesign { get; } = new List<VBGroupRight>();

    public virtual ICollection<VBUserACClassDesign> VBUserACClassDesign_ACClassDesign { get; } = new List<VBUserACClassDesign>();

    public virtual ICollection<VBUser> VBUser_MenuACClassDesign { get; } = new List<VBUser>();

    public virtual ACClass ValueTypeACClass { get; set; }
}
