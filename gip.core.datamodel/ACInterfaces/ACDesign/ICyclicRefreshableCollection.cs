namespace gip.core.datamodel
{
    // ***********************************************************************
    // Assembly         : gip.core.datamodel
    // Author           : aagincic
    // Created          : 26-01-2023
    //
    // Last Modified By : aagincic
    // Last Modified On : 26-01-2023
    public interface ICyclicRefreshableCollection
    {
        /// <summary>
        /// Enable refresh already GUI loaded elements
        /// </summary>
        void Refresh();
    }
}
