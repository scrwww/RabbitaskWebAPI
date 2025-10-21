using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitaskWebAPI.Data;
using RabbitaskWebAPI.DTOs.Common;
using RabbitaskWebAPI.DTOs.TipoUsuario;
using RabbitaskWebAPI.DTOs.Usuario;
using RabbitaskWebAPI.Models;

namespace RabbitaskWebAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioController : BaseController
    {
        private readonly RabbitaskContext _context;
        private readonly Services.IUserAuthorizationService _authService;
        public UsuarioController(
            RabbitaskContext context,
            Services.IUserAuthorizationService authService,
            ILogger<UsuarioController> logger)
            : base(logger)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// Pega o dados do usuário atual
        /// </summary>
        [HttpGet("eu")]
        public async Task<ActionResult<ApiResponse<UsuarioDto>>> GetCurrentUser()
        {
            try
            {
                var cdUsuarioAtual = _authService.GetCurrentUserId();

                var usuario = await _context.Usuarios
                    .Include(u => u.CdTipoUsuarioNavigation)
                    .Where(u => u.CdUsuario == cdUsuarioAtual)
                    .Select(u => new UsuarioDto
                    {
                        Cd = u.CdUsuario,
                        Nome = u.NmUsuario,
                        Email = u.NmEmail,
                        Telefone = u.CdTelefone,
                        Tipo = new TipoUsuarioDto { Cd = u.CdTipoUsuarioNavigation.CdTipoUsuario, Nome = u.CdTipoUsuarioNavigation.NmTipoUsuario }
                    })
                    .FirstOrDefaultAsync();

                if (usuario == null)
                {
                    return ErrorResponse<UsuarioDto>(404, "Usuário não encontrado");
                }

                return SuccessResponse(usuario, "Perfil carregado com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<UsuarioDto>(ex, nameof(GetCurrentUser));
            }
        }

        /// <summary>
        /// Pega todos os usuários gerenciados pelo agente
        /// Retorna apenas a si msm + usuarios conectados se for Agente
        /// </summary>
        [HttpGet("gerenciaveis")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UsuarioResumoDto>>>> GetUsuariosGerenciaveis()
        {
            try
            {
                var cdUsuarioAtual = _authService.GetCurrentUserId();
                var cdUsuariosGeridos = await _authService.GetManagedUserIdsAsync(cdUsuarioAtual);

                var usuarios = await _context.Usuarios
                    .Where(u => cdUsuariosGeridos.Contains(u.CdUsuario))
                    .Select(u => new UsuarioResumoDto
                    {
                        Cd = u.CdUsuario,
                        Nome = u.NmUsuario,
                        Email = u.NmEmail
                    })
                    .OrderBy(u => u.Nome)
                    .ToListAsync();

                return SuccessResponse<IEnumerable<UsuarioResumoDto>>(
                    usuarios,
                    $"Encontrados {usuarios.Count} usuários gerenciáveis");
            }
            catch (Exception ex)
            {
                return HandleException<IEnumerable<UsuarioResumoDto>>(ex, nameof(GetUsuariosGerenciaveis));
            }
        }

        /// <summary>
        /// Pega o usuário por cd (se tiver permissão)
        /// </summary>
        [HttpGet("{codigo:int}")]
        public async Task<ActionResult<ApiResponse<UsuarioDto>>> GetUsuario(int codigo)
        {
            try
            {
                var cdUsuarioAtual = _authService.GetCurrentUserId();

                // CHECAGEM MAROTA DE PERMISSÃO
                if (!await _authService.CanManageUserAsync(cdUsuarioAtual, codigo))
                {
                    return ErrorResponse<UsuarioDto>(403,
                        "Você não tem permissão para acessar este usuário");
                }

                var usuario = await _context.Usuarios
                    .Include(u => u.CdTipoUsuarioNavigation)
                    .Where(u => u.CdUsuario == codigo)
                    .Select(u => new UsuarioDto
                    {
                        Cd = u.CdUsuario,
                        Nome = u.NmUsuario,
                        Email = u.NmEmail,
                        Telefone = u.CdTelefone,
                        Tipo = new TipoUsuarioDto { Cd = u.CdTipoUsuarioNavigation.CdTipoUsuario, Nome = u.CdTipoUsuarioNavigation.NmTipoUsuario }
                    })
                    .FirstOrDefaultAsync();

                if (usuario == null)
                {
                    return ErrorResponse<UsuarioDto>(404, "Usuário não encontrado");
                }

                return SuccessResponse(usuario, "Usuário encontrado com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<UsuarioDto>(ex, nameof(GetUsuario));
            }
        }

        /// <summary>
        /// Atualiza as info do usuário
        /// </summary>
        [HttpPut("eu")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateCurrentUser(
            [FromBody] UsuarioUpdateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray();

                    return ErrorResponse<object>(400, "Dados inválidos fornecidos", errors);
                }

                var cdUsuarioAtual = _authService.GetCurrentUserId();

                var usuario = await _context.Usuarios.FindAsync(cdUsuarioAtual);
                if (usuario == null)
                {
                    return ErrorResponse<object>(404, "Usuário não encontrado");
                }

                if (!string.IsNullOrWhiteSpace(dto.Nome))
                    usuario.NmUsuario = dto.Nome.Trim();

                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    var emailExists = await _context.Usuarios
                        .AnyAsync(u => u.NmEmail == dto.Email && u.CdUsuario != cdUsuarioAtual);

                    if (emailExists)
                    {
                        return ErrorResponse<object>(409, "Email já está em uso");
                    }

                    usuario.NmEmail = dto.Email.Trim();
                }

                if (!string.IsNullOrWhiteSpace(dto.Telefone))
                    usuario.CdTelefone = dto.Telefone.Trim();

                if (!string.IsNullOrWhiteSpace(dto.NovaSenha))
                {
                   
                    usuario.NmSenha = HasherSenha.Hash(dto.NovaSenha);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Usuário {CdUsuario} atualizou seu perfil", cdUsuarioAtual);

                return SuccessResponse("Perfil atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<object>(ex, nameof(UpdateCurrentUser));
            }
        }

        /// <summary>
        /// Atualiza os dados do usuario - esse é pro agente
        /// </summary>
        [HttpPut("{codigo:int}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateUsuario(
            int codigo,
            [FromBody] UsuarioUpdateDto dto)
        {
            try
            {
                var cdUsuarioAtual = _authService.GetCurrentUserId();

                if (!await _authService.CanManageUserAsync(cdUsuarioAtual, codigo))
                {
                    return ErrorResponse<object>(403,
                        "Você não tem permissão para atualizar este usuário");
                }

                var usuario = await _context.Usuarios.FindAsync(codigo);
                if (usuario == null)
                {
                    return ErrorResponse<object>(404, "Usuário não encontrado");
                }

                if (!string.IsNullOrWhiteSpace(dto.Nome))
                    usuario.NmUsuario = dto.Nome.Trim();

                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    var emailExists = await _context.Usuarios
                        .AnyAsync(u => u.NmEmail == dto.Email && u.CdUsuario != codigo);

                    if (emailExists)
                    {
                        return ErrorResponse<object>(409, "Email já está em uso");
                    }

                    usuario.NmEmail = dto.Email.Trim();
                }

                if (!string.IsNullOrWhiteSpace(dto.Telefone))
                    usuario.CdTelefone = dto.Telefone.Trim();

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Usuário {ManagerId} atualizou o perfil do usuário {UserId}",
                    cdUsuarioAtual, codigo);

                return SuccessResponse("Usuário atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<object>(ex, nameof(UpdateUsuario));
            }
        }

        /// <summary>
        /// Pega as estatísticas do usuário (task counts, taxa de cumprimento, etc.)
        /// </summary>
        [HttpGet("{codigo:int}/estatisticas")]
        public async Task<ActionResult<ApiResponse<UsuarioEstatisticasDto>>> GetEstatisticas(int codigo)
        {
            try
            {
                var cdUsuarioAtual = _authService.GetCurrentUserId();

                if (!await _authService.CanManageUserAsync(cdUsuarioAtual, codigo))
                {
                    return ErrorResponse<UsuarioEstatisticasDto>(403,
                        "Você não tem permissão para acessar as estatísticas deste usuário");
                }

                var tarefas = await _context.Tarefas
                    .Where(t => t.CdUsuario == codigo)
                    .ToListAsync();

                var totalTarefas = tarefas.Count;
                var tarefasConcluidas = tarefas.Count(t => t.DtConclusao.HasValue);
                var tarefasPendentes = totalTarefas - tarefasConcluidas;
                var tarefasAtrasadas = tarefas
                    .Count(t => !t.DtConclusao.HasValue
                             && t.DtPrazo.HasValue
                             && t.DtPrazo.Value < DateTime.Now);

                var estatisticas = new UsuarioEstatisticasDto
                {
                    TotalTarefas = totalTarefas,
                    TarefasConcluidas = tarefasConcluidas,
                    TarefasPendentes = tarefasPendentes,
                    TarefasAtrasadas = tarefasAtrasadas,
                    TaxaConclusao = totalTarefas > 0
                        ? Math.Round((double)tarefasConcluidas / totalTarefas * 100, 2)
                        : 0
                };

                return SuccessResponse(estatisticas, "Estatísticas carregadas com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<UsuarioEstatisticasDto>(ex, nameof(GetEstatisticas));
            }
        }

        /// <summary>
        /// Pega todos os usuarios conectados ao agente autenticado
        /// </summary>
        [HttpGet("meus-usuarios")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UsuarioResumoDto>>>> GetMeusUsuarios()
        {
            try
            {
                var cdUsuarioAtual = _authService.GetCurrentUserId();

                // checa se é Agente
                var isAgente = await _authService.IsAgenteAsync(cdUsuarioAtual);
                if (!isAgente)
                {
                    return ErrorResponse<IEnumerable<UsuarioResumoDto>>(403,
                        "Apenas Agentes podem acessar esta funcionalidade");
                }

                var usuarios = await _context.ConexaoUsuarios
                    .Where(c => c.CdUsuarioAgente == cdUsuarioAtual)
                    .Include(c => c.CdUsuarioNavigation)
                    .Select(c => new UsuarioResumoDto
                    {
                        Cd = c.CdUsuarioNavigation.CdUsuario,
                        Nome = c.CdUsuarioNavigation.NmUsuario,
                        Email = c.CdUsuarioNavigation.NmEmail
                    })
                    .OrderBy(u => u.Nome)
                    .ToListAsync();

                return SuccessResponse<IEnumerable<UsuarioResumoDto>>(
                    usuarios,
                    $"Encontrados {usuarios.Count} usuários gerenciados");
            }
            catch (Exception ex)
            {
                return HandleException<IEnumerable<UsuarioResumoDto>>(ex, nameof(GetMeusUsuarios));
            }
        }

        /// <summary>
        /// Conecta um agente a um usuário comum
        /// </summary>
        [HttpPost("conectar")]
        public async Task<ActionResult<ApiResponse<object>>> ConectarUsuarios(
            [FromBody] ConectarUsuariosDto dto)
        {
            try
            {
                var agente = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.CdUsuario == dto.CdAgente && u.CdTipoUsuario == 2);

                if (agente == null)
                {
                    return ErrorResponse<object>(404, "Agente não encontrado");
                }

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.CdUsuario == dto.CdUsuario && u.CdTipoUsuario == 1);

                if (usuario == null)
                {
                    return ErrorResponse<object>(404, "Usuário não encontrado");
                }

                // checar se a conexão já existe... - v -
                var connectionExists = await _context.ConexaoUsuarios
                    .AnyAsync(c => c.CdUsuarioAgente == dto.CdAgente && c.CdUsuario == dto.CdUsuario);

                if (connectionExists)
                {
                    return ErrorResponse<object>(409, "Conexão já existe");
                }

                // CONNECT THEM HEHEHE
                var conexao = new ConexaoUsuario
                {
                    CdUsuarioAgente = dto.CdAgente,
                    CdUsuario = dto.CdUsuario
                };

                _context.ConexaoUsuarios.Add(conexao);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Agente {CdUsuarioAgente} conectado ao usuário {CdUsuario}",
                    dto.CdAgente, dto.CdUsuario);

                return SuccessResponse("Usuários conectados com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<object>(ex, nameof(ConectarUsuarios));
            }
        }

        /// <summary>
        /// Disconecta um Agente de um Usuario Comum
        /// </summary>
        [HttpDelete("desconectar")]
        public async Task<ActionResult<ApiResponse<object>>> DesconectarUsuarios(
            [FromQuery] int cdAgente,
            [FromQuery] int cdUsuario)
        {
            try
            {
                var conexao = await _context.ConexaoUsuarios
                    .FirstOrDefaultAsync(c => c.CdUsuarioAgente == cdAgente && c.CdUsuario == cdUsuario);

                if (conexao == null)
                {
                    return ErrorResponse<object>(404, "Conexão não encontrada");
                }

                _context.ConexaoUsuarios.Remove(conexao);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Agente {CdUsuarioAgente} desconectado do usuário {CdUsuario}",
                    cdAgente, cdUsuario);

                return SuccessResponse("Usuários desconectados com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<object>(ex, nameof(DesconectarUsuarios));
            }
        }
    }
}