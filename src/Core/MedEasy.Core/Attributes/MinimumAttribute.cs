using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MedEasy.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class MinimumAttribute : RangeAttribute
    {
        public MinimumAttribute(int minimum) : base(minimum, int.MaxValue)
        {
        }

        public MinimumAttribute(double minimum) : base(minimum, double.MaxValue)
        {

        }

        public MinimumAttribute(long minimum) : base(minimum, long.MaxValue)
        {

        }

        public MinimumAttribute(float minimum) : base(minimum, float.MaxValue)
        {
        }
        
    }
}
