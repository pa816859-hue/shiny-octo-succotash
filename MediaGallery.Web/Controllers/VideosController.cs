using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaGallery.Web.Services;
using MediaGallery.Web.Services.Models;
using MediaGallery.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MediaGallery.Web.Controllers;

public class VideosController : Controller
{
    private readonly IVideoService _videoService;

    public VideosController(IVideoService videoService)
    {
        _videoService = videoService ?? throw new ArgumentNullException(nameof(videoService));
    }

    [HttpGet]
    public async Task<IActionResult> Watch(CancellationToken cancellationToken)
    {
        var nextVideo = await _videoService.GetNextVideoAsync(cancellationToken).ConfigureAwait(false);
        var viewModel = new VideoWatchViewModel(ToViewModel(nextVideo));
        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Liked(CancellationToken cancellationToken)
    {
        var likedVideos = await _videoService.GetLikedVideosAsync(cancellationToken).ConfigureAwait(false);
        var likedViewModels = likedVideos
            .Select(ToViewModel)
            .OfType<VideoPlaybackViewModel>()
            .ToList();

        return View(new VideoLikedListViewModel(likedViewModels));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Next([FromBody] VideoActionRequest request, CancellationToken cancellationToken)
    {
        if (request is null || request.VideoId <= 0)
        {
            return BadRequest(new { error = "Video id must be provided." });
        }

        var nextVideo = await _videoService.SkipAsync(request.VideoId, cancellationToken).ConfigureAwait(false);
        var viewModel = ToViewModel(nextVideo);
        return Json(CreateResponse(viewModel));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Like([FromBody] VideoActionRequest request, CancellationToken cancellationToken)
    {
        if (request is null || request.VideoId <= 0)
        {
            return BadRequest(new { error = "Video id must be provided." });
        }

        var nextVideo = await _videoService.LikeAsync(request.VideoId, cancellationToken).ConfigureAwait(false);
        var viewModel = ToViewModel(nextVideo);
        return Json(CreateResponse(viewModel));
    }

    private static VideoPlaybackViewModel? ToViewModel(VideoPlaybackModel? model)
    {
        if (model is null)
        {
            return null;
        }

        return new VideoPlaybackViewModel(model.VideoId, model.SourceUrl, model.AddedOn, model.IsLiked);
    }

    private static object CreateResponse(VideoPlaybackViewModel? model)
    {
        if (model is null)
        {
            return new { hasVideo = false };
        }

        return new
        {
            hasVideo = true,
            video = new
            {
                videoId = model.VideoId,
                sourceUrl = model.SourceUrl,
                addedOn = model.AddedOn.ToString("O"),
                isLiked = model.IsLiked
            }
        };
    }

    public sealed class VideoActionRequest
    {
        public long VideoId { get; set; }
    }
}
