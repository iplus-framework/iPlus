using System;


namespace gip.core.dbsyncer.Messages
{

    /// <summary>
    /// Base message returned form DBSyncer
    /// </summary>
    public abstract class BaseSyncMessage
    {
        /// <summary>
        /// Success of operation flag
        /// </summary>
        public bool Success{ get; set; }

        /// <summary>
        /// Textual message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Overriding ToString for object to provide relevant debug informations
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string outString = "";
            if(Success)
            {
                outString += "Successfully.";
            }
            else
            {
                outString += "Not successfully.";
            }
            outString += Environment.NewLine;
            outString += Message;
            return outString;
        }
    }
}
