namespace MediaGallery.Web.ViewModels;

public sealed class PhotoFeedViewModel
{
    public PhotoFeedViewModel(PhotoDisplayViewModel? currentPhoto)
    {
        CurrentPhoto = currentPhoto;
    }

    public PhotoDisplayViewModel? CurrentPhoto { get; }

    public bool HasPhoto => CurrentPhoto is not null;
}
