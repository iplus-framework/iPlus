namespace gip.bso.iplus.Manager
{
    public static class InsertInMarksSQLExt
    {
        public static string InsertInMarks(this string sql)
        {
            return string.Format("'{0}'", sql);
        }
    }
}
