namespace MedEasy.Objects
{
    /// <summary>
    /// An instance of this class represents a heart beat measurement
    /// </summary>
    public class Heartbeat : PhysiologicalMeasurement
    {
        public int Bpm { get; set; }
    }
}