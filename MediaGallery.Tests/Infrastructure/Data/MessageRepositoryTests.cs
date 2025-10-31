using MediaGallery.Tests.Infrastructure.Data;
using MediaGallery.Web.Infrastructure.Data;
using Xunit;

namespace MediaGallery.Tests.Infrastructure.Data;

public class MessageRepositoryTests
{
    [Fact]
    public async Task GetMessagesAsync_NormalizesPaginationAndMapsNulls()
    {
        var sentDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        var reader = new FakeDbDataReader.Builder()
            .WithColumn("ChannelID", typeof(long))
            .WithColumn("MessageID", typeof(long))
            .WithColumn("UserID", typeof(long))
            .WithColumn("SentDate", typeof(DateTime))
            .WithColumn("MessageText", typeof(string))
            .WithColumn("PhotoID", typeof(long))
            .WithColumn("VideoID", typeof(long))
            .WithRow(42L, 1001L, 500L, sentDate, DBNull.Value, DBNull.Value, 900L)
            .Build();

        var executor = new FakeSqlCommandExecutor(new[] { reader });
        var repository = new MessageRepository(new FakeSqlConnectionFactory(), executor);

        var messages = await repository.GetMessagesAsync(42L, -10, 0, CancellationToken.None);

        Assert.Single(messages);
        var message = messages[0];
        Assert.Equal(42L, message.ChannelId);
        Assert.Equal(1001L, message.MessageId);
        Assert.Equal(500L, message.UserId);
        Assert.Equal(sentDate, message.SentDate);
        Assert.Null(message.MessageText);
        Assert.Null(message.PhotoId);
        Assert.Equal(900L, message.VideoId);

        Assert.Single(executor.CapturedParameters);
        var parameters = executor.CapturedParameters[0];
        Assert.Equal(42L, parameters["@ChannelId"]);
        Assert.Equal(0, parameters["@Offset"]);
        Assert.Equal(1, parameters["@PageSize"]);
    }
}
