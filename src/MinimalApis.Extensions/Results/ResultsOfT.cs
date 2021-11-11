using MinimalApis.Extensions.Metadata;

namespace MinimalApis.Extensions.Results;

/// <summary>
/// Represents the result of an <see cref="Endpoint"/> route handler delegate that can return more than one <see cref="IResult"/> type.
/// </summary>
public abstract class ResultsBase : IResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResultsBase"/> class.
    /// </summary>
    /// <param name="activeResult">The <see cref="IResult"/> returned.</param>
    protected ResultsBase(IResult activeResult)
    {
        Result = activeResult;
    }

    /// <summary>
    /// Gets the actual <see cref="IResult"/> returned by the <see cref="Endpoint"/> route handler delegate.
    /// </summary>
    public IResult Result { get; }

    /// <summary>
    /// Writes an HTTP response reflecting the result.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext"/> for the current request.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous execute operation.</returns>
    public async Task ExecuteAsync(HttpContext httpContext)
    {
        await Result.ExecuteAsync(httpContext);
    }

    /// <summary>
    /// Gets the <see cref="Endpoint"/> metadata for the set of result types that the given
    /// <see cref="Endpoint"/> route handler delegate delclares it can return.
    /// </summary>
    /// <param name="endpoint">The <see cref="Endpoint"/>.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <param name="resultTypes">The different result types the route handler delegate can return.</param>
    /// <returns></returns>
    protected static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services, params Type[] resultTypes)
    {
        var metadata = new List<object>();

        foreach (var resultType in resultTypes)
        {
            if (resultType.IsAssignableTo(typeof(IProvideEndpointResponseMetadata)))
            {
                metadata.AddRange(IProvideEndpointResponseMetadata.GetMetadataLateBound(resultType, endpoint, services));
            }
        }

        return metadata;
    }
}

/// <summary>
/// Represents the result of an <see cref="Endpoint"/> route handler delegate that can return two different <see cref="IResult"/> types.
/// </summary>
/// <typeparam name="TResult1">The first result type.</typeparam>
/// <typeparam name="TResult2">The second result type.</typeparam>
public sealed class Results<TResult1, TResult2> : ResultsBase, IProvideEndpointResponseMetadata
    where TResult1 : IResult
    where TResult2 : IResult
{

    private Results(IResult activeResult) : base(activeResult)
    {

    }

    /// <summary>
    /// Converts the <typeparamref name="TResult1"/> to a <see cref="Results{TResult1, TResult2}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2>(TResult1 result) => new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult2"/> to a <see cref="Results{TResult1, TResult2}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2>(TResult2 result) => new(result);

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="endpoint">The <see cref="Endpoint"/> to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services) => GetMetadata(endpoint, services, typeof(TResult1), typeof(TResult2));
}

/// <summary>
/// Represents the result of an <see cref="Endpoint"/> route handler delegate that can return three different <see cref="IResult"/> types.
/// </summary>
/// <typeparam name="TResult1">The first result type.</typeparam>
/// <typeparam name="TResult2">The second result type.</typeparam>
/// <typeparam name="TResult3">The third result type.</typeparam>
public sealed class Results<TResult1, TResult2, TResult3> : ResultsBase, IProvideEndpointResponseMetadata
    where TResult1 : IResult
    where TResult2 : IResult
    where TResult3 : IResult
{
    private Results(IResult activeResult) : base(activeResult)
    {

    }

    /// <summary>
    /// Converts the <typeparamref name="TResult1"/> to a <see cref="Results{TResult1, TResult2, TResult3}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3>(TResult1 result) => new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult2"/> to a <see cref="Results{TResult1, TResult2, TResult3}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3>(TResult2 result) => new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult3"/> to a <see cref="Results{TResult1, TResult2, TResult3}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3>(TResult3 result) => new(result);

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="endpoint">The <see cref="Endpoint"/> to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services) => GetMetadata(endpoint, services, typeof(TResult1), typeof(TResult2), typeof(TResult3));
}

/// <summary>
/// Represents the result of an <see cref="Endpoint"/> route handler delegate that can return four different <see cref="IResult"/> types.
/// </summary>
/// <typeparam name="TResult1">The first result type.</typeparam>
/// <typeparam name="TResult2">The second result type.</typeparam>
/// <typeparam name="TResult3">The third result type.</typeparam>
/// <typeparam name="TResult4">The fourth result type.</typeparam>
public sealed class Results<TResult1, TResult2, TResult3, TResult4> : ResultsBase, IProvideEndpointResponseMetadata
    where TResult1 : IResult
    where TResult2 : IResult
    where TResult3 : IResult
    where TResult4 : IResult
{
    private Results(IResult activeResult) : base(activeResult)
    {

    }

    /// <summary>
    /// Converts the <typeparamref name="TResult1"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4>(TResult1 result) => new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult2"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4>(TResult2 result) => new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult3"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4>(TResult3 result) => new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult4"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4>(TResult4 result) => new(result);

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="endpoint">The <see cref="Endpoint"/> to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services) => GetMetadata(endpoint, services, typeof(TResult1), typeof(TResult2), typeof(TResult3), typeof(TResult4));
}

/// <summary>
/// Represents the result of an <see cref="Endpoint"/> route handler delegate that can return five different <see cref="IResult"/> types.
/// </summary>
/// <typeparam name="TResult1">The first result type.</typeparam>
/// <typeparam name="TResult2">The second result type.</typeparam>
/// <typeparam name="TResult3">The third result type.</typeparam>
/// <typeparam name="TResult4">The fourth result type.</typeparam>
/// <typeparam name="TResult5">The fifth result type.</typeparam>
public sealed class Results<TResult1, TResult2, TResult3, TResult4, TResult5> : ResultsBase, IProvideEndpointResponseMetadata
    where TResult1 : IResult
    where TResult2 : IResult
    where TResult3 : IResult
    where TResult4 : IResult
    where TResult5 : IResult
{
    private Results(IResult activeResult) : base(activeResult)
    {

    }

    /// <summary>
    /// Converts the <typeparamref name="TResult1"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5>(TResult1 result) => new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult2"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5>(TResult2 result) => new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult3"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5>(TResult3 result) => new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult4"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5>(TResult4 result) => new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult5"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5>(TResult5 result) => new(result);

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="endpoint">The <see cref="Endpoint"/> to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services) => GetMetadata(endpoint, services, typeof(TResult1), typeof(TResult2), typeof(TResult3), typeof(TResult4));
}

/// <summary>
/// Represents the result of an <see cref="Endpoint"/> route handler delegate that can return six different <see cref="IResult"/> types.
/// </summary>
/// <typeparam name="TResult1">The first result type.</typeparam>
/// <typeparam name="TResult2">The second result type.</typeparam>
/// <typeparam name="TResult3">The third result type.</typeparam>
/// <typeparam name="TResult4">The fourth result type.</typeparam>
/// <typeparam name="TResult5">The fifth result type.</typeparam>
/// <typeparam name="TResult6">The sixth result type.</typeparam>
public sealed class Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6> : ResultsBase, IProvideEndpointResponseMetadata
    where TResult1 : IResult
    where TResult2 : IResult
    where TResult3 : IResult
    where TResult4 : IResult
    where TResult5 : IResult
    where TResult6 : IResult
{
    private Results(IResult activeResult) : base(activeResult)
    {

    }

    /// <summary>
    /// Converts the <typeparamref name="TResult1"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5, TResult6}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(TResult1 result) => new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult2"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5, TResult6}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(TResult2 result) => new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult3"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5, TResult6}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(TResult3 result) => new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult4"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5, TResult6}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(TResult4 result) => new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult5"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5, TResult6}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(TResult5 result) => new(result);

    /// <summary>
    /// Converts the <typeparamref name="TResult6"/> to a <see cref="Results{TResult1, TResult2, TResult3, TResult4, TResult5, TResult6}"/>.
    /// </summary>
    /// <param name="result">The result.</param>
    public static implicit operator Results<TResult1, TResult2, TResult3, TResult4, TResult5, TResult6>(TResult6 result) => new(result);

    /// <summary>
    /// Provides metadata for parameters to <see cref="Endpoint"/> route handler delegates.
    /// </summary>
    /// <param name="endpoint">The <see cref="Endpoint"/> to provide metadata for.</param>
    /// <param name="services">The <see cref="IServiceProvider"/>.</param>
    /// <returns>The metadata.</returns>
    public static IEnumerable<object> GetMetadata(Endpoint endpoint, IServiceProvider services) => GetMetadata(endpoint, services, typeof(TResult1), typeof(TResult2), typeof(TResult3), typeof(TResult4));
}
