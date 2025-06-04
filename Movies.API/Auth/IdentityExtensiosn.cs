namespace Movies.API.Auth;

public static class IdentityExtensiosn
{
    public static Guid? GetUserId(this HttpContext context)
    {
        var userId = context.User.Claims.SingleOrDefault(x => x.Type ==AuthConstants.UserIdClaimName);

        if(Guid.TryParse(userId?.Value, out var parseId))
        {
            return parseId;
        }

        return null;
    }
}
