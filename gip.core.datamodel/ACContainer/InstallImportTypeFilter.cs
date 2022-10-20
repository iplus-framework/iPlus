using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    public delegate void ImportTypeFilterChange(InstallImportTypeFilter item);
    public class InstallImportTypeFilter
    {
        public event ImportTypeFilterChange OnImportTypeFilterChange;
        private bool _IsChecked { get; set; }

        public string ACCaption { get; set; }

        public bool IsChecked
        {
            get
            {
                return _IsChecked;
            }
            set
            {
                if (_IsChecked != value)
                {
                    _IsChecked = value;
                    if (OnImportTypeFilterChange != null)
                        OnImportTypeFilterChange(this);
                }
            }
        }

        public Type Type { get; set; }

        public override string ToString()
        {
            return string.Format(@"{0} ({1})", ACCaption, Type.Name);
        }
    }
}
