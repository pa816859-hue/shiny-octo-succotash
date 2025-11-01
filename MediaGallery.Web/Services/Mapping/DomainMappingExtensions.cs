using System;
using System.Collections.Generic;
using System.Linq;
using MediaGallery.Web.Infrastructure.Data.Dto;
using MediaGallery.Web.Services.Models;

namespace MediaGallery.Web.Services.Mapping;

public static class DomainMappingExtensions
{
    public static RecentMessage ToRecentMessage(this MessageDetailDto message)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        return new RecentMessage(
            message.ChannelId,
            message.MessageId,
            message.UserId,
            message.Username,
            message.FirstName,
            message.LastName,
            message.SentDate,
            message.MessageText,
            message.PhotoId,
            message.PhotoPath,
            message.VideoId,
            message.VideoPath);
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

    public static UserSummary ToUserSummary(this UserDto user)
    {
        if (user is null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        return new UserSummary(user.UserId, user.Username, user.FirstName, user.LastName, user.LastUpdate);
    }

    public static TaggedPhoto ToTaggedPhoto(this TagDetailDto detail)
    {
        if (detail is null)
        {
            throw new ArgumentNullException(nameof(detail));
        }

        var tags = new List<PhotoTag>
        {
            new(detail.Tag, detail.Score)
        };

        return new TaggedPhoto(
            detail.PhotoId,
            detail.PhotoPath,
            detail.PhotoAddedOn,
            tags,
            detail.ChannelId,
            detail.MessageId,
            detail.SentDate,
            detail.MessageText,
            detail.UserId,
            detail.Username,
            detail.FirstName,
            detail.LastName);
    }

    public static IReadOnlyList<PhotoTagSummary> ToPhotoTagSummaries(this IEnumerable<TagSummaryDto> tags, UserDto? user = null)
    {
        if (tags is null)
        {
            throw new ArgumentNullException(nameof(tags));
        }

        return tags
            .Select(tag => new PhotoTagSummary(
                tag.Tag,
                tag.PhotoCount,
                user?.UserId,
                user?.Username,
                user?.FirstName,
                user?.LastName))
            .OrderByDescending(summary => summary.PhotoCount)
            .ThenBy(summary => summary.Tag, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static DashboardSummary ToDashboardSummary(this DashboardSummaryDto summary)
    {
        if (summary is null)
        {
            throw new ArgumentNullException(nameof(summary));
        }

        return new DashboardSummary(
            summary.TotalMessages,
            summary.TotalPhotos,
            summary.TotalVideos,
            summary.ActiveChannels,
            summary.TotalUsers,
            summary.LastMessageSentAt);
    }
}
