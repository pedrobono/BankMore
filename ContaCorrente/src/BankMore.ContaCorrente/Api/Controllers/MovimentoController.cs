using BankMore.ContaCorrente.Application.Commands;
using BankMore.ContaCorrente.Application.Validators;
using BankMore.ContaCorrente.Domain.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankMore.ContaCorrente.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovimentoController : ControllerBase {
    private readonly IMediator _mediator;

    public MovimentoController(IMediator mediator) {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegistrarMovimento([FromBody] RegistrarMovimentoCommand comando) {
        var contaIdLogada = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(contaIdLogada)) {
            return StatusCode(403, new { message = "Token inválido ou expirado", failureType = "INVALID_TOKEN" });
        }

        comando.ContaIdLogada = Guid.Parse(contaIdLogada);

        var validador = new RegistrarMovimentoValidator();
        var resultado = await validador.ValidateAsync(comando);

        if (!resultado.IsValid) {
            var erros = string.Join(", ", resultado.Errors.Select(e => e.ErrorMessage));
            return BadRequest(new { message = erros, failureType = "INVALID_DATA" });
        }

        try {
            await _mediator.Send(comando);
            return NoContent();
        }
        catch (BusinessException ex) {
            return BadRequest(new { message = ex.Message, failureType = ex.FailureType });
        }
    }
}