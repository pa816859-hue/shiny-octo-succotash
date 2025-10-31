using MediaGallery.Web.Infrastructure.Data;
using Xunit;

namespace MediaGallery.Tests.Infrastructure.Data;

public class UserRepositoryTests
{
    [Fact]
    public async Task GetUsersAsync_ClampsPageSizeAndHandlesNullableNames()
    {
        var lastUpdate = new DateTime(2024, 2, 1, 8, 0, 0, DateTimeKind.Utc);

        var reader = new FakeDbDataReader.Builder()
            .WithColumn("UserID", typeof(long))
            .WithColumn("LastUpdate", typeof(DateTime))
            .WithColumn("FirstName", typeof(string))
            .WithColumn("LastName", typeof(string))
            .WithColumn("Username", typeof(string))
            .WithRow(777L, lastUpdate, DBNull.Value, "Doe", DBNull.Value)
            .Build();

        var executor = new FakeSqlCommandExecutor(new[] { reader });
        var repository = new UserRepository(new FakeSqlConnectionFactory(), executor);

        var users = await repository.GetUsersAsync(25, 1_000, CancellationToken.None);

        Assert.Single(users);
        var user = users[0];
        Assert.Equal(777L, user.UserId);
        Assert.Equal(lastUpdate, user.LastUpdate);
        Assert.Null(user.FirstName);
        Assert.Equal("Doe", user.LastName);
        Assert.Null(user.Username);

        Assert.Single(executor.CapturedParameters);
        var parameters = executor.CapturedParameters[0];
        Assert.Equal(25, parameters["@Offset"]);
        Assert.Equal(500, parameters["@PageSize"]);
    }
}
