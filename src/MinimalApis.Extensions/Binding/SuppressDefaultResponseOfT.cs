using System.Reflection;
using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Binding;

/// <summary>
/// Suprresses the default response logic of <see cref="RequestDelegateFactory"/> when accepted as a parameter to a route handler.
/// Default binding of the <typeparamref name="TValue"/> will still occur.
/// </summary>
/// <typeparam name="TValue">The <see cref="Type"/> of the parameter.</typeparam>
public class SuppressDefaultResponse<TValue> : IProvideEndpointParameterMetadata
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SuppressDefaultResponse{TValue}"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="statusCode">The type of the parameter that the default response is being suppressed for.</param>
    public SuppressDefaultResponse(TValue? value, int statusCode)
    {
        Value = value;
        StatusCode = statusCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SuppressDefaultResponse{TValue}"/> class using the provided <see cref="Exception"/>.
    /// </summary>
    /// <param name="exception">The <see cref="Exception"/>.</param>
    public SuppressDefaultResponse(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception, nameof(exception));

        Exception = exception;
    }

    /// <summary>
    /// The value bound by <see cref="RequestDelegateFactory"/>.
    /// </summary>
    public TValue? Value { get; }

    /// <summary>
    /// The status code that <see cref="RequestDelegateFactory"/> would have returned.
    /// </summary>
    public int StatusCode { get; }

    /// <summary>
    /// The exception that occurred if binding failed.
    /// </summary>
    public Exception? Exception { get; }

    /// <summary>
    /// Binds the specified parameter from <see cref="HttpContext.Request"/>. This method is called by the framework on your behalf
    /// when populating parameters of a mapped route handler.
    /// </summary>
    /// <param name="context">The <see cref="HttpContext"/> to bind the parameter from.</param>
    /// <param name="parameter">The route handler parameter being bound to.</param>
    /// <returns>An instance of <see cref="SuppressDefaultResponse{TValue}"/>.</returns>
    public static async ValueTask<SuppressDefaultResponse<TValue?>> BindAsync(HttpContext context, ParameterInfo parameter)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        ArgumentNullException.ThrowIfNull(parameter, nameof(parameter));

        try
        {
            // Manually invoke the default binding logic
            var (boundValue, statusCode) = await DefaultBinder<TValue>.GetValueAsync(context);
            return new SuppressDefaultResponse<TValue?>(boundValue, statusCode);
        }
        catch (Exception ex)
        {
            // Exception occurred during binding!
            return new SuppressDefaultResponse<TValue?>(ex);
        }
    }

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="parameter">The parameter to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static IEnumerable<object> GetMetadata(ParameterInfo parameter, IServiceProvider services) =>
        IProvideEndpointParameterMetadata.GetDefaultMetadataForWrapperType<TValue>(parameter, services);
}
