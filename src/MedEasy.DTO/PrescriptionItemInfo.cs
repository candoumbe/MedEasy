
namespace MedEasy.DTO
{
    public class PrescriptionItemInfo : IResource<int>
    {
        public int Id { get; set; }

        /// <summary>
        /// Prescription's category
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Code of the prescription
        /// </summary>
        public string Code { get; set; }
    }
}