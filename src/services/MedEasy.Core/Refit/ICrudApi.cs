namespace MedEasy.Core.Refit;

using global::Refit;

using MedEasy.Models;

using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Basic CRUD API 
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TResource"></typeparam>
public interface ICrudApi<TKey, TResource>
    where TKey : IEquatable<TKey>
    where TResource: class
{
    /// <summary>
    /// Gets a page of results
    /// </summary>
    /// <returns></returns>
    [Head("{key}")]
    [Get("?page={page}&pageSize={pageSize}")]
    Task<ApiResponse<PageModel<TResource>>> ReadPage(int page, int pageSize, CancellationToken ct = default);

    /// <summary>
    /// Gets a page of results
    /// </summary>
    /// <returns></returns>
    [Get("{key}")]
    [Head("{key}")]
    Task<ApiResponse<TResource>> Read(TKey key, CancellationToken ct = default);



}
