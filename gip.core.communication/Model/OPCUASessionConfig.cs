// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using gip.core.datamodel;
using System;
using System.ComponentModel;

namespace gip.core.communication
{
    /// <summary>
    /// Model for the OPC UA Browser: one configured OPCUAClientACSession, resolved either from the
    /// real component instance (service mode) or from its <see cref="ACClass"/> configuration
    /// (client mode, instance exists only as ACComponentProxy). Declared as ACClass so that
    /// VBContent/VBShowColumns (e.g. VBComboBox ACCaption) can be resolved by the layout engine.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'OPC UA Session'}de{'OPC UA Session'}", Global.ACKinds.TACSimpleClass, Global.ACStorableTypes.NotStorable, true, true)]
    public class OPCUASessionConfig : INotifyPropertyChanged
    {
        #region c'tors

        internal OPCUASessionConfig()
        {
        }

        #endregion

        #region Properties

        ACClass _ConfigClass;
        /// <summary>ACClass (app-tree node) of the configured session component.</summary>
        [ACPropertyInfo(400)]
        public ACClass ConfigClass
        {
            get => _ConfigClass;
            internal set
            {
                _ConfigClass = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ACIdentifier));
                OnPropertyChanged(nameof(ACCaption));
            }
        }

        /// <summary>The real component instance if it runs in this process; null when only a proxy exists.</summary>
        internal OPCUAClientACSession RealSession { get; set; }

        string _EndpointURL;
        [ACPropertyInfo(401, "", "en{'Server Endpoint URL'}de{'Server Endpoint URL'}")]
        public string EndpointURL
        {
            get => _EndpointURL;
            internal set
            {
                _EndpointURL = value;
                OnPropertyChanged();
            }
        }

        OPCUserAuthenticationMode _AuthMode;
        [ACPropertyInfo(402, "", "en{'User authentication mode'}de{'Benutzer-Authentifizierung'}")]
        public OPCUserAuthenticationMode AuthMode
        {
            get => _AuthMode;
            internal set
            {
                _AuthMode = value;
                OnPropertyChanged();
            }
        }

        string _Username;
        [ACPropertyInfo(403, "", "en{'Username'}de{'Benutzername'}")]
        public string Username
        {
            get => _Username;
            internal set
            {
                _Username = value;
                OnPropertyChanged();
            }
        }

        string _Password;
        [ACPropertyInfo(404)]
        public string Password
        {
            get => _Password;
            internal set
            {
                _Password = value;
                OnPropertyChanged();
            }
        }

        /// <summary>ACIdentifier of the session's ACClass - used as identifier in messages.</summary>
        [ACPropertyInfo(405)]
        public string ACIdentifier
        {
            get
            {
                return ConfigClass != null ? ConfigClass.ACIdentifier : null;
            }
        }

        /// <summary>Caption shown in the session combobox; falls back to the ACIdentifier.</summary>
        [ACPropertyInfo(406, "", "en{'Session'}de{'Session'}")]
        public string ACCaption
        {
            get
            {
                if (ConfigClass == null)
                    return null;
                string caption = ConfigClass.ACCaption;
                return String.IsNullOrEmpty(caption) ? ConfigClass.ACIdentifier : caption;
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
