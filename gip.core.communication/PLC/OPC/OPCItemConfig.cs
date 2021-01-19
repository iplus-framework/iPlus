using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.IO;
using System.Xml;
using gip.core.datamodel;
using gip.core.autocomponent;

namespace gip.core.communication
{
    [DataContract]
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'OPC-Configuration'}de{'OPC-Konfiguration'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class OPCItemConfig : INotifyPropertyChanged
    {
        public static void InitProperty(Database database, IACObject acObject)
        {
        }

        private string _OPCAddr;
        [DataMember]
        [ACPropertyInfo(9999)]
        public string OPCAddr 
        {
            get
            {
                return _OPCAddr;
            }

            set
            {
                _OPCAddr = value;
                OnPropertyChanged("OPCAddr");
            }
        }

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
