using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CryptoExchange.Net.Interfaces;
using CryptoExchange.Net.Objects;
using CryptoExchange.Net.OrderBook;
using CryptoExchange.Net.Sockets;
using NUnit.Framework;

namespace CryptoExchange.Net.UnitTests;

[TestFixture]
public class SymbolOrderBookTests
{
    private static readonly OrderBookOptions defaultOrderBookOptions = new();

    [TestCase]
    public void GivenEmptyBidList_WhenBestBid_ThenEmptySymbolOrderBookEntry()
    {
        TestableSymbolOrderBook symbolOrderBook = new TestableSymbolOrderBook();
        Assert.IsNotNull(symbolOrderBook.BestBid);
        Assert.AreEqual(0m, symbolOrderBook.BestBid.Price);
        Assert.AreEqual(0m, symbolOrderBook.BestAsk.Quantity);
    }

    [TestCase]
    public void GivenEmptyAskList_WhenBestAsk_ThenEmptySymbolOrderBookEntry()
    {
        TestableSymbolOrderBook symbolOrderBook = new TestableSymbolOrderBook();
        Assert.IsNotNull(symbolOrderBook.BestBid);
        Assert.AreEqual(0m, symbolOrderBook.BestBid.Price);
        Assert.AreEqual(0m, symbolOrderBook.BestAsk.Quantity);
    }

    [TestCase]
    public void GivenEmptyBidAndAskList_WhenBestOffers_ThenEmptySymbolOrderBookEntries()
    {
        TestableSymbolOrderBook symbolOrderBook = new TestableSymbolOrderBook();
        Assert.IsNotNull(symbolOrderBook.BestOffers);
        Assert.IsNotNull(symbolOrderBook.BestOffers.Bid);
        Assert.IsNotNull(symbolOrderBook.BestOffers.Ask);
        Assert.AreEqual(0m, symbolOrderBook.BestOffers.Bid.Price);
        Assert.AreEqual(0m, symbolOrderBook.BestOffers.Bid.Quantity);
        Assert.AreEqual(0m, symbolOrderBook.BestOffers.Ask.Price);
        Assert.AreEqual(0m, symbolOrderBook.BestOffers.Ask.Quantity);
    }

    [TestCase]
    public void CalculateAverageFillPrice()
    {
        TestableSymbolOrderBook orderbook = new TestableSymbolOrderBook();
        orderbook.SetData(
            new List<ISymbolOrderBookEntry>
            {
                new BookEntry { Price = 1, Quantity = 1 }, new BookEntry { Price = 1.1m, Quantity = 1 }
            },
            new List<ISymbolOrderBookEntry>
            {
                new BookEntry { Price = 1.2m, Quantity = 1 }, new BookEntry { Price = 1.3m, Quantity = 1 }
            });

        CallResult<decimal> resultBids = orderbook.CalculateAverageFillPrice(2, OrderBookEntryType.Bid);
        CallResult<decimal> resultAsks = orderbook.CalculateAverageFillPrice(2, OrderBookEntryType.Ask);
        CallResult<decimal> resultBids2 = orderbook.CalculateAverageFillPrice(1.5m, OrderBookEntryType.Bid);
        CallResult<decimal> resultAsks2 = orderbook.CalculateAverageFillPrice(1.5m, OrderBookEntryType.Ask);

        Assert.True(resultBids.Success);
        Assert.True(resultAsks.Success);
        Assert.AreEqual(1.05m, resultBids.Data);
        Assert.AreEqual(1.25m, resultAsks.Data);
        Assert.AreEqual(1.06666667m, resultBids2.Data);
        Assert.AreEqual(1.23333333m, resultAsks2.Data);
    }

    private class TestableSymbolOrderBook : SymbolOrderBook
    {
        public TestableSymbolOrderBook() : base("Test", "BTC/USD", defaultOrderBookOptions)
        {
        }


        protected override Task<CallResult<bool>> DoResyncAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        protected override Task<CallResult<UpdateSubscription>> DoStartAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public void SetData(IEnumerable<ISymbolOrderBookEntry> bids, IEnumerable<ISymbolOrderBookEntry> asks)
        {
            Status = OrderBookStatus.Synced;
            this.bids.Clear();
            foreach (ISymbolOrderBookEntry bid in bids)
            {
                this.bids.Add(bid.Price, bid);
            }

            this.asks.Clear();
            foreach (ISymbolOrderBookEntry ask in asks)
            {
                this.asks.Add(ask.Price, ask);
            }
        }
    }

    public class BookEntry : ISymbolOrderBookEntry
    {
        public decimal Quantity { get; set; }
        public decimal Price { get; set; }
    }
}