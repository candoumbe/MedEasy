using Forms;

using System;
using System.Collections.Generic;
using System.Text;

namespace Measures.Models.v1
{
    public class NewMeasureFormModel
    {
        public string Name { get; set; }

        public IEnumerable<FormField> Fields { get; set; }
    }
}
