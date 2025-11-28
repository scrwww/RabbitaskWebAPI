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
        public async Task<ActionResult<ApiResponse<UsuarioDto>>> ObterUsuarioAtual()
        {
            try
            {
                var cdUsuarioAtual = _authService.ObterCdUsuarioAtual();

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
                    return RespostaErro<UsuarioDto>(404, "Usuário não encontrado");
                }

                return RespostaSucesso(usuario, "Perfil carregado com sucesso");
            }
            catch (Exception ex)
            {
                return TratarExcecao<UsuarioDto>(ex, nameof(ObterUsuarioAtual));
            }
        }

        /// <summary>
        /// Pega todos os usuários gerenciados pelo agente
        /// Retorna apenas a si msm + usuarios conectados se for Agente
        /// </summary>
        [HttpGet("gerenciaveis")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UsuarioResumoDto>>>> ObterUsuariosGerenciaveis()
        {
            try
            {
                var cdUsuarioAtual = _authService.ObterCdUsuarioAtual();
                var cdUsuariosGeridos = await _authService.ObterCdsUsuariosGerenciadosAsync(cdUsuarioAtual);

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

                return RespostaSucesso<IEnumerable<UsuarioResumoDto>>(
                    usuarios,
                    $"Encontrados {usuarios.Count} usuários gerenciáveis");
            }
            catch (Exception ex)
            {
                return TratarExcecao<IEnumerable<UsuarioResumoDto>>(ex, nameof(ObterUsuariosGerenciaveis));
            }
        }

        /// <summary>
        /// Pega o usuário por cd (se tiver permissão)
        /// </summary>
        [HttpGet("{codigo:int}")]
        public async Task<ActionResult<ApiResponse<UsuarioDto>>> ObterUsuario(int codigo)
        {
            try
            {
                var cdUsuarioAtual = _authService.ObterCdUsuarioAtual();

                // CHECAGEM MAROTA DE PERMISSÃO
                if (!await _authService.PodeGerenciarUsuarioAsync(cdUsuarioAtual, codigo))
                {
                    return RespostaErro<UsuarioDto>(403,
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
                    return RespostaErro<UsuarioDto>(404, "Usuário não encontrado");
                }

                return RespostaSucesso(usuario, "Usuário encontrado com sucesso");
            }
            catch (Exception ex)
            {
                return TratarExcecao<UsuarioDto>(ex, nameof(ObterUsuario));
            }
        }

        /// <summary>
        /// Atualiza as info do usuário
        /// </summary>
        [HttpPut("eu")]
        public async Task<ActionResult<ApiResponse<object>>> AtualizarUsuarioAtual(
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

                    return RespostaErro<object>(400, "Dados inválidos fornecidos", errors);
                }

                var cdUsuarioAtual = _authService.ObterCdUsuarioAtual();

                var usuario = await _context.Usuarios.FindAsync(cdUsuarioAtual);
                if (usuario == null)
                {
                    return RespostaErro<object>(404, "Usuário não encontrado");
                }

                if (!string.IsNullOrWhiteSpace(dto.Nome))
                    usuario.NmUsuario = dto.Nome.Trim();

                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    var emailExists = await _context.Usuarios
                        .AnyAsync(u => u.NmEmail == dto.Email && u.CdUsuario != cdUsuarioAtual);

                    if (emailExists)
                    {
                        return RespostaErro<object>(409, "Email já está em uso");
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

                return RespostaSucesso("Perfil atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return TratarExcecao<object>(ex, nameof(AtualizarUsuarioAtual));
            }
        }

        /// <summary>
        /// Atualiza os dados do usuario - esse é pro agente
        /// </summary>
        [HttpPut("{codigo:int}")]
        public async Task<ActionResult<ApiResponse<object>>> AtualizarUsuario(
            int codigo,
            [FromBody] UsuarioUpdateDto dto)
        {
            try
            {
                var cdUsuarioAtual = _authService.ObterCdUsuarioAtual();

                if (!await _authService.PodeGerenciarUsuarioAsync(cdUsuarioAtual, codigo))
                {
                    return RespostaErro<object>(403,
                        "Você não tem permissão para atualizar este usuário");
                }

                var usuario = await _context.Usuarios.FindAsync(codigo);
                if (usuario == null)
                {
                    return RespostaErro<object>(404, "Usuário não encontrado");
                }

                if (!string.IsNullOrWhiteSpace(dto.Nome))
                    usuario.NmUsuario = dto.Nome.Trim();

                if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    var emailExists = await _context.Usuarios
                        .AnyAsync(u => u.NmEmail == dto.Email && u.CdUsuario != codigo);

                    if (emailExists)
                    {
                        return RespostaErro<object>(409, "Email já está em uso");
                    }

                    usuario.NmEmail = dto.Email.Trim();
                }

                if (!string.IsNullOrWhiteSpace(dto.Telefone))
                    usuario.CdTelefone = dto.Telefone.Trim();

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Usuário {ManagerId} atualizou o perfil do usuário {UserId}",
                    cdUsuarioAtual, codigo);

                return RespostaSucesso("Usuário atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return TratarExcecao<object>(ex, nameof(AtualizarUsuario));
            }
        }

        /// <summary>
        /// Pega as estatísticas do usuário (task counts, taxa de cumprimento, etc.)
        /// </summary>
        [HttpGet("{codigo:int}/estatisticas")]
        public async Task<ActionResult<ApiResponse<UsuarioEstatisticasDto>>> ObterEstatisticas(int codigo)
        {
            try
            {
                var cdUsuarioAtual = _authService.ObterCdUsuarioAtual();

                if (!await _authService.PodeGerenciarUsuarioAsync(cdUsuarioAtual, codigo))
                {
                    return RespostaErro<UsuarioEstatisticasDto>(403,
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

                return RespostaSucesso(estatisticas, "Estatísticas carregadas com sucesso");
            }
            catch (Exception ex)
            {
                return TratarExcecao<UsuarioEstatisticasDto>(ex, nameof(ObterEstatisticas));
            }
        }

        /// <summary>
        /// Pega todos os usuarios conectados ao agente autenticado
        /// </summary>
        [HttpGet("meus-usuarios")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UsuarioResumoDto>>>> ObterMeusUsuarios()
        {
            try
            {
                var cdUsuarioAtual = _authService.ObterCdUsuarioAtual();

                // checa se é Agente
                var isAgente = await _authService.EhAgenteAsync(cdUsuarioAtual);
                if (!isAgente)
                {
                    return RespostaErro<IEnumerable<UsuarioResumoDto>>(403,
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

                return RespostaSucesso<IEnumerable<UsuarioResumoDto>>(
                    usuarios,
                    $"Encontrados {usuarios.Count} usuários gerenciados");
            }
            catch (Exception ex)
            {
                return TratarExcecao<IEnumerable<UsuarioResumoDto>>(ex, nameof(ObterMeusUsuarios));
            }
        }

        /// <summary>
        /// Retorna todos os agentes conectados ao usuário padrão autenticado.
        /// Apenas usuários padrão (não-agentes) podem acessar esta funcionalidade.
        /// </summary>
        [HttpGet("meus-agentes")]
        public async Task<ActionResult<ApiResponse<IEnumerable<UsuarioResumoDto>>>> ObterMeusAgentes()
        {
            try
            {
                var cdUsuarioAtual = _authService.ObterCdUsuarioAtual();

                var isAgente = await _authService.EhAgenteAsync(cdUsuarioAtual);
                if (isAgente)
                {
                    return RespostaErro<IEnumerable<UsuarioResumoDto>>(403,
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

                return RespostaSucesso<IEnumerable<UsuarioResumoDto>>(
                    agentes,
                    $"Encontrados {agentes.Count} agentes conectados");
            }
            catch (Exception ex)
            {
                return TratarExcecao<IEnumerable<UsuarioResumoDto>>(ex, nameof(ObterMeusAgentes));
            }
        }

        /// <summary>
        /// Usuário padrão gera um código temporário de conexão.
        /// </summary>
        [HttpPost("gerar-codigo")]
        public async Task<IActionResult> GerarCodigo()
        {
            int cdUsuario = _authService.ObterCdUsuarioAtual();

            if (await _authService.EhAgenteAsync(cdUsuario))
                return Forbid("Agentes não podem gerar códigos.");

            var codigo = _conexaoService.CriarCodigoConexao(cdUsuario);

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
        public async Task<IActionResult> ConectarComCodigo(string codigo)
        {
            int cdAgente = _authService.ObterCdUsuarioAtual();

            if (!await _authService.EhAgenteAsync(cdAgente))
                return Forbid("Somente agentes podem usar códigos de conexão.");

            var usuario = _conexaoService.ValidarCodigo(codigo);
            if (usuario == null)
                return BadRequest("Código inválido ou expirado.");

            bool jaExiste = await _context.ConexaoUsuarios.AnyAsync(c =>
                c.CdUsuarioAgente == cdAgente && c.CdUsuario == usuario.CdUsuario);

            if (jaExiste)
                return Conflict("Este usuário já está conectado a você.");

            var conexao = new ConexaoUsuario
            {
                CdUsuarioAgente = cdAgente,
                CdUsuario = usuario.CdUsuario
            };

            _context.ConexaoUsuarios.Add(conexao);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Agente {CdAgente} conectou com usuário {CdUsuario}", cdAgente, usuario.CdUsuario);

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
                    return RespostaErro<object>(404, "Conexão não encontrada");
                }

                _context.ConexaoUsuarios.Remove(conexao);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Agente {CdUsuarioAgente} desconectado do usuário {CdUsuario}",
                    cdAgente, cdUsuario);

                return RespostaSucesso("Usuários desconectados com sucesso");
            }
            catch (Exception ex)
            {
                return TratarExcecao<object>(ex, nameof(DesconectarUsuarios));
            }
        }
    }
}