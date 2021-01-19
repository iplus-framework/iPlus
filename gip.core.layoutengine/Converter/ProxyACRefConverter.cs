using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using gip.core.datamodel;
using System.Windows.Markup;
using System.Collections;
using System.Windows.Data;
using System.Reflection;
using System.Windows.Threading;
using System.Windows;
using System.ComponentModel;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Der ProxyACRefConverter ist eine Hilfsklasse für das Aktualisieren von Eigenschaften von Proxy-ACComponents,
    /// die per ACRef von einer anderen ACComponent referenziert wird.
    /// Wenn sich nun die Proxy-Komponente in der ACRef dynamisch ändert, müssen dann die Eigenschaften der neuen Proxy-Komponente
    /// auch an den Target-Dependency-Properties automatisch aktualisiert werden. 
    /// Damit das funktioniert benötigt man diese Converter-Klasse
    /// </summary>
    public class ProxyACRefConverter : IValueConverter
    {
        public ProxyACRefConverter(string completePath)
        {
            CompletePath = completePath;
        }

        public static ProxyACRefConverter IfACRefGenerateConverter(out Binding binding, ref IACType dcACTypeInfo, ref string dcPath, ref object dcSource, ref Global.ControlModes dcRightControlMode)
        {
            binding = null;
            if (dcPath == null)
                return null;
            int indexOfACRefValueT = dcPath.IndexOf("ValueT.ValueT");
            if (indexOfACRefValueT != 0
                || dcSource == null
                || !(dcSource is IACPropertyBase)
                || dcPath.Length <= 14)
                return null;

            ProxyACRefConverter converter = new ProxyACRefConverter(dcPath);
            binding = converter.GenerateBinding(ref dcACTypeInfo, ref dcPath, ref dcSource, ref dcRightControlMode);
            binding.Converter = converter;
            return converter;
        }

        #region Properties
        private IACPropertyBase _currentProperty = null;
        private IACContainerRef _currentACRef = null;

        /// <summary>
        /// ValueT.ValueT.xxx.ValueT
        /// ValueT.ValueT.xxx.ValueT
        /// </summary>
        public string CompletePath
        {
            get;
            protected set;
        }

        public string Path1ToACRef
        {
            get
            {
                if (String.IsNullOrEmpty(CompletePath))
                    return null;
                if (CompletePath.Length < 6)
                    return null;
                string path1 = CompletePath.Substring(0,6);
                if (path1 != Const.ValueT)
                    return null;
                return path1;
            }
        }

        public string Path2ToACObject
        {
            get
            {
                if (String.IsNullOrEmpty(CompletePath))
                    return null;
                if (CompletePath.Length < 13)
                    return null;
                string path2 = CompletePath.Substring(0,13);
                if (path2 != "ValueT.ValueT")
                    return null;
                return path2;
            }
        }

        public string Path3ToPropertyOfACObject
        {
            get
            {
                if (String.IsNullOrEmpty(CompletePath))
                    return null;
                if (CompletePath.Length < 15)
                    return null;
                string restOfPath = CompletePath.Substring(14);
                int indexOfACRefValueT2 = CompletePath.IndexOf(".ValueT",14);
                if (indexOfACRefValueT2 < 0)
                    return CompletePath;
                else
                    return CompletePath.Substring(0, indexOfACRefValueT2);
            }
        }

        public string Path3PropertyName
        {
            get
            {
                if (String.IsNullOrEmpty(CompletePath))
                    return null;
                if (CompletePath.Length < 15)
                    return null;
                string restOfPath = CompletePath.Substring(14);
                int indexOfACRefValueT2 = restOfPath.IndexOf(".ValueT");
                if (indexOfACRefValueT2 <= 0)
                {
                    return restOfPath;
                }
                else
                {
                    return restOfPath.Substring(0, indexOfACRefValueT2);
                }
            }
        }

        public BindingExpressionBase ParentBinding
        {
            get;
            set;
        }
        #endregion

        #region methods

        protected Binding GenerateBinding(ref IACType dcACTypeInfo, ref string dcPath, ref object dcSource, ref Global.ControlModes dcRightControlMode)
        {
            Binding bindingIACProp = new Binding();
            bindingIACProp.Source = dcSource;
            bindingIACProp.Path = new PropertyPath(Const.ValueT);
            bindingIACProp.Mode = BindingMode.OneWay;
            bindingIACProp.NotifyOnSourceUpdated = true;
            bindingIACProp.NotifyOnTargetUpdated = true;

            dcRightControlMode = Global.ControlModes.Enabled;
            _currentACRef = dcSource.GetValue(Const.ValueT) as IACContainerRef;
            if (_currentACRef != null)
            {
                _currentACRef.ObjectAttached += new EventHandler(acRef_ObjectAttached);
                _currentACRef.ObjectDetached += new EventHandler(acRef_ObjectDetached);

                IACComponent acComponent = _currentACRef.Value as IACComponent;
                if (acComponent != null && !String.IsNullOrEmpty(Path3PropertyName))
                {
                    IACPropertyBase acProperty = acComponent.GetMember(Path3PropertyName) as IACPropertyBase;
                    if (acProperty != null)
                    {
                        dcACTypeInfo = acProperty.ACType;
                        dcSource = acProperty;
                        dcPath = Const.ValueT;

                        _currentProperty = acProperty;
                        acProperty.PropertyChanged += new PropertyChangedEventHandler(acProperty_PropertyChanged);
                    }
                }
            }

            return bindingIACProp;
        }


        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IACContainerRef acRef = value as IACContainerRef;
            if (acRef == null)
            {
                UnsubScribeAllEvents();
                _currentACRef = null;
                _currentProperty = null;
            }
            else
            {
                if (_currentACRef != null && acRef != _currentACRef)
                {
                    UnsubScribeAllEvents();
                    _currentACRef = null;
                    _currentProperty = null;
                }
                if (_currentACRef == null)
                {
                    _currentACRef = acRef;
                    SubscribeAllEvents();
                }
            }

            if (_currentProperty != null)
                return _currentProperty.Value;
            else if (_currentACRef != null && _currentACRef.Value != null && !String.IsNullOrEmpty(Path3PropertyName))
            {
                IACComponent acComponent = _currentACRef.Value as IACComponent;
                if (acComponent != null)
                    return acComponent.ACUrlCommand(this.Path3PropertyName);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private void UnsubScribeAllEvents()
        {
            UnsubScribePropertyChanged();
            if (_currentACRef != null)
            {
                _currentACRef.ObjectAttached -= acRef_ObjectAttached;
                _currentACRef.ObjectDetached -= acRef_ObjectDetached;
            }
        }

        private void SubscribeAllEvents()
        {
            if (_currentACRef == null)
                return;
            _currentACRef.ObjectAttached += new EventHandler(acRef_ObjectAttached);
            _currentACRef.ObjectDetached += new EventHandler(acRef_ObjectDetached);
            SubScribePropertyChanged();
        }

        private void UnsubScribePropertyChanged()
        {
            if (_currentProperty != null)
                _currentProperty.PropertyChanged -= acProperty_PropertyChanged;
        }

        private void SubScribePropertyChanged()
        {
            if (_currentProperty != null)
                UnsubScribePropertyChanged();
            _currentProperty = null;
            if (_currentACRef == null)
                return;
            IACComponent acComponent = _currentACRef.Value as IACComponent;
            if (acComponent != null && !String.IsNullOrEmpty(Path3PropertyName))
            {
                IACPropertyBase acProperty = acComponent.GetMember(Path3PropertyName) as IACPropertyBase;
                if (acProperty != null)
                {
                    _currentProperty = acProperty;
                    acProperty.PropertyChanged += new PropertyChangedEventHandler(acProperty_PropertyChanged);
                }
            }
        }

        void acRef_ObjectDetached(object sender, EventArgs e)
        {
            UnsubScribePropertyChanged();
            _currentProperty = null;
            UpdateTarget();
        }

        void acRef_ObjectAttached(object sender, EventArgs e)
        {
            if (_currentACRef != null)
            {
                SubScribePropertyChanged();
            }
            UpdateTarget();
        }


        void acProperty_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            UpdateTarget();
        }

        private void UpdateTarget()
        {
            if (ParentBinding != null)
            {
                PropertyInfo pi = ParentBinding.GetType().GetProperty("Target");
                if (pi != null)
                {
                    DependencyObject dispObj = pi.GetValue(ParentBinding, null) as DependencyObject;
                    if (dispObj != null)
                    {
                        dispObj.Dispatcher.BeginInvoke((Action)(() => ParentBinding.UpdateTarget()));
                    }
                }
            }
        }


        //public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        //{
        //    if (values == null || values.Length < 1 || parameter == null)
        //        return "";
        //    string propertyUrl = parameter as string;
        //    if (String.IsNullOrEmpty(propertyUrl))
        //        return "";

        //    IACComponent acComponent = values[0] as IACComponent;
        //    IACPropertyBase acProperty = acComponent.GetMember(propertyUrl) as IACPropertyBase;
        //    if (acProperty != null)
        //    {
        //        if (_currentProperty != acProperty)
        //        {
        //            if (_currentProperty != null)
        //                _currentProperty.PropertyChanged -= acProperty_PropertyChanged;
        //            acProperty.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(acProperty_PropertyChanged);
        //            _currentProperty = acProperty;
        //        }
        //        return acProperty.Value;
        //    }
        //    else
        //    {
        //        if (_currentProperty != null)
        //        {
        //            _currentProperty.PropertyChanged -= acProperty_PropertyChanged;
        //            _currentProperty = null;
        //        }

        //        return acComponent.ACUrlCommand(propertyUrl);
        //    }

        //    //if (values == null || values.Length < 2 || 
        //    //    (values[0] == System.Windows.DependencyProperty.UnsetValue && values[1] == System.Windows.DependencyProperty.UnsetValue))
        //    //    return "";
        //    //if (values[1] == System.Windows.DependencyProperty.UnsetValue)
        //    //    return values[0];
        //    //else if (values[0] == System.Windows.DependencyProperty.UnsetValue)
        //    //    return values[1];
        //    //if (values[0] is IACComponent || values[0] is IACContainer)
        //    //    return values[1];
        //    //return values[0];
        //}

        //public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        //{
        //    throw new NotImplementedException();
        //}


        #endregion


    }
}
