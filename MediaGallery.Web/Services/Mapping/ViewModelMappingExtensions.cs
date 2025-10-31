using System;
using System.Collections.Generic;
using System.Linq;
using MediaGallery.Web.Infrastructure.Data.Dto;
using MediaGallery.Web.Services.Models;
using MediaGallery.Web.ViewModels;

namespace MediaGallery.Web.Services.Mapping;

public static class ViewModelMappingExtensions
{
    public static RecentMessagesViewModel ToRecentMessagesViewModel(
        this IEnumerable<MessageDetailDto> messages,
        PaginationMetadata pagination)
    {
        if (messages is null)
        {
            throw new ArgumentNullException(nameof(messages));
        }

        if (pagination is null)
        {
            throw new ArgumentNullException(nameof(pagination));
        }

        var messageList = messages
            .Select(message => message.ToRecentMessage())
            .ToList();

        return new RecentMessagesViewModel(messageList, pagination);
    }

    public static UserProfileViewModel ToUserProfileViewModel(
        this UserDto user,
        IEnumerable<UserTagDto>? tags,
        IEnumerable<MessageDetailDto> messages,
        PaginationMetadata pagination)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (messages is null)
        {
            throw new ArgumentNullException(nameof(messages));
        }

        if (pagination is null)
        {
            throw new ArgumentNullException(nameof(pagination));
        }

        var profile = user.ToUserProfile(tags);
        var messageList = messages.Select(message => message.ToRecentMessage()).ToList();
        return new UserProfileViewModel(profile, messageList, pagination);
    }

    public static TagIndexViewModel ToTagIndexViewModel(
        this IEnumerable<TagSummaryDto> tags,
        UserDto? user,
        PaginationMetadata pagination)
    {
        if (tags is null)
        {
            throw new ArgumentNullException(nameof(tags));
        }

        if (pagination is null)
        {
            throw new ArgumentNullException(nameof(pagination));
        }

        var summaries = tags.ToPhotoTagSummaries(user);
        return new TagIndexViewModel(summaries, pagination);
    }

    public static TagDetailViewModel ToTagDetailViewModel(
        this IEnumerable<TagDetailDto> details,
        string tag,
        PaginationMetadata pagination)
    {
        if (details is null)
        {
            throw new ArgumentNullException(nameof(details));
        }

        if (pagination is null)
        {
            throw new ArgumentNullException(nameof(pagination));
        }

        var taggedPhotos = details
            .Select(detail => detail.ToTaggedPhoto())
            .ToList();

        return new TagDetailViewModel(tag, taggedPhotos, pagination);
    }
}
