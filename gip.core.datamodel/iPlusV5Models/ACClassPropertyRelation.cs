using System;
using System.Collections.Generic;
using gip.core.datamodel;

namespace gip.core.datamodel;

public partial class ACClassPropertyRelation : VBEntityObject
{
    Guid _ACClassPropertyRelationID;
    public Guid ACClassPropertyRelationID 
    { 
        get { return _ACClassPropertyRelationID; }
        set { SetProperty<Guid>(ref _ACClassPropertyRelationID, value); } 
    }

    Guid _SourceACClassID;
    public Guid SourceACClassID 
    { 
        get { return _SourceACClassID; }
        set { SetProperty<Guid>(ref _SourceACClassID, value); } 
    }

    Guid? _SourceACClassPropertyID;
    public Guid? SourceACClassPropertyID 
    { 
        get { return _SourceACClassPropertyID; }
        set { SetProperty<Guid?>(ref _SourceACClassPropertyID, value); } 
    }

    Guid _TargetACClassID;
    public Guid TargetACClassID 
    { 
        get { return _TargetACClassID; }
        set { SetProperty<Guid>(ref _TargetACClassID, value); } 
    }

    Guid? _TargetACClassPropertyID;
    public Guid? TargetACClassPropertyID 
    { 
        get { return _TargetACClassPropertyID; }
        set { SetProperty<Guid?>(ref _TargetACClassPropertyID, value); } 
    }

    short _ConnectionTypeIndex;
    public short ConnectionTypeIndex 
    { 
        get { return _ConnectionTypeIndex; }
        set { SetProperty<short>(ref _ConnectionTypeIndex, value); } 
    }

    short _DirectionIndex;
    public short DirectionIndex 
    { 
        get { return _DirectionIndex; }
        set { SetProperty<short>(ref _DirectionIndex, value); } 
    }

    string _XMLValue;
    public string XMLValue 
    { 
        get { return _XMLValue; }
        set { SetProperty<string>(ref _XMLValue, value); } 
    }

    short _LogicalOperationIndex;
    public short LogicalOperationIndex 
    { 
        get { return _LogicalOperationIndex; }
        set { SetProperty<short>(ref _LogicalOperationIndex, value); } 
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

    double? _Multiplier;
    public double? Multiplier 
    { 
        get { return _Multiplier; }
        set { SetProperty<double?>(ref _Multiplier, value); } 
    }

    double? _Divisor;
    public double? Divisor 
    { 
        get { return _Divisor; }
        set { SetProperty<double?>(ref _Divisor, value); } 
    }

    string _ConvExpressionT;
    public string ConvExpressionT 
    { 
        get { return _ConvExpressionT; }
        set { SetProperty<string>(ref _ConvExpressionT, value); } 
    }

    string _ConvExpressionS;
    public string ConvExpressionS 
    { 
        get { return _ConvExpressionS; }
        set { SetProperty<string>(ref _ConvExpressionS, value); } 
    }

    short _RelationWeight;
    public short RelationWeight 
    { 
        get { return _RelationWeight; }
        set { SetProperty<short>(ref _RelationWeight, value); } 
    }

    short _UseFactor;
    public short UseFactor 
    { 
        get { return _UseFactor; }
        set { SetProperty<short>(ref _UseFactor, value); } 
    }

    DateTime _LastManipulationDT;
    public DateTime LastManipulationDT 
    { 
        get { return _LastManipulationDT; }
        set { SetProperty<DateTime>(ref _LastManipulationDT, value); } 
    }

    bool _IsDeactivated;
    public bool IsDeactivated 
    { 
        get { return _IsDeactivated; }
        set { SetProperty<bool>(ref _IsDeactivated, value); } 
    }

    short? _DisplayGroup;
    public short? DisplayGroup 
    { 
        get { return _DisplayGroup; }
        set { SetProperty<short?>(ref _DisplayGroup, value); } 
    }

    string _GroupName;
    public string GroupName 
    { 
        get { return _GroupName; }
        set { SetProperty<string>(ref _GroupName, value); } 
    }

    string _StateName;
    public string StateName 
    { 
        get { return _StateName; }
        set { SetProperty<string>(ref _StateName, value); } 
    }

    public virtual ICollection<ACClassConfig> ACClassConfig_ACClassPropertyRelation { get; } = new List<ACClassConfig>();

    public virtual ICollection<ACClassMethodConfig> ACClassMethodConfig_VBiACClassPropertyRelation { get; } = new List<ACClassMethodConfig>();

    public virtual ICollection<ACProgramConfig> ACProgramConfig_ACClassPropertyRelation { get; } = new List<ACProgramConfig>();

    public virtual ACClass SourceACClass { get; set; }

    public virtual ACClassProperty SourceACClassProperty { get; set; }

    public virtual ACClass TargetACClass { get; set; }

    public virtual ACClassProperty TargetACClassProperty { get; set; }

    public virtual ICollection<VBConfig> VBConfig_ACClassPropertyRelation { get; } = new List<VBConfig>();
}
