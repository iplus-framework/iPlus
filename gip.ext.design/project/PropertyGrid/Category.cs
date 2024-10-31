// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using gip.ext.design.PropertyGrid;

namespace gip.ext.design.PropertyGrid
{
    /// <summary>
    /// View-Model class for a property grid category.
    /// </summary>
    public class Category : INotifyPropertyChanged
    {
        // don't warn on missing XML comments in View-Model
#pragma warning disable 1591

        public Category(string name)
        {
            Name = name;
            Properties = new PropertyNodeCollection();
            //MoreProperties = new ObservableCollection<PropertyNode>();
        }

        public string Name { get; private set; }
        public PropertyNodeCollection Properties { get; private set; }

        private PropertyNode _SelectedNode = null;
        public PropertyNode SelectedNode
        {
            get
            {
                return _SelectedNode;
            }

            set
            {
                _SelectedNode = value;
                RaisePropertyChanged("SelectedNode");
            }
        }
        //public ObservableCollection<PropertyNode> MoreProperties { get; private set; }

        bool isExpanded = true;

        public bool IsExpanded
        {
            get
            {
                return isExpanded;
            }
            set
            {
                isExpanded = value;
                RaisePropertyChanged("IsExpanded");
            }
        }

        //bool showMore;
        //internal bool ShowMoreByFilter;

        //public bool ShowMore {
        //    get {
        //        return showMore;
        //    }
        //    set {
        //        showMore = value;
        //        RaisePropertyChanged("ShowMore");
        //    }
        //}

        bool isVisible;

        public bool IsVisible
        {
            get
            {
                return isVisible;
            }
            set
            {
                isVisible = value;
                RaisePropertyChanged("IsVisible");
            }
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
