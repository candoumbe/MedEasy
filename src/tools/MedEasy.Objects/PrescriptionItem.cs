
using System;

namespace MedEasy.Objects
{
    public class PrescriptionItem : AuditableEntity<Guid, PrescriptionItem>
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

        public PrescriptionItem(Guid id) : base(id)
        {

        }
    }
}