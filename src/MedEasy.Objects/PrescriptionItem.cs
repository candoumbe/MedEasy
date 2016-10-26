using System.ComponentModel.DataAnnotations;

namespace MedEasy.Objects
{
    public class PrescriptionItem : AuditableEntity<int, PrescriptionItem>
    {


        /// <summary>
        /// Category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Code of the prescription
        /// </summary>
        public string Code { get; set; }


        /// <summary>
        /// Name of the prescription
        /// </summary>
        public string Designation { get; set; }

        /// <summary>
        /// Quantity
        /// </summary>
        public decimal Quantity { get; set; }

        /// <summary>
        /// Additional note
        /// </summary>
        public string Notes { get; set; }
    }
}