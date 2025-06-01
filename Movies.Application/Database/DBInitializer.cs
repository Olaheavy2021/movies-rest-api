using Dapper;

namespace Movies.Application.Database;

public class DBInitializer
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    public DBInitializer(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task InitializeAsync()
    {
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await connection.ExecuteAsync("""
             CREATE TABLE IF NOT EXISTS movies (
                id UUID primary key,
                slug TEXT not null,
                title TEXT not null,
                yearofrelease integer not null
            );
            """);

        await connection.ExecuteAsync("""
            CREATE UNIQUE INDEX CONCURRENTLY IF NOT EXISTS
            movies_slug_idx ON movies
            using btree(slug);
            """);

        await connection.ExecuteAsync("""
             CREATE TABLE IF NOT EXISTS genres (
              movieId UUID references movies(Id),
              name TEXT not null
            );
            """);
    }
}
