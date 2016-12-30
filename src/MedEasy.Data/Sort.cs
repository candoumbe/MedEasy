using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MedEasy.Data
{
    public class Sort
    {
        public LambdaExpression Expression { get; set; }


        public SortDirection Direction { get; set; }
    }
}
