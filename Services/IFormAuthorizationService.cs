using FormsApi.Dtos;
using FormsApi.Models;

namespace FormsApi.Services
{
    public interface IFormAuthorizationService
    {
        Task<bool> UserCanViewAsync(ClaimsPrincipalLike user, FormData form, CancellationToken ct = default);

        Task<bool> UserCanModifyAsync(ClaimsPrincipalLike user, FormData form, CancellationToken ct = default);
    }
}
