using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RabbitaskWebAPI.Data;
using RabbitaskWebAPI.DTOs.Common;
using RabbitaskWebAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PhoneNumbers;

namespace RabbitaskWebAPI.Controllers
{
    [Route("api/[controller]")]
    public class AuthController : BaseController
    {
        private readonly RabbitaskContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(
            RabbitaskContext context, 
            IConfiguration configuration,
            ILogger<AuthController> logger)
            : base(logger)
        {
            _context = context;
            _configuration = configuration;
        }

        /// <summary>
        /// Autentica um usuário e retorna um token JWT
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray();
                    return ErrorResponse<LoginResponseDto>(400, "Dados inválidos", errors);
                }

                var hashedPassword = HasherSenha.Hash(loginDto.Senha);
                var user = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.NmEmail == loginDto.Email && u.NmSenha == hashedPassword);

                if (user == null)
                {
                    _logger.LogWarning("Tentativa de login falhou para email: {Email}", loginDto.Email);
                    return ErrorResponse<LoginResponseDto>(401, "Credenciais inválidas");
                }

                var token = GerarTokenJwt(user);

                _logger.LogInformation("Usuário {UserId} autenticado com sucesso", user.CdUsuario);

                return SuccessResponse(new LoginResponseDto 
                { 
                    Token = token,
                    CdUsuario = user.CdUsuario,
                    NmUsuario = user.NmUsuario,
                    Email = user.NmEmail
                }, "Login realizado com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<LoginResponseDto>(ex, nameof(Login));
            }
        }

        /// <summary>
        /// Registra um novo usuário no sistema
        /// </summary>
        [HttpPost("cadastrar")]
        public async Task<ActionResult<ApiResponse<CadastroResponseDto>>> Register([FromBody] CadastrarDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray();
                    return ErrorResponse<CadastroResponseDto>(400, "Dados inválidos", errors);
                }

                // Validar email único
                if (await _context.Usuarios.AnyAsync(u => u.NmEmail == dto.NmEmail))
                {
                    return ErrorResponse<CadastroResponseDto>(409, "Email já está em uso");
                }

                // Validar tipo de usuário
                if (!await _context.TipoUsuarios.AnyAsync(t => t.CdTipoUsuario == dto.CdTipoUsuario))
                {
                    return ErrorResponse<CadastroResponseDto>(400, "Tipo de usuário inválido");
                }

                // Validar e formatar telefone
                var telefoneValidado = await ValidarTelefone(dto.CdTelefone);
                if (telefoneValidado == null)
                {
                    return ErrorResponse<CadastroResponseDto>(400, "Telefone inválido");
                }

                // Verificar telefone único
                if (await _context.Usuarios.AnyAsync(u => u.CdTelefone == telefoneValidado))
                {
                    return ErrorResponse<CadastroResponseDto>(409, "Telefone já está em uso");
                }

                // Validar senha
                var senhaErros = ValidarSenha(dto.NmSenha);
                if (senhaErros.Any())
                {
                    return ErrorResponse<CadastroResponseDto>(400, "Senha não atende aos requisitos", senhaErros.ToArray());
                }

                // Criar usuário
                var usuario = new Usuario
                {
                    NmUsuario = dto.NmUsuario.Trim(),
                    NmEmail = dto.NmEmail.Trim().ToLower(),
                    NmSenha = HasherSenha.Hash(dto.NmSenha),
                    CdTelefone = telefoneValidado,
                    CdTipoUsuario = dto.CdTipoUsuario
                };

                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Novo usuário cadastrado: {UserId} - {Email}", usuario.CdUsuario, usuario.NmEmail);

                return SuccessResponse(new CadastroResponseDto
                {
                    CdUsuario = usuario.CdUsuario,
                    NmUsuario = usuario.NmUsuario,
                    Email = usuario.NmEmail
                }, "Cadastro realizado com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<CadastroResponseDto>(ex, nameof(Register));
            }
        }

        #region Métodos Auxiliares

        /// <summary>
        /// Gera um token JWT para o usuário autenticado
        /// </summary>
        private string GerarTokenJwt(Usuario user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.CdUsuario.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.NmEmail),
                new Claim(ClaimTypes.NameIdentifier, user.CdUsuario.ToString()),
                new Claim(ClaimTypes.Email, user.NmEmail),
                new Claim(ClaimTypes.Name, user.NmUsuario)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT_KEY"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT_ISSUER"],
                audience: _configuration["JWT_AUDIENCE"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2), 
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Valida e formata um número de telefone brasileiro
        /// </summary>
        private async Task<string?> ValidarTelefone(string telefone)
        {
            try
            {
                PhoneNumberUtil numberUtil = PhoneNumberUtil.GetInstance();
                PhoneNumber phoneNumber = numberUtil.Parse(telefone, "BR");

                if (!numberUtil.IsValidNumber(phoneNumber))
                {
                    return null;
                }

                return numberUtil.Format(phoneNumber, PhoneNumberFormat.E164);
            }
            catch (NumberParseException ex)
            {
                _logger.LogWarning("Erro ao validar telefone: {Telefone}. Erro: {Message}", telefone, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Valida requisitos de senha forte
        /// </summary>
        private List<string> ValidarSenha(string senha)
        {
            var erros = new List<string>();

            if (string.IsNullOrWhiteSpace(senha))
            {
                erros.Add("Senha é obrigatória");
                return erros;
            }

            if (senha.Length < 8)
                erros.Add("Senha deve ter no mínimo 8 caracteres");

            if (senha.Length > 100)
                erros.Add("Senha deve ter no máximo 100 caracteres");

            if (!senha.Any(char.IsUpper))
                erros.Add("Senha deve conter pelo menos uma letra maiúscula");

            if (!senha.Any(char.IsLower))
                erros.Add("Senha deve conter pelo menos uma letra minúscula");

            if (!senha.Any(char.IsDigit))
                erros.Add("Senha deve conter pelo menos um número");

            if (!senha.Any(c => !char.IsLetterOrDigit(c)))
                erros.Add("Senha deve conter pelo menos um caractere especial");

            return erros;
        }

        #endregion
    }

    #region DTOs

    public class LoginDto
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória")]
        public string Senha { get; set; }
    }

    public class LoginResponseDto
    {
        public string Token { get; set; }
        public int CdUsuario { get; set; }
        public string NmUsuario { get; set; }
        public string Email { get; set; }
    }

    public class CadastrarDto
    {
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Nome deve ter entre 3 e 100 caracteres")]
        public string NmUsuario { get; set; }

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string NmEmail { get; set; }

        [Required(ErrorMessage = "Senha é obrigatória")]
        public string NmSenha { get; set; }

        [Required(ErrorMessage = "Telefone é obrigatório")]
        [Phone(ErrorMessage = "Telefone inválido")]
        public string CdTelefone { get; set; }

        [Required(ErrorMessage = "Tipo de usuário é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "Tipo de usuário inválido")]
        public int CdTipoUsuario { get; set; }
    }

    public class CadastroResponseDto
    {
        public int CdUsuario { get; set; }
        public string NmUsuario { get; set; }
        public string Email { get; set; }
    }

    #endregion
}
