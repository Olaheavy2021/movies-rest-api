using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Movies.API.Auth;
using Movies.API.Mapping;
using Movies.Application.Services;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.API.Controllers;


[ApiController]
[ApiVersion(1.0)]
public class MoviesController(IMovieService movieService, IOutputCacheStore outputCacheStore) : ControllerBase
{
    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    [HttpPost(ApiEndpoints.Movies.Create)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAsync([FromBody] CreateMovieRequest request, CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie();
        await movieService.CreateAsync(movie, cancellationToken);
        await outputCacheStore.EvictByTagAsync("movies", cancellationToken);
        return CreatedAtAction(nameof(GetAsync), new { idOrSlug = movie.Id.ToString() }, movie.MapToResponse());
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    [OutputCache(PolicyName = "MovieCache")]
    [ActionName(nameof(GetAsync))]
    //[ResponseCache(Duration = 30, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAsync([FromRoute] string idOrSlug, [FromServices] LinkGenerator linkGenerator, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();

        var movie = Guid.TryParse(idOrSlug, out var id) 
            ? await movieService.GetByIdAsync(id,userId, cancellationToken) : 
            await movieService.GetBySlugAsync(idOrSlug,userId,cancellationToken);

        if (movie is null)
        {
            return NotFound();
        }

        var response = movie.MapToResponse();
        var movieObj = new { id = movie.Id };

        response.Links.Add(new Link
        {
            Rel = "self",
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(GetAsync),values: new {idOrSlug = movie.Id})!,
            Type = "GET"
        });

        response.Links.Add(new Link
        {
            Rel = "self",
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(UpdateAsync), values: movieObj)!,
            Type = "PUT"
        });

        response.Links.Add(new Link
        {
            Rel = "self",
            Href = linkGenerator.GetPathByAction(HttpContext, nameof(DeleteAsync), values: movieObj)!,
            Type = "DELETE"
        });

        return Ok(response);
    }

    [HttpGet(ApiEndpoints.Movies.GetAll)]
    [OutputCache(PolicyName = "MovieCache")]
    //[ResponseCache(Duration = 30, VaryByQueryKeys = new []{"title", "year", "sortBy", "page", "pageSize"}, VaryByHeader = "Accept, Accept-Encoding", Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(typeof(MoviesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAsync([FromQuery]GetAllMoviesRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var options = request.MapToOptions()
            .WithUserId(userId);

        var movies = await movieService.GetAllAsync(options, cancellationToken);
        var movieCount = await movieService.GetCountAsync(request.Title, request.Year, cancellationToken);
        return Ok(movies.MapToResponse(request.Page, request.PageSize, movieCount));
    }

    [Authorize(AuthConstants.TrustedMemberPolicyName)]
    [HttpPut(ApiEndpoints.Movies.Update)]
    [ActionName(nameof(UpdateAsync))]
    [ProducesResponseType(typeof(MovieResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationFailureResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] UpdateMovieRequest request, CancellationToken cancellationToken)
    {
        var movie = request.MapToMovie(id);
        var userId = HttpContext.GetUserId();

        var updatedMovie = await movieService.UpdateAsync(movie, userId, cancellationToken);
        if (updatedMovie is null)
        {
            return NotFound();
        }

        await outputCacheStore.EvictByTagAsync("movies", cancellationToken);
        var response = movie.MapToResponse();
        return Ok(response);
    }

    [Authorize(AuthConstants.AdminUserPolicyName)]
    [HttpDelete(ApiEndpoints.Movies.Delete)]
    [ActionName(nameof(DeleteAsync))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var deleted = await movieService.DeleteByIdAsync(id, cancellationToken);
        if (!deleted)
        {
            return NotFound();
        }

        await outputCacheStore.EvictByTagAsync("movies", cancellationToken);
        return NoContent();
    }
}
