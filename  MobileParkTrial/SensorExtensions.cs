namespace MobileParkTrial;

public static class SensorExtensions
{
    public static DateTime ToDateTime(this long unixTime)
    {
        return DateTimeOffset.FromUnixTimeSeconds(unixTime).DateTime;
    }
    
    public static long ToUnixTime(this DateTime dateTime)
    {
        return (long)dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    } 
    
    public static SensorType ToSensorType(this byte type)
    {
        return type switch
        {
            1 => SensorType.Temperature,
            2 => SensorType.Humidity,
            3 => SensorType.Pressure,
            _ => SensorType.None
        };
    }
}