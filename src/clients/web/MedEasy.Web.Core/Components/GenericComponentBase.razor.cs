using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedEasy.Web.Core.Components
{
    /// <summary>
    /// Base class for components that
    /// </summary>
    /// <typeparam name="T">Type of the model associated with the component</typeparam>
    public abstract class GenericComponentBase<T> : ComponentBase
    {
        [Parameter] public T ViewModel { get; set; }
    }
}
