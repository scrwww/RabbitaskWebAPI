// Services/AuthorizationService.cs
using Microsoft.EntityFrameworkCore;
using RabbitaskWebAPI.Data;
using System.Security.Claims;

namespace RabbitaskWebAPI.Services
{
    /// <summary>
    /// Implementation of authorization service
    /// Single source of truth for all authorization logic
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

        public int GetCurrentUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;

            if (user == null || !user.Identity?.IsAuthenticated == true)
            {
                _logger.LogWarning("Attempted to get user ID from unauthenticated context");
                throw new UnauthorizedAccessException("User is not authenticated");
            }

            // Try different claim types that might contain the user ID
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? user.FindFirst("sub")?.Value
                             ?? user.FindFirst("id")?.Value
                             ?? user.FindFirst("userId")?.Value
                             ?? user.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                _logger.LogError("User ID claim not found in token. Available claims: {Claims}",
                    string.Join(", ", user.Claims.Select(c => $"{c.Type}={c.Value}")));
                throw new UnauthorizedAccessException("User ID not found in authentication token");
            }

            if (!int.TryParse(userIdClaim, out int userId))
            {
                _logger.LogError("Failed to parse user ID from claim value: {ClaimValue}", userIdClaim);
                throw new UnauthorizedAccessException("Invalid user ID format in token");
            }

            return userId;
        }

        public async Task<bool> IsAgenteAsync(int userId)
        {
            try
            {
                var isAgente = await _context.Usuarios
                    .Where(u => u.CdUsuario == userId && u.CdTipoUsuario == 2)
                    .AnyAsync();

                _logger.LogDebug("User {UserId} is Agente: {IsAgente}", userId, isAgente);

                return isAgente;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if user {UserId} is Agente", userId);
                throw;
            }
        }

        public async Task<List<int>> GetManagedUserIdsAsync(int userId)
        {
            try
            {
                var managedIds = new List<int> { userId }; // Always include self

                // Check if user is an Agente
                var isAgente = await IsAgenteAsync(userId);

                if (isAgente)
                {
                    // Get all Usuario Comum IDs connected to this Agente
                    var connectedUserIds = await _context.ConexaoUsuarios
                        .Where(c => c.CdUsuarioAgente == userId)
                        .Select(c => c.CdUsuario)
                        .ToListAsync();

                    managedIds.AddRange(connectedUserIds);

                    _logger.LogDebug(
                        "Agente {AgenteId} manages {Count} users: {UserIds}",
                        userId,
                        managedIds.Count,
                        string.Join(", ", managedIds));
                }
                else
                {
                    _logger.LogDebug("Usuario Comum {UserId} manages only themselves", userId);
                }

                return managedIds;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting managed user IDs for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> CanManageUserAsync(int managerId, int targetUserId)
        {
            try
            {
                // Can always manage yourself
                if (managerId == targetUserId)
                {
                    _logger.LogDebug("User {UserId} can manage themselves", managerId);
                    return true;
                }

                // Check if manager is an Agente connected to target user
                var canManage = await _context.ConexaoUsuarios
                    .AnyAsync(c => c.CdUsuarioAgente == managerId && c.CdUsuario == targetUserId);

                _logger.LogDebug(
                    "User {ManagerId} can manage user {TargetId}: {CanManage}",
                    managerId,
                    targetUserId,
                    canManage);

                return canManage;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error checking if user {ManagerId} can manage user {TargetId}",
                    managerId,
                    targetUserId);
                throw;
            }
        }
    }
}
