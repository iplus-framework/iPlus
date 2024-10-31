#region Copyright and License Information
// This is a modification for iplus-framework of the Fluent Ribbon Control Suite
// https://github.com/fluentribbon/Fluent.Ribbon
// Copyright (c) Degtyarev Daniel, Rikker Serg. 2009-2010.  All rights reserved.
// 
// This code was originally distributed under the Microsoft Public License (Ms-PL). The modifications by gipSoft d.o.o. are now distributed under GPLv3.
// The license is available online https://github.com/fluentribbon/Fluent.Ribbonlicense
#endregion

using System;

namespace Fluent
{
    /// <summary>
    /// Repesents scalable ribbon contol
    /// </summary>
    public interface IScalableRibbonControl
    {
        /// <summary>
        /// Enlarge control size
        /// </summary>
        void Enlarge();
        /// <summary>
        /// Reduce control size
        /// </summary>
        void Reduce();

        /// <summary>
        /// Occurs when contol is scaled
        /// </summary>
        event EventHandler Scaled;
    }
}
