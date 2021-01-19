namespace gip.core.dbsyncer.Messages
{
    /// <summary>
    /// version message with info update not needed
    /// </summary>
    public class NoUpdateNeeded:BaseSyncMessage
    {
        /// <summary>
        /// Construct not update needed message
        /// </summary>
        /// <param name="context"></param>
        /// <param name="version"></param>
        public NoUpdateNeeded(string context, int version)
        {
            Success = true;
            Message = string.Format(@"Version of context [{0}] is up to date: {1}", context, version);
        }
    }
}
