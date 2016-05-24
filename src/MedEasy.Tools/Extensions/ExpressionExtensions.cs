using MedEasy.Tools;

namespace System.Linq.Expressions
{
    public static class ExpressionExtensions
    {

        /// <summary>
        /// Creates a 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Expression<Func<T, bool>> True<T>() => arg => true;



        public static Expression<Func<T, T>> Identity<T>() => x => x; 

        /// <summary>
        /// Combines two expressions <paramref name="first"/> and <paramref name="second"/> as defined in http://en.wikipedia.org/wiki/Function_composition.
        /// The result of f.Combine(g) for a given  
        /// </summary>
        /// <typeparam name="T1">Input type of the first function</typeparam>
        /// <typeparam name="T2">Output type of the second function</typeparam>
        /// <typeparam name="T3"></typeparam>
        /// <param name="first">the first expression</param>
        /// <param name="second">An expressions</param>
        /// <returns>The combined expression</returns>
        public static Expression<Func<T1, T3>> Combine<T1, T2, T3>(this Expression<Func<T1, T2>> first, Expression<Func<T2, T3>> second)
        {
            if (first == null)
            {
                throw new ArgumentNullException($"{nameof(first)}");
            }

            if (second == null)
            {
                throw new ArgumentNullException($"{nameof(second)}");
            }

            var param = Expression.Parameter(typeof(T1), "param");

            var newFirst = new ReplaceVisitor(first.Parameters.First(), param).Visit(first.Body);

            var newSecond = new ReplaceVisitor(second.Parameters.First(), newFirst).Visit(second.Body);

            return Expression.Lambda<Func<T1, T3>>(newSecond, param);
        }


        /// <summary>
        /// Combines two expressions using the logical <code>AND</code>
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="first">the first expression</param>
        /// <param name="second">the second expression</param>
        /// <returns>A new expression that represents <code>(T x) => f && g</code></returns>
        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> first, Expression<Func<T, bool>> second)
        {

            if (first == null)
            {
                throw new ArgumentNullException("f");
            }

            if (second == null)
            {
                throw new ArgumentNullException("g");
            }


            var parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new ReplaceVisitor(first.Parameters.First(), parameter);
            Expression left = leftVisitor.Visit(first.Body);

            ReplaceVisitor rightVisitor = new ReplaceVisitor(second.Parameters.First(), parameter);
            Expression right = rightVisitor.Visit(second.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left, right), parameter);
        }


        /// <summary>
        /// Combines two expressions using the logical <code>||</code> operator 
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="f">the first expression</param>
        /// <param name="g">the second expression</param>
        /// <returns>A new expression that represents <code>(T x) => f || g</code></returns>

        public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new ReplaceVisitor(expr1.Parameters.First(), parameter);
            Expression left = leftVisitor.Visit(expr1.Body);

            ReplaceVisitor rightVisitor = new ReplaceVisitor(expr2.Parameters.First(), parameter);
            Expression right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(left, right), parameter);
        }


        /// <summary>
        /// Combines two expressions using the logical <code>|</code> operator 
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="f">the first expression</param>
        /// <param name="g">the second expression</param>
        /// <returns>A new expression that represents <code>(T x) => f | g</code></returns>

        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            var parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new ReplaceVisitor(expr1.Parameters.First(), parameter);
            Expression left = leftVisitor.Visit(expr1.Body);

            ReplaceVisitor rightVisitor = new ReplaceVisitor(expr2.Parameters.First(), parameter);
            Expression right = rightVisitor.Visit(expr2.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.Or(left, right), parameter);
        }

        /// <summary>
        /// Combines two expressions using the logical <code>&</code> operator 
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="f">the first expression</param>
        /// <param name="g">the second expression</param>
        /// <returns>A new expression that represents <code>(T x) => f & g</code></returns>

        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> f, Expression<Func<T, bool>> g)
        {
            var parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new ReplaceVisitor(f.Parameters.First(), parameter);
            Expression left = leftVisitor.Visit(f.Body);

            ReplaceVisitor rightVisitor = new ReplaceVisitor(g.Parameters.First(), parameter);
            Expression right = rightVisitor.Visit(g.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.And(left, right), parameter);
        }

        /// <summary>
        /// Combines two expressions using the logical <code>&lt;</code> operator 
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="f">the first expression</param>
        /// <param name="g">the second expression</param>
        /// <returns>A new expression that represents <code>(T x) => f < g</code></returns>

        public static Expression<Func<T, bool>> LessThan<T>(this Expression<Func<T, bool>> f, Expression<Func<T, bool>> g)
        {
            var parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new ReplaceVisitor(f.Parameters.First(), parameter);
            Expression left = leftVisitor.Visit(f.Body);

            ReplaceVisitor rightVisitor = new ReplaceVisitor(g.Parameters.First(), parameter);
            Expression right = rightVisitor.Visit(g.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.LessThan(left, right), parameter);
        }


        /// <summary>
        /// Combines two expressions using the logical <code>&</code> operator 
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="f">the first expression</param>
        /// <param name="g">the second expression</param>
        /// <returns>A new expression that represents <code>(T x) => f > g</code></returns>

        public static Expression<Func<T, bool>> GreaterThan<T>(this Expression<Func<T, bool>> f, Expression<Func<T, bool>> g)
        {
            var parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new ReplaceVisitor(f.Parameters.First(), parameter);
            Expression left = leftVisitor.Visit(f.Body);

            ReplaceVisitor rightVisitor = new ReplaceVisitor(g.Parameters.First(), parameter);
            Expression right = rightVisitor.Visit(g.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.GreaterThan(left, right), parameter);
        }


        /// <summary>
        /// Combines two expressions using the logical <code>&</code> operator 
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="f">the first expression</param>
        /// <param name="g">the second expression</param>
        /// <returns>A new expression that represents <code>(T x) => f >= g</code></returns>

        public static Expression<Func<T, bool>> GreaterThanOrEqual<T>(this Expression<Func<T, bool>> f, Expression<Func<T, bool>> g)
        {
            var parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new ReplaceVisitor(f.Parameters.First(), parameter);
            Expression left = leftVisitor.Visit(f.Body);

            ReplaceVisitor rightVisitor = new ReplaceVisitor(g.Parameters.First(), parameter);
            Expression right = rightVisitor.Visit(g.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.GreaterThanOrEqual(left, right), parameter);
        }


        /// <summary>
        /// Combines two expressions using the logical <code>&</code> operator 
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="f">the first expression</param>
        /// <param name="g">the second expression</param>
        /// <returns>A new expression that represents <code>(T x) => f <= g</code></returns>

        public static Expression<Func<T, bool>> LessThanOrEqual<T>(this Expression<Func<T, bool>> f, Expression<Func<T, bool>> g)
        {
            var parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new ReplaceVisitor(f.Parameters.First(), parameter);
            Expression left = leftVisitor.Visit(f.Body);

            ReplaceVisitor rightVisitor = new ReplaceVisitor(g.Parameters.First(), parameter);
            Expression right = rightVisitor.Visit(g.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.LessThanOrEqual(left, right), parameter);
        }


        /// <summary>
        /// Combines two expressions using the logical <code>&</code> operator 
        /// </summary>
        /// <typeparam name="T">Type of the input of the two functions to combines</typeparam>
        /// <param name="f">the first expression</param>
        /// <param name="g">the second expression</param>
        /// <returns>A new expression that represents <code>(T x) => f == g</code></returns>

        public static Expression<Func<T, bool>> Equal<T>(this Expression<Func<T, bool>> f, Expression<Func<T, bool>> g)
        {
            var parameter = Expression.Parameter(typeof(T));

            ReplaceVisitor leftVisitor = new ReplaceVisitor(f.Parameters.First(), parameter);
            Expression left = leftVisitor.Visit(f.Body);

            ReplaceVisitor rightVisitor = new ReplaceVisitor(g.Parameters.First(), parameter);
            Expression right = rightVisitor.Visit(g.Body);

            return Expression.Lambda<Func<T, bool>>(Expression.Equal(left, right), parameter);
        }

    }


   
}
