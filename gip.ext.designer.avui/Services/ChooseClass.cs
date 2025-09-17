// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Linq;

namespace gip.ext.designer.avui.Services
{
    public class ChooseClass : INotifyPropertyChanged
    {
        public ChooseClass(IEnumerable<Assembly> assemblies)
        {
            foreach (var a in assemblies)
            {
                foreach (var t in a.GetExportedTypes())
                {
                    if (t.IsClass)
                    {
                        if (t.IsAbstract) continue;
                        if (t.IsNested) continue;
                        if (t.IsGenericTypeDefinition) continue;
                        if (t.GetConstructor(Type.EmptyTypes) == null) continue;
                        projectClasses.Add(t);
                    }
                }
            }

            projectClasses.Sort((c1, c2) => c1.Name.CompareTo(c2.Name));
            classes = new ObservableCollection<Type>(FilteredClasses);
        }

        List<Type> projectClasses = new List<Type>();

        ObservableCollection<Type> classes;

        public ObservableCollection<Type> Classes
        {
            get { return classes; }
        }

        private IEnumerable<Type> FilteredClasses
        {
            get
            {
                return projectClasses.Where(FilterPredicate);
            }
        }

        string filter;

        public string Filter
        {
            get
            {
                return filter;
            }
            set
            {
                filter = value;
                RefreshClasses();
                RaisePropertyChanged("Filter");
            }
        }

        bool showSystemClasses;

        public bool ShowSystemClasses
        {
            get
            {
                return showSystemClasses;
            }
            set
            {
                showSystemClasses = value;
                RefreshClasses();
                RaisePropertyChanged("ShowSystemClasses");
            }
        }

        public Type CurrentClass
        {
            get 
            {
                // TODO: Old Implememnation returned Classes.CurrentItem from CollectionView
                return classes.FirstOrDefault(); 
            }
        }

        private void RefreshClasses()
        {
            classes.Clear();
            foreach (var type in FilteredClasses)
            {
                classes.Add(type);
            }
        }

        bool FilterPredicate(Type c)
        {
            if (!ShowSystemClasses)
            {
                if (c.Namespace.StartsWith("System") || c.Namespace.StartsWith("Microsoft"))
                {
                    return false;
                }
            }
            return Match(c.Name, Filter);
        }

        static bool Match(string className, string filter)
        {
            if (string.IsNullOrEmpty(filter))
                return true;
            else
                return className.StartsWith(filter, StringComparison.InvariantCultureIgnoreCase);
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        void RaisePropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }
        #endregion
    }
}
