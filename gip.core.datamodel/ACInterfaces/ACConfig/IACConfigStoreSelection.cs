using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gip.core.datamodel
{
    /// <summary>
    /// The same configuration-value (stored in IACConfig.Value) can be stored in different Config-Tables that are build on each other.
    /// We call this scenario "Config-Parameter overriding" or the principle of "strict entity separation with progressive concretization".
    /// This interface helps a IACConfigProvider to find out which IACConfigStore's must be considered for parameter overriding.
    /// This interface is implemented by Business objects (BSO) that displays Workflows an want's to store Workflow-Configurations
    /// in the contet of their "Main-Table". e.g. BSO for MaterialWorkflows, BOM's or Production-Orders.
    /// Also Workflow-Nodes implements this interface, because they know the data-tables they work with.
    /// </summary>
    public interface IACConfigStoreSelection : IACComponent
    {
        /// <summary>
        /// Returns which IACConfigStore's must be considered for Parameter Overriding.
        /// Normally one Business-Object reutrns only one IACConfigStore.
        /// e.g. For BOM's this is the current selected Partslist-Entity or at ProductionOrders is it ProdOrderPartslist.
        /// </summary>
        List<IACConfigStore> MandatoryConfigStores { get;}

        /// <summary>
        /// The most top IACConfigStore from MandatoryConfigStores that should be used to store new Config-Values.
        /// When the user want's to change a parameter of a selectd workflow-node on the GUI, then this entity will be used from the IACConfigProvider
        /// to add the Configuration.
        /// </summary>
        IACConfigStore CurrentConfigStore { get; }


        /// <summary>
        /// Used for define is control dialog shown only for preview.
        /// This is the case when live Workflows are displayed on the GUI.
        /// </summary>
        bool IsReadonly { get; }
    }
}
