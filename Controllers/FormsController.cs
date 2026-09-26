
using FormsApi.Data;
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

    [HttpGet]

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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([FromBody] FormData formdata)
    {
        if (ModelState.IsValid)
        {
            var created = _formDataRepository.CreateAsync(formdata, cancellationToken: default);

            return Ok(created);
        }
        return View(formdata);
    }
}
