// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public interface IVBProgress
    {
        void Start();

        /// <summary>
        /// Adds a sub task for displaying a progress bar in the datagrid.
        /// </summary>
        /// <param name="subTaskName">Name of the sub task.</param>
        /// <param name="progressRangeFrom">The progress range from.</param>
        /// <param name="progressRangeTo">The progress range to.</param>
        void AddSubTask(string subTaskName, int progressRangeFrom, int progressRangeTo);

        /// <summary>
        /// Reports the progress.
        /// </summary>
        /// <param name="subTaskName">Name of the sub task. If null, then progress in header will be refreshed.</param>
        /// <param name="progressCurrent">Current progress. If null then current progress will not be changed.</param>
        /// <param name="newProgressText">The new progress text. If null, than current text will bot ne changed.</param>
        void ReportProgress(string subTaskName, int? progressCurrent, string newProgressText = null);

        void CaclulateTotalProgress();

        void Complete();
    }
}
