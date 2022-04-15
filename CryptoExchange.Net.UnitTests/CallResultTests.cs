using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using CryptoExchange.Net.Objects;
using NUnit.Framework;

namespace CryptoExchange.Net.UnitTests;

[TestFixture()]
internal class CallResultTests
{
    [Test]
    public void TestBasicErrorCallResult()
    {
        CallResult result = new(new ServerError("TestError"));

        Assert.AreEqual(result.Error.Message, "TestError");
        Assert.IsFalse(result);
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void TestBasicSuccessCallResult()
    {
        CallResult result = new(null);

        Assert.IsNull(result.Error);
        Assert.IsTrue(result);
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void TestCallResultError()
    {
        CallResult<object> result = new(new ServerError("TestError"));

        Assert.AreEqual(result.Error.Message, "TestError");
        Assert.IsNull(result.Data);
        Assert.IsFalse(result);
        Assert.IsFalse(result.Success);
    }

    [Test]
    public void TestCallResultSuccess()
    {
        CallResult<object> result = new(new object());

        Assert.IsNull(result.Error);
        Assert.IsNotNull(result.Data);
        Assert.IsTrue(result);
        Assert.IsTrue(result.Success);
    }

    [Test]
    public void TestCallResultSuccessAs()
    {
        CallResult<TestObjectResult> result = new(new TestObjectResult());
        CallResult<TestObject2> asResult = result.As(result.Data.InnerData);

        Assert.IsNull(asResult.Error);
        Assert.IsNotNull(asResult.Data);
        Assert.IsTrue(asResult.Data is TestObject2);
        Assert.IsTrue(asResult);
        Assert.IsTrue(asResult.Success);
    }

    [Test]
    public void TestCallResultErrorAs()
    {
        CallResult<TestObjectResult> result = new(new ServerError("TestError"));
        CallResult<TestObject2> asResult = result.As<TestObject2>(default);

        Assert.IsNotNull(asResult.Error);
        Assert.AreEqual(asResult.Error.Message, "TestError");
        Assert.IsNull(asResult.Data);
        Assert.IsFalse(asResult);
        Assert.IsFalse(asResult.Success);
    }

    [Test]
    public void TestCallResultErrorAsError()
    {
        CallResult<TestObjectResult> result = new(new ServerError("TestError"));
        CallResult<TestObject2> asResult = result.AsError<TestObject2>(new ServerError("TestError2"));

        Assert.IsNotNull(asResult.Error);
        Assert.AreEqual(asResult.Error.Message, "TestError2");
        Assert.IsNull(asResult.Data);
        Assert.IsFalse(asResult);
        Assert.IsFalse(asResult.Success);
    }

    [Test]
    public void TestWebCallResultErrorAsError()
    {
        WebCallResult<TestObjectResult> result = new(new ServerError("TestError"));
        WebCallResult<TestObject2> asResult = result.AsError<TestObject2>(new ServerError("TestError2"));

        Assert.IsNotNull(asResult.Error);
        Assert.AreEqual(asResult.Error.Message, "TestError2");
        Assert.IsNull(asResult.Data);
        Assert.IsFalse(asResult);
        Assert.IsFalse(asResult.Success);
    }

    [Test]
    public void TestWebCallResultSuccessAsError()
    {
        WebCallResult<TestObjectResult> result = new(
            HttpStatusCode.OK,
            new List<KeyValuePair<string, IEnumerable<string>>>(),
            TimeSpan.FromSeconds(1),
            "{}",
            "https://test.com/api",
            null,
            HttpMethod.Get,
            new List<KeyValuePair<string, IEnumerable<string>>>(),
            new TestObjectResult(),
            null);
        WebCallResult<TestObject2> asResult = result.AsError<TestObject2>(new ServerError("TestError2"));

        Assert.IsNotNull(asResult.Error);
        Assert.AreEqual(asResult.Error.Message, "TestError2");
        Assert.AreEqual(asResult.ResponseStatusCode, HttpStatusCode.OK);
        Assert.AreEqual(asResult.ResponseTime, TimeSpan.FromSeconds(1));
        Assert.AreEqual(asResult.RequestUrl, "https://test.com/api");
        Assert.AreEqual(asResult.RequestMethod, HttpMethod.Get);
        Assert.IsNull(asResult.Data);
        Assert.IsFalse(asResult);
        Assert.IsFalse(asResult.Success);
    }

    [Test]
    public void TestWebCallResultSuccessAsSuccess()
    {
        WebCallResult<TestObjectResult> result = new(
            HttpStatusCode.OK,
            new List<KeyValuePair<string, IEnumerable<string>>>(),
            TimeSpan.FromSeconds(1),
            "{}",
            "https://test.com/api",
            null,
            HttpMethod.Get,
            new List<KeyValuePair<string, IEnumerable<string>>>(),
            new TestObjectResult(),
            null);
        WebCallResult<TestObject2> asResult = result.As(result.Data.InnerData);

        Assert.IsNull(asResult.Error);
        Assert.AreEqual(asResult.ResponseStatusCode, HttpStatusCode.OK);
        Assert.AreEqual(asResult.ResponseTime, TimeSpan.FromSeconds(1));
        Assert.AreEqual(asResult.RequestUrl, "https://test.com/api");
        Assert.AreEqual(asResult.RequestMethod, HttpMethod.Get);
        Assert.IsNotNull(asResult.Data);
        Assert.IsTrue(asResult);
        Assert.IsTrue(asResult.Success);
    }
}

public class TestObjectResult
{
    public TestObject2 InnerData;

    public TestObjectResult()
    {
        InnerData = new TestObject2();
    }
}

public class TestObject2
{
}