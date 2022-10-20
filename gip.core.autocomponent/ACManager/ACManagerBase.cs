using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    public class ACManagerBase : INotifyPropertyChanged
    {
        #region private members
        private IACEntityObjectContext _ObjectContext = null;
        private IRoot _Root = null;
        #endregion

        #region c´tors
        /// <summary>
        /// Konstruktor für "normale" Anwendungen
        /// </summary>
        public ACManagerBase(IACEntityObjectContext database, IRoot root)
        {
            _ObjectContext = database;
            _Root = root;
        }
        #endregion

        #region public methods
        public IACEntityObjectContext ObjectContext
        {
            get
            {
                // Für "normale" Anwendungen
                return _ObjectContext;
            }
        }

        public IRoot Root
        {
            get
            {
                return _Root;
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Tritt ein, wenn sich ein Eigenschaftswert ändert.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

    }
}
