using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using gip.core.datamodel;

namespace gip.core.autocomponent
{
    /// <summary>
    /// ACPSubscriptionACObject beschreibt das zu abbonierende ACObject
    /// </summary>
    [DataContract]
    public abstract class ACPSubscrObjBase : ACRef<ACComponent>
    {
        #region c'tors
        public ACPSubscrObjBase() : base()
        {
            _IsWeakReference = true;
        }

        public ACPSubscrObjBase(string ACUrl) : base (ACUrl,null,true)
        {
            _IsWeakReference = true;
            FetchAllProperties = true;
        }

        public ACPSubscrObjBase(string acUrl, IACObject parentACObject, bool autoAttach = false)
            : base(acUrl, null, autoAttach)
        {
            _IsWeakReference = true;
            FetchAllProperties = true;
        }

        // Konstruktor für Client-Seite: ACObject kann übergeben werden wenn Subscription oder Point-Änderungen gemacht werden, 
        // weil Subscription kurzzeitig lebt und durch die Referenz der Zugriff schneller ist.
        // Bei UnSubscribe, darf dieser Konstruktor nicht verwendet werden, weil das Objekt nicht vollständig gelöscht wird.
        // Ebenfalls darf dieser auf Server-Seite nicht verwendet werden, damit Objekte kurzzeitig Restartet werden können.
        public ACPSubscrObjBase(ACComponent acObject) : base (acObject,null)
        {
            _IsWeakReference = true;
            FetchAllProperties = true;
        }
        #endregion

        #region Members

        /// <summary>
        /// Kennzeichnet, das Client, das Objekt das erste mal anfordert => 
        /// Alle Properties müssen einmal übertragen werden
        /// </summary>
        [IgnoreDataMember]
        public bool FetchAllProperties 
        { 
            get; 
            set;
        }

        /// <summary>
        /// Returns the parent object
        /// </summary>
        /// <value>Reference to the parent object</value>
        [IgnoreDataMember]
        public override IACObject ParentACObject
        {
            get
            {
                if (!IsAttached)
                    return null;
                if (!IsObjLoaded)
                    return null;
                return ValueT as ACComponent;
            }
        }

        [IgnoreDataMember]
        internal ACPSubscrObjServiceShared SharedSubscriptionObject { get; set; }

        #endregion
    }
}
