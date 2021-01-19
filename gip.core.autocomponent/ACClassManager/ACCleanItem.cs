using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACCleanItem'}de{'ACCleanItem'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, false)]
    public class ACCleanItem : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region ctor's
        public ACCleanItem(string assemblyQualifiedName)
        {
            // FullClassName
            int indexOf = 0;
            if (assemblyQualifiedName.Contains('+'))
                indexOf = assemblyQualifiedName.IndexOf('+');
            else
                indexOf = assemblyQualifiedName.IndexOf(',');
            FullClassName = assemblyQualifiedName.Substring(0, indexOf);

            // ClassName, Namespace
            ClassName = FullClassName.Substring(FullClassName.LastIndexOf('.') + 1, FullClassName.Length - (FullClassName.LastIndexOf('.') + 1));
            if (FullClassName.LastIndexOf('.') > 0)
                Namespace = FullClassName.Substring(0, FullClassName.LastIndexOf('.'));

            // AssemblyName
            var tmpName = assemblyQualifiedName.Substring(assemblyQualifiedName.IndexOf(',') + 1, assemblyQualifiedName.Length - assemblyQualifiedName.IndexOf(',') - 1);
            tmpName = tmpName.Trim();
            tmpName = tmpName.Substring(0, tmpName.IndexOf(','));
            AssemblyName = tmpName;
        }
        #endregion
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

        private string _ClassName;
        [ACPropertyInfo(9999, "ClassName", "en{'Name'}de{'Name'}")]
        public string ClassName
        {
            get
            {
                return _ClassName;
            }
            set
            {
                if (_ClassName != value)
                {
                    _ClassName = value;
                    OnPropertyChanged("ClassName");
                }
            }
        }

        private string _Namespace;
        [ACPropertyInfo(9999, "Namespace", "en{'Namespace'}de{'Namespace'}")]
        public string Namespace
        {
            get
            {
                return _Namespace;
            }
            set
            {
                if (_Namespace != value)
                {
                    _Namespace = value;
                    OnPropertyChanged("Namespace");
                }
            }
        }

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
