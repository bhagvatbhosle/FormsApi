
using Azure.Core;
using FormsApi.Data;
using FormsApi.Dtos;
using FormsApi.Models;
using FormsApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog.Core;

[ApiController]
[Route("api/[controller]")]
public class FormsController : Controller
{
    private readonly IFormDataRepository _formDataRepository;
    private readonly ILogger<FormsController> _logger;

    public FormsController(IFormDataRepository formDataRepository, ILogger<FormsController> logger)
    {
        _logger = logger;
        _formDataRepository = formDataRepository;
    }

    [HttpGet("{id:guid}")]

    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var formData = await _formDataRepository.GetByIdAsync(id, cancellationToken);

        if (formData == null)
        {
            _logger.LogWarning("Form data with ID {Id} not found.", id);
            return NotFound();
        }

        _logger.LogInformation("Retrieved form data with ID {Id}.", id);
        return Ok(formData);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFormRequest request)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var formdata = new FormData
        {
            Id = Guid.NewGuid(),
            Subject = request.Subject!,
            Description = request.Description,
            DueDate = request.DueDate,
            Priority = request.Priority,
            Critical = request.Critical,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "Bhagvat",
        };

        var created = await _formDataRepository.CreateAsync(formdata, CancellationToken.None);
        return Ok(created);
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] FormListQuery query)
    {
        var (items, total) = await _formDataRepository.ListAsync(query.Page, query.PageSize, query.SubjectFilter);

        var result = new PagedResult<FormResponse>(items.Select(FormResponse.FromEntity).ToList(), query.Page, query.PageSize, total);

        return Ok(result);
    }

    public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount)
    {
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
    }
}
