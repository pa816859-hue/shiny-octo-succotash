namespace MediaGallery.Web.Services.Models;

public sealed class VideoContributor
{
    public VideoContributor(long userId, string? username, string? firstName, string? lastName)
    {
        UserId = userId;
        Username = username;
        FirstName = firstName;
        LastName = lastName;
    }

    public long UserId { get; }

    public string? Username { get; }

    public string? FirstName { get; }

    public string? LastName { get; }

    public string DisplayName => DisplayNameFormatter.Build(UserId, Username, FirstName, LastName);
}
