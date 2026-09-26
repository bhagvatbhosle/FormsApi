using FormsApi.Dtos;
using FormsApi.Models;
using FormsApi.Services;

namespace FormsApi.Implementations
{
    public class StubFormAuthorizationService : IFormAuthorizationService
    {
        public Task<bool> UserCanViewAsync(ClaimsPrincipalLike user, FormData form, CancellationToken ct = default)
            => Task.FromResult(true);

        public Task<bool> UserCanModifyAsync(ClaimsPrincipalLike user, FormData form, CancellationToken ct = default)
            => Task.FromResult(true);
    }
}
