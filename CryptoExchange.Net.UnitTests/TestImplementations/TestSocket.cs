using System;
using System.Security.Authentication;
using System.Text;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;

namespace CryptoExchange.Net.UnitTests.TestImplementations;

public class TestSocket : IWebsocket
{
    public static int lastId;
    public static object lastIdLock = new();

    public TestSocket()
    {
        lock (lastIdLock)
        {
            Id = lastId + 1;
            lastId++;
        }
    }

    public bool CanConnect { get; set; }
    public bool Connected { get; set; }
    public bool ShouldReconnect { get; set; }
    public DateTime? DisconnectTime { get; set; }
    public bool PingConnection { get; set; }
    public TimeSpan PingInterval { get; set; }

    public int ConnectCalls { get; private set; }

    public event Action OnClose;
    public event Action<string> OnMessage;
    public event Action<Exception> OnError;
    public event Action OnOpen;

    public int Id { get; }
    public TimeSpan Timeout { get; set; }
    public Func<string, string> DataInterpreterString { get; set; }
    public Func<byte[], string> DataInterpreterBytes { get; set; }
    public string Url { get; }
    public bool IsClosed => !Connected;
    public bool IsOpen => Connected;
    public SslProtocols SSLProtocols { get; set; }
    public Encoding Encoding { get; set; }
    public bool Reconnecting { get; set; }
    public string Origin { get; set; }
    public int? RatelimitPerSecond { get; set; }

    public double IncomingKbps => throw new NotImplementedException();

    public Task<bool> ConnectAsync()
    {
        Connected = CanConnect;
        ConnectCalls++;
        if (CanConnect)
        {
            InvokeOpen();
        }

        return Task.FromResult(CanConnect);
    }

    public void Send(string data)
    {
        if (!Connected)
        {
            throw new Exception("Socket not connected");
        }
    }

    public void Reset()
    {
    }

    public Task CloseAsync()
    {
        Connected = false;
        DisconnectTime = DateTime.UtcNow;
        OnClose?.Invoke();
        return Task.FromResult(0);
    }

    public void Dispose()
    {
    }

    public void SetProxy(ApiProxy proxy)
    {
        throw new NotImplementedException();
    }

    public void SetProxy(string host, int port)
    {
        throw new NotImplementedException();
    }

    public void InvokeClose()
    {
        Connected = false;
        DisconnectTime = DateTime.UtcNow;
        OnClose?.Invoke();
    }

    public void InvokeOpen()
    {
        OnOpen?.Invoke();
    }

    public void InvokeMessage(string data)
    {
        OnMessage?.Invoke(data);
    }

    public void InvokeError(Exception error)
    {
        OnError?.Invoke(error);
    }
}