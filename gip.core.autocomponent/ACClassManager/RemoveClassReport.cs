using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'RemoveClassReport'}de{'RemoveClassReport'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, false)]
    public class RemoveClassReport : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MsgWithDetails MsgWithDetails { get; set; }

        private string _ErrorMessage;
        [ACPropertyInfo(9999, "ErrorMessage", "en{'Success'}de{'Erfolg'}")]
        public string ErrorMessage
        {
            get
            {
                return _ErrorMessage;
            }
            set
            {
                if(_ErrorMessage != value)
                {
                    _ErrorMessage = value;
                    OnPropertyChanged("ErrorMessage");
                }
            }
        }

        private bool _Success;
        [ACPropertyInfo(9999, "Success", "en{'Success'}de{'Erfolg'}")]
        public bool Success
        {
            get
            {
                return _Success;
            }
            set
            {
                if (_Success != value)
                {
                    _Success = value;
                    OnPropertyChanged("Success");
                }
            }
        }
        public Guid ACClassID { get; set; }

        private string _ACIdentifier;
        [ACPropertyInfo(9999, "ACIdentifier", "en{'ACIdentifier'}de{'ACIdentifier'}")]
        public string ACIdentifier
        {
            get
            {
                return _ACIdentifier;
            }
            set
            {
                if(_ACIdentifier != value)
                {
                    _ACIdentifier = value;
                    OnPropertyChanged("ACIdentifier");
                }
            }
        }

        private string _AssemblyName;
        [ACPropertyInfo(9999, "AssemblyName", "en{'Assembly-qualified name'}de{'Assembly qualifizierter Name'}")]
        public string AssemblyName
        {
            get
            {
                return _AssemblyName;
            }
            set
            {
                if (_AssemblyName != value)
                {
                    _AssemblyName = value;
                    OnPropertyChanged("AssemblyName");
                }
            }
        }


        private string _FullClassName;
        [ACPropertyInfo(9999, "FullClassName", "en{'Full Name'}de{'Name'}")]
        public string FullClassName
        {
            get
            {
                return _FullClassName;
            }
            set
            {
                if (_FullClassName != value)
                {
                    _FullClassName = value;
                    OnPropertyChanged("FullClassName");
                }
            }
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", Success, FullClassName);
        }


        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
