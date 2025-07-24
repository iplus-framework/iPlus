// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using gip.core.autocomponent;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;

namespace gip.bso.iplus 
{
    #region Wrapper Classes for WPF Binding

    [ACClassInfo(Const.PackName_VarioSystem, "en{'Chatbot History'}de{'Chatbot Historie'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, false, false)]
    public class ChatConfigWrapper : ChatWrapperBase
    {
        private Guid _ACClassConfigID;
        private string _Comment;
        private DateTime _InsertDate;

        public ChatConfigWrapper(IACObject parentACObject, Guid aCClassConfigID, string comment, DateTime insertDate) : base(parentACObject)
        {
            _ACClassConfigID = aCClassConfigID;
            _Comment = comment;
            _InsertDate = insertDate;
        }

        public Guid ACClassConfigID
        {
            get => _ACClassConfigID;
            set
            {
                if (_ACClassConfigID != value)
                {
                    _ACClassConfigID = value;
                    OnPropertyChanged(nameof(ACClassConfigID));
                }
            }
        }

        [ACPropertyInfo(1, "", "en{'Chat Description'}de{'Chat Beschreibung'}")]
        public string Comment
        {
            get => _Comment;
            set
            {
                if (_Comment != value)
                {
                    _Comment = value;
                    OnPropertyChanged(nameof(Comment));
                }
            }
        }

        [ACPropertyInfo(2, "", "en{'Inserted At'}de{'Angelegt am'}")]
        public DateTime InsertDate
        {
            get => _InsertDate;
            set
            {
                if (_InsertDate != value)
                {
                    _InsertDate = value;
                    OnPropertyChanged(nameof(InsertDate));
                }
            }
        }

        public override ChatRole? ChatMessageRole => null;
    }
    #endregion
}