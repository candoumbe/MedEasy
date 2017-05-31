using System;

namespace MedEasy.DTO
{
    /// <summary>
    /// Binary content
    /// </summary>
    public class DocumentInfo : Resource<Guid>
    {
        /// <summary>
        /// Binary content
        /// </summary>
        public byte[] Content { get; set; }

    }
}
