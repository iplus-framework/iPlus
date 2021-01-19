using System;

namespace gip.core.datamodel
{
    public interface IACVBNoManager : IACComponent
    {
        /// <summary>
        /// Returns a new secondary key for a database table (Entity-Class).
        /// Use the businessobject gip.bso.BSONoConfiguration to configure a individual rule for generating this key.
        /// You can override this method if you want to implement a project specific behaviour.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="type">Type of the entity class (Database table)</param>
        /// <param name="entityNoFieldName">Name of the Field in the database table that stores the secondary key.</param>
        /// <param name="formatNewNo">A Format-String how the sequantially generated nummer should be formatted.</param>
        /// <param name="invoker">The invoker who calls this method.</param>
        /// <returns>A unique secondary key</returns>
        /// <seealso cref="gip.bso.BSONoConfiguration" />
        string GetNewNo(IACEntityObjectContext context, Type type, string entityNoFieldName, string formatNewNo, IACComponent invoker = null);
    }
}
