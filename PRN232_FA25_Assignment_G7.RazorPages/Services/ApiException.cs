using System;
using System.Net;

namespace PRN232_FA25_Assignment_G7.RazorPages.Services;

public class ApiException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public string? Content { get; }

    public ApiException(HttpStatusCode statusCode, string? content)
        : base($"API Error: {statusCode}")
    {
        StatusCode = statusCode;
        Content = content;
    }

    public ApiException(HttpStatusCode statusCode, string? content, string message)
        : base(message)
    {
        StatusCode = statusCode;
        Content = content;
    }
}
