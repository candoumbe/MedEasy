using Newtonsoft.Json;

namespace Measures.DTO
{
    /// <summary>
    /// Search criteria for <see cref="BloodPressureInfo"/> resources.
    /// </summary>
    [JsonObject]
    public class SearchBloodPressureInfo : SearchMeasureInfo<BloodPressureInfo>
    {
        
    }
}