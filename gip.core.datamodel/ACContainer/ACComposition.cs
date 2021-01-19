// ***********************************************************************
// Assembly         : gip.core.datamodel
// Author           : DLisak
// Created          : 10-16-2012
//
// Last Modified By : DLisak
// Last Modified On : 10-16-2012
// ***********************************************************************
// <copyright file="ACComposition.cs" company="gip mbh, Oftersheim, Germany">
//     Copyright (c) gip mbh, Oftersheim, Germany. All rights reserved.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace gip.core.datamodel
{
    /// <summary>
    /// Container für Definition einer Beziehung zwischen einer ACClass und einem IACObject (ValueT) Serialisierbar
    /// Verwendung: Konfiguration im iPlus-Studio
    /// </summary>
    /// <seealso cref="gip.core.datamodel.IACContainerT{gip.core.datamodel.IACObject}" />
    /// <seealso cref="gip.core.datamodel.IACAttach" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    [ACSerializeableInfo()]
    [DataContract]
    [ACClassInfo(Const.PackName_VarioSystem, "en{'ACCompositionObject'}de{'ACCompositionObject'}", Global.ACKinds.TACClass, Global.ACStorableTypes.NotStorable, true, false)]
    public class ACComposition : IACContainerT<IACObject>, IACAttach, INotifyPropertyChanged
    {
        #region IACContainerT
        /// <summary>Gets or sets the encapsulated value of the generic type T</summary>
        /// <value>The Value-Property as generic type</value>
        [ACPropertyInfo(1, "", "en{'Reference'}de{'Verweis'}")]
        public IACObject ValueT
        {
            get
            {
                if (ACUrlComposition.StartsWith(Const.ContextDatabase + "\\"))
                {
                    return Database.ACUrlCommand(ACUrlComposition.Substring(9)) as IACObject;
                }
                else
                {
                    return Database.ACUrlCommand(ACUrlComposition) as IACObject;
                }
            }
            set
            {
                ACUrlComposition = value.GetACUrl();
                OnPropertyChanged(Const.ValueT);
            }
        }

        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        public object Value
        {
            get
            {
                return ValueT;
            }
            set
            {
                ValueT = value as IACObject;
            }
        }

        /// <summary>Metadata (iPlus-Type) of the Value-Property. ATTENTION: ACClass is a EF-Object. Therefore the access to Navigation-Properties must be secured using the QueryLock_1X000 of the Global Database-Context!</summary>
        /// <value>Metadata (iPlus-Type) of the Value-Property as ACClass</value>
        public ACClass ValueTypeACClass
        {
            get 
            {
                return ValueT == null ? null : ValueT.ACType as ACClass;
            }
        }

        /// <summary>Unique Identifier in a Parent-/Child-Relationship.</summary>
        /// <value>The Unique Identifier as string</value>
        public string ACIdentifier
        {
            get
            {
                if (ValueT == null)
                    return "";
                return ValueT.ACIdentifier;
            }
        }

        /// <summary>Translated Label/Description of this instance (depends on the current logon)</summary>
        /// <value>  Translated description</value>
        [ACPropertyInfo(9999)]
        public string ACCaption
        {
            get
            {
                if (ValueT == null)
                    return "";
                return ValueT.ACCaption;
            }
        }
        #endregion

        /// <summary>
        /// Gets or sets the AC URL composition.
        /// </summary>
        /// <value>The AC URL composition.</value>
        [DataMember]
        public String  ACUrlComposition
        {
            get;
            set;
        }

        /// <summary>
        /// GetComposition (Thread-Safe)
        /// </summary>
        /// <param name="database">database is locked in method</param>
        /// <returns>IACObject.</returns>
        public IACObject GetComposition(Database database)
        {

            using (ACMonitor.Lock(database.QueryLock_1X000))
            {
                if (ACUrlComposition != null && ACUrlComposition.StartsWith(Const.ContextDatabase + "\\"))
                {
                    return database.ACUrlCommand(ACUrlComposition.Substring(9)) as IACObject;
                }
                else
                {
                    return database.ACUrlCommand(ACUrlComposition) as IACObject;
                }
            }
        }

        /// <summary>
        /// Sets the composition.
        /// </summary>
        /// <param name="acObject">The ac object.</param>
        public void SetComposition(IACObject acObject)
        {
            ACUrlComposition = acObject.GetACUrl();
        }


        /// <summary>
        /// The _ is system
        /// </summary>
        bool _IsSystem;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value><c>true</c> if this instance is system; otherwise, <c>false</c>.</value>
        [DataMember]
        [ACPropertyInfo(1, "", "en{'Assembly'}de{'Assambly'}")]
        public bool IsSystem
        {
            get
            {
                return _IsSystem;
            }
            set
            {
                _IsSystem = value;
                OnPropertyChanged("IsSystem");
            }
        }

        /// <summary>
        /// The _ appendix
        /// </summary>
        string _Appendix;
        /// <summary>
        /// Gets or sets the appendix.
        /// </summary>
        /// <value>The appendix.</value>
        [DataMember]
        [ACPropertyInfo(1, "", "en{'Appendix'}de{'Appendix'}")]
        public string Appendix
        {
            get
            {
                return _Appendix;
            }
            set
            {
                _Appendix = value;
                OnPropertyChanged("Appendix");
            }
        }

        /// <summary>
        /// The _ is primary
        /// </summary>
        bool _IsPrimary;
        /// <summary>
        /// Gets or sets a value indicating whether this instance is primary.
        /// </summary>
        /// <value><c>true</c> if this instance is primary; otherwise, <c>false</c>.</value>
        [DataMember]
        [ACPropertyInfo(1, "", "en{'Primary'}de{'Primär'}")]
        public bool IsPrimary
        {
            get
            {
                return _IsPrimary;
            }
            set
            {
                _IsPrimary = value;
                OnPropertyChanged("IsPrimary");
            }
        }

        /// <summary>
        /// Returns a <see cref=TypeAnalyser._TypeName_String /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref=TypeAnalyser._TypeName_String /> that represents this instance.</returns>
        public override string ToString()
        {
            IACObject acObject = GetComposition(gip.core.datamodel.Database.GlobalDatabase);
            if (acObject == null)
                return "";
            return acObject.ACCaption;
        }

        /// <summary>
        /// Tritt ein, wenn sich ein Eigenschaftswert ändert.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Called when [property changed].
        /// </summary>
        /// <param name="name">The name.</param>
        [ACMethodInfo("ACComponent", "en{'PropertyChanged'}de{'PropertyChanged'}", 9999)]
        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// The _ database
        /// </summary>
        Database _Database = null;
        /// <summary>
        /// Gets the database.
        /// </summary>
        /// <value>The database.</value>
        Database Database
        {
            get
            {
                if (_Database != null)
                    return _Database;
                return Database.GlobalDatabase;
            }
        }


        #region IACAttach
        /// <summary>Attaches the deserialized encapuslated objects to the parent context.</summary>
        /// <param name="parentACObject">The parent context. Normally this is a EF-Context (IACEntityObjectContext).</param>
        public void AttachTo(IACObject parentACObject)
        {
            _Database = parentACObject as Database;
            if (ObjectAttached != null)
                ObjectAttached(this, new EventArgs());
        }

        /// <summary>Detaches the encapuslated objects from the parent context.</summary>
        /// <param name="detachFromContext">If attached object is a Entity object, then it will be detached from Change-Tracking if this parameter is set to true.</param>
        public void Detach(bool detachFromContext = false)
        {
            if (ObjectDetaching != null)
                ObjectDetaching(this, new EventArgs());
            if (ObjectDetached != null)
                ObjectDetached(this, new EventArgs());
        }

        /// <summary>Gets a value indicating whether the encapuslated objects are attached.</summary>
        /// <value>
        ///   <c>true</c> if the encapuslated objects are attached; otherwise, <c>false</c>.</value>
        public bool IsAttached 
        {
            get
            {
                return _Database != null;
            }
        }

        #endregion


        /// <summary>
        /// Occurs when encapuslated objects were detached.
        /// </summary>
        public event EventHandler ObjectDetached;

        /// <summary>
        /// Occurs before the deserialized content will be attached to be able to access the encapuslated objects later.
        /// </summary>
        public event EventHandler ObjectDetaching;

        /// <summary>
        /// Occurs when encapuslated objects were attached.
        /// </summary>
        public event EventHandler ObjectAttached;
    }
}
