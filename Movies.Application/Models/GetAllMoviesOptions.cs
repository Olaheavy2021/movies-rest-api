public class GetAllMoviesOptions
{
    public string? Title { get; set; }

    public int? YearOfRelease { get; set; }

    public Guid? UserId { get; set; }

    public string? SortField { get; set; }

    public SortOrder? SortOrder { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}

public enum SortOrder
{
    Unsorted,
    Ascending,
    Descending
}
