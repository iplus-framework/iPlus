﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class VBGroupRight : VBEntityObject
{
    Guid _VBGroupRightID;
    public Guid VBGroupRightID 
    { 
        get { return _VBGroupRightID; }
        set { SetProperty<Guid>(ref _VBGroupRightID, value); } 
    }

    Guid _VBGroupID;
    public Guid VBGroupID 
    { 
        get { return _VBGroupID; }
        set { SetProperty<Guid>(ref _VBGroupID, value); } 
    }

    Guid _ACClassID;
    public Guid ACClassID 
    { 
        get { return _ACClassID; }
        set { SetProperty<Guid>(ref _ACClassID, value); } 
    }

    Guid? _ACClassPropertyID;
    public Guid? ACClassPropertyID 
    { 
        get { return _ACClassPropertyID; }
        set { SetProperty<Guid?>(ref _ACClassPropertyID, value); } 
    }

    Guid? _ACClassMethodID;
    public Guid? ACClassMethodID 
    { 
        get { return _ACClassMethodID; }
        set { SetProperty<Guid?>(ref _ACClassMethodID, value); } 
    }

    Guid? _ACClassDesignID;
    public Guid? ACClassDesignID 
    { 
        get { return _ACClassDesignID; }
        set { SetProperty<Guid?>(ref _ACClassDesignID, value); } 
    }

    short _ControlModeIndex;
    public short ControlModeIndex 
    { 
        get { return _ControlModeIndex; }
        set { SetProperty<short>(ref _ControlModeIndex, value); } 
    }

    public virtual ACClass ACClass { get; set; }

    public virtual ReferenceEntry ACClassReference 
    { 
        get { return Context.Entry(this).Reference("ACClass"); }
    }
    
    public virtual ACClassDesign ACClassDesign { get; set; }

    public virtual ReferenceEntry ACClassDesignReference 
    { 
        get { return Context.Entry(this).Reference("ACClassDesign"); }
    }
    
    public virtual ACClassMethod ACClassMethod { get; set; }

    public virtual ReferenceEntry ACClassMethodReference 
    { 
        get { return Context.Entry(this).Reference("ACClassMethod"); }
    }
    
    public virtual ACClassProperty ACClassProperty { get; set; }

    public virtual ReferenceEntry ACClassPropertyReference 
    { 
        get { return Context.Entry(this).Reference("ACClassProperty"); }
    }
    
    public virtual VBGroup VBGroup { get; set; }

    public virtual ReferenceEntry VBGroupReference 
    { 
        get { return Context.Entry(this).Reference("VBGroup"); }
    }
    }