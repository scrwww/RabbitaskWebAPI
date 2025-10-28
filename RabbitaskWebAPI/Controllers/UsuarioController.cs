using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RabbitaskWebAPI.Services;
using RabbitaskWebAPI.Models;
using System.Security.Claims;

namespace RabbitaskWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly ICodigoConexaoService _codigoConexaoService;
        private readonly Services.IUserAuthorizationService _authService;

        public UsuarioController(
            ICodigoConexaoService codigoConexaoService,
            Services.IUserAuthorizationService authService)
        {
            _codigoConexaoService = codigoConexaoService;
            _authService = authService;
        }

        /// <summary>
        /// Gera um código de conexão para o usuário autenticado
        /// GET: api/Usuario/gerar-codigo
        /// </summary>
        [HttpGet("gerar-codigo")]
        [Authorize] // Requer que o usuário esteja autenticado
        public IActionResult GerarCodigoConexao()
        {
            try
            {
                // Obtém o ID do usuário autenticado do token JWT
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out int usuarioId))
                {
                    return Unauthorized(new { mensagem = "Usuário não autenticado" });
                }

                // Gera um novo código
                var codigoConexao = _codigoConexaoService.CriarCodigoConexao(usuarioId);

                return Ok(new
                {
                    codigo = codigoConexao.Codigo,
                    expiraEm = codigoConexao.DataExpiracao,
                    mensagem = "Código gerado com sucesso. Use este código para conectar outro dispositivo."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao gerar código", erro = ex.Message });
            }
        }

        /// <summary>
        /// Conecta um dispositivo usando um código de conexão
        /// GET: api/Usuario/conectar?cd=ABC12345
        /// </summary>
        [HttpGet("conectar")]
        public IActionResult ConectarComCodigo([FromQuery] string cd)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cd))
                {
                    return BadRequest(new { mensagem = "Código de conexão é obrigatório" });
                }

                // Valida o código e obtém o usuário
                var usuario = _codigoConexaoService.ValidarCodigo(cd.ToUpper());

                if (usuario == null)
                {
                    return BadRequest(new { mensagem = "Código inválido, expirado ou já utilizado" });
                }

                return Ok(new
                {
                    usuario = new
                    {
                        cd = usuario.CdUsuario,
                        nome = usuario.NmUsuario,
                        email = usuario.NmEmail
                    },
                    mensagem = "Conexão realizada com sucesso"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao conectar", erro = ex.Message });
            }
        }

        /// <summary>
        /// Limpa códigos expirados (pode ser chamado por um job agendado)
        /// POST: api/Usuario/limpar-codigos
        /// </summary>
        [HttpPost("limpar-codigos")]
        [Authorize(Roles = "Admin")] // Apenas admins podem executar
        public IActionResult LimparCodigosExpirados()
        {
            try
            {
                _codigoConexaoService.LimparCodigosExpirados();
                return Ok(new { mensagem = "Códigos expirados removidos com sucesso" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao limpar códigos", erro = ex.Message });
            }
        }
    }
}