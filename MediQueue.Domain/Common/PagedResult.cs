// e:\ITI\MY-Projects\MediQueue EMR Clinic System\MediQueue.Server\MediQueue.Domain\Common\PagedResult.cs
using System.Collections.Generic;

namespace MediQueue.Domain.Common;

/// <summary>
/// Represents a paginated result set.
/// </summary>
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; }
    public int TotalCount { get; }
    public int PageNumber { get; }
    public int PageSize { get; }

    public PagedResult(IEnumerable<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
