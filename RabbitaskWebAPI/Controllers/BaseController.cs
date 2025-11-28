// Controllers/BaseController.cs
using Microsoft.AspNetCore.Mvc;
using RabbitaskWebAPI.DTOs.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RabbitaskWebAPI.Controllers
{
    /// <summary>
    /// Controller base com funcionalidades comuns a todos os controllers
    /// Centraliza lógica repetitiva e padroniza comportamentos - especialmente os das respostas :D
    /// </summary>
    [ApiController]
    public abstract class BaseController : ControllerBase
    {
        protected readonly ILogger _logger;

        protected BaseController(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// extrai o ID do usuário autenticado do token JWT
        /// método protegido disponível para todos os controllers filhos
        /// </summary>
        protected int ObterIdUsuarioAutenticado()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                             User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                             User.FindFirst("sub");

            if (userIdClaim == null)
            {
                _logger.LogWarning("Tentativa de acesso sem token válido");
                throw new UnauthorizedAccessException("Token JWT não contém informação do usuário");
            }

            if (!int.TryParse(userIdClaim.Value, out int cdUsuario))
            {
                _logger.LogWarning("Token contém ID de usuário em formato inválido: {UserId}", userIdClaim.Value);
                throw new UnauthorizedAccessException("ID do usuário tem formato inválido");
            }

            return cdUsuario;
        }

        /// <summary>
        /// pega informações básicas do usuário autenticado
        /// </summary>
        protected (int Id, string Email, string Nome) ObterInfoUsuarioAutenticado()
        {
            var codig = ObterIdUsuarioAutenticado();
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? "";
            var nome = User.FindFirst(ClaimTypes.Name)?.Value ?? "";

            return (codig, email, nome);
        }

        /// <summary>
        /// respostas padronizadas - isso foi bem útil
        /// </summary>
        protected ActionResult<ApiResponse<T>> RespostaSucesso<T>(T dados, string mensagem = "Operação realizada com sucesso")
        {
            return Ok(ApiResponse<T>.CreateSuccess(dados, mensagem));
        }

        protected ActionResult<ApiResponse<object>> RespostaSucesso(string mensagem = "Operação realizada com sucesso")
        {
            return Ok(ApiResponse<object>.CreateSuccess(mensagem));
        }

        protected ActionResult<ApiResponse<T>> RespostaErro<T>(int codigoStatus, string mensagem, params string[] erros)
        {
            var resposta = ApiResponse<T>.CreateError(mensagem, erros);

            return codigoStatus switch
            {
                400 => BadRequest(resposta),
                401 => StatusCode(401, resposta),
                403 => StatusCode(403, resposta),
                404 => NotFound(resposta),
                409 => Conflict(resposta),
                _ => StatusCode(codigoStatus, resposta)
            };
        }

        /// <summary>
        /// Tratar as exceções
        /// </summary>
        protected ActionResult<ApiResponse<T>> TratarExcecao<T>(Exception ex, string nomeDaOperacao)
        {
            return ex switch
            {
                UnauthorizedAccessException => RespostaErro<T>(401, "Acesso não autorizado", ex.Message),
                ArgumentException => RespostaErro<T>(400, "Dados inválidos", ex.Message),
                InvalidOperationException => RespostaErro<T>(409, "Operação não permitida", ex.Message),
                _ => TratarExcecaoGenerica<T>(ex, nomeDaOperacao)
            };
        }

        private ActionResult<ApiResponse<T>> TratarExcecaoGenerica<T>(Exception ex, string nomeDaOperacao)
        {
            var cdCorrelacao = Guid.NewGuid().ToString("N")[..8];
            _logger.LogError(ex, "Erro inesperado em {OperationName}. CorrelationId: {CorrelationId}",
                nomeDaOperacao, cdCorrelacao);

            return RespostaErro<T>(500,
                "Erro interno do servidor",
                $"Entre em contato com o suporte informando o código: {cdCorrelacao}");
        }
    }
}