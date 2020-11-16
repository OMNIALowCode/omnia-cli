using System;
using System.Net.Http;
using System.Threading;
using Moq;
using Moq.Protected;

namespace UnitTests.Extensions
{
    public static class MockHttpMessageHandlerExtensions
    {
        public static void VerifyRequestHasBeenMade(this Mock<HttpMessageHandler> self,
            HttpMethod httpMethod, Uri uri, Times times)
        {
            self.Protected().Verify(
                "SendAsync",
                times, 
                ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == httpMethod 
                        && req.RequestUri == uri
                ),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}
