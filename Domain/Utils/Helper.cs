using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Utils
{
    public static class Helper
    {
        public static Expression<Func<T, bool>> CreateWhere<T>(string propertyName, object searchVal)
        {
            var argument = Expression.Parameter(typeof(T), "p");
            var property = Expression.Property(argument, propertyName);
            var value = searchVal == null ? Expression.Constant(null, property.Type) : Expression.Constant(ConvertTo(searchVal, property.Type), property.Type);
            var body = Expression.Equal(property, value);
            return Expression.Lambda<Func<T, bool>>(body, argument);
        }
        private static object ConvertTo(object value, Type type)
        {
            var t = Nullable.GetUnderlyingType(type) ?? type;
            return (value == null) ? null : Convert.ChangeType(value, t, CultureInfo.InvariantCulture);
        }

        public static IQueryable<T> WhereFilter<T>(this IQueryable<T> source, string searchField, object searchVal)
           where T : class
        {
            var predicate = CreateWhere<T>(searchField, searchVal);
            return predicate != null ? source.Where<T>(predicate) : source;
        }
    }
}
