using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitaskWebAPI.Data;
using RabbitaskWebAPI.Models;
using RabbitaskWebAPI.DTOs.Common;
using RabbitaskWebAPI.DTOs.Usuario;
using RabbitaskWebAPI.DTOs.TipoUsuario;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using RabbitaskWebAPI.RequestModels.Usuario;

namespace RabbitaskWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : BaseController
    {
        private readonly RabbitaskContext _context;
        private readonly IConfiguration _configuration;

        private const int MIN_PASSWORD_LENGTH = 8;
        private const int JWT_EXPIRY_HOURS = 2;

        public UsuarioController(RabbitaskContext context, ILogger<UsuarioController> logger, IConfiguration configuration)
            : base(logger)
        {
            _context = context;
            _configuration = configuration;
        }

        #region Endpoints

        //[HttpGet("health")]
        //public ActionResult<ApiResponse<HealthStatusDto>> HealthCheck()
        //{
        //    try
        //    {
        //        var canConnect = _context.Database.CanConnect();

        //        var healthStatus = new HealthStatusDto
        //        {
        //            Status = canConnect ? "Healthy" : "Degraded",
        //            Timestamp = DateTime.UtcNow,
        //            DatabaseConnection = canConnect,
        //            Version = "1.0.0"
        //        };

        //        return SuccessResponse(healthStatus, "API está funcionando corretamente");
        //    }
        //    catch (Exception ex)
        //    {
        //        return HandleException<HealthStatusDto>(ex, nameof(HealthCheck));
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult<ApiResponse<UsuarioCriadoDto>>> CadastrarUsuario([FromQuery] CadastrarUsuarioRequest request) 
        {
            {
                try
                {
                    if (!IsValidEmail(request.Email))
                        throw new ArgumentException("Formato de email inválido");

                    if (!IsValidPhoneNumber(request.Telefone))
                        throw new ArgumentException("Formato de telefone inválido");

                    using var transaction = await _context.Database.BeginTransactionAsync();
                    try
                    {
                        var existingUser = await _context.Usuarios
                            .FirstOrDefaultAsync(u => u.NmEmail == request.Email.ToLower().Trim()
                                                   || (request.Telefone != null && u.CdTelefone == request.Telefone));

                        if (existingUser != null)
                        {
                            if (existingUser.NmEmail == request.Email.ToLower().Trim())
                                return ErrorResponse<UsuarioCriadoDto>(409, "Email já está em uso", "Este email já está cadastrado");

                            if (existingUser.CdTelefone == request.Telefone)
                                return ErrorResponse<UsuarioCriadoDto>(409, "Telefone já está em uso", "Este telefone já está cadastrado");
                        }

                        var tipoUsuarioValido = await _context.TipoUsuarios
                            .AnyAsync(t => t.CdTipoUsuario == request.TipoUsuario);

                        if (!tipoUsuarioValido)
                            throw new ArgumentException("O tipo de usuário informado não existe");

                        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Senha);

                        var novoUsuario = new Usuario
                        {
                            NmUsuario = request.Nome.Trim(),
                            NmEmail = request.Senha.ToLower().Trim(),
                            NmSenha = hashedPassword,
                            CdTipoUsuario = request.TipoUsuario,
                            CdTelefone = request.Telefone?.Trim()
                        };

                        _context.Usuarios.Add(novoUsuario);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();

                        _logger.LogInformation("Usuário {UsuarioId} cadastrado. Email: {Email}",
                            novoUsuario.CdUsuario, novoUsuario.NmEmail);

                        var dto = new UsuarioCriadoDto
                        {
                            Cd = novoUsuario.CdUsuario,
                            Nome = novoUsuario.NmUsuario,
                            Email = novoUsuario.NmEmail,
                            TipoUsuario = request.TipoUsuario
                        };

                        return CreatedAtAction(nameof(GetUsuarioPorCd),
                            new { cd = novoUsuario.CdUsuario },
                            ApiResponse<UsuarioCriadoDto>.CreateSuccess(dto, "Usuário cadastrado com sucesso"));
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    return HandleException<UsuarioCriadoDto>(ex, nameof(CadastrarUsuario));
                }
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromQuery] LoginRequest request) 
        {
            try
            {
                _logger.LogInformation("Tentativa de login para email: {Email}", request.Email);

                var usuario = await _context.Usuarios
                    .Include(u => u.CdTipoUsuarioNavigation)
                    .FirstOrDefaultAsync(u => u.NmEmail == request.Email.ToLower().Trim());

                if (usuario == null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.NmSenha))
                {
                    _logger.LogWarning("Tentativa de login falhou para email: {Email}", request.Email);
                    return ErrorResponse<LoginResponseDto>(401, "Credenciais inválidas");
                }

                var token = await GerarTokenJWT(usuario);

                var dto = new LoginResponseDto
                {
                    Token = token.TokenString,
                    Usuario = new UsuarioDto
                    {
                        Cd = usuario.CdUsuario,
                        Nome = usuario.NmUsuario,
                        Email = usuario.NmEmail,
                        TipoUsuario = usuario.CdTipoUsuarioNavigation != null ? new TipoUsuarioDto
                        {
                            Cd = usuario.CdTipoUsuarioNavigation.CdTipoUsuario,
                            Nome = usuario.CdTipoUsuarioNavigation.NmTipoUsuario
                        } : null
                    },
                    ExpiresAt = token.ExpiresAt
                };

                return SuccessResponse(dto, "Login realizado com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<LoginResponseDto>(ex, nameof(Login));
            }
        }

        [HttpGet("perfil")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UsuarioPerfilDto>>> GetPerfilUsuario()
        {
            try
            {
                var cdUsuario = ObterIdUsuarioAutenticado();

                var usuario = await _context.Usuarios
                    .Include(u => u.CdTipoUsuarioNavigation)
                    .Where(u => u.CdUsuario == cdUsuario)
                    .Select(u => new UsuarioPerfilDto
                    {
                        Cd = u.CdUsuario,
                        Nome = u.NmUsuario,
                        Email = u.NmEmail,
                        Telefone = u.CdTelefone,
                        TipoUsuario = u.CdTipoUsuarioNavigation != null ? new TipoUsuarioDto
                        {
                            Cd = u.CdTipoUsuarioNavigation.CdTipoUsuario,
                            Nome = u.CdTipoUsuarioNavigation.NmTipoUsuario
                        } : null
                    })
                    .FirstOrDefaultAsync();

                if (usuario == null)
                    return ErrorResponse<UsuarioPerfilDto>(404, "Usuário não encontrado");

                return SuccessResponse(usuario, "Perfil encontrado com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<UsuarioPerfilDto>(ex, nameof(GetPerfilUsuario));
            }
        }

        [HttpGet("{cd:int}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<UsuarioPerfilDto>>> GetUsuarioPorCd(int cd)
        {
            try
            {
                var usuario = await _context.Usuarios
                    .Include(u => u.CdTipoUsuarioNavigation)
                    .Where(u => u.CdUsuario == cd)
                    .Select(u => new UsuarioPerfilDto
                    {
                        Cd = u.CdUsuario,
                        Nome = u.NmUsuario,
                        Email = u.NmEmail,
                        Telefone = u.CdTelefone,
                        TipoUsuario = u.CdTipoUsuarioNavigation != null ? new TipoUsuarioDto
                        {
                            Cd = u.CdTipoUsuarioNavigation.CdTipoUsuario,
                            Nome = u.CdTipoUsuarioNavigation.NmTipoUsuario
                        } : null
                    })
                    .FirstOrDefaultAsync();

                if (usuario == null)
                    return ErrorResponse<UsuarioPerfilDto>(404, "Usuário não encontrado");

                return SuccessResponse(usuario, "Usuário encontrado com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<UsuarioPerfilDto>(ex, nameof(GetUsuarioPorCd));
            }
        }

        [HttpGet("debug/claims")]
        [Authorize]
        public ActionResult<ApiResponse<object>> DebugClaims()
        {
#if DEBUG
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return SuccessResponse<object>(new
            {
                Claims = claims,
                IsAuthenticated = User.Identity?.IsAuthenticated ?? false,
                AuthenticationType = User.Identity?.AuthenticationType
            }, "Debug de claims");
#else
            return NotFound();
#endif
        }

        #endregion

        #region Métodos Privados

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var emailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");
            return emailRegex.IsMatch(email);
        }

        private static bool IsValidPhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber)) return true;
            var numbersOnly = Regex.Replace(phoneNumber, @"[^\d]", "");
            return numbersOnly.Length >= 10 && numbersOnly.Length <= 11;
        }

        private async Task<(string TokenString, DateTime ExpiresAt)> GerarTokenJWT(Usuario usuario)
        {
            var jwtConfig = _configuration.GetSection("JwtConfig");
            var expiresAt = DateTime.UtcNow.AddHours(JWT_EXPIRY_HOURS);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.CdUsuario.ToString()),
                new Claim(ClaimTypes.NameIdentifier, usuario.CdUsuario.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, usuario.NmEmail ?? string.Empty),
                new Claim(ClaimTypes.Email, usuario.NmEmail ?? string.Empty),
                new Claim(ClaimTypes.Name, usuario.NmUsuario ?? string.Empty),
                new Claim("tipo_usuario", usuario.CdTipoUsuario?.ToString() ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: creds
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }

        #endregion
    }
}
