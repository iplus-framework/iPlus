// Copyright (c) 2024, gipSoft d.o.o.
// Licensed under the GNU GPLv3 License. See LICENSE file in the project root for full license information.
using System;

namespace gip.core.datamodel
{
    /// <summary>
    /// Interface for the root-instance of the whole application tree.
    /// It provides access to the cild instances: Environment, Messages, Queries, Resources and Businessobjects.
    /// </summary>
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IRoot'}de{'IRoot'}", Global.ACKinds.TACInterface)]
    public interface IRoot : IACComponent
    {
        #region Manager
        /// <summary>
        /// Gets the businessobjects.
        /// </summary>
        /// <value>The businessobjects.</value>
        IBusinessobjects Businessobjects { get; }

        /// <summary>
        /// Gets the environment.
        /// </summary>
        /// <value>The environment.</value>
        IEnvironment Environment { get; }

        /// <summary>
        /// Gets the queries.
        /// </summary>
        /// <value>The queries.</value>
        IQueries Queries { get; }

        /// <summary>
        /// Gets the component pool.
        /// </summary>
        /// <value>The component pool.</value>
        ACComponentPool ComponentPool { get; }

        string TypeNameOfAppContext { get; }
        #endregion

        #region Environment
        /// <summary>
        /// Reference to the WPF-Masterpage. This property is null if iPlus is started as a Windows-Service or Console-Application.
        /// </summary>
        /// <value>Reference to the WPF-Masterpage</value>
        IRootPageWPF RootPageWPF
        {
            get;
            set;
        }

        IWPFServices WPFServices { get; }

        #endregion

        #region Client-Side-Send-Methods
        /// <summary>
        /// Sends a client-side message to the server
        /// </summary>
        /// <param name="acMessage">Message</param>
        /// <param name="forACComponent">Adressed component</param>
        void SendACMessage(IWCFMessage acMessage, IACComponent forACComponent);

        /// <summary>
        /// Method sends a PropertyValueEvent from this Client/Proxy-Instance
        /// to the Real instance on Server-side
        /// </summary>
        /// <param name="eventArgs">The event args.</param>
        /// <param name="forACComponent">Adressed component</param>
        void SendPropertyValue(IACPropertyNetValueEvent eventArgs, IACComponent forACComponent);

        /// <summary>
        /// Method subscribes an new generated Proxy-Instance for retrieving ValueEvents from the Server
        /// </summary>
        /// <param name="forACComponent">Adressed component</param>
        void SubscribeACObject(IACComponent forACComponent);

        /// <summary>
        /// Method unsubscribes an unloaded Proxy-Instance
        /// </summary>
        /// <param name="forACComponent">Adressed instance of a ACComponent</param>
        void UnSubscribeACObject(IACComponent forACComponent);

        /// <summary>
        /// Activates Sending of Subscription to server.
        /// Method will be called, when a common set of Objects are generated
        /// </summary>
        void SendSubscriptionInfoToServer(bool queued = false);

        /// <summary>
        /// Makes an Entry in Dispatcher-List, that a changed Point must be send to the Server
        /// </summary>
        /// <param name="forACComponent">Adressed component</param>
        void MarkACObjectOnChangedPointForServer(IACComponent forACComponent);

        #endregion

        #region Server-Side-Send-Methods

        /// <summary>
        /// Sends a server-side message to all clients
        /// </summary>
        /// <param name="acMessage">The ac message.</param>
        void BroadcastACMessage(IWCFMessage acMessage);

        /// <summary>
        /// Method sends a PropertyValueEvent from this Real/Server-Instance
        /// to all Proxy-Instances which has subscribed it
        /// </summary>
        /// <param name="eventArgs">The event args.</param>
        /// <param name="forACComponent">Adressed component</param>
        void BroadcastPropertyValue(IACPropertyNetValueEvent eventArgs, IACComponent forACComponent);

        /// <summary>
        /// Makes an Entry in Dispatcher-List, that a changed Point must be send to Clients
        /// </summary>
        /// <param name="forACComponent">Adressed component</param>
        void MarkACObjectOnChangedPointForClient(IACComponent forACComponent);

        /// <summary>
        /// Gets the current invoking user.
        /// </summary>
        /// <value>The current invoking user.</value>
        VBUser CurrentInvokingUser { get; }
        #endregion

        /// <summary>
        /// Gets the AC type from AC URL.
        /// </summary>
        /// <param name="acUrl">The ac URL.</param>
        /// <returns>IACType.</returns>
        IACType GetACTypeFromACUrl(string acUrl);

        /// <summary>
        /// Gets a value indicating whether this instance has AC model server.
        /// </summary>
        /// <value><c>true</c> if this instance has AC model server; otherwise, <c>false</c>.</value>
        bool HasACModelServer { get; }

        /// <summary>
        /// Gets or sets a value indicating whether [prop persistence off].
        /// </summary>
        /// <value><c>true</c> if [prop persistence off]; otherwise, <c>false</c>.</value>
        bool PropPersistenceOff { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IRoot"/> is initialized.
        /// </summary>
        /// <value><c>true</c> if initialized; otherwise, <c>false</c>.</value>
        bool Initialized { get; }

        /// <summary>
        /// Instance for dumping the applicationtree and writing some performance logs
        /// </summary>
        /// <value>
        /// The vb dump.
        /// </value>
        IRuntimeDump VBDump { get; }

        string IPlusDocsServerURL { get; }

        IACVBNoManager NoManager { get; }

        IDisposable UsingThread(VBUser user);
    }
}
