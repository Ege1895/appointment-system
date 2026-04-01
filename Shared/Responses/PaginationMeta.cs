namespace Shared.Responses;

public class PaginationMeta
{
    public int Page { get; set; }

    public int PageSize { get; set; }

    public int TotalCount { get; set; }

    public int TotalPages { get; set; }

    public static PaginationMeta Create(int page, int pageSize, int totalCount)
    {
        int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        return new PaginationMeta
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }
}