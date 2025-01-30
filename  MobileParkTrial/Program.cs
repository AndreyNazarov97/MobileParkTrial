using MobileParkTrial;

Console.WriteLine("Enter command (start, stop, info, statistics): ");
var sensorService = new SensorService();
while (true)
{
    Console.WriteLine("Enter command (start, stop, info, statistics): ");
    var command = Console.ReadLine()?.Trim().ToLower();
    switch (command)
    {
        case "start":
            if (!sensorService.IsRunning)
                await sensorService.StartProcessing("127.0.0.1", 5000);
            break;
        case "stop":
            sensorService.StopProcessing();
            break;
        case "info":
            sensorService.ShowAverages();
            break;
        case "statistics":
            sensorService.ShowStatistics();
            break;
    }
}