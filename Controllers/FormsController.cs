using Azure.Core;
using FormsApi.Data;
using FormsApi.Dtos;
using FormsApi.Models;
using FormsApi.Repositories;
using FormsApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog.Core;
using System.Net;
using FormsApi.Extensions;

[ApiController]
[Route("api/[controller]")]
public class FormsController : Controller
{
    private readonly IFormDataRepository _formDataRepository;
    private readonly IFormAuthorizationService _authorization;
    private readonly ILogger<FormsController> _logger;

    public FormsController(IFormDataRepository formDataRepository, IFormAuthorizationService authorization, ILogger<FormsController> logger)
    {
        _logger = logger;
        _formDataRepository = formDataRepository;
        _authorization = authorization;
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FormResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var formData = await _formDataRepository.GetByIdAsync(id);

        if (formData == null)
        {
            _logger.LogWarning("Form data with ID {Id} not found.", id);
            return NotFound();
        }

        _logger.LogInformation("Retrieved form data with ID {Id}.", id);
        return Ok(formData);
    }

    [HttpPost]
    [ProducesResponseType(typeof(FormResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateFormRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var formdata = new FormData
        {
            Id = Guid.NewGuid(),
            Subject = request.Subject?.Sanitize()!,
            Description = request.Description?.Sanitize(),
            DueDate = request.DueDate,
            Priority = request.Priority,
            Critical = request.Critical,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = CurrentUserId(),
        };

        var created = await _formDataRepository.CreateAsync(formdata);
        return Ok(created);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<FormResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] FormListQuery query)
    {
        var (items, total) = await _formDataRepository.ListAsync(query.Page, query.PageSize, query.SubjectFilter?.Sanitize()!);

        var result = new PagedResult<FormResponse>(items.Select(FormResponse.FromEntity).ToList(), query.Page, query.PageSize, total);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(FormResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFormRequest request)
    {
        var existing = await _formDataRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return NotFound(new ApiError("Not Found", StatusCodes.Status404NotFound, $"Form '{id}' was not found."));
        }

        // Add authorization check here if needed, e.g., check if the current user is allowed to update this form.
        if (!await _authorization.UserCanModifyAsync(CurrentUser(), existing))
        {
            return StatusCode(StatusCodes.Status403Forbidden,
            new ApiError("Forbidden", StatusCodes.Status403Forbidden, "You do not have permission to access this form."));
        }

        if (string.IsNullOrWhiteSpace(request.RowVersion))
        {
            return BadRequest(new ApiError("Validation Failed", StatusCodes.Status400BadRequest,
                Errors: new Dictionary<string, string[]> { ["RowVersion"] = new[] { "RowVersion is required." } }));
        }

        byte[] expectedRowVersion;
        try
        {
            expectedRowVersion = Convert.FromBase64String(request.RowVersion!);
        }
        catch (FormatException)
        {
            return BadRequest(new ApiError("Validation Failed", StatusCodes.Status400BadRequest,
                Errors: new Dictionary<string, string[]> { ["RowVersion"] = new[] { "RowVersion must be a valid base64 value." } }));
        }

        var updated = await _formDataRepository.UpdateAsync(id, expectedRowVersion, entity =>
        {
            entity.Subject = request.Subject?.Sanitize()!;
            entity.Description = request.Description?.Sanitize();
            entity.DueDate = request.DueDate;
            entity.Priority = request.Priority;
            entity.Critical = request.Critical;
        });

        return Ok(FormResponse.FromEntity(updated));
    }
    
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _formDataRepository.GetByIdAsync(id);
        if (existing is null)
        {
            return NotFound($"Form '{id}' was not found.");
        }

        // Add authorization check here if needed, e.g., check if the current user is allowed to delete this form.
        if (!await _authorization.UserCanModifyAsync(CurrentUser(), existing))
        {
            return StatusCode(StatusCodes.Status403Forbidden,
             new ApiError("Forbidden", StatusCodes.Status403Forbidden, "You do not have permission to access this form."));
        }

        await _formDataRepository.SoftDeleteAsync(id);

        return NoContent();
    }

    public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount)
    {
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }

    private string CurrentUserId() => User?.Identity?.Name ?? "anonymous";

    private ClaimsPrincipalLike CurrentUser() =>
        new(CurrentUserId(), Array.Empty<string>());
}
