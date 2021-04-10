namespace Identity.DTO
{
    /// <summary>
    /// Wraps informations about a <see cref="Claim"/>
    /// </summary>
    public class ClaimInfo
    {
        public string Type { get; set; }

        public string Value { get; set; }
    }
}