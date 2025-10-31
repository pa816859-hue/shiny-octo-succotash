using MediaGallery.Web.ViewModels;

namespace MediaGallery.Web.Services;

public interface ITagService
{
    Task<TagIndexViewModel> GetTagIndexAsync(
        int pageNumber,
        int pageSize,
        long? userId,
        CancellationToken cancellationToken = default);

    Task<TagDetailViewModel> GetTagDetailAsync(
        string tag,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
