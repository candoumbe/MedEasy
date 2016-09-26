using MedEasy.DAL.Repositories;
using System.Collections.Generic;

namespace System.Linq
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> entries, IEnumerable<OrderClause<T>> orderBy)
        {
            OrderClause<T> previousClause = null;
            foreach (OrderClause<T> orderClause in orderBy)
            {
                switch (orderClause.Direction)
                {
                    case SortDirection.Ascending:
                        if (previousClause != null)
                        {
                            entries = Queryable.ThenBy(entries, (dynamic)orderClause.Expression);
                        }
                        else
                        {
                            entries = Queryable.OrderBy(entries, (dynamic)orderClause.Expression);
                        }
                        break;
                    case SortDirection.Descending:
                        if (previousClause != null)
                        {
                            entries = Queryable.ThenByDescending(entries, (dynamic)orderClause.Expression);
                        }
                        else
                        {
                            entries = Queryable.OrderByDescending(entries, (dynamic)orderClause.Expression);
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                previousClause = orderClause;
            }
            return entries;
        }

        public static IQueryable<T> Include<T>(this IQueryable<T> entries, IEnumerable<IncludeClause<T>> includes)
        {
            if (includes != null)
            {
                foreach (IncludeClause<T> includeClause in includes)
                {
                    entries = QueryableExtensions.Include(entries, (dynamic)includeClause.Expression);
                }
            }
            return entries;
        }
    }

}
