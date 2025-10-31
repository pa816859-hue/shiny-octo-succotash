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
        this IEnumerable<MessageDto> messages,
        IReadOnlyDictionary<long, UserDto>? users,
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
            .Select(message =>
            {
                users?.TryGetValue(message.UserId, out var userDto);
                return message.ToRecentMessage(userDto);
            })
            .ToList();

        return new RecentMessagesViewModel(messageList, pagination);
    }

    public static UserProfileViewModel ToUserProfileViewModel(
        this UserDto user,
        IEnumerable<UserTagDto>? tags,
        PaginationMetadata pagination)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (pagination is null)
        {
            throw new ArgumentNullException(nameof(pagination));
        }

        var profile = user.ToUserProfile(tags);
        return new UserProfileViewModel(profile, pagination);
    }

    public static TagIndexViewModel ToTagIndexViewModel(
        this IEnumerable<PhotoTagDto> tags,
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
        this IEnumerable<(PhotoDto Photo, IEnumerable<PhotoTagDto> Tags, UserDto? User)> photos,
        string tag,
        PaginationMetadata pagination)
    {
        if (photos is null)
        {
            throw new ArgumentNullException(nameof(photos));
        }

        if (pagination is null)
        {
            throw new ArgumentNullException(nameof(pagination));
        }

        var taggedPhotos = photos
            .Select(entry =>
            {
                var tagSet = entry.Tags ?? Enumerable.Empty<PhotoTagDto>();
                return entry.Photo.ToTaggedPhoto(tagSet, entry.User);
            })
            .ToList();

        return new TagDetailViewModel(tag, taggedPhotos, pagination);
    }
}
