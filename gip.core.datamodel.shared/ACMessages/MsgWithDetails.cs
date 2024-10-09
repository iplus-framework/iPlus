// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="MsgWithDetails.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.Collections.ObjectModel;
using System.Linq;

namespace gip.core.datamodel
{
    /// <summary>
    /// Class MsgWithDetails
    /// </summary>
    [DataContract]
#if NETFRAMEWORK
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Messagedetail'}de{'Meldungdetail'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
#endif
    public class MsgWithDetails : Msg
    {
        #region c'tors
        /// <summary>
        /// Initializes a new instance of the <see cref="MsgWithDetails"/> class.
        /// </summary>
        public MsgWithDetails() : base()
        {
        }

        public MsgWithDetails(IList<Msg> msgDetails)
            : base()
        {
            UpdateFrom(msgDetails);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Msg"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="alarmPropertyName">Name of the alarm property.</param>
        public MsgWithDetails(string source, string alarmPropertyName)
            : base(source, alarmPropertyName)
        {
        }

#if NETFRAMEWORK
        public MsgWithDetails(IACComponent source, eMsgLevel msgLevel, string className, string methodName, int sourceRowID, string translID)
            : base(source, msgLevel, className, methodName, sourceRowID, translID)
        {
        }

        public MsgWithDetails(IACComponent source, eMsgLevel msgLevel, string className, string methodName, int sourceRowID, string translID, params object[] parameter)
            : base(source, msgLevel, className, methodName, sourceRowID, translID, parameter)
        {
        }

        public MsgWithDetails(string message, IACComponent source, eMsgLevel msgLevel, string className, string methodName, int sourceRowID)
            : base(message, source, msgLevel, className, methodName, sourceRowID)
        {
        }
#endif
        #endregion

        #region IMsgWithDetails Members

        [IgnoreDataMember]
        ObservableCollection<Msg> _MsgDetails = new ObservableCollection<Msg>();

        /// <summary>
        /// List of collected submessages
        /// </summary>
        /// <value>IList{Msg}</value>
        [DataMember]
#if NETFRAMEWORK
        [ACPropertyList(9999, "Details", "en{'Messagelist'}de{'Meldungsliste'}")]
#endif
        public IList<Msg> MsgDetails
        {
            get
            {
                return _MsgDetails;
            }
            set
            {
                UpdateFrom(value);
            }
        }


        [IgnoreDataMember]
        Msg _CurrentMsgDetail;

        [IgnoreDataMember]
#if NETFRAMEWORK
        [ACPropertyCurrent(9999, "Details")]
#endif
        public Msg CurrentMsgDetail
        {
            get
            {
                return _CurrentMsgDetail;
            }
            set
            {
                _CurrentMsgDetail = value;
                OnPropertyChanged("CurrentMsgDetail");
            }
        }

        [IgnoreDataMember]
        Msg _SelectedMsgDetail;

        [IgnoreDataMember]
#if NETFRAMEWORK
        [ACPropertySelected(9999, "Details")]
#endif
        public Msg SelectedMsgDetail
        {
            get
            {
                return _SelectedMsgDetail;
            }
            set
            {
                _SelectedMsgDetail = value;
                OnPropertyChanged("SelectedMsgDetail");
            }
        }

        //public override string Message 
        //{
        //    get
        //    {
        //        return String.IsNullOrEmpty(base.Message) ? DetailsAsText : base.Message;
        //    }
        //    set
        //    {
        //        base.Message = value;
        //    }
        //}

        public override string InnerMessage
        {
            get
            {
                string head = Message;
                string details = DetailsAsText;
                if (!String.IsNullOrEmpty(head))
                {
                    if (!String.IsNullOrEmpty(details))
                        return head + Environment.NewLine + details;
                    else
                        return head;
                }
                else
                    return details;
            }
        }


        [IgnoreDataMember]
        public string DetailsAsText
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                foreach (Msg msg in MsgDetails)
                {
                    builder.AppendLine(msg.InnerMessage);
                }
                return builder.ToString();
            }
        }


        /// <summary>
        /// Gets the MSG details count.
        /// </summary>
        /// <value>The MSG details count.</value>
        [IgnoreDataMember]
#if NETFRAMEWORK
        [ACPropertyInfo(9999)]
#endif
        public int MsgDetailsCount
        {
            get
            {
                if (_MsgDetails == null)
                    return 0;
                return _MsgDetails.Count;
            }
        }

        /// <summary>
        /// Adds the detail message.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        public void AddDetailMessage(Msg msg)
        {
            if (msg == null)
                return;
            if (_MsgDetails == null)
                _MsgDetails = new ObservableCollection<Msg>();
            if (msg is MsgWithDetails)
            {
                MsgWithDetails tmpMsg = msg as MsgWithDetails;
                if (tmpMsg.MsgDetails != null && tmpMsg.MsgDetails.Any())
                {
                    foreach (Msg chMsg in tmpMsg.MsgDetails)
                    {
                        _MsgDetails.Add(chMsg);
                    }
                }
                else
                    _MsgDetails.Add(msg);
            }
            else
            {
                _MsgDetails.Add(msg);
            }
        }

        /// <summary>
        /// Clears the MSG details.
        /// </summary>
        public void ClearMsgDetails()
        {
            if (_MsgDetails != null)
                _MsgDetails.Clear();
        }

        public void UpdateFrom(MsgWithDetails msgDetails)
        {
            if (msgDetails == null)
                return;
            MessageLevel = msgDetails.MessageLevel;
            Source = msgDetails.Source;
            ACIdentifier = msgDetails.ACIdentifier;
            Message = msgDetails.Message;
            XMLConfig = msgDetails.XMLConfig;
            UpdateFrom(msgDetails.MsgDetails);
        }

        public void UpdateFrom(IList<Msg> msgDetails, bool clearDetails = true)
        {
            if (clearDetails)
                ClearMsgDetails();
            if (msgDetails != null)
            {
                foreach (Msg msg in msgDetails)
                {
                    AddDetailMessage(msg);
                }
            }
        }

        public void Append(MsgWithDetails msgDetails)
        {
            if (msgDetails == null)
                return;
            UpdateFrom(msgDetails.MsgDetails, false);
        }

        public override bool IsSucceded()
        {
            if (!base.IsSucceded())
                return false;
            if (MsgDetailsCount > 0)
            {
                foreach (var detailMsg in MsgDetails)
                {
                    if (!detailMsg.IsSucceded())
                        return false;
                }
            }
            return true;
        }

        public override bool HasWarnings()
        {
            if (base.HasWarnings())
                return true;
            if (MsgDetailsCount > 0)
            {
                foreach (var detailMsg in MsgDetails)
                {
                    if (detailMsg.HasWarnings())
                        return true;
                }
            }
            return false;
        }

        public override bool CanRetry()
        {
            if (!base.CanRetry())
                return false;
            if (MsgDetailsCount > 0)
            {
                foreach (var detailMsg in MsgDetails)
                {
                    if (!detailMsg.CanRetry())
                        return false;
                }
            }
            return true;
        }

        public override bool HasInfos()
        {
            if (base.HasInfos())
                return true;
            if (MsgDetailsCount > 0)
            {
                foreach (var detailMsg in MsgDetails)
                {
                    if (detailMsg.HasInfos())
                        return true;
                }
            }
            return false;
        }

        #endregion

        #region IMsg Members

        /// <summary>
        /// Determines whether the specified MSG is equal.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <returns><c>true</c> if the specified MSG is equal; otherwise, <c>false</c>.</returns>
        public override bool IsEqual(Msg msg)
        {
            if (msg == null)
                return false;
            if (!(msg is MsgWithDetails))
                return false;
            if (MessageLevel != msg.MessageLevel)
                return false;
            if (Source != msg.Source)
                return false;
            if (ACIdentifier != msg.ACIdentifier)
                return false;
            if (Message != msg.Message)
                return false;
            if (XMLConfig != msg.XMLConfig)
                return false;
            MsgWithDetails msgDet = msg as MsgWithDetails;
            if (MsgDetailsCount != msgDet.MsgDetailsCount)
                return false;
            if (MsgDetailsCount == 0)
                return true;

            for (int i = 0; i < MsgDetailsCount; i++)
            {
                if (!MsgDetails[i].IsEqual(msgDet.MsgDetails[i]))
                    return false;
            }

            return true;
        }
        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"[{MessageLevel}] {DetailsAsText}";
        }

        #endregion
    }
}
