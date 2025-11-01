using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaGallery.Web.Services;
using MediaGallery.Web.Services.Models;
using MediaGallery.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace MediaGallery.Web.Controllers;

public class UsersController : Controller
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService ?? throw new ArgumentNullException(nameof(userService));
    }

    [HttpGet]
    public async Task<IActionResult> Profile(
        [FromRoute] long id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationHelper.DefaultPageSizeLimit,
        [FromQuery] long? channelId = null,
        [FromQuery] bool mediaOnly = false,
        [FromQuery] MessageSortOrder sortOrder = MessageSortOrder.NewestFirst,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest(new { error = "User id must be greater than 0." });
        }

        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = PaginationHelper.ClampPageSize(pageSize);

        try
        {
            var profile = await _userService
                .GetUserProfileAsync(id, normalizedPage, normalizedPageSize, channelId, mediaOnly, sortOrder, cancellationToken)
                .ConfigureAwait(false);

            var query = new MessageQueryViewModel(normalizedPage, normalizedPageSize, channelId, id, mediaOnly, sortOrder);
            var viewModel = new UserProfilePageViewModel(profile, query);

            return View(viewModel);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    public async Task<IActionResult> ProfileData(
        [FromRoute] long id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationHelper.DefaultPageSizeLimit,
        [FromQuery] long? channelId = null,
        [FromQuery] bool mediaOnly = false,
        [FromQuery] MessageSortOrder sortOrder = MessageSortOrder.NewestFirst,
        CancellationToken cancellationToken = default)
    {
        if (id <= 0)
        {
            return BadRequest(new { error = "User id must be greater than 0." });
        }

        if (!PaginationHelper.TryValidate(page, pageSize, out var errorMessage))
        {
            return BadRequest(new { error = errorMessage });
        }

        var normalizedPageSize = PaginationHelper.ClampPageSize(pageSize);

        try
        {
            var viewModel = await _userService
                .GetUserProfileAsync(id, page, normalizedPageSize, channelId, mediaOnly, sortOrder, cancellationToken)
                .ConfigureAwait(false);

            var messages = new PagedResult<RecentMessage>(viewModel.Messages, viewModel.Pagination);
            var result = new UserProfileDataResult(viewModel.User, messages);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
