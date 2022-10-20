namespace gip.core.reporthandler.Flowdoc
{
    /// <summary>
    /// Interface for property values
    /// </summary>
    public interface IInlinePropertyValue : IInlineValue, IDictRef
    {
        /// <summary>
        /// ACUrl to Property
        /// </summary>
        string VBContent { get; set; }
    }
}
