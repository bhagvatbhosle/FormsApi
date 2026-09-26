using FormsApi.Data;
using FormsApi.Exceptions;
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

        public async Task<FormData> CreateAsync(FormData formdata)
        {
            _context.FormData.Add(formdata);
            await _context.SaveChangesAsync();
            return formdata;
        }

        public async Task<FormData?> GetByIdAsync(Guid id)
        {
            return await _context.FormData.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);
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

        public async Task<FormData> UpdateAsync(Guid id, byte[] expectedRowVersion, Action<FormData> applyChanges)
        {
            var entity = await _context.FormData.FirstOrDefaultAsync(f => f.Id == id)
                ?? throw new FormNotFoundException(id);

            _context.Entry(entity).Property(nameof(FormData.RowVersion)).OriginalValue = expectedRowVersion;

            applyChanges(entity);
            entity.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new FormConcurrencyException(id);
            }

            return entity;
        }

        public async Task SoftDeleteAsync(Guid id)
        {
            var entity = await _context.FormData.FirstOrDefaultAsync(f => f.Id == id)
                ?? throw new FormNotFoundException(id);

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
    }
}
