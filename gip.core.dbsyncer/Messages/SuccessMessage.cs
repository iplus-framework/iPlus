// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.dbsyncer.Messages
{
    /// <summary>
    /// Override of BasicMessage used for message success operation message
    /// </summary>
    public class SuccessMessage:BaseSyncMessage
    {
        /// <summary>
        /// Construct success operation message
        /// </summary>
        /// <param name="context"></param>
        /// <param name="startDbVersion"></param>
        /// <param name="currentDbVersion"></param>
        public SuccessMessage(string context, int startDbVersion, int currentDbVersion)
        {
            Success = true;
            Message = string.Format(@"Context [{0}] is successfuly updated from {1} to {2} version!",context, startDbVersion, currentDbVersion);
        }
    }
}
