// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿namespace gip.core.dbsyncer.Messages
{
    /// <summary>
    /// version message with info update not needed
    /// </summary>
    public class NoUpdateNeeded:BaseSyncMessage
    {
        /// <summary>
        /// Construct not update needed message
        /// </summary>
        /// <param name="context"></param>
        /// <param name="version"></param>
        public NoUpdateNeeded(string context, int version)
        {
            Success = true;
            Message = string.Format(@"Version of context [{0}] is up to date: {1}", context, version);
        }
    }
}
