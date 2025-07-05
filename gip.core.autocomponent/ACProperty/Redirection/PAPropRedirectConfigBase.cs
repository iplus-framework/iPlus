// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
﻿using System.Runtime.CompilerServices;
using gip.core.datamodel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.autocomponent
{
    [DataContract]
    [ACSerializeableInfo]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'Config. property operation(export, redirection...)'}de{'Config. property operation(export, redirection...'}", Global.ACKinds.TACAbstractClass, Global.ACStorableTypes.NotStorable, true, false)]
    public abstract class PAPropRedirectConfigBase : INotifyPropertyChanged, IACObject
    {

        bool _ExportOff = false;
        [DataMember]
        [ACPropertyInfo(9999, "", "en{'Don't export'}de{'Nicht exportieren'}")]
        public bool ExportOff
        {
            get
            {
                return _ExportOff;
            }
            set
            {
                _ExportOff = value;
                OnPropertyChanged("ExportOff");
            }
        }

        private string _ValidationMethodName;
        [DataMember]
        [ACPropertyInfo(9999, "", "en{'Script method for validate values'}de{'Script method for validate values'}")]
        public string ValidationMethodName
        {
            get
            {
                return _ValidationMethodName;
            }
            set
            {
                _ValidationMethodName = value;
                OnPropertyChanged("ValidationMethodName");
            }
        }

        gip.core.datamodel.Global.InterpolationMethod _Interpolation = 0;
        [DataMember]
        [ACPropertyInfo(9999, "", "en{'Interpolation method'}de{'Interpolationsmethode'}")]
        public gip.core.datamodel.Global.InterpolationMethod Interpolation
        {
            get
            {
                return _Interpolation;
            }
            set
            {
                _Interpolation = value;
                OnPropertyChanged("Interpolation");
            }
        }

        int _IRange = 0;
        [DataMember]
        [ACPropertyInfo(9999, "", "en{'Interpolation range'}de{'Interpolations-Bereich'}")]
        public int IRange
        {
            get
            {
                return _IRange;
            }
            set
            {
                _IRange = value;
                OnPropertyChanged("IRange");
            }
        }

        public int InterpolationRange
        {
            get
            {
                if (_IRange < 0)
                    return 0;
                else if (_IRange > 20)
                    return 20;
                return _IRange;
            }
        }


        double _IDecay = 0;
        [DataMember]
        [ACPropertyInfo(9999, "", "en{'Interpolation decay'}de{'Interpolation Dämpfung'}")]
        public double IDecay
        {
            get
            {
                return _IDecay;
            }
            set
            {
                _IDecay = value;
                OnPropertyChanged("IDecay");
            }
        }

        public double InterpolationDecay
        {
            get
            {
                if (_IDecay < 0)
                    return 0;
                else if (_IDecay > 1)
                    return 1;
                return _IDecay;
            }
        }

        private double? _InterpolationLackOfDataReturnValue;

        [DataMember]
        [ACPropertyInfo(9999, "", "en{'Interpolation lack of data return value'}de{'Interpolation lack of data return value'}")]
        public double? InterpolationLackOfDataReturnValue
        {
            get => _InterpolationLackOfDataReturnValue;
            set
            {
                _InterpolationLackOfDataReturnValue = value;
                OnPropertyChanged("InterpolationLackOfDataReturnValue");
            }
        }

        private TimeSpan? _InterpolationMaxTimeSpanBackwards;

        [DataMember]
        [ACPropertyInfo(9999, "", "en{'Interpolation maximum timespan backwards'}de{'Interpolation maximum timespan backwards'}")]
        public TimeSpan? InterpolationMaxTimeSpanBackwards
        {
            get => _InterpolationMaxTimeSpanBackwards;
            set
            {
                _InterpolationMaxTimeSpanBackwards = value;
                OnPropertyChanged("InterpolationDataMissingReturnValue");
            }
        }

        /// <summary>
        /// Factor if Value-Count in queried time range are not enough for InterpolationMethod
        /// </summary>
        public double? EnlargeTimeRange
        {
            get;
            set;
        }

        #region INotifyPropertyChanged Member

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region IACObject
        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public abstract string ACIdentifier
        {
            get;
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        public abstract string ACCaption
        {
            get;
        }

        /// <summary>
        /// Metadata (iPlus-Type) of this instance. ATTENTION: IACType are EF-Objects. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!
        /// </summary>
        /// <value>  iPlus-Type (EF-Object from ACClass*-Tables)</value>
        public IACType ACType
        {
            get
            {
                return this.ReflectACType();
            }
        }

        /// <summary>
        /// A "content list" contains references to the most important data that this instance primarily works with. It is primarily used to control the interaction between users, visual objects, and the data model in a generic way. For example, drag-and-drop or context menu operations. A "content list" can also be null.
        /// </summary>
        /// <value> A nullable list ob IACObjects.</value>
        public IEnumerable<IACObject> ACContentList
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// The ACUrlCommand is a universal method that can be used to query the existence of an instance via a string (ACUrl) to:
        /// 1. get references to components,
        /// 2. query property values,
        /// 3. execute method calls,
        /// 4. start and stop Components,
        /// 5. and send messages to other components.
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>Result if a property was accessed or a method was invoked. Void-Methods returns null.</returns>
        public object ACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// This method is called before ACUrlCommand if a method-command was encoded in the ACUrl
        /// </summary>
        /// <param name="acUrl">String that adresses a command</param>
        /// <param name="acParameter">Parameters if a method should be invoked</param>
        /// <returns>true if ACUrlCommand can be invoked</returns>
        public bool IsEnabledACUrlCommand(string acUrl, params object[] acParameter)
        {
            return this.ReflectIsEnabledACUrlCommand(acUrl, acParameter);
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        public IACObject ParentACObject
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a ACUrl relatively to the passed object.
        /// If the passed object is null then the absolute path is returned
        /// </summary>
        /// <param name="rootACObject">Object for creating a realtive path to it</param>
        /// <returns>ACUrl as string</returns>
        public virtual string GetACUrl(IACObject rootACObject = null)
        {
            return ACIdentifier;
        }

        /// <summary>
        /// Method that returns a source and path for WPF-Bindings by passing a ACUrl.
        /// </summary>
        /// <param name="acUrl">ACUrl of the Component, Property or Method</param>
        /// <param name="acTypeInfo">Reference to the iPlus-Type (ACClass)</param>
        /// <param name="source">The Source for WPF-Databinding</param>
        /// <param name="path">Relative path from the returned source for WPF-Databinding</param>
        /// <param name="rightControlMode">Information about access rights for the requested object</param>
        /// <returns><c>true</c> if binding could resolved for the passed ACUrl<c>false</c> otherwise</returns>
        public bool ACUrlBinding(string acUrl, ref IACType acTypeInfo, ref object source, ref string path, ref Global.ControlModes rightControlMode)
        {
            return this.ReflectACUrlBinding(acUrl, ref acTypeInfo, ref source, ref path, ref rightControlMode);
        }

        public bool ACUrlTypeInfo(string acUrl, ref ACUrlTypeInfo acUrlTypeInfo)
        {
            return this.ReflectACUrlTypeInfo(acUrl, ref acUrlTypeInfo);
        }
        #endregion
    }
}
