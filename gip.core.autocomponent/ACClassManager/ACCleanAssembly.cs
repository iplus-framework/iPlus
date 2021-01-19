using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace gip.core.autocomponent
{
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACCleanAssembly'}de{'ACCleanAssembly'}", Global.ACKinds.TACDBA, Global.ACStorableTypes.NotStorable, false, false)]
    public class ACCleanAssembly : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        public ACCleanAssembly(string assemblyName)
        {
            AssemblyName = assemblyName;
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

        #region CleanItem
        private ACCleanItem _SelectedCleanItem;
        /// <summary>
        /// Selected property for ACCleanItem
        /// </summary>
        /// <value>The selected CleanItem</value>
        [ACPropertySelected(9999, "CleanItem", "en{'TODO: CleanItem'}de{'TODO: CleanItem'}")]
        public ACCleanItem SelectedCleanItem
        {
            get
            {
                return _SelectedCleanItem;
            }
            set
            {
                if (_SelectedCleanItem != value)
                {
                    _SelectedCleanItem = value;
                    OnPropertyChanged("SelectedCleanItem");
                }
            }
        }


        private List<ACCleanItem> _CleanItemList;
        /// <summary>
        /// List property for ACCleanItem
        /// </summary>
        /// <value>The CleanItem list</value>
        [ACPropertyList(9999, "CleanItem")]
        public List<ACCleanItem> CleanItemList
        {
            get
            {
                if (_CleanItemList == null)
                    _CleanItemList = new List<ACCleanItem>();
                return _CleanItemList;
            }
            set
            {
                _CleanItemList = value;
            }
        }

        #endregion


        public override string ToString()
        {
            int count = 0;
            if (CleanItemList != null)
                count = CleanItemList.Count;
            return string.Format("{0}[{1}]", AssemblyName, count);
        }

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
