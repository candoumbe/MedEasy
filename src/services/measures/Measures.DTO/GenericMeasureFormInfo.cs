﻿using Forms;

using System;
using System.Collections;
using System.Collections.Generic;

namespace Measures.DTO
{
    public class GenericMeasureFormInfo
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<FormField> Fields { get; set; }

        public DateTime UpdatedDate { get; set; }
    }
}
