using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACPackage : VBEntityObject
{

    public ACPackage()
    {
    }

    private ACPackage(ILazyLoader lazyLoader)
    {
        LazyLoader = lazyLoader;
    }

    private ILazyLoader LazyLoader { get; set; }
    Guid _ACPackageID;
    public Guid ACPackageID 
    {
        get { return _ACPackageID; }
        set { SetProperty<Guid>(ref _ACPackageID, value); }
    }

    string _ACPackageName;
    public string ACPackageName 
    {
        get { return _ACPackageName; }
        set { SetProperty<string>(ref _ACPackageName, value); }
    }

    string _Comment;
    public string Comment 
    {
        get { return _Comment; }
        set { SetProperty<string>(ref _Comment, value); }
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

    private ICollection<ACClass> _ACClass_ACPackage;
    public virtual ICollection<ACClass> ACClass_ACPackage
    {
        get => LazyLoader.Load(this, ref _ACClass_ACPackage);
        set => _ACClass_ACPackage = value;
    }

    public virtual CollectionEntry ACClass_ACPackageReference
    {
        get { return Context.Entry(this).Collection("ACClass_ACPackage"); }
    }
}
