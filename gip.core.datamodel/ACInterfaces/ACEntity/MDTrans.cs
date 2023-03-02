namespace gip.core.datamodel
{
    public class MDTrans : IMDTrans
    {
        public MDTrans(string mDNameTrans, string mDKey)
        {
            MDNameTrans = mDNameTrans;
            MDKey = mDKey;
        }

        public string MDNameTrans { get; set; }
        public string MDKey { get; set; }
    }
}
