using System;
using System.Net.Http;
using System.Threading.Tasks;
using Polly;
using Xunit;

namespace PollyHttpClientRetry.Tests;

public class PollyHttpClientRetryTests
{
  [Fact]
  public async Task ShouldRetryOnClosedSocket()
  {
    const int maxRetryCount = 5;
    const double durationBetweenRetries = 250;

    var actualRetries = 0;

    var retryPolicy = Policy
      .Handle<HttpRequestException>()
      .WaitAndRetryAsync(
        maxRetryCount,
        retryCount =>
        {
          actualRetries = retryCount;
          return TimeSpan.FromMilliseconds(
            durationBetweenRetries * Math.Pow(
              2,
              retryCount - 1
            )
          );
        }
      );

    var httpClient = new HttpClient();

    // this url doesn't exist, hence a HttpRequestException will be thrown
    var url = new Uri("http://timeout.alexanderzeitler.com");
    try
    {
      using var response = await retryPolicy.ExecuteAsync(
        () => httpClient.SendAsync(
          new HttpRequestMessage
          {
            Method = HttpMethod.Get,
            RequestUri = url
          }
        )
      );
      Assert.Equal(
        maxRetryCount,
        actualRetries
      );
    }
    catch (Exception e)
    {
      Assert.Equal(
        maxRetryCount,
        actualRetries
      );
    }
  }
}