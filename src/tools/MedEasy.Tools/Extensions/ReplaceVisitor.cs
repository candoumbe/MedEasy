using System.Linq.Expressions;

namespace MedEasy.Tools
{
    internal class ReplaceVisitor : ExpressionVisitor
    {
        private readonly Expression _from, _to;
        public ReplaceVisitor(Expression from, Expression to)
        {
            this._from = from;
            this._to = to;
        }
        public override Expression Visit(Expression node)
        {
            return node == _from ? _to : base.Visit(node);
        }
    }
}
