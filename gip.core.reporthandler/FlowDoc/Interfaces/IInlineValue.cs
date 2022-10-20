namespace gip.core.reporthandler.Flowdoc
{
    /// <summary>
    /// Interface for values
    /// </summary>
    public interface IInlineValue
    {
        /// <summary>
        /// Gets or sets the value format
        /// </summary>
        string StringFormat { get; set; }

        /// <summary>Gets or sets the encapsulated value as a boxed type</summary>
        /// <value>The boxed value.</value>
        object Value { get; set; }

        int MaxLength { get; set; }
    }
}
