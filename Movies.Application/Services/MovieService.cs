using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;

namespace Movies.Application.Services;

public class MovieService(IMovieRepository movieRepository,IRatingRepository ratingRepository, IValidator<Movie> movieValidator, IValidator<GetAllMoviesOptions> optionsValidator) : IMovieService
{
    public  async Task<bool> CreateAsync(Movie movie, CancellationToken cancellationToken)
    {
        await movieValidator.ValidateAndThrowAsync(movie, cancellationToken);
        return await movieRepository.CreateAsync(movie, cancellationToken);
    }

    public Task<bool> DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return movieRepository.DeleteByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<Movie>> GetAllAsync(GetAllMoviesOptions options, CancellationToken cancellationToken)
    {
        await optionsValidator.ValidateAndThrowAsync(options, cancellationToken: cancellationToken);
        return await movieRepository.GetAllAsync(options, cancellationToken);
    }

    public Task<Movie?> GetByIdAsync(Guid id, Guid? userId, CancellationToken cancellationToken)
    {
        return movieRepository.GetByIdAsync(id,userId, cancellationToken);
    }

    public Task<Movie?> GetBySlugAsync(string slug, Guid? userId, CancellationToken cancellationToken)
    {
        return movieRepository.GetBySlugAsync(slug, userId, cancellationToken);
    }

    public async Task<Movie?> UpdateAsync(Movie movie, Guid? userId, CancellationToken cancellationToken)
    {
        await movieValidator.ValidateAndThrowAsync(movie, cancellationToken: cancellationToken);
        var movieExists = await movieRepository.ExistsByIdAsync(movie.Id, cancellationToken);
        if (!movieExists)
        {
            return null;
        }

        await movieRepository.UpdateAsync(movie, cancellationToken);

        if (!userId.HasValue)
        {
            var rating = await ratingRepository.GetRatingAsync(movie.Id, cancellationToken);
            movie.Rating = rating;
            return movie;
        }

        var ratings = await ratingRepository.GetRatingAsync(movie.Id, userId.Value, cancellationToken);
        movie.Rating = ratings.Rating;
        movie.UserRating = ratings.UserRating;
        return movie;
    }

    public Task<int> GetCountAsync(string? title, int? yearOfRelease, CancellationToken token = default)
    {
        return movieRepository.GetCountAsync(title, yearOfRelease, token);
    }
}
