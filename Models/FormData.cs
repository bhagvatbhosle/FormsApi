using System.ComponentModel.DataAnnotations;

namespace FormsApi.Models;

public class FormData : IValidatableObject
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Subject is required.")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Subject must be 1-200 characters.")]
    public string Subject { get; set; } = string.Empty;

    [StringLength(1024, ErrorMessage = "Description must be 1024 characters or fewer.")]
    public string? Description { get; set; }

    [DataType(DataType.Date)]
    public DateTime? DueDate { get; set; }

    [Range(1, 10, ErrorMessage = "Priority must be an integer between 1 and 10.")]
    public int? Priority { get; set; }

    public bool? Critical { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [Required(ErrorMessage = "CreatedBy is required.")]
    [StringLength(256, ErrorMessage = "CreatedBy must be 256 characters or fewer.")]
    public string CreatedBy { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DueDate.HasValue && DueDate.Value <= DateTime.UtcNow)
        {
            yield return new ValidationResult(
                "DueDate must be a valid date in the future.",
                new[] { nameof(DueDate) });
        }
    }

    /// <summary>
    /// Concurrency token (SQL Server ROWVERSION). EF Core uses this to detect
    /// lost updates on PUT/DELETE via optimistic concurrency.
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}
