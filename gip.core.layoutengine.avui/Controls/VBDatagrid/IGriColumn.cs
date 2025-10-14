using gip.core.datamodel;

namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Interface for VBDataGrid columns.
    /// </summary>
    public interface IGriColumn
    {
        /// <summary>
        /// Represents the name of a bounded object(ACPropertySelected[...]) property which is marked with [ACPropertyInfo(...)] attribute.
        /// </summary>
        string VBContent { get; }

        /// <summary>
        /// The ACColumn item.
        /// </summary>
        ACColumnItem ACColumnItem { get; set; }
        /// <summary>
        /// Defines is column read only or not.
        /// </summary>
        bool VBIsReadOnly { get; set; }

        /// <summary>
        /// The column right control mode.
        /// </summary>
        Global.ControlModes RightControlMode { get; }

        /// <summary>
        /// The ACType of column.
        /// </summary>
        ACClassProperty ColACType { get; }

        /// <summary>
        /// The VBDataGrid.
        /// </summary>
        VBDataGrid VBDataGrid { get; }

        /// <summary>
        /// WPF-Property
        /// </summary>
        bool IsReadOnly { get; set; }

        /// <summary>
        /// Refreshes the IsReadOnly-Property
        /// </summary>
        /// <param name="newReadOnlyState">-1 default, 0 = Unset Readonly  from BSO, 1 = Set Readonly from BSO</param>
        void RefreshReadOnlyProperty(short newReadOnlyState = -1);

        /// <summary>
        /// Represents the Deinitialization method for VBControl.
        /// </summary>
        /// <param name="bso">The BSO parameter.</param>
        void DeInitVBControl(IACComponent bso);
    }
}
