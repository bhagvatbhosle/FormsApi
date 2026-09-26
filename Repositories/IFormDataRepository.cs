using FormsApi.Models;

namespace FormsApi.Repositories
{
    public interface IFormDataRepository
    {
        Task<FormData> CreateAsync(FormData formdata, CancellationToken cancellationToken = default);
        Task<FormData?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
