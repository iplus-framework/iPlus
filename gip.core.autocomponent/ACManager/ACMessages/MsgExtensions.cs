using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public static class MsgExtensions
    {
        /// <summary>
        /// Meldung ist Erfolgreich, wenn keine dlFailure, dlErrors oder dlException aufgetreten sind
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool IsSucceded(this Msg msg)
        {
            if (msg != null)
                switch (msg.MessageLevel)
                {
                    case eMsgLevel.Failure:
                    case eMsgLevel.Error:
                    case eMsgLevel.Exception:
                        return false;
                }
            if (msg is MsgWithDetails)
            {
                MsgWithDetails msgDet = msg as MsgWithDetails;
                if (msgDet.MsgDetailsCount > 0)
                {
                    foreach (var detailMsg in msgDet.MsgDetails)
                    {
                        if (!detailMsg.IsSucceded())
                            return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Beinhaltet die Meldung dlWarning
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool HasWarnings(this Msg msg)
        {
            switch (msg.MessageLevel)
            {
                case eMsgLevel.Warning:
                    return true;
            }
            if (msg is MsgWithDetails)
            {
                MsgWithDetails msgDet = msg as MsgWithDetails;
                if (msgDet.MsgDetailsCount > 0)
                {
                    foreach (var detailMsg in msgDet.MsgDetails)
                    {
                        if (detailMsg.HasWarnings())
                            return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Wieder
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public static bool CanRetry(this Msg msg)
        {
            switch (msg.MessageLevel)
            {
                case eMsgLevel.Error:
                case eMsgLevel.Exception:
                    return false;
            }
            if (msg is MsgWithDetails)
            {
                MsgWithDetails msgDet = msg as MsgWithDetails;
                if (msgDet.MsgDetailsCount > 0)
                {
                    foreach (var detailMsg in msgDet.MsgDetails)
                    {
                        if (!detailMsg.CanRetry())
                            return false;
                    }
                }
            }
            return true;
        }
    }
}
