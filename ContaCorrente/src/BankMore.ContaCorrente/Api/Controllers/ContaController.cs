using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Application.Validators;
using FluentValidation.Results;
using System;
using System.Security.Claims;
using BankMore.ContaCorrente.Domain.Exceptions;

namespace BankMore.ContaCorrente.Api.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class ContaController : ControllerBase {
        private readonly IMediator _mediator;

        public ContaController(IMediator mediator) {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
        public async Task<IActionResult> CriarConta([FromBody] CriarContaCommand comando) {
            var validador = new CriarContaValidador();
            ValidationResult resultado = await validador.ValidateAsync(comando);

            if (!resultado.IsValid) {
                var erros = string.Join(", ", resultado.Errors.Select(e => e.ErrorMessage));
                return BadRequest(new { message = erros, failureType = "INVALID_DOCUMENT" });
            }

            try {
                var numeroConta = await _mediator.Send(comando); return Created("", new { numeroConta });
            }
            catch (BusinessException ex) {
                return BadRequest(new { message = ex.Message, failureType = ex.FailureType });
            }
        }

        [HttpPatch("me/inactivate")]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> InativarConta([FromBody] InativarContaCommand comando) {
            var contaId = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(contaId)) {
                return StatusCode(403, new { message = "Token inválido ou expirado", failureType = "INVALID_TOKEN" });
            }

            comando.ContaId = Guid.Parse(contaId);

            try {
                await _mediator.Send(comando);
                return NoContent();
            }
            catch (UnauthorizedAccessException) {
                return Unauthorized(new { message = "Senha inválida", failureType = "USER_UNAUTHORIZED" });
            }
            catch (BusinessException ex) {
                return BadRequest(new { message = ex.Message, failureType = ex.FailureType });
            }
        }

        [HttpPost("resolve")]
        [Authorize]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ResolverConta([FromBody] ResolverContaCommand comando) {
            try {
                var resultado = await _mediator.Send(comando);
                return Ok(new { contaId = resultado.ContaId, numeroConta = resultado.NumeroConta });
            }
            catch (BusinessException ex) {
                return NotFound(new { message = ex.Message, failureType = ex.FailureType });
            }
        }
    }
}
