// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System;

namespace gip.core.dbsyncer.Messages
{
    /// <summary>
    /// Error version of message
    /// </summary>
    public class ErrorMessage:BaseSyncMessage
    {
        /// <summary>
        /// Process exception to DBSyncer message
        /// </summary>
        /// <param name="ec"></param>
        public ErrorMessage(Exception ec)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            Success = false;
            sb.AppendLine("Execption occured while updating a database!");
            Exception tmpException = ec;
            while (tmpException != null)
            {
                sb.AppendLine(tmpException.Message);
                tmpException = tmpException.InnerException;
            }
            //sb.AppendLine(ec.StackTrace);
            Message += sb.ToString();
        }
    }
}
