using Monitoramento.Domain.Models.V1.Authorization;

namespace Monitoramento.Service.V1.Authorization
{
    public interface IApiAuthorizationService
    {
        Task<AuthorizationResponse> PostAuthorization(AuthorizationRequest value);
        Task<AuthorizationResponse> PostAuthorization(AuthorizationConfigurationRequest value);
    }
}
