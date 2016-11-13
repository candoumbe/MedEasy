
using System.ComponentModel.DataAnnotations;

namespace MedEasy.DTO
{
    public class PrescriptionItemInfo : ResourceBase<int>
    {
        /// <summary>
        /// Prescription's category
        /// </summary>
        public int CategoryId { get; set; }


        /// <summary>
        /// Category
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Code of the prescription
        /// </summary>
        [Required]
        public string Code { get; set; }


        /// <summary>
        /// Name of the prescription
        /// </summary>
        [Required]
        public string Designation { get; set; }

        /// <summary>
        /// Quantity of the prescription
        /// </summary>
        [Range(0, double.MaxValue)]
        public decimal Quantity { get; set; }

        /// <summary>
        /// Additional notes for the prescription
        /// </summary>
        public string Notes { get; set; }
    }
}