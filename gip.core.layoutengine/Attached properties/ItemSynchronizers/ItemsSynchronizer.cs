using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace gip.core.layoutengine
{
    public abstract class ItemsSynchronizer : IWeakEventListener, IDisposable
    {
        private bool _Disposed = false;

        protected bool _TargetLock = false;
        protected IValueConverter _ItemConverter = null;
        

        #region Properties

        public abstract IList Source { get; }
        public abstract ICollection Target { get; }

        public IValueConverter ItemConverter
        {
            get { return _ItemConverter; }
        }

        #endregion
        

        #region Protected

        protected abstract bool OnWeakEvent(Type managerType, object sender, EventArgs e);

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            return this.OnWeakEvent(managerType, sender, e); 
        }

        #endregion
        

        #region IDisposable

        protected abstract void OnDisposing();

        public void Dispose()
        {
            if (!_Disposed)
            {
                _Disposed = true;
                OnDisposing();
            }
            GC.SuppressFinalize(this);
        }

        #endregion

    }
}
