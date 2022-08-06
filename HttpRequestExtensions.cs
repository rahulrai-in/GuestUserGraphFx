using System.Net;
using Microsoft.Azure.Functions.Worker.Http;

namespace GuestUserGraphFx;

public static class HttpRequestExtensions
{
    private const string BadRequestErrorTypeUrl = "https://httpstatuses.com/400";
    private const string Title = "One or more errors occurred.";

    public static async Task<HttpResponseData> BadRequestAsync(
        this HttpRequestData request,
        string error)
    {
        var response = request.CreateResponse(HttpStatusCode.BadRequest);
        var errorDetails = new
        {
            Type = BadRequestErrorTypeUrl,
            Status = HttpStatusCode.BadRequest,
            Title,
            Detail = error,
            Instance = request.Url.AbsoluteUri
        };
        await response.WriteAsJsonAsync(errorDetails, HttpStatusCode.BadRequest);
        return response;
    }

    public static async Task<HttpResponseData> Ok<TResult>(
        this HttpRequestData request,
        TResult result)
    {
        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(result);
        return response;
    }
}