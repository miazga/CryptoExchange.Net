using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;
using NUnit.Framework;

namespace CryptoExchange.Net.UnitTests;

[TestFixture()]
public class AsyncResetEventTests
{
    [Test]
    public async Task InitialFalseAndResetFalse_Should_BothCompleteAfterSingleSet()
    {
        AsyncResetEvent evnt = new(false, false);

        Task<bool> waiter1 = evnt.WaitAsync();
        Task<bool> waiter2 = evnt.WaitAsync();

        evnt.Set();

        bool result1 = await waiter1;
        bool result2 = await waiter2;

        Assert.True(result1);
        Assert.True(result2);
    }

    [Test]
    public async Task InitialTrueAndResetFalse_Should_BothCompleteImmediately()
    {
        AsyncResetEvent evnt = new(true, false);

        Task<bool> waiter1 = evnt.WaitAsync();
        Task<bool> waiter2 = evnt.WaitAsync();

        bool result1 = await waiter1;
        bool result2 = await waiter2;

        Assert.True(result1);
        Assert.True(result2);
    }

    [Test]
    public async Task InitialFalseAndResetTrue_Should_CompleteEachAfterASet()
    {
        AsyncResetEvent evnt = new();

        Task<bool> waiter1 = evnt.WaitAsync();
        Task<bool> waiter2 = evnt.WaitAsync();

        evnt.Set();

        bool result1 = await waiter1;

        Assert.True(result1);
        Assert.True(waiter2.Status != TaskStatus.RanToCompletion);

        evnt.Set();

        bool result2 = await waiter2;

        Assert.True(result2);
    }

    [Test]
    public async Task InitialTrueAndResetTrue_Should_CompleteFirstImmediatelyAndSecondAfterSet()
    {
        AsyncResetEvent evnt = new(true);

        Task<bool> waiter1 = evnt.WaitAsync();
        Task<bool> waiter2 = evnt.WaitAsync();

        bool result1 = await waiter1;

        Assert.True(result1);
        Assert.True(waiter2.Status != TaskStatus.RanToCompletion);
        evnt.Set();

        bool result2 = await waiter2;

        Assert.True(result2);
    }

    [Test]
    public async Task Awaiting10TimesOnSameEvent_Should_AllCompleteAfter10Sets()
    {
        AsyncResetEvent evnt = new();

        List<Task<bool>> waiters = new();
        for (int i = 0; i < 10; i++)
        {
            waiters.Add(evnt.WaitAsync());
        }

        List<bool> results = null;
        Task resultsWaiter = Task.Run(async () =>
        {
            await Task.WhenAll(waiters);
            results = waiters.Select(w => w.Result).ToList();
        });

        for (int i = 1; i <= 10; i++)
        {
            evnt.Set();
            Assert.AreEqual(10 - i, waiters.Count(w => w.Status != TaskStatus.RanToCompletion));
        }

        await resultsWaiter;

        Assert.AreEqual(10, results.Count(r => r));
    }

    [Test]
    public async Task WaitingShorterThanTimeout_Should_ReturnTrue()
    {
        AsyncResetEvent evnt = new();

        Task<bool> waiter1 = evnt.WaitAsync(TimeSpan.FromMilliseconds(100));
        await Task.Delay(50);
        evnt.Set();

        bool result1 = await waiter1;

        Assert.True(result1);
    }

    [Test]
    public async Task WaitingLongerThanTimeout_Should_ReturnFalse()
    {
        AsyncResetEvent evnt = new();

        Task<bool> waiter1 = evnt.WaitAsync(TimeSpan.FromMilliseconds(100));

        bool result1 = await waiter1;

        Assert.False(result1);
    }
}