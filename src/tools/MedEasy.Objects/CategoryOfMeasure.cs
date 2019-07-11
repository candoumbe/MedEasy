using System;

namespace MedEasy.Objects
{
    public class CategoryOfMeasure : AuditableEntity<Guid, CategoryOfMeasure>
    {
        /// <summary>
        /// Name of the category
        /// </summary>
        public string Name { get; set; }


        public CategoryOfMeasure(Guid id) : base(id)
        {

        }
    }
}
