using System.Collections.Generic;
using System.Linq;

namespace MediaGallery.Web.ViewModels;

public sealed class PhotoLikedListViewModel
{
    public PhotoLikedListViewModel(IEnumerable<PhotoDisplayViewModel> photos)
    {
        Photos = photos?.ToList() ?? new List<PhotoDisplayViewModel>();
    }

    public IReadOnlyList<PhotoDisplayViewModel> Photos { get; }

    public int TotalPhotos => Photos.Count;

    public bool HasPhotos => TotalPhotos > 0;
}
