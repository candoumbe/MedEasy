using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MedEasy.DTO
{
    /// <summary>
    /// Wraps data to create a new <see cref="PrescriptionHeaderInfo"/>
    /// </summary>
    public class CreatePrescriptionInfo
    {
        /// <summary>
        /// When the prescription was delivered
        /// </summary>
        [Required]
        public DateTimeOffset DeliveryDate { get; set; }
        /// <summary>
        /// How long is the prescription valid.
        /// 
        /// </summary>
        /// <remarks>
        /// This defines how long from <see cref="DeliveryDate"/> the current prescription will be valid
        /// </remarks>
        [Range(0, int.MaxValue)]
        public int Duration { get; set; }

        /// <summary>
        /// Content of the prescription
        /// </summary>
        public ICollection<PrescriptionItemInfo> Items { get; set; } = new List<PrescriptionItemInfo>();

        /// <summary>
        /// Id of the doctor who created the prescription
        /// </summary>
        public int PrescriptorId { get; set; }
    }
}
