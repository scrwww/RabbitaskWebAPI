using RabbitaskWebAPI.Data;
using System.Security.Claims;

namespace RabbitaskWebAPI.Services
{
    /// <summary>
    /// Service para gerenciar autorização/permissão de usuários
    /// </summary>
    public interface IUserAuthorizationService
    {
        /// <summary>
        /// Pega o código do usuário autenticado a partir do token JWT
        /// </summary>
        /// <returns>User ID</returns>
        /// <exception cref="UnauthorizedAccessException">Se o usuário não foi encontrado no jwt</exception>
        int GetCurrentUserId();

        /// <summary>
        /// Checa se o usuário é do tipo Agente
        /// </summary>
        /// <param name="cdUsuario">Código do usuário para checar</param>
        /// <returns>True se for agente, e se não for, False</returns>
        Task<bool> IsAgenteAsync(int cdUsuario);

        /// <summary>
        /// Pega todos os códigos de usuários que o usuário gerenciador pode acessar
        /// - Para Usuario Comum: retorna o próprio código
        /// - Para Agente: retorna o próprio código + todos os usuários comuns conectados a ele
        /// </summary>
        /// <param name="cdUsuario">O código do agente</param>
        /// <returns>Lista de usuários gerenciaveis</returns>
        Task<List<int>> GetManagedUserIdsAsync(int cdUsuario);

        /// <summary>
        /// Checa se o usuário gerenciador tem permissão para gerenciar o usuário alvo
        /// Retorna true se:
        /// - cdUsuarioGerenciador == cdUsuarioAlvo (pode se gerenciar)
        /// - cdUsuarioGerenciador é Agente e cdUsuarioAlvo é um dos usuários conectados a ele
        /// </summary>
        /// <param name="cdUsuarioGerenciador">O usuário gerenciador</param>
        /// <param name="cdUsuarioAlvo">O usuário alvo</param>
        /// <returns>True se for autorizado, false se não for</returns>
        Task<bool> CanManageUserAsync(int cdUsuarioGerenciador, int cdUsuarioAlvo);
    }
}