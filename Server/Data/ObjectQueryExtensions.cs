using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace SmartProctor.Server.Data
{
    public static class ObjectQueryExtensions
    {
        public static IQueryable<T> Includes<T>(this IQueryable<T> obj, string[] includes) where T : class
        {
            if (includes == null)
                return obj;

            foreach (var item in includes)
            {
                obj = obj.Include(item);
            }
            return obj;
        }

        public static IOrderedQueryable<T> OrderBy<T,TK>(this IQueryable<T> obj, Expression<Func<T, TK>> orderBy, OrderingType orderingType) where T : class
        {
            if (orderBy==null)
                throw new Exception("OrderBy can not be Null！");

            return orderingType == OrderingType.Ascending ? obj.OrderBy(orderBy) : obj.OrderByDescending(orderBy);
        }
    }
}