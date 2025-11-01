using System;
namespace MediaGallery.Web.ViewModels;

public sealed class HomeDashboardViewModel
{
    public HomeDashboardViewModel(
        RecentMessagesViewModel recentMedia,
        TagIndexViewModel tagSummaries,
        DateTime generatedOn)
    {
        RecentMedia = recentMedia ?? throw new ArgumentNullException(nameof(recentMedia));
        TagSummaries = tagSummaries ?? throw new ArgumentNullException(nameof(tagSummaries));
        GeneratedOn = generatedOn;
    }

    public RecentMessagesViewModel RecentMedia { get; }

    public TagIndexViewModel TagSummaries { get; }

    public DateTime GeneratedOn { get; }
}
