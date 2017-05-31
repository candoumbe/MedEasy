using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
#if NETSTANDARD1_1
using System.Collections.Concurrent;
#endif
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
            return groups.ToDictionary(g => g.Key, g => g.AsEnumerable());
        }

        /// <summary>
        /// Tests if <paramref name="items"/> contains exactly one item
        /// </summary>
        /// <typeparam name="T">Type of the </typeparam>
        /// <param name="items">Collection to test</param>
        /// <returns><c>true</c> if <paramref name="items"/> contains exactly one element</returns>
        /// <see cref="Exactly{T}(IEnumerable{T}, Expression{Func{T, bool}}, int)"/>
        public static bool Once<T>(this IEnumerable<T> items) => Once(items, x => true);



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

            return Exactly(items, predicate, 1);
        }

        /// <summary>
        /// Tests if <paramref name="items"/> contains one or more items that verify the specified <paramref name="predicate"/>
        /// </summary>
        /// <typeparam name="T">Type of the </typeparam>
        /// <param name="items">Collection to test</param>
        /// <param name="predicate">predicate to use</param>
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

        /// <summary>
        /// Tests if <paramref name="items"/> contains one or more items that verify the specified <paramref name="predicate"/>
        /// </summary>
        /// <typeparam name="T">Type of the </typeparam>
        /// <param name="items">Collection to test</param>
        /// <param name="predicate">predicate to use</param>
        /// <returns><c>true</c> if <paramref name="items"/> contains one or more one element that fullfills <paramref name="predicate"/></returns>
        public static bool AtLeastOnce<T>(this IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            return items.Any();
        }


        /// <summary>
        /// Tests if <paramref name="items"/> contains at least <paramref name="count"/> elements that match <paramref name="predicate"/>
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <typeparam name="T">Type of elements</typeparam>
        /// <param name="items">the collection to test</param>
        /// <param name="predicate">the predicate</param>
        /// <param name="count">the number of occurrence</param>
        /// <returns><c>true</c> if <paramref name="items"/> contains <paramref name="count"/> elements or more that match <paramref name="predicate"/></returns>
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

        /// <summary>
        /// Tests if <paramref name="items"/> contains <strong>exactly</strong> <paramref name="count"/> elements that match <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements of</typeparam>
        /// <param name="items">collection under test</param>
        /// <param name="predicate">predicate to match</param>
        /// <param name="count">number of elements in <paramref name="items"/> that must match</param>
        /// <returns><c>true</c> if <paramref name="items"/> contains <strong>exactly</strong> <paramref name="count"/> elements that match <paramref name="predicate"/> and <c>false</c> otherwise</returns>
        /// <exception cref="ArgumentNullException">if <paramref name="items"/> or <paramref name="predicate"/> are null</exception>
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





        /// <summary>
        /// Tests if there are <paramref name="count"/> elements at most that match <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="T">Type of the elements of the collection to test</typeparam>
        /// <param name="items"></param>
        /// <param name="predicate">Filter that <paramref name="count"/> elements should match.</param>
        /// <param name="count">Number of elements that match <paramref name="predicate"/></param>
        /// <returns><c>true</c> if there are 0 to <paramref name="count"/> elements that matches <paramref name="predicate"/> and <c>false</c> otherwise.</returns>
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
            foreach (T item in source)
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

#if NETSTANDARD1_1
        /// <summary>
        /// Asynchronously run the 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="body"></param>
        /// <param name="dop"></param>
        /// <returns></returns>
        public static Task ForEachAsync<T>(this IEnumerable<T> source, Func<T, Task> body, int? dop = null)
        {
            Task t = Task.WhenAll(
                from partition in Partitioner.Create(source)
                        .GetPartitions(dop.GetValueOrDefault(Environment.ProcessorCount))
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

            return t;
        }
#endif

    }
}