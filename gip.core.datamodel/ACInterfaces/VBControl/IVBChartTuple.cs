// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="IVBDialog.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace gip.core.datamodel
{
    public interface IVBChartTuple
    {
        object Value1 { get; }
        object Value2 { get; }
    }

    public interface IVBChartTupleT<T1, T2> : IVBChartTuple
    {
        T1 ValueT1 { get; }
        T2 ValueT2 { get; }
    }
}
