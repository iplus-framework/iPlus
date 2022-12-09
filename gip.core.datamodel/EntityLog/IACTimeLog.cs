using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace gip.core.datamodel
{
    public interface IACTimeLog : INotifyPropertyChanged, IACObject
    {
        DateTime? StartDate
        {
            get;
            set;
        }

        DateTime? EndDate
        {
            get;
            set;
        }
    }
}
