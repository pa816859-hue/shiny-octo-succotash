using System;
using System.Collections.Generic;
using System.Linq;
using MediaGallery.Web.Infrastructure.Data.Dto;
using MediaGallery.Web.Services.Models;

namespace MediaGallery.Web.Services.Mapping;

public static class DomainMappingExtensions
{
    public static RecentMessage ToRecentMessage(this MessageDto message, UserDto? user = null)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var sourceUserId = user?.UserId ?? message.UserId;

        return new RecentMessage(
            message.ChannelId,
            message.MessageId,
            sourceUserId,
            user?.Username,
            user?.FirstName,
            user?.LastName,
            message.SentDate,
            message.MessageText,
            message.PhotoId,
            message.VideoId);
    }

    public static UserProfile ToUserProfile(this UserDto user, IEnumerable<UserTagDto>? tags = null)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        var tagList = tags?.Select(tag => new UserTag(tag.Tag, tag.Weight)).ToList()
                       ?? new List<UserTag>();

        return new UserProfile(
            user.UserId,
            user.Username,
            user.FirstName,
            user.LastName,
            user.LastUpdate,
            tagList);
    }

    public static TaggedPhoto ToTaggedPhoto(this PhotoDto photo, IEnumerable<PhotoTagDto> tags, UserDto? user = null)
    {
        if (photo is null)
        {
            throw new ArgumentNullException(nameof(photo));
        }

        if (tags is null)
        {
            throw new ArgumentNullException(nameof(tags));
        }

        var photoTags = tags
            .Where(tag => tag.PhotoId == photo.PhotoId)
            .Select(tag => new PhotoTag(tag.Tag, tag.Score))
            .OrderByDescending(tag => tag.Score)
            .ThenBy(tag => tag.Tag, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new TaggedPhoto(
            photo.PhotoId,
            photo.FilePath,
            photo.AddedOn,
            photoTags,
            user?.UserId,
            user?.Username,
            user?.FirstName,
            user?.LastName);
    }

    public static IReadOnlyList<PhotoTagSummary> ToPhotoTagSummaries(this IEnumerable<PhotoTagDto> tags, UserDto? user = null)
    {
        if (tags is null)
        {
            throw new ArgumentNullException(nameof(tags));
        }

        var summaries = tags
            .GroupBy(tag => tag.Tag, StringComparer.OrdinalIgnoreCase)
            .Select(group => new PhotoTagSummary(
                group.Key,
                group.Select(item => item.PhotoId).Distinct().Count(),
                user?.UserId,
                user?.Username,
                user?.FirstName,
                user?.LastName))
            .OrderByDescending(summary => summary.PhotoCount)
            .ThenBy(summary => summary.Tag, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return summaries;
    }
}
