﻿#if NET6_0
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// An action result which formats the given object as JSON.
/// </summary>
public sealed partial class JsonHttpResult<TValue> : IResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Json"/> class with the values.
    /// </summary>
    /// <param name="value">The value to format in the entity body.</param>
    /// <param name="jsonSerializerOptions">The serializer settings.</param>
    internal JsonHttpResult(TValue? value, JsonSerializerOptions? jsonSerializerOptions)
        : this(value, statusCode: null, contentType: null, jsonSerializerOptions: jsonSerializerOptions)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Json"/> class with the values.
    /// </summary>
    /// <param name="value">The value to format in the entity body.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="jsonSerializerOptions">The serializer settings.</param>
    internal JsonHttpResult(TValue? value, int? statusCode, JsonSerializerOptions? jsonSerializerOptions)
        : this(value, statusCode: statusCode, contentType: null, jsonSerializerOptions: jsonSerializerOptions)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Json"/> class with the values.
    /// </summary>
    /// <param name="value">The value to format in the entity body.</param>
    /// <param name="contentType">The value for the <c>Content-Type</c> header</param>
    /// <param name="jsonSerializerOptions">The serializer settings.</param>
    internal JsonHttpResult(TValue? value, string? contentType, JsonSerializerOptions? jsonSerializerOptions)
        : this(value, statusCode: null, contentType: contentType, jsonSerializerOptions: jsonSerializerOptions)
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Json"/> class with the values.
    /// </summary>
    /// <param name="value">The value to format in the entity body.</param>
    /// <param name="statusCode">The HTTP status code of the response.</param>
    /// <param name="jsonSerializerOptions">The serializer settings.</param>
    /// <param name="contentType">The value for the <c>Content-Type</c> header</param>
    internal JsonHttpResult(TValue? value, int? statusCode, string? contentType, JsonSerializerOptions? jsonSerializerOptions)
    {
        Value = value;
        JsonSerializerOptions = jsonSerializerOptions;
        ContentType = contentType;

        if (value is ProblemDetails problemDetails)
        {
            statusCode ??= problemDetails.Status;
        }

        StatusCode = statusCode;
    }

    /// <summary>
    /// Gets or sets the serializer settings.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions { get; internal init; }

    /// <summary>
    /// Gets the object result.
    /// </summary>
    public TValue? Value { get; }

    /// <summary>
    /// Gets the value for the <c>Content-Type</c> header.
    /// </summary>
    public string? ContentType { get; internal set; }

    /// <summary>
    /// Gets the HTTP status code.
    /// </summary>
    public int? StatusCode { get; }

    /// <inheritdoc/>
    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        if (StatusCode is { } statusCode)
        {
            httpContext.Response.StatusCode = statusCode;
        }

        return httpContext.Response.WriteAsJsonAsync(Value, null, ContentType);
    }
}
#endif
