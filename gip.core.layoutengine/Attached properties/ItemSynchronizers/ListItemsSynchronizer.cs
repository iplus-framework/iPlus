using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Collections.Specialized;

namespace gip.core.layoutengine
{
    public class ListItemsSynchronizer : ItemsSynchronizer
    {

        private const String _ExSource = "gip.core.layoutengine.ListItemsSynchronizer.";

        private static readonly Type _Type = typeof(object);

        private IList _Source;
        private IList _Target;


        #region Properties

        public override IList Source
        {
            get { return _Source; }
        }

        public override ICollection Target
        {
            get { return _Target; }
        }

        #endregion


        #region Constructors

        public ListItemsSynchronizer(IList source, IList target, SynchronizationModes initialSynchronizationMode, IValueConverter itemConveter)
        {
            if (source == null)
                throw new ArgumentNullException("source") { Source = _ExSource + "New" };
            else if (target == null)
                throw new ArgumentNullException("target") { Source = _ExSource + "New" };
            else if (!(source is INotifyCollectionChanged))
                throw new Exception("Source list must implement INotifyCollectionChanged interface.") { Source = _ExSource + "New" };
            else if (target.IsReadOnly)
                throw new Exception("Target collection cannot be read-only.") { Source = _ExSource + "New" };

            _Source = source;
            _Target = target;
            _ItemConverter = itemConveter;

            switch (initialSynchronizationMode)
            {
                case SynchronizationModes.SourceToTarget:
                    _Target.Clear();

                    if (_ItemConverter != null)
                    {
                        foreach (object item in _Source)
                        {
                            _Target.Add(item);
                        }
                    }
                    else
                    {
                        foreach (object item in _Source)
                        {
                            _Target.Add(_ItemConverter.Convert(item, _Type, null, System.Globalization.CultureInfo.CurrentCulture));
                        }
                    }

                    break;


                case SynchronizationModes.TargetToSource:
                    _Source.Clear();

                    foreach (object item in _Target)
                    {
                        _Source.Add(item);
                    }

                    break;

            }

            CollectionChangedEventManager.AddListener((INotifyCollectionChanged)_Source, this);
        }

        #endregion

        #region Protected

        protected override bool OnWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (_Source.Equals(sender) && !_TargetLock)
            {
                NotifyCollectionChangedEventArgs args = (NotifyCollectionChangedEventArgs)e;

                _TargetLock = true;
                try
                {
                    if (_ItemConverter == null)
                    {
                        switch (args.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                for (int i = 0; i < args.NewItems.Count; i++)
                                {
                                    if (!_Target.Contains(args.NewItems[i]))
                                        _Target.Insert(args.NewStartingIndex + i, args.NewItems[i]);
                                }

                                break;


                            case NotifyCollectionChangedAction.Remove:
                                for (int i = 0; i < args.OldItems.Count; i++)
                                {
                                    _Target.Remove(args.OldItems[i]);
                                }

                                break;


                            case NotifyCollectionChangedAction.Replace:
                                for (int i = 0; i < args.NewItems.Count; i++)
                                {
                                    _Target[args.NewStartingIndex + i] = args.NewItems[i];
                                }

                                break;


                            case NotifyCollectionChangedAction.Move:
                                break;


                            case NotifyCollectionChangedAction.Reset:
                                _Target.Clear();

                                break;
                        }
                    }
                    else
                    {
                        switch (args.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                for (int i = 0; i < args.NewItems.Count; i++)
                                {
                                    _Target.Insert(args.NewStartingIndex + i, _ItemConverter.Convert(args.NewItems[i], _Type, null, System.Globalization.CultureInfo.CurrentCulture));
                                }

                                break;


                            case NotifyCollectionChangedAction.Remove:
                                for (int i = 0; i < args.OldItems.Count; i++)
                                {
                                    _Target.Remove(_ItemConverter.Convert(args.OldItems[i], _Type, null, System.Globalization.CultureInfo.CurrentCulture));
                                }

                                break;


                            case NotifyCollectionChangedAction.Replace:
                                for (int i = 0; i < args.NewItems.Count; i++)
                                {
                                    _Target[args.NewStartingIndex + i] = _ItemConverter.Convert(args.NewItems[i], _Type, null, System.Globalization.CultureInfo.CurrentCulture);
                                }

                                break;


                            case NotifyCollectionChangedAction.Move:
                                break;


                            case NotifyCollectionChangedAction.Reset:
                                _Target.Clear();

                                break;
                        }
                    }
                }
                finally
                {
                    _TargetLock = false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion

        #region IDisposable

        protected override void OnDisposing()
        {
            CollectionChangedEventManager.RemoveListener((INotifyCollectionChanged)_Source, this);
            _Source = null;
            _Target = null;
            _ItemConverter = null;
        }

        #endregion
    }
}
