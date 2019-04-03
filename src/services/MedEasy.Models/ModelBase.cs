using MedEasy.RestObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedEasy.Models
{
    public class ModelBase<T> : Resource<T>
        where T : IEquatable<T>
    {
    }
}
