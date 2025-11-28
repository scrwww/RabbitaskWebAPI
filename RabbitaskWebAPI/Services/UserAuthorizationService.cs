// Services/UserAuthorizationService.cs
using Microsoft.EntityFrameworkCore;
using RabbitaskWebAPI.Data;
using System.Security.Claims;

namespace RabbitaskWebAPI.Services
{
    /// <summary>
    /// Implementação do serviço de autorização
    /// Fonte única de verdade para toda lógica de autorização
    /// </summary>
    public class UserAuthorizationService : IUserAuthorizationService
    {
        private readonly RabbitaskContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserAuthorizationService> _logger;

        public UserAuthorizationService(
            RabbitaskContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserAuthorizationService> logger)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public int ObterCdUsuarioAtual()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity?.IsAuthenticated == true)
            {
                _logger.LogWarning("Tentativa de obter código do usuário em contexto não autenticado");
                throw new UnauthorizedAccessException("Usuário não autenticado");
            }

            // Tenta diferentes tipos de claim que podem conter o código do usuário
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? user.FindFirst("sub")?.Value
                             ?? user.FindFirst("id")?.Value
                             ?? user.FindFirst("userId")?.Value
                             ?? user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogError("Claim de código do usuário não encontrado no token. Claims disponíveis: {Claims}",
                    string.Join(", ", user.Claims.Select(c => $"{c.Type}={c.Value}")));
                throw new UnauthorizedAccessException("Código do usuário não encontrado no token de autenticação");
            }

            if (!int.TryParse(userIdClaim, out int cdUsuario))
            {
                _logger.LogError("Falha ao analisar código do usuário do valor do claim: {ClaimValue}", userIdClaim);
                throw new UnauthorizedAccessException("Formato inválido do código do usuário no token");
            }

            return cdUsuario;
        }

        public async Task<bool> EhAgenteAsync(int cdUsuario)
        {
            try
            {
                var ehAgente = await _context.Usuarios
                    .Where(u => u.CdUsuario == cdUsuario && u.CdTipoUsuario == 2)
                    .AnyAsync();

                _logger.LogDebug("Usuário {CdUsuario} é Agente: {EhAgente}", cdUsuario, ehAgente);

                return ehAgente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar se usuário {CdUsuario} é Agente", cdUsuario);
                throw;
            }
        }

        public async Task<List<int>> ObterCdsUsuariosGerenciadosAsync(int cdUsuario)
        {
            try
            {
                var cdsGerenciados = new List<int> { cdUsuario }; // Sempre inclui a si mesmo

                // Verifica se o usuário é um Agente
                var ehAgente = await EhAgenteAsync(cdUsuario);

                if (ehAgente)
                {
                    // Obtém todos os códigos de Usuário Comum conectados a este Agente
                    var cdsUsuariosConectados = await _context.ConexaoUsuarios
                        .Where(c => c.CdUsuarioAgente == cdUsuario)
                        .Select(c => c.CdUsuario)
                        .ToListAsync();

                    cdsGerenciados.AddRange(cdsUsuariosConectados);

                    _logger.LogDebug(
                        "Agente {CdAgente} gerencia {Count} usuários: {Codigos}",
                        cdUsuario,
                        cdsGerenciados.Count,
                        string.Join(", ", cdsGerenciados));
                }
                else
                {
                    _logger.LogDebug("Usuário Comum {CdUsuario} gerencia apenas a si mesmo", cdUsuario);
                }

                return cdsGerenciados;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter códigos dos usuários gerenciados para o usuário {CdUsuario}", cdUsuario);
                throw;
            }
        }

        public async Task<bool> PodeGerenciarUsuarioAsync(int cdUsuarioGerenciador, int cdUsuarioAlvo)
        {
            try
            {
                // Sempre pode gerenciar a si mesmo
                if (cdUsuarioGerenciador == cdUsuarioAlvo)
                {
                    _logger.LogDebug("Usuário {CdUsuario} pode gerenciar a si mesmo", cdUsuarioGerenciador);
                    return true;
                }

                // Verifica se o gerenciador é um Agente conectado ao usuário alvo
                var podeGerenciar = await _context.ConexaoUsuarios
                    .AnyAsync(c => c.CdUsuarioAgente == cdUsuarioGerenciador && c.CdUsuario == cdUsuarioAlvo);

                _logger.LogDebug(
                    "Usuário {CdGerenciador} pode gerenciar usuário {CdAlvo}: {PodeGerenciar}",
                    cdUsuarioGerenciador,
                    cdUsuarioAlvo,
                    podeGerenciar);

                return podeGerenciar;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Erro ao verificar se usuário {CdGerenciador} pode gerenciar usuário {CdAlvo}",
                    cdUsuarioGerenciador,
                    cdUsuarioAlvo);
                throw;
            }
        }
    }
}
