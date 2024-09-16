using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Monitoramento.Domain.Models.V1.Authorization;
using Monitoramento.Service.V1.Authorization;
using System.Threading.Tasks;

namespace Monitoramento_Api.Controllers.V1
{
    [AllowAnonymous]
    [ApiController]
    [ApiVersion("1")]
    [Route("api/v{version:apiVersion}/authorization")]
    public class AuthorizationController : ControllerBase
    {
        private readonly IApiAuthorizationService _service;

        public AuthorizationController(IApiAuthorizationService service)
        {
            _service = service;
        }


        [HttpPost("token")]
        [ProducesResponseType(typeof(AuthorizationResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> PostToken([FromBody] AuthorizationRequest value)
        {
            var @return = await _service.PostAuthorization(value);
            return Ok(@return);
        }

        [HttpPost("configuration-token")]
        [ProducesResponseType(typeof(AuthorizationResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> PostConfigurationToken([FromBody] AuthorizationConfigurationRequest value)
        {
            var @return = await _service.PostAuthorization(value);
            return Ok(@return);
        }
    }
}
