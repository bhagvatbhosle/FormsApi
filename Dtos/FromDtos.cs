namespace FormsApi.Dtos
{
    public record CreateFormRequest(string Subject, string? Description, DateTime? DueDate, int? Priority, bool? Critical);

    public record UpdateFormRequest(string Subject, string? Description, DateTime? DueDate, int? Priority, bool? Critical, string RowVersion);

    public record FormListQuery(int Page = 1, int PageSize = 20, string? SubjectFilter = null);

    public record FormResponse(
        Guid Id,
        string Subject,
        string? Description,
        DateTime? DueDate,
        int? Priority,
        bool? Critical,
        DateTime CreatedAt,
        DateTime? UpdatedAt,
        string CreatedBy,
        string RowVersion
    )
    {
        public static FormResponse FromEntity(Models.FormData f) => new(
            f.Id, f.Subject, f.Description, f.DueDate, f.Priority, f.Critical,
            f.CreatedAt, f.UpdatedAt, f.CreatedBy, f.RowVersion is null ? string.Empty : Convert.ToBase64String(f.RowVersion));
    }

    public record ApiError(string Title, int Status, string? Detail = null, IDictionary<string, string[]>? Errors = null);
}
