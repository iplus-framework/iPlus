﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class VBGroupRight : VBEntityObject 
{

    public VBGroupRight()
    {
    }

    private VBGroupRight(ILazyLoader lazyLoader)
    {
        LazyLoader = lazyLoader;
    }

    private ILazyLoader LazyLoader { get; set; }
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

    private ACClass _ACClass;
    public virtual ACClass ACClass
    { 
        get { return LazyLoader.Load(this, ref _ACClass); }
        set { SetProperty<ACClass>(ref _ACClass, value); }
    }

    public bool ACClass_IsLoaded
    {
        get
        {
            return _ACClass != null;
        }
    }

    public virtual ReferenceEntry ACClassReference 
    {
        get { return Context.Entry(this).Reference("ACClass"); }
    }
    
    private ACClassDesign _ACClassDesign;
    public virtual ACClassDesign ACClassDesign
    { 
        get { return LazyLoader.Load(this, ref _ACClassDesign); }
        set { SetProperty<ACClassDesign>(ref _ACClassDesign, value); }
    }

    public bool ACClassDesign_IsLoaded
    {
        get
        {
            return _ACClassDesign != null;
        }
    }

    public virtual ReferenceEntry ACClassDesignReference 
    {
        get { return Context.Entry(this).Reference("ACClassDesign"); }
    }
    
    private ACClassMethod _ACClassMethod;
    public virtual ACClassMethod ACClassMethod
    { 
        get { return LazyLoader.Load(this, ref _ACClassMethod); }
        set { SetProperty<ACClassMethod>(ref _ACClassMethod, value); }
    }

    public bool ACClassMethod_IsLoaded
    {
        get
        {
            return _ACClassMethod != null;
        }
    }

    public virtual ReferenceEntry ACClassMethodReference 
    {
        get { return Context.Entry(this).Reference("ACClassMethod"); }
    }
    
    private ACClassProperty _ACClassProperty;
    public virtual ACClassProperty ACClassProperty
    { 
        get { return LazyLoader.Load(this, ref _ACClassProperty); }
        set { SetProperty<ACClassProperty>(ref _ACClassProperty, value); }
    }

    public bool ACClassProperty_IsLoaded
    {
        get
        {
            return _ACClassProperty != null;
        }
    }

    public virtual ReferenceEntry ACClassPropertyReference 
    {
        get { return Context.Entry(this).Reference("ACClassProperty"); }
    }
    
    private VBGroup _VBGroup;
    public virtual VBGroup VBGroup
    { 
        get { return LazyLoader.Load(this, ref _VBGroup); }
        set { SetProperty<VBGroup>(ref _VBGroup, value); }
    }

    public bool VBGroup_IsLoaded
    {
        get
        {
            return _VBGroup != null;
        }
    }

    public virtual ReferenceEntry VBGroupReference 
    {
        get { return Context.Entry(this).Reference("VBGroup"); }
    }
    }
