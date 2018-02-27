namespace MedEasy.Objects
{
    public class CategoryOfMeasure : AuditableEntity<int, CategoryOfMeasure>
    {
        /// <summary>
        /// Name of the category
        /// </summary>
        public string Name { get; set; }
    }
}
