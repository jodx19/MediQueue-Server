// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Application\Common\PagedResult.cs
using System;
using System.Collections.Generic;

namespace MediQueue.Application.Common;

/// <summary>
/// Represents a paginated result set in the application layer.
/// </summary>
public class PagedResult<T>
{
    public List<T> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;

    private PagedResult(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    public static PagedResult<T> Create(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        return new PagedResult<T>(items, totalCount, pageNumber, pageSize);
    }
}
