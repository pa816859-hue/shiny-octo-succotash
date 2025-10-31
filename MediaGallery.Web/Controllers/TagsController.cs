using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaGallery.Web.Services;
using MediaGallery.Web.Services.Models;
using MediaGallery.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MediaGallery.Web.Controllers;

public class TagsController : Controller
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationHelper.DefaultPageSizeLimit,
        [FromQuery] long? userId = null,
        CancellationToken cancellationToken = default)
    {
        if (userId.HasValue && userId.Value <= 0)
        {
            return BadRequest(new { error = "User id must be greater than 0 when specified." });
        }

        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = PaginationHelper.ClampPageSize(pageSize);

        var viewModel = await _tagService
            .GetTagIndexAsync(normalizedPage, normalizedPageSize, userId, cancellationToken)
            .ConfigureAwait(false);

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> IndexData(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationHelper.DefaultPageSizeLimit,
        [FromQuery] long? userId = null,
        CancellationToken cancellationToken = default)
    {
        if (userId.HasValue && userId.Value <= 0)
        {
            return BadRequest(new { error = "User id must be greater than 0 when specified." });
        }

        if (!PaginationHelper.TryValidate(page, pageSize, out var errorMessage))
        {
            return BadRequest(new { error = errorMessage });
        }

        var normalizedPageSize = PaginationHelper.ClampPageSize(pageSize);

        var viewModel = await _tagService
            .GetTagIndexAsync(page, normalizedPageSize, userId, cancellationToken)
            .ConfigureAwait(false);

        var result = new PagedResult<PhotoTagSummary>(viewModel.Tags, viewModel.Pagination);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(
        [FromRoute] string tag,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationHelper.DefaultPageSizeLimit,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return BadRequest(new { error = "Tag must be provided." });
        }

        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = PaginationHelper.ClampPageSize(pageSize);

        try
        {
            var viewModel = await _tagService
                .GetTagDetailAsync(tag, normalizedPage, normalizedPageSize, cancellationToken)
                .ConfigureAwait(false);

            return View(viewModel);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<IActionResult> DetailData(
        [FromRoute] string tag,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationHelper.DefaultPageSizeLimit,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return BadRequest(new { error = "Tag must be provided." });
        }

        if (!PaginationHelper.TryValidate(page, pageSize, out var errorMessage))
        {
            return BadRequest(new { error = errorMessage });
        }

        var normalizedPageSize = PaginationHelper.ClampPageSize(pageSize);

        try
        {
            var viewModel = await _tagService
                .GetTagDetailAsync(tag, page, normalizedPageSize, cancellationToken)
                .ConfigureAwait(false);

            var photos = new PagedResult<TaggedPhoto>(viewModel.Photos, viewModel.Pagination);
            var result = new TagDetailDataResult(viewModel.Tag, photos);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
