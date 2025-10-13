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
        /// respostas padronizada.. isso foi bme útil
        /// </summary>
        protected ActionResult<ApiResponse<T>> SuccessResponse<T>(T data, string message = "Operação realizada com sucesso")
        {
            return Ok(ApiResponse<T>.CreateSuccess(data, message));
        }

        protected ActionResult<ApiResponse<object>> SuccessResponse(string message = "Operação realizada com sucesso")
        {
            return Ok(ApiResponse<object>.CreateSuccess(message));
        }

        protected ActionResult<ApiResponse<T>> ErrorResponse<T>(int statusCode, string message, params string[] errors)
        {
            var response = ApiResponse<T>.CreateError(message, errors);

            return statusCode switch
            {
                400 => BadRequest(response),
                401 => StatusCode(401, response),
                403 => StatusCode(403, response),
                404 => NotFound(response),
                409 => Conflict(response),
                _ => StatusCode(statusCode, response)
            };
        }

        /// <summary>
        /// Tratar as exceções
        /// </summary>
        protected ActionResult<ApiResponse<T>> HandleException<T>(Exception ex, string operationName)
        {
            return ex switch
            {
                UnauthorizedAccessException => ErrorResponse<T>(401, "Acesso não autorizado", ex.Message),
                ArgumentException => ErrorResponse<T>(400, "Dados inválidos", ex.Message),
                InvalidOperationException => ErrorResponse<T>(409, "Operação não permitida", ex.Message),
                _ => HandleGenericException<T>(ex, operationName)
            };
        }

        private ActionResult<ApiResponse<T>> HandleGenericException<T>(Exception ex, string operationName)
        {
            var correlationId = Guid.NewGuid().ToString("N")[..8];
            _logger.LogError(ex, "Erro inesperado em {OperationName}. CorrelationId: {CorrelationId}",
                operationName, correlationId);

            return ErrorResponse<T>(500,
                "Erro interno do servidor",
                $"Entre em contato com o suporte informando o código: {correlationId}");
        }
    }
}