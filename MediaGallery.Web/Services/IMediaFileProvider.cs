namespace MediaGallery.Web.Services;

public interface IMediaFileProvider
{
    string RootDirectory { get; }

    string GetAbsolutePath(string relativePath);
}
