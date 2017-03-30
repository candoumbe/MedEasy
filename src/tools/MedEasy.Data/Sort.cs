using System.Linq.Expressions;

namespace MedEasy.Data
{
    public class Sort
    {
        public LambdaExpression Expression { get; set; }


        public SortDirection Direction { get; set; }
    }
}
