using Forms;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Measures.DTO
{
    public class CreateGenericMeasureFormInfo
    {
        private IEnumerable<FormField> _fields;

        public string Name { get; set; }

        public IEnumerable<FormField> Fields
        {
            get => _fields ?? Enumerable.Empty<FormField>();
            set => _fields = value ?? Enumerable.Empty<FormField>();
        }

        public CreateGenericMeasureFormInfo()
        {
            _fields = Enumerable.Empty<FormField>();
        }
    }
}
