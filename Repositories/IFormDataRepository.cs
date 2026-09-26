using FormsApi.Models;

namespace FormsApi.Repositories
{
    public interface IFormDataRepository
    {
        Task<FormData> CreateAsync(FormData formdata);
        Task<FormData?> GetByIdAsync(Guid id);
        Task<(List<FormData> items, int total)> ListAsync(int page, int pageSize, string subjectFilter);
        Task<FormData> UpdateAsync(Guid id, byte[] expectedRowVersion, Action<FormData> applyChanges);
        Task SoftDeleteAsync(Guid id);
    }
}
