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
        
    }
}
