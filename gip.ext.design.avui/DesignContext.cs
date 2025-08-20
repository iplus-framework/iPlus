// This is a modification for iplus-framework from Copyright (c) AlphaSierraPapa for the SharpDevelop Team
// This code was originally distributed under the GNU LGPL. The modifications by gipSoft d.o.o. are now distributed under GPLv3.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace gip.ext.design.avui
{
    /// <summary>
    /// iplus Extension
    /// </summary>
    public class DesignItemsChangedEventArgs : EventArgs
    {
        #region c'tors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="affectedItems"></param>
        public DesignItemsChangedEventArgs(ICollection<DesignItem> affectedItems)
            : base()
        {
            _AffectedItems = affectedItems;
        }
        #endregion

        ICollection<DesignItem> _AffectedItems;
        /// <summary>
        /// 
        /// </summary>
        public ICollection<DesignItem> AffectedItems
        {
            get
            {
                return _AffectedItems;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void DesignContextItemsChanged(object sender, DesignItemsChangedEventArgs e);

    /// <summary>
	/// The context that the designer uses.
	/// </summary>
	public abstract class DesignContext
	{
		readonly ServiceContainer _services = new ServiceContainer();
		
		/// <summary>
		/// Creates a new DesignContext instance.
		/// </summary>
		protected DesignContext()
		{
			_services.AddService(typeof(Extensions.ExtensionManager), new Extensions.ExtensionManager(this));
		}
		
		/// <summary>
		/// Gets the <see cref="ServiceContainer"/>.
		/// </summary>
		public ServiceContainer Services {
			[DebuggerStepThrough]
			get { return _services; }
		}
		
		/// <summary>
		/// Gets the root design item.
		/// </summary>
		public abstract DesignItem RootItem {
			get;
		}

		/// <summary>
		/// Save the designed elements as XML.
		/// </summary>
		public abstract void Save(XmlWriter writer);
		
		/// <summary>
		/// Opens a new change group used to batch several changes.
		/// ChangeGroups work as transactions and are used to support the Undo/Redo system.
		/// </summary>
		public abstract ChangeGroup OpenGroup(string changeGroupTitle, ICollection<DesignItem> affectedItems);

        /// <summary>
        /// iplus Extension
        /// </summary>
        public event DesignContextItemsChanged DesignContextItemsChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="affectedItems"></param>
        protected void OnDesignContextItemsChanged(ICollection<DesignItem> affectedItems)
        {
            if (DesignContextItemsChanged != null)
            {
                DesignContextItemsChanged(this, new DesignItemsChangedEventArgs(affectedItems));
            }
        }
    }
}
