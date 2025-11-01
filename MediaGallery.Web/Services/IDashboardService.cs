using MediaGallery.Web.ViewModels;

namespace MediaGallery.Web.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default);
}
