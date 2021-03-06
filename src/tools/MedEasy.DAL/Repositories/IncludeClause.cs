namespace MedEasy.DAL.Repositories
{
    using System;
    using System.Linq.Expressions;

    public class IncludeClause<T>
    {


        private IncludeClause(LambdaExpression selector)
        {
            Expression = selector;
        }

        public static IncludeClause<T> Create<TProperty>(Expression<Func<T, TProperty>> selector) => new(selector);

        public LambdaExpression Expression { get; }
    }
}