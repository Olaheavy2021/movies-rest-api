using FluentValidation;
using Movies.Application.Models;
using Movies.Application.Repositories;
using Movies.Application.Services;

namespace Movies.Application.Validators;

public class MovieValidators : AbstractValidator<Movie>
{
    private readonly IMovieRepository _movieRepository;
    public MovieValidators(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;

        RuleFor(movie => movie.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(movie => movie.Genres)
            .NotEmpty().WithMessage("At least one genre is required.");

        RuleFor(movie => movie.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

        RuleFor(movie => movie.YearOfRelease)
            .LessThanOrEqualTo(DateTime.Now.Year).WithMessage("Year of release cannot be in the future.");

        RuleFor(movie => movie.Slug)
            .MustAsync(ValidateSlug)
            .WithMessage("The movie already exists in the system");
    }

    private async Task<bool> ValidateSlug(Movie movie, string slug, CancellationToken cancellationToken)
    {
        var existingMovie = await _movieRepository.GetBySlugAsync(slug);

        if (existingMovie is not null )
        {
            return existingMovie.Id == movie.Id;
        }

        return existingMovie is null;
    }
}
