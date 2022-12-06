using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClassWFEdge : VBEntityObject
{
    Guid _ACClassWFEdgeID;
    public Guid ACClassWFEdgeID 
    { 
        get { return _ACClassWFEdgeID; }
        set { SetProperty<Guid>(ref _ACClassWFEdgeID, value); } 
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

    Guid _SourceACClassWFID;
    public Guid SourceACClassWFID 
    { 
        get { return _SourceACClassWFID; }
        set { SetProperty<Guid>(ref _SourceACClassWFID, value); } 
    }

    Guid _SourceACClassPropertyID;
    public Guid SourceACClassPropertyID 
    { 
        get { return _SourceACClassPropertyID; }
        set { SetProperty<Guid>(ref _SourceACClassPropertyID, value); } 
    }

    Guid? _SourceACClassMethodID;
    public Guid? SourceACClassMethodID 
    { 
        get { return _SourceACClassMethodID; }
        set { SetProperty<Guid?>(ref _SourceACClassMethodID, value); } 
    }

    Guid _TargetACClassWFID;
    public Guid TargetACClassWFID 
    { 
        get { return _TargetACClassWFID; }
        set { SetProperty<Guid>(ref _TargetACClassWFID, value); } 
    }

    Guid _TargetACClassPropertyID;
    public Guid TargetACClassPropertyID 
    { 
        get { return _TargetACClassPropertyID; }
        set { SetProperty<Guid>(ref _TargetACClassPropertyID, value); } 
    }

    Guid? _TargetACClassMethodID;
    public Guid? TargetACClassMethodID 
    { 
        get { return _TargetACClassMethodID; }
        set { SetProperty<Guid?>(ref _TargetACClassMethodID, value); } 
    }

    short _ConnectionTypeIndex;
    public short ConnectionTypeIndex 
    { 
        get { return _ConnectionTypeIndex; }
        set { SetProperty<short>(ref _ConnectionTypeIndex, value); } 
    }

    int _BranchNo;
    public int BranchNo 
    { 
        get { return _BranchNo; }
        set { SetProperty<int>(ref _BranchNo, value); } 
    }

    public virtual ACClassMethod ACClassMethod { get; set; }

    public virtual ReferenceEntry ACClassMethodReference 
    { 
        get { return Context.Entry(this).Reference("ACClassMethod"); }
    }
    
    public virtual ACClassMethod SourceACClassMethod { get; set; }

    public virtual ReferenceEntry SourceACClassMethodReference 
    { 
        get { return Context.Entry(this).Reference("SourceACClassMethod"); }
    }
    
    public virtual ACClassProperty SourceACClassProperty { get; set; }

    public virtual ReferenceEntry SourceACClassPropertyReference 
    { 
        get { return Context.Entry(this).Reference("SourceACClassProperty"); }
    }
    
    public virtual ACClassWF SourceACClassWF { get; set; }

    public virtual ReferenceEntry SourceACClassWFReference 
    { 
        get { return Context.Entry(this).Reference("SourceACClassWF"); }
    }
    
    public virtual ACClassMethod TargetACClassMethod { get; set; }

    public virtual ReferenceEntry TargetACClassMethodReference 
    { 
        get { return Context.Entry(this).Reference("TargetACClassMethod"); }
    }
    
    public virtual ACClassProperty TargetACClassProperty { get; set; }

    public virtual ReferenceEntry TargetACClassPropertyReference 
    { 
        get { return Context.Entry(this).Reference("TargetACClassProperty"); }
    }
    
    public virtual ACClassWF TargetACClassWF { get; set; }

    public virtual ReferenceEntry TargetACClassWFReference 
    { 
        get { return Context.Entry(this).Reference("TargetACClassWF"); }
    }
    }
