namespace Movies.Contracts.Requests;

public class UpdateMovieRequest
{
    public required string Title { get; init; }

    public required int YearOfRelease { get; init; }

    public IEnumerable<string> Genres { get; init; } = [];
}
