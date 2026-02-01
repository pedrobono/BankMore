using BankMore.ContaCorrente.Application.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BankMore.ContaCorrente.Api.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator) {
            _mediator = mediator;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginCommand comando) {
            try {
                var response = await _mediator.Send(comando);
                return Ok(response);
            }
            catch (UnauthorizedAccessException) {
                return Unauthorized(new { message = "Usuário ou senha inválidos", failureType = "USER_UNAUTHORIZED" });
            }
        }
    }
}
