using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public sealed class SingleInstance : IDisposable
{
    private readonly string _mutexName;
    private readonly string _pipeName;
    private readonly Mutex _mutex;
    private readonly bool _isPrimary;
    private readonly CancellationTokenSource _cts = new();

    public bool IsPrimary => _isPrimary;
    public event Action<string[]>? ArgumentsReceived;

    public SingleInstance(string key) // e.g., "DebounceMyMouse"
    {
        _mutexName = $@"Local\{key}";
        _pipeName = $"{key}.pipe";

        _mutex = new Mutex(initiallyOwned: true, name: _mutexName, out var createdNew);
        _isPrimary = createdNew;

        if (_isPrimary)
            _ = Task.Run(ListenLoopAsync);
    }

    public void NotifyFirstInstance(string[] args)
    {
        try
        {
            using var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out,
                PipeOptions.CurrentUserOnly);
            client.Connect(1500);
            var payload = string.Join('\0', args ?? Array.Empty<string>());
            var bytes = Encoding.UTF8.GetBytes(payload);
            client.Write(bytes, 0, bytes.Length);
            client.Flush();
        }
        catch { /* primary not ready or no listener */ }
    }

    private async Task ListenLoopAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            using var server = new NamedPipeServerStream(_pipeName, PipeDirection.In,
                maxNumberOfServerInstances: 1, PipeTransmissionMode.Message,
                PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

            await server.WaitForConnectionAsync(_cts.Token).ConfigureAwait(false);

            using var ms = new MemoryStream();
            var buf = new byte[1024];
            do
            {
                int n = await server.ReadAsync(buf, 0, buf.Length, _cts.Token).ConfigureAwait(false);
                if (n <= 0) break;
                ms.Write(buf, 0, n);
            } while (!server.IsMessageComplete);

            var text = Encoding.UTF8.GetString(ms.ToArray());
            var args = string.IsNullOrEmpty(text) ? Array.Empty<string>() : text.Split('\0');

            try { ArgumentsReceived?.Invoke(args); } catch { /* swallow */ }
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _mutex.ReleaseMutex();
        _mutex.Dispose();
        _cts.Dispose();
    }
}
