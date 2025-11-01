using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaGallery.Web.Services;
using MediaGallery.Web.Services.Models;
using MediaGallery.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MediaGallery.Web.Controllers;

public class PhotosController : Controller
{
    private readonly IPhotoService _photoService;

    public PhotosController(IPhotoService photoService)
    {
        _photoService = photoService ?? throw new ArgumentNullException(nameof(photoService));
    }

    [HttpGet]
    public async Task<IActionResult> Feed(CancellationToken cancellationToken)
    {
        var nextPhoto = await _photoService.GetNextPhotoAsync(cancellationToken).ConfigureAwait(false);
        var viewModel = new PhotoFeedViewModel(ToViewModel(nextPhoto));
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Liked(CancellationToken cancellationToken)
    {
        var likedPhotos = await _photoService.GetLikedPhotosAsync(cancellationToken).ConfigureAwait(false);
        var likedViewModels = likedPhotos
            .Select(ToViewModel)
            .OfType<PhotoDisplayViewModel>()
            .ToList();

        return View(new PhotoLikedListViewModel(likedViewModels));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Next([FromBody] PhotoActionRequest request, CancellationToken cancellationToken)
    {
        if (request is null || request.PhotoId <= 0)
        {
            return BadRequest(new { error = "Photo id must be provided." });
        }

        var nextPhoto = await _photoService.SkipAsync(request.PhotoId, cancellationToken).ConfigureAwait(false);
        var viewModel = ToViewModel(nextPhoto);
        return Json(CreateResponse(viewModel));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Like([FromBody] PhotoActionRequest request, CancellationToken cancellationToken)
    {
        if (request is null || request.PhotoId <= 0)
        {
            return BadRequest(new { error = "Photo id must be provided." });
        }

        var nextPhoto = await _photoService.LikeAsync(request.PhotoId, cancellationToken).ConfigureAwait(false);
        var viewModel = ToViewModel(nextPhoto);
        return Json(CreateResponse(viewModel));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlike([FromBody] PhotoActionRequest request, CancellationToken cancellationToken)
    {
        if (request is null || request.PhotoId <= 0)
        {
            return BadRequest(new { error = "Photo id must be provided." });
        }

        var removed = await _photoService.RemoveLikeAsync(request.PhotoId, cancellationToken).ConfigureAwait(false);
        return Json(new { removed });
    }

    private static PhotoDisplayViewModel? ToViewModel(PhotoDisplayModel? model)
    {
        if (model is null)
        {
            return null;
        }

        return new PhotoDisplayViewModel(model.PhotoId, model.SourceUrl, model.AddedOn, model.IsLiked);
    }

    private static object CreateResponse(PhotoDisplayViewModel? model)
    {
        if (model is null)
        {
            return new { hasPhoto = false };
        }

        return new
        {
            hasPhoto = true,
            photo = new
            {
                photoId = model.PhotoId,
                sourceUrl = model.SourceUrl,
                addedOn = model.AddedOn.ToString("O"),
                isLiked = model.IsLiked
            }
        };
    }

    public sealed class PhotoActionRequest
    {
        public long PhotoId { get; set; }
    }
}
