using System;
using MediaGallery.Tests.Infrastructure.Data;
using MediaGallery.Web.Infrastructure.Data;
using Xunit;

namespace MediaGallery.Tests.Infrastructure.Data;

public class MessageRepositoryTests
{
    [Fact]
    public async Task GetRecentMessagesAsync_NormalizesPaginationAndMapsNulls()
    {
        var sentDate = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);

        var reader = new FakeDbDataReader.Builder()
            .WithColumn("ChannelID", typeof(long))
            .WithColumn("MessageID", typeof(long))
            .WithColumn("UserID", typeof(long))
            .WithColumn("Username", typeof(string))
            .WithColumn("FirstName", typeof(string))
            .WithColumn("LastName", typeof(string))
            .WithColumn("SentDate", typeof(DateTime))
            .WithColumn("MessageText", typeof(string))
            .WithColumn("PhotoID", typeof(long))
            .WithColumn("PhotoPath", typeof(string))
            .WithColumn("VideoID", typeof(long))
            .WithColumn("VideoPath", typeof(string))
            .WithRow(42L, 1001L, 500L, "handle", "Jane", "Doe", sentDate, DBNull.Value, 55L, "photo.jpg", DBNull.Value, DBNull.Value)
            .Build();

        var executor = new FakeSqlCommandExecutor(new[] { reader });
        var repository = new MessageRepository(new FakeSqlConnectionFactory(), executor);

        var messages = await repository.GetRecentMessagesAsync(-10, 0, 42L, 500L, sortAscending: false, mediaOnly: true, CancellationToken.None);

        Assert.Single(messages);
        var message = messages[0];
        Assert.Equal(42L, message.ChannelId);
        Assert.Equal(1001L, message.MessageId);
        Assert.Equal(500L, message.UserId);
        Assert.Equal("handle", message.Username);
        Assert.Equal("Jane", message.FirstName);
        Assert.Equal("Doe", message.LastName);
        Assert.Equal(sentDate, message.SentDate);
        Assert.Null(message.MessageText);
        Assert.Equal(55L, message.PhotoId);
        Assert.Equal("photo.jpg", message.PhotoPath);
        Assert.Null(message.VideoId);
        Assert.Null(message.VideoPath);

        Assert.Single(executor.CapturedParameters);
        var parameters = executor.CapturedParameters[0];
        Assert.Equal(0, parameters["@Offset"]);
        Assert.Equal(1, parameters["@Limit"]);
        Assert.Equal(42L, parameters["@ChannelId"]);
        Assert.Equal(500L, parameters["@UserId"]);
        Assert.Equal(1, parameters["@MediaOnly"]);
        Assert.Equal(0, parameters["@SortAscending"]);
    }
}
