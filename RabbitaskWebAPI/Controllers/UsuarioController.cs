using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Generators;
using RabbitaskWebAPI.Data;
using RabbitaskWebAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace RabbitaskWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly RabbitaskContext _context;
        private readonly IConfiguration _configuration;

        public UsuarioController(RabbitaskContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new { Message = "API funcionando", Timestamp = DateTime.Now });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (_context.Usuarios.Any(u => u.NmEmail == request.Email))
                return BadRequest("Email já está em uso.");
            if (_context.Usuarios.Any(u => u.CdTelefone == request.Telefone))
                return BadRequest("Telefone já está em uso");

            if(request.Password.Length < 8)
                return BadRequest("A senha deve ter pelo menos 8 caracteres.");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new Usuario
            {
                NmUsuario = request.Name,
                NmEmail = request.Email,
                NmSenha = hashedPassword,
                CdTipoUsuario = request.TipoUsuario,
                CdTelefone = request.Telefone
            };

            _context.Usuarios.Add(newUser);
            _context.SaveChanges();

            return Ok(new { Message = "Usuário registrado" });
        }

        public class RegisterRequest
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public int TipoUsuario { get; set; }
            public string? Telefone { get; set; }
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                                 User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                                 User.FindFirst("sub");

                if (userIdClaim == null)
                {
                    return Unauthorized("Claim do ID do usuário não encontrado no token");
                }

                if (!int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized("Formato do ID incorreto");
                }

                var user = _context.Usuarios
                    .Include(u => u.CdTipoUsuarioNavigation)
                    .Where(u => u.CdUsuario == userId)
                    .Select(u => new
                    {
                        u.CdUsuario,
                        u.NmUsuario,
                        u.NmEmail,
                        TipoUsuario = u.CdTipoUsuarioNavigation != null ? new
                        {
                            u.CdTipoUsuarioNavigation.CdTipoUsuario,
                            u.CdTipoUsuarioNavigation.NmTipoUsuario
                        } : null
                    })
                    .FirstOrDefault();

                if (user == null)
                    return NotFound("Usuário não encontrado");

                return Ok(user);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            try
            {
                var user = _context.Usuarios.FirstOrDefault(u => u.NmEmail == request.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.NmSenha))
                {
                    return Unauthorized("Login ou senha inválido(s)");
                }

                var jwtConfig = _configuration.GetSection("JwtConfig");

                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.CdUsuario.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.CdUsuario.ToString()), // Standard claim type
                    new Claim(JwtRegisteredClaimNames.Email, user.NmEmail ?? string.Empty),
                    new Claim(ClaimTypes.Email, user.NmEmail ?? string.Empty),
                    new Claim("tipo_usuario", user.CdTipoUsuario?.ToString() ?? ""),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Key"]!));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: jwtConfig["Issuer"],
                    audience: jwtConfig["Audience"],
                    claims: claims,
                    expires: DateTime.UtcNow.AddHours(2),
                    signingCredentials: creds
                );

                var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

                return Ok(new
                {
                    Token = tokenString,
                    UserId = user.CdUsuario,
                    Email = user.NmEmail,
                    ExpiresAt = DateTime.UtcNow.AddHours(2)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        [HttpGet("debug-claims")]
        [Authorize]
        public IActionResult DebugClaims()
        {
            var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Ok(new { Claims = claims, IsAuthenticated = User.Identity.IsAuthenticated });
        }
    }
}