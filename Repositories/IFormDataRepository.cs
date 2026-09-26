using FormsApi.Models;

namespace FormsApi.Repositories
{
    public interface IFormDataRepository
    {
        Task<FormData> CreateAsync(FormData formdata, CancellationToken cancellationToken = default);
        Task<FormData?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<(List<FormData> items, int total)> ListAsync(int page, int pageSize, string subjectFilter);
    }
}
