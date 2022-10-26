using System;
using System.ComponentModel;

namespace gip.core.datamodel
{

    /// <summary>Abstract interface for implementing classes which can be listed in the ACMemberList of a ACComponent. Members can be Properties, Points and other ACComponent-Instances (childs)</summary>
    /// <seealso cref="gip.core.datamodel.IACObject" />
    /// <seealso cref="System.ComponentModel.INotifyPropertyChanged" />
    [ACClassInfo(Const.PackName_VarioSystem, "en{'IACMember'}de{'IACMember'}", Global.ACKinds.TACInterface)]
    [ACQueryInfoPrimary(Const.PackName_VarioSystem, Const.QueryPrefix + "ACMember", "en{'ACMember'}de{'ACMember'}", typeof(IACMember), "IACMember", Const.ACCaptionPrefix, Const.ACCaptionPrefix)]
    public interface IACMember : IACObject, INotifyPropertyChanged
    {
        /// <summary>
        /// Parent ACComponent where this instance belongs to.
        /// </summary>
        /// <value>The parent ac component.</value>
        IACComponent ParentACComponent { get; }


        /// <summary>
        /// Smart-Pointer to the Parent ACComponent where this instance belongs to.
        /// </summary>
        /// <value>The parent ac component.</value>
        ACRef<IACComponent> ACRef { get; }


        /// <summary>
        /// This method is called from the iPlus-Framework for each member of a ACComponent when a component was recycled from the component-pool (ACInitState.RecycledFromPool) instead of a new creation.
        /// </summary>
        /// <param name="recycledComponent">The recycled component.</param>
        void RecycleMemberAndAttachTo(IACComponent recycledComponent);


        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        [ACPropertyInfo(2)]
        object Value { get; set; }

        /// <summary>
        /// Must be called inside the class that implements IACMember every time when the the encapsulated value-Property has changed.
        /// If the implementation implements INotifyPropertyChanged also then OnPropertyChanged() must be called inside the implementation of OnMemberChanged().
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data. Is not null if the change of the encapsulated value was detected by a callback of the PropertyChangedEvent or CollectionChanged-Event. Then the EventArgs will be passed.</param>
        void OnMemberChanged(EventArgs e = null);

    }
}
