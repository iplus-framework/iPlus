namespace gip.core.layoutengine.avui
{
    /// <summary>
    /// Mode of the WPF-Control to control the Drag-and-Drop behaviour.
    /// </summary>
    public enum DragMode
    {
        /// <summary>
        /// Drag-And-Drop is disabled
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// Drag-And-Drop is enabled
        /// </summary>
        Enabled = 1,

        /// <summary>
        /// Drag-And-Drop is enabled only in combination with a pressed key
        /// </summary>
        EnabledMove = 2,
    }
}
