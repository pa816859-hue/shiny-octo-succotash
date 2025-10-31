using System;
using System.Collections.Generic;
using System.Linq;
using MediaGallery.Web.Infrastructure.Data;
using MediaGallery.Web.Services.Mapping;
using MediaGallery.Web.Services.Models;
using MediaGallery.Web.ViewModels;

namespace MediaGallery.Web.Services;

public class UserService : IUserService
{
    private const int DefaultTagPageSize = 100;

    private readonly IUserRepository _userRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IMessageRepository _messageRepository;

    public UserService(
        IUserRepository userRepository,
        ITagRepository tagRepository,
        IMessageRepository messageRepository)
    {
        _userRepository = userRepository;
        _tagRepository = tagRepository;
        _messageRepository = messageRepository;
    }

    public async Task<UserProfileViewModel> GetUserProfileAsync(
        long userId,
        int pageNumber,
        int pageSize,
        long? channelId,
        bool mediaOnly,
        MessageSortOrder sortOrder,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageNumber));
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        var user = await _userRepository.GetUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);
        if (user is null)
        {
            throw new KeyNotFoundException($"User {userId} was not found.");
        }

        var tags = await _tagRepository
            .GetUserTagsAsync(userId, 0, DefaultTagPageSize, cancellationToken)
            .ConfigureAwait(false);

        var offset = (pageNumber - 1) * pageSize;
        var fetchLimit = checked(pageSize + 1);
        var sortAscending = sortOrder == MessageSortOrder.OldestFirst;

        var messages = await _messageRepository
            .GetRecentMessagesAsync(offset, fetchLimit, channelId, userId, sortAscending, mediaOnly, cancellationToken)
            .ConfigureAwait(false);

        var hasNextPage = messages.Count > pageSize;
        var trimmedMessages = hasNextPage ? messages.Take(pageSize).ToList() : messages.ToList();
        var pagination = new PaginationMetadata(pageNumber, pageSize, hasNextPage, pageNumber > 1);

        return user.ToUserProfileViewModel(tags, trimmedMessages, pagination);
    }
}
