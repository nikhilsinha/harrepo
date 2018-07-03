using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using HarRepo.Controllers;
using HarRepo.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace HarRepo.Tests
{
    public class HarControllerTests
    {
        [Fact]
        public void Test_Get_NotFound()
        {
            var mockCache = new Mock<IMemoryCache>();
            object h = null;
            mockCache.Setup(repo => repo.TryGetValue(It.IsAny<object>(), out h)).Returns(false);

            HarController controller = new HarController(mockCache.Object);
            IActionResult result = controller.Get(1, null);
            Assert.NotNull(result);
            ObjectResult objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public void Test_Get_Found()
        {
            var mockCache = new Mock<IMemoryCache>();
            Har toReturn = GenerateSimpleHarObject();
            object h = (object)toReturn;
            mockCache.Setup(repo => repo.TryGetValue(1, out h)).Returns(true);

            HarController controller = new HarController(mockCache.Object);
            IActionResult result = controller.Get(1, null);
            Assert.NotNull(result);

            ObjectResult objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult.StatusCode);

            Object val = objectResult.Value;
            bool success = GetProperty<bool>(val, "Success");
            Assert.Equal(true, success);

            int status = GetProperty<int>(val, "Status");
            Assert.Equal(200, status);

            Har model = GetProperty<Har>(val, "Model");
            Assert.Equal(toReturn, model);
        }

        [Fact]
        public void Test_Get_Found_Query_Blocked()
        {
            var mockCache = new Mock<IMemoryCache>();
            Har toReturn = GenerateSimpleHarObject();
            object h = (object)toReturn;
            mockCache.Setup(repo => repo.TryGetValue(1, out h)).Returns(true);

            HarController controller = new HarController(mockCache.Object);
            IActionResult result = controller.Get(1, "blocked");
            Assert.NotNull(result);

            ObjectResult objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult.StatusCode);

            Object val = objectResult.Value;

            bool success  = GetProperty<bool>(val, "Success");
            Assert.Equal(true, success);


            int status = GetProperty<int>(val, "Status");
            Assert.Equal(200, status);

            Har model = GetProperty<Har>(val, "Model");
            Assert.Equal(toReturn, model);

            BlockedQuery bq = GetProperty<BlockedQuery>(val, "Query");
            Assert.Equal(toReturn.Log.Entries[3], bq.Longest);
            Assert.Equal(toReturn.Log.Entries[4], bq.Shortest);
            Assert.Equal(toReturn.Log.Entries[1], bq.SecondShortest);

        }

        [Fact]
        public void Test_Get_Found_Query_BodySize()
        {
            var mockCache = new Mock<IMemoryCache>();
            Har toReturn = GenerateSimpleHarObject();
            object h = (object)toReturn;
            mockCache.Setup(repo => repo.TryGetValue(1, out h)).Returns(true);

            HarController controller = new HarController(mockCache.Object);
            IActionResult result = controller.Get(1, "bodysize");
            Assert.NotNull(result);

            ObjectResult objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult.StatusCode);

            Object val = objectResult.Value;

            bool success = GetProperty<bool>(val, "Success");
            Assert.Equal(true, success);


            int status = GetProperty<int>(val, "Status");
            Assert.Equal(200, status);

            Har model = GetProperty<Har>(val, "Model");
            Assert.Equal(toReturn, model);

            BodySizeQuery bq = GetProperty<BodySizeQuery>(val, "Query");
            Assert.Equal(30, bq.TotalSize);
            Assert.Equal(6, bq.AverageSize);

        }

        [Fact]
        public void Test_Get_Found_Query_QueryString()
        {
            var mockCache = new Mock<IMemoryCache>();
            Har toReturn = GenerateSimpleHarObject();
            object h = (object)toReturn;
            mockCache.Setup(repo => repo.TryGetValue(1, out h)).Returns(true);

            HarController controller = new HarController(mockCache.Object);
            IActionResult result = controller.Get(1, "querystring");
            Assert.NotNull(result);

            ObjectResult objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult.StatusCode);

            Object val = objectResult.Value;

            bool success = GetProperty<bool>(val, "Success");
            Assert.Equal(true, success);


            int status = GetProperty<int>(val, "Status");
            Assert.Equal(200, status);

            Har model = GetProperty<Har>(val, "Model");
            Assert.Equal(toReturn, model);

            List<string> urls = GetProperty<List<string>>(val, "Query");
            Assert.Equal(2, urls.Count);
            Assert.Equal("www.google.com?b=1", urls[0]);
            Assert.Equal("www.yahoo.com?a=1", urls[1]);

        }

        [Fact]
        public void Test_Get_Found_Query_UnknownQuery()
        {
            var mockCache = new Mock<IMemoryCache>();
            Har toReturn = GenerateSimpleHarObject();
            object h = (object)toReturn;
            mockCache.Setup(repo => repo.TryGetValue(1, out h)).Returns(true);

            HarController controller = new HarController(mockCache.Object);
            IActionResult result = controller.Get(1, "adfsdafsd");
            Assert.NotNull(result);

            ObjectResult objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(400, objectResult.StatusCode);

            Object val = objectResult.Value;

            bool success = GetProperty<bool>(val, "Success");
            Assert.Equal(false, success);


            int status = GetProperty<int>(val, "Status");
            Assert.Equal(400, status);

            string message = GetProperty<string>(val, "Message");
            Assert.Equal("Unknown Query: adfsdafsd", message);

        }

        [Fact]
        public void Test_Get_Found_Query_SlowestPage()
        {
            var mockCache = new Mock<IMemoryCache>();
            Har toReturn = GenerateSimpleHarObject();
            object h = (object)toReturn;
            mockCache.Setup(repo => repo.TryGetValue(1, out h)).Returns(true);

            HarController controller = new HarController(mockCache.Object);
            IActionResult result = controller.Get(1, "slowestpage");
            Assert.NotNull(result);

            ObjectResult objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult.StatusCode);

            Object val = objectResult.Value;

            bool success = GetProperty<bool>(val, "Success");
            Assert.Equal(true, success);


            int status = GetProperty<int>(val, "Status");
            Assert.Equal(200, status);

            Har model = GetProperty<Har>(val, "Model");
            Assert.Equal(toReturn, model);

            SlowestPageQuery bq = GetProperty<SlowestPageQuery>(val, "Query");
            Assert.Equal("www.ask.com", bq.Url);
            Assert.Equal(68, bq.Size);

        }

        public T GetProperty<T>(Object o, string propertyName)
        {
            Type t = o.GetType();
            PropertyInfo pi = t.GetProperty(propertyName);
            return (T)pi.GetValue(o);
        }

        [Fact]
        public void Test_Delete_NotFound()
        {
            var mockCache = new Mock<IMemoryCache>();
            object h = null;
            mockCache.Setup(repo => repo.TryGetValue(It.IsAny<object>(), out h)).Returns(false);

            HarController controller = new HarController(mockCache.Object);
            IActionResult result = controller.Delete(1);
            Assert.NotNull(result);
            ObjectResult objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public void Test_Delete_Found()
        {
            var mockCache = new Mock<IMemoryCache>();
            Har toReturn = GenerateSimpleHarObject();
            object h = (object)toReturn;
            mockCache.Setup(repo => repo.TryGetValue(1, out h)).Returns(true);

            HarController controller = new HarController(mockCache.Object);
            IActionResult result = controller.Delete(1);
            Assert.NotNull(result);
            ObjectResult objectResult = result as ObjectResult;
            Assert.NotNull(objectResult);
            Assert.Equal(200, objectResult.StatusCode);

            Object val = objectResult.Value;

            bool success = GetProperty<bool>(val, "Success");
            Assert.Equal(true, success);


            int status = GetProperty<int>(val, "Status");
            Assert.Equal(200, status);

            Har model = GetProperty<Har>(val, "Model");
            Assert.Equal(toReturn, model);
        }


        // Post and Put aren't tested because Moq doesn't support extension methods




        private Har GenerateSimpleHarObject()
        {
            Har har = new Har();
            har.Log = new Log();
            Entry[] entries = new Entry[5];
            har.Log.Entries = entries;

            entries[0] = new Entry
            {
                Timings = new Timing
                {
                    Blocked = -1,
                    Connect = 3,
                    Dns = 1,
                    Receive = 1,
                    Send = 34,
                    Ssl = 2,
                    Wait = 4
                },
                Request = new Request
                {
                    BodySize = 0,
                    HeadersSize = 3,
                    HttpVersion = "Some Version",
                    Method = "Get",
                    Url = "www.google.com?b=1"
                },
                Time = 45

            };

            entries[1] = new Entry
            {
                Timings = new Timing
                {
                    Blocked = 2,
                    Connect = 3,
                    Dns = 1,
                    Receive = 1,
                    Send = 34,
                    Ssl = 2,
                    Wait = 4
                },
                Request = new Request
                {
                    BodySize = 4,
                    HeadersSize = 3,
                    HttpVersion = "Some Version",
                    Method = "POST",
                    Url = "www.google.com"
                },
                Time = 47
            };

            entries[2] = new Entry
            {
                Timings = new Timing
                {
                    Blocked = 5,
                    Connect = 3,
                    Dns = 1,
                    Receive = 1,
                    Send = 34,
                    Ssl = 2,
                    Wait = 4
                },
                Request = new Request
                {
                    BodySize = 23,
                    HeadersSize = 3,
                    HttpVersion = "Some Version",
                    Method = "Get",
                    Url = "www.yahoo.com?a=1"
                },
                Time = 50
            };

            entries[3] = new Entry
            {
                Timings = new Timing
                {
                    Blocked = 23,
                    Connect = 3,
                    Dns = 1,
                    Receive = 1,
                    Send = 34,
                    Ssl = 2,
                    Wait = 4
                },
                Request = new Request
                {
                    BodySize = 1,
                    HeadersSize = 3,
                    HttpVersion = "Some Version",
                    Method = "Get",
                    Url = "www.ask.com"
                },
                Time = 68
            };

            entries[4] = new Entry
            {
                Timings = new Timing
                {
                    Blocked = 1,
                    Connect = 3,
                    Dns = 1,
                    Receive = 1,
                    Send = 34,
                    Ssl = 2,
                    Wait = 4
                },
                Request = new Request
                {
                    BodySize = 2,
                    HeadersSize = 3,
                    HttpVersion = "Some Version",
                    Method = "Get",
                    Url = "www.vanillajs.com"
                },
                Time = 46
            };


            return har;
        }
    }
}
