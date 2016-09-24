using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Extension method to convert a group to a dictionary of T
        /// </summary>
        /// <typeparam name="TKey">type of the key in the group</typeparam>
        /// <typeparam name="TElement">type of the element groupêd</typeparam>
        /// <param name="groups"></param>
        /// <returns>a dictionary</returns>
        public static IDictionary<TKey, IEnumerable<TElement>> ToDictionary<TKey, TElement>(this IEnumerable<IGrouping<TKey, TElement>> groups)
        {
            if (groups == null)
            {
                throw new ArgumentNullException(nameof(groups));
            }
            return groups.ToDictionary(g => g.Key, g => g.ToList().AsEnumerable());
        }

        /// <summary>
        /// Tests if <paramref name="items"/> contains exactly one item that verify the specified <paramref name="predicate"/>
        /// </summary>
        /// <typeparam name="T">Type of the </typeparam>
        /// <param name="items">Collection to test</param>
        /// <param name="predicate">re</param>
        /// <returns><c>true</c> if <paramref name="items"/> contains exactly one element that fullfills <paramref name="predicate"/></returns>
        public static bool Once<T>(this IEnumerable<T> items, Expression<Func<T, bool>> predicate)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            return !Equals(items.SingleOrDefault(predicate.Compile()), default(T));
        }

        /// <summary>
        /// Tests if <paramref name="items"/> contains one or more items that verify the specified <paramref name="predicate"/>
        /// </summary>
        /// <typeparam name="T">Type of the </typeparam>
        /// <param name="items">Collection to test</param>
        /// <param name="predicate">re</param>
        /// <returns><c>true</c> if <paramref name="items"/> contains one or more one element that fullfills <paramref name="predicate"/></returns>
        public static bool AtLeastOnce<T>(this IEnumerable<T> items, Expression<Func<T, bool>> predicate)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return items.Any(predicate.Compile());
        }

        public static bool AtLeast<T>(this IEnumerable<T> items, Expression<Func<T, bool>> predicate, int count)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return items.Count(predicate.Compile()) >= count;
        }

        public static bool Exactly<T>(this IEnumerable<T> items, Expression<Func<T, bool>> predicate, int count)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return items.Count(predicate.Compile()) == count;
        }

        public static bool AtMost<T>(this IEnumerable<T> items, Expression<Func<T, bool>> predicate, int count)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }
            return items.Count(predicate.Compile()) <= count;
        }

        
        /// <summary>
        /// Synchronously iterates over source an execute the <paramref name="body"/> action
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">
        /// </param>
        /// <param name="body">
        ///     code to be execute the <paramref name="body"/> action on each item of the <paramref name="source" />
        /// </param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> body)
        {
            IList<Exception> exceptions = null;
            foreach (var item in source)
            {
                try
                {
                    body(item);
                }
                catch (Exception exc)
                {
                    if (exceptions == null)
                    {
                        exceptions = new List<Exception>();
                    }
                    exceptions.Add(exc);
                }
            }
            if (exceptions != null)
            {
                throw new AggregateException(exceptions);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="body"></param>
        /// <param name="dop"></param>
        /// <returns></returns>
        public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body, int? dop = null)
        {
            Task t = null;
            if (dop.HasValue)
            {
                t = Task.WhenAll(
                    from partition in Partitioner.Create(source).GetPartitions(dop.Value)
                    select Task.Run(async delegate
                    {
                        using (partition)
                        {
                            while (partition.MoveNext())
                            {
                                await body(partition.Current);
                            }
                        }
                    }));
            }
            else
            {
                t = Task.WhenAll(
                    from item in source
                    select Task.Run(() => body(item)));
            }


            return t;
        }

    }
}