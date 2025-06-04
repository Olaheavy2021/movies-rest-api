namespace Movies.Contracts.Requests;

public class GetAllMoviesRequest : PagedRequest
{
    public required string? Title { get; set; }

    public required int? Year { get; set; }

    public required string? SortBy { get; init; }
}
