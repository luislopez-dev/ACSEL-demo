namespace AcselDemo.Dtos.Common;

public class PaginationDto
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }
    public string Controller { get; init; } = string.Empty;
    public string Action { get; init; } = "Index";
}
