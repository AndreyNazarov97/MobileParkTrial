namespace MobileParkTrial;

public class SensorData
{
    public UInt32 Id { get; set; }
    
    public DateTime Received { get; set; }
    public SensorType Type { get; }
    public double Value { get; }

    public SensorData(UInt32 id, DateTime received, SensorType type, double value)
    {
        Type = type;
        Value = value;
    }
}