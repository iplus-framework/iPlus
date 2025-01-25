using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace gip.core.datamodel
{
    public static class IncludeExtension
    {
        public static IQueryable<T> IncludeExp<T, TProperty>(
        this IQueryable<T> query,
        Expression<Func<T, TProperty>> navigationPropertyPath) where T : class
        {
            if (navigationPropertyPath == null)
                throw new ArgumentNullException(nameof(navigationPropertyPath));

            string propertyPath = GetPropertyPath(navigationPropertyPath);
            return query.Include(propertyPath);
        }

        private static string GetPropertyPath<T, TProperty>(Expression<Func<T, TProperty>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new InvalidOperationException("Supported only direct access to properties!");
            }

            var path = memberExpression.ToString(); // "x => x.ProdOrderPartslist.ProdOrder"
            return path.Substring(path.IndexOf('.') + 1); // Uklanja "x => "
        }
    }
}
