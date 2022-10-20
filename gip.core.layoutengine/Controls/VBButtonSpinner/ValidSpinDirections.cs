using System;

namespace gip.core.layoutengine
{
    /// <summary>
    /// Represents spin directions that are valid.
    /// </summary>
    /// <summary xml:lang="de">
    /// Stellt die gültigen Drehrichtungen dar.
    /// </summary>
    [Flags]
    public enum ValidSpinDirections : short
    {
        /// <summary>
        /// Can not increase nor decrease.
        /// </summary>
        None = 0,

        /// <summary>
        /// Can increase.
        /// </summary>
        Increase = 1,

        /// <summary>
        /// Can decrease.
        /// </summary>
        Decrease = 2
    }
}
