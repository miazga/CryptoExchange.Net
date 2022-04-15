using System;
using System.Threading;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.Sockets;
using CryptoExchange.Net.UnitTests.TestImplementations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace CryptoExchange.Net.UnitTests;

[TestFixture]
public class SocketClientTests
{
    [TestCase]
    public void SettingOptions_Should_ResultInOptionsSet()
    {
        //arrange
        //act
        TestSocketClient client = new(new TestOptions
        {
            SubOptions = new RestApiClientOptions { BaseAddress = "http://test.address.com" },
            ReconnectInterval = TimeSpan.FromSeconds(6)
        });


        //assert
        Assert.IsTrue(client.SubClient.Options.BaseAddress == "http://test.address.com");
        Assert.IsTrue(client.ClientOptions.ReconnectInterval.TotalSeconds == 6);
    }

    [TestCase(true)]
    [TestCase(false)]
    public void ConnectSocket_Should_ReturnConnectionResult(bool canConnect)
    {
        //arrange
        TestSocketClient client = new();
        TestSocket socket = client.CreateSocket();
        socket.CanConnect = canConnect;

        //act
        CallResult<bool> connectResult = client.ConnectSocketSub(new SocketConnection(client, null, socket));

        //assert
        Assert.IsTrue(connectResult.Success == canConnect);
    }

    [TestCase]
    public void SocketMessages_Should_BeProcessedInDataHandlers()
    {
        // arrange
        TestSocketClient client = new(new TestOptions { ReconnectInterval = TimeSpan.Zero, LogLevel = LogLevel.Debug });
        TestSocket socket = client.CreateSocket();
        socket.ShouldReconnect = true;
        socket.CanConnect = true;
        socket.DisconnectTime = DateTime.UtcNow;
        SocketConnection sub = new(client, null, socket);
        ManualResetEvent rstEvent = new(false);
        JToken result = null;
        sub.AddSubscription(SocketSubscription.CreateForIdentifier(10, "TestHandler", true, messageEvent =>
        {
            result = messageEvent.JsonData;
            rstEvent.Set();
        }));
        client.ConnectSocketSub(sub);

        // act
        socket.InvokeMessage("{\"property\": 123}");
        rstEvent.WaitOne(1000);

        // assert
        Assert.IsTrue((int)result["property"] == 123);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void SocketMessages_Should_ContainOriginalDataIfEnabled(bool enabled)
    {
        // arrange
        TestSocketClient client = new(new TestOptions
        {
            ReconnectInterval = TimeSpan.Zero, LogLevel = LogLevel.Debug, OutputOriginalData = enabled
        });
        TestSocket socket = client.CreateSocket();
        socket.ShouldReconnect = true;
        socket.CanConnect = true;
        socket.DisconnectTime = DateTime.UtcNow;
        SocketConnection sub = new(client, null, socket);
        ManualResetEvent rstEvent = new(false);
        string original = null;
        sub.AddSubscription(SocketSubscription.CreateForIdentifier(10, "TestHandler", true, messageEvent =>
        {
            original = messageEvent.OriginalData;
            rstEvent.Set();
        }));
        client.ConnectSocketSub(sub);

        // act
        socket.InvokeMessage("{\"property\": 123}");
        rstEvent.WaitOne(1000);

        // assert
        Assert.IsTrue(original == (enabled ? "{\"property\": 123}" : null));
    }

    [TestCase]
    public void DisconnectedSocket_Should_Reconnect()
    {
        // arrange
        bool reconnected = false;
        TestSocketClient client = new(new TestOptions { ReconnectInterval = TimeSpan.Zero, LogLevel = LogLevel.Debug });
        TestSocket socket = client.CreateSocket();
        socket.ShouldReconnect = true;
        socket.CanConnect = true;
        socket.DisconnectTime = DateTime.UtcNow;
        SocketConnection sub = new(client, null, socket);
        sub.ShouldReconnect = true;
        client.ConnectSocketSub(sub);
        ManualResetEvent rstEvent = new(false);
        sub.ConnectionRestored += a =>
        {
            reconnected = true;
            rstEvent.Set();
        };

        // act
        socket.InvokeClose();
        rstEvent.WaitOne(1000);

        // assert
        Assert.IsTrue(reconnected);
    }

    [TestCase()]
    public void UnsubscribingStream_Should_CloseTheSocket()
    {
        // arrange
        TestSocketClient client = new(new TestOptions { ReconnectInterval = TimeSpan.Zero, LogLevel = LogLevel.Debug });
        TestSocket socket = client.CreateSocket();
        socket.CanConnect = true;
        SocketConnection sub = new(client, null, socket);
        client.ConnectSocketSub(sub);
        UpdateSubscription ups = new(sub, SocketSubscription.CreateForIdentifier(10, "Test", true, e => { }));

        // act
        client.UnsubscribeAsync(ups).Wait();

        // assert
        Assert.IsTrue(socket.Connected == false);
    }

    [TestCase()]
    public void UnsubscribingAll_Should_CloseAllSockets()
    {
        // arrange
        TestSocketClient client = new(new TestOptions { ReconnectInterval = TimeSpan.Zero, LogLevel = LogLevel.Debug });
        TestSocket socket1 = client.CreateSocket();
        TestSocket socket2 = client.CreateSocket();
        socket1.CanConnect = true;
        socket2.CanConnect = true;
        SocketConnection sub1 = new(client, null, socket1);
        SocketConnection sub2 = new(client, null, socket2);
        client.ConnectSocketSub(sub1);
        client.ConnectSocketSub(sub2);

        // act
        client.UnsubscribeAllAsync().Wait();

        // assert
        Assert.IsTrue(socket1.Connected == false);
        Assert.IsTrue(socket2.Connected == false);
    }

    [TestCase()]
    public void FailingToConnectSocket_Should_ReturnError()
    {
        // arrange
        TestSocketClient client = new(new TestOptions { ReconnectInterval = TimeSpan.Zero, LogLevel = LogLevel.Debug });
        TestSocket socket = client.CreateSocket();
        socket.CanConnect = false;
        SocketConnection sub = new(client, null, socket);

        // act
        CallResult<bool> connectResult = client.ConnectSocketSub(sub);

        // assert
        Assert.IsFalse(connectResult.Success);
    }
}