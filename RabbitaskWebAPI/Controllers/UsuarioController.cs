using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitaskWebAPI.Data;
using RabbitaskWebAPI.DTOs.Common;
using RabbitaskWebAPI.DTOs.TipoUsuario;
using RabbitaskWebAPI.DTOs.Usuario;
using RabbitaskWebAPI.Models;
using RabbitaskWebAPI.Services;

namespace RabbitaskWebAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioController : BaseController
    {
        private readonly RabbitaskContext _context;
        private readonly Services.IUserAuthorizationService _authService;
        private readonly Services.ICodigoConexaoService _conexaoService;
        public UsuarioController(
            RabbitaskContext context,
            Services.IUserAuthorizationService authService,
            Services.ICodigoConexaoService codigoConexaoService,
            ILogger<UsuarioController> logger)
            : base(logger)
        {
            _conexaoService = codigoConexaoService;
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
        /// Retorna todos os agentes conectados ao usuário padrão autenticado.
        /// Apenas usuários padrão (não-agentes) podem acessar esta funcionalidade.
        /// </summary>
        [HttpGet("meus-agentes")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UsuarioResumoDto>>>> GetMeusAgentes()
        {
            try
            {
                var cdUsuarioAtual = _authService.GetCurrentUserId();

                var isAgente = await _authService.IsAgenteAsync(cdUsuarioAtual);
                if (isAgente)
                {
                    return ErrorResponse<IEnumerable<UsuarioResumoDto>>(403,
                        "Apenas usuários padrão podem acessar esta funcionalidade");
                }

                var agentes = await _context.ConexaoUsuarios
                    .Where(c => c.CdUsuario == cdUsuarioAtual)
                    .Include(c => c.CdUsuarioAgenteNavigation)
                    .Select(c => new UsuarioResumoDto
                    {
                        Cd = c.CdUsuarioAgenteNavigation.CdUsuario,
                        Nome = c.CdUsuarioAgenteNavigation.NmUsuario,
                        Email = c.CdUsuarioAgenteNavigation.NmEmail
                    })
                    .OrderBy(u => u.Nome)
                    .ToListAsync();

                return SuccessResponse<IEnumerable<UsuarioResumoDto>>(
                    agentes,
                    $"Encontrados {agentes.Count} agentes conectados");
            }
            catch (Exception ex)
            {
                return HandleException<IEnumerable<UsuarioResumoDto>>(ex, nameof(GetMeusAgentes));
            }
        }

        /// <summary>
        /// Usuário padrão gera um código temporário de conexão.
        /// </summary>
        [HttpPost("gerar-codigo")]
        public async Task<IActionResult> GerarCodigo()
        {
            int userId = _authService.GetCurrentUserId();

            if (await _authService.IsAgenteAsync(userId))
                return Forbid("Agentes não podem gerar códigos.");

            var codigo = _conexaoService.CriarCodigoConexao(userId);

            return Ok(new
            {
                codigo = codigo.Codigo,
                expiraEm = codigo.DataExpiracao
            });
        }

        /// <summary>
        /// Usuário agente usa um código válido para criar uma conexão com um usuário padrão.
        /// </summary>
        [HttpPost("conectar/{codigo}")]
        public async Task<IActionResult> ConectarPorCodigo(string codigo)
        {
            int agenteId = _authService.GetCurrentUserId();

            if (!await _authService.IsAgenteAsync(agenteId))
                return Forbid("Somente agentes podem usar códigos de conexão.");

            var usuario = _conexaoService.ValidarCodigo(codigo);
            if (usuario == null)
                return BadRequest("Código inválido ou expirado.");

            bool jaExiste = await _context.ConexaoUsuarios.AnyAsync(c =>
                c.CdUsuarioAgente == agenteId && c.CdUsuario == usuario.CdUsuario);

            if (jaExiste)
                return Conflict("Este usuário já está conectado a você.");

            var conexao = new ConexaoUsuario
            {
                CdUsuarioAgente = agenteId,
                CdUsuario = usuario.CdUsuario
            };

            _context.ConexaoUsuarios.Add(conexao);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Agente {AgenteId} conectou com usuário {UsuarioId}", agenteId, usuario.CdUsuario);

            return Ok(new
            {
                sucesso = true,
                usuarioConectado = usuario.NmUsuario,
                usuarioId = usuario.CdUsuario
            });
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