// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 11-07-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-26-2012
// ***********************************************************************
// <copyright file="ACMemberIndexer.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class that enables to access ACProperties of a Component through a string-index
    /// </summary>
    /// <typeparam name="I"></typeparam>
    /// <typeparam name="T"></typeparam>
    public class ACMemberIndexer<I,T>
    {
        /// <summary>
        /// The getter
        /// </summary>
        private Func<I,T> getter;
        /// <summary>
        /// The setter
        /// </summary>
        private Action<I, T> setter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ACMemberIndexer{I, T}"/> class.
        /// </summary>
        /// <param name="g">The g.</param>
        /// <param name="s">The s.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public ACMemberIndexer(Func<I, T> g, Action<I, T> s)
        {
            if (g == null || s == null)
                throw new ArgumentNullException();
            getter = g;
            setter = s;
        }

        public void ACDeInit()
        {
            getter = null;
            setter = null;
        }

        /// <summary>
        /// Gets or sets at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>`1.</returns>
        public T this[I index]
        {
            get
            {
                return getter(index);
            }
            set
            {
                setter(index, value);
            }
        }
    }
}
