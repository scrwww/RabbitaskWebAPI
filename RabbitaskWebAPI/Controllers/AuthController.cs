using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RabbitaskWebAPI.Data;
using RabbitaskWebAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PhoneNumbers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
        private readonly RabbitaskContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(RabbitaskContext context, IConfiguration configuration)
        {
                _context = context;
                _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
                var hashedPassword = HasherSenha.Hash(loginDto.Senha);
                var user = _context.Usuarios
                        .FirstOrDefault(u => u.NmEmail == loginDto.Email && u.NmSenha == hashedPassword);

                if (user == null)
                        return Unauthorized("Credenciais inválidas");

                var claims = new[]
                {
                        new Claim(JwtRegisteredClaimNames.Sub, user.CdUsuario.ToString()),
                        new Claim(JwtRegisteredClaimNames.Email, user.NmEmail.ToString())
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT_KEY"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                                issuer: _configuration["JWT_ISSUER"],
                                audience: _configuration["JWT_AUDIENCE"],
                                claims: claims,
                                expires: DateTime.Now.AddHours(2),
                                signingCredentials: creds);

                return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
        [HttpPost("cadastrar")]
        public IActionResult Register([FromBody] CadastrarDto dto)
        {
                if (_context.Usuarios.Any(u => u.NmEmail == dto.NmEmail))
                        return Conflict("Email já está em uso");

                if (!_context.TipoUsuarios.Any(t => t.CdTipoUsuario == dto.CdTipoUsuario))
                        return Conflict("Tipo de usuário inválido");

                PhoneNumberUtil numberUtil = PhoneNumberUtil.GetInstance();
                PhoneNumber telefone = numberUtil.Parse(dto.CdTelefone, "BR");
                if(!numberUtil.IsValidNumber(telefone))
                {
                        return Conflict("Telefone inválido");
                }


                string cdTelefone = numberUtil.Format(telefone, PhoneNumberFormat.E164);

                if (_context.Usuarios.Any(u => u.CdTelefone == cdTelefone))
                {
                        return Conflict("Telefone já está em uso");
                }

                var usuario = new Usuario
                {
                        NmUsuario = dto.NmUsuario,
                        NmEmail = dto.NmEmail,
                        NmSenha = HasherSenha.Hash(dto.NmSenha),
                        CdTelefone = cdTelefone,
                        CdTipoUsuario = dto.CdTipoUsuario
                };

                _context.Usuarios.Add(usuario);
                _context.SaveChanges();

                return Ok(new { usuario.CdUsuario, usuario.NmEmail });
        }
}

public class LoginDto
{
        public string Email { get; set; }
        public string Senha { get; set; }
}

public class CadastrarDto
{
        public string NmUsuario { get; set; }

        [EmailAddress]
        public string NmEmail { get; set; }
        public string NmSenha { get; set; }

        [Phone]
        public string CdTelefone { get; set; }
        public int CdTipoUsuario { get; set; }
}
