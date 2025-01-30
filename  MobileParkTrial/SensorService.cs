using System.Net.Sockets;

namespace MobileParkTrial;

public class SensorService
{
    private const int MaxDataPoints = 1000;
    private const int TrimSize = 500;

    private TcpClient _client = null!;
    private NetworkStream _stream = null!;
    private readonly List<SensorData> _sensorDataList = [];
    private readonly Lock _lock = new();
    private readonly CancellationTokenSource _cts = new();

    public bool IsRunning { get; private set; }
    public IReadOnlyList<SensorData> SensorDataList => _sensorDataList.AsReadOnly();
    public void AddData(SensorData[] data) => _sensorDataList.AddRange(data);

    public async Task StartProcessing(string host, int port)
    {
        try
        {
            _client = new TcpClient(host, port);
            _stream = _client.GetStream();
            IsRunning = true;
            Console.WriteLine("Connected to server.");
            
            _ = Task.Run(() => ReadSensorData(_cts.Token));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting: {ex.Message}");
        }
    }

    public void StopProcessing()
    {
        IsRunning = false;
        _cts.Cancel();
        _stream.Close();
        _client.Close();
        Console.WriteLine("Disconnected from server.");
    }

    internal async Task ReadSensorData(CancellationToken ctsToken)
    {
        try
        {
            while (IsRunning)
            {
                var buffer = new byte[2];
                if (await _stream.ReadAsync(buffer.AsMemory(0, 2), ctsToken) != 2) break;
                var messageLength = BitConverter.ToUInt16(buffer, 0);
                var data = new byte[messageLength];
                if (await _stream.ReadAsync(data.AsMemory(0, messageLength), ctsToken) != messageLength) break;
                ProcessMessage(data);
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException)
        {
            Console.WriteLine("Reading stopped.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading data: {ex.Message}");
        }
    }

    private void ProcessMessage(byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        var received = br.ReadInt64().ToDateTime();
        var instanceId = br.ReadUInt32();
        lock (_lock)
        {
            while (ms.Position < ms.Length)
            {
                var type = br.ReadByte().ToSensorType();
                var value = br.ReadDouble();
                _sensorDataList.Add(new SensorData(instanceId, received, type, value));

                if (_sensorDataList.Count > MaxDataPoints)
                {
                    _sensorDataList.RemoveRange(0, TrimSize);
                }
                Console.WriteLine($"Sensor {instanceId} {type}: {value:F2} at {received}");
            }
        }
    }

    public void ShowAverages()
    {
        lock (_lock)
        {
            if (_sensorDataList.Count == 0)
            {
                Console.WriteLine("No data available.");
                return;
            }

            var last10Messages = _sensorDataList.TakeLast(10);

            var grouped = last10Messages
                .GroupBy(s => s.Type)
                .Select(g => new { Type = g.Key, Average = g.Average(s => s.Value) });

            foreach (var g in grouped)
                Console.WriteLine($"Sensor {g.Type}: {g.Average:F2}");
        }
    }

    public void ShowStatistics()
    {
        lock (_lock)
        {
            Console.WriteLine($"Connected: {IsRunning}, Messages received: {_sensorDataList.Count}");
        }
    }
}