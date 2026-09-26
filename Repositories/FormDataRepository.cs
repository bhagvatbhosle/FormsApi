using FormsApi.Data;
using FormsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FormsApi.Repositories
{
    public class FormDataRepository : IFormDataRepository
    {
        private readonly AppDbContext _context;

        public FormDataRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<FormData> CreateAsync(FormData formdata, CancellationToken cancellationToken = default)
        {
            _context.FormData.Add(formdata);
            await _context.SaveChangesAsync(cancellationToken);
            return formdata;
        }

        public async Task<FormData?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.FormData.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        }

        public async Task<(List<FormData> items, int total)> ListAsync(int page, int pageSize, string subjectFilter)
        {
            var query = _context.FormData.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(subjectFilter))
            {
                query = query.Where(f => f.Subject.Contains(subjectFilter));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(f => f.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
