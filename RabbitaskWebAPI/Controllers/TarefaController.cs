using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitaskWebAPI.Data;
using RabbitaskWebAPI.Models;
using RabbitaskWebAPI.DTOs.Common;
using RabbitaskWebAPI.DTOs.Tarefa;
using RabbitaskWebAPI.DTOs.Usuario;
using RabbitaskWebAPI.DTOs.Tag;
using RabbitaskWebAPI.DTOs.Prioridade;
using RabbitaskWebAPI.Services;

namespace RabbitaskWebAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TarefaController : BaseController
    {
        private readonly RabbitaskContext _context;
        private readonly Services.IUserAuthorizationService _authService;

        public TarefaController(
            RabbitaskContext context,
            Services.IUserAuthorizationService authService,
            ILogger<TarefaController> logger)
            : base(logger)
        {
            _context = context;
            _authService = authService;
        }

        /// <summary>
        /// pega todas as tarefas
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<TarefaDto>>>> GetTarefas(
            [FromQuery] int? cdUsuario = null,
            [FromQuery] bool incluirConectados = false,
            [FromQuery] int? cdPrioridade = null,
            [FromQuery] bool? concluidas = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int paginaTamanho = 10)
        {
            try
            {
                var cdUsuarioAtual = _authService.GetCurrentUserId();
                var cdUsuariosGeridos = await _authService.GetManagedUserIdsAsync(cdUsuarioAtual);

                if (cdUsuario.HasValue && !cdUsuariosGeridos.Contains(cdUsuario.Value))
                {
                    return ErrorResponse<IEnumerable<TarefaDto>>(403,
                        "Você não tem permissão para acessar tarefas deste usuário");
                }

                var cdUsuarioQuery = cdUsuario ?? cdUsuarioAtual;
                

                // Build na query
                var query = _context.Tarefas
                    .Where(t => cdUsuarioQuery == t.CdUsuario)
                    .Include(t => t.CdTags)
                    .Include(t => t.CdPrioridadeNavigation)
                    .Include(t => t.CdUsuarioNavigation)
                    .Include(t => t.CdUsuarioProprietarioNavigation)
                    .AsQueryable();

                // aplica os filters
                if (cdPrioridade.HasValue)
                {
                    query = query.Where(t => t.CdPrioridade == cdPrioridade.Value);
                }

                if (concluidas.HasValue)
                {
                    query = concluidas.Value
                        ? query.Where(t => t.DtConclusao != null)
                        : query.Where(t => t.DtConclusao == null);
                }

                var totalItems = await query.CountAsync();

                // aplica o modelo de página
                var tarefas = await query
                    .OrderByDescending(t => t.DtCriacao)
                    .Skip((pagina - 1) * paginaTamanho)
                    .Take(paginaTamanho)
                    .Select(t => new TarefaDto
                    {
                        Cd = t.CdTarefa,
                        Nome = t.NmTarefa,
                        Descricao = t.DsTarefa,
                        DataPrazo = t.DtPrazo,
                        DataConclusao = t.DtConclusao,
                        DataCriacao = t.DtCriacao,
                        Prioridade = t.CdPrioridadeNavigation != null
                            ? new PrioridadeDto
                            {
                                Cd = t.CdPrioridadeNavigation.CdPrioridade,
                                Nome = t.CdPrioridadeNavigation.NmPrioridade
                            }
                            : null,
                        Usuario = new UsuarioResumoDto
                        {
                            Cd = t.CdUsuarioNavigation.CdUsuario,
                            Nome = t.CdUsuarioNavigation.NmUsuario
                        },
                        UsuarioProprietario = t.CdUsuarioProprietarioNavigation != null
                            ? new UsuarioResumoDto
                            {
                                Cd = t.CdUsuarioProprietarioNavigation.CdUsuario,
                                Nome = t.CdUsuarioProprietarioNavigation.NmUsuario
                            }
                            : null,
                        Tags = t.CdTags.Select(tag => new TagDto
                        {
                            Cd = tag.CdTag,
                            Nome = tag.NmTag
                        }).ToList()
                    })
                    .ToListAsync();

                var totalPages = (int)Math.Ceiling((double)totalItems / paginaTamanho);

                return SuccessResponse<IEnumerable<TarefaDto>>(
                    tarefas,
                    $"Encontradas {totalItems} tarefas (página {pagina} de {totalPages})");
            }
            catch (Exception ex)
            {
                return HandleException<IEnumerable<TarefaDto>>(ex, nameof(GetTarefas));
            }
        }

        /// <summary>
        /// pega as tarefas pendentes
        /// </summary>
        [HttpGet("pendentes")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TarefaDto>>>> GetTarefasPendentes(
            [FromQuery] int? usuarioId = null)
        {
            try
            {
                var cdUsuarioAtual = _authService.GetCurrentUserId();
                var managedUserIds = await _authService.GetManagedUserIdsAsync(cdUsuarioAtual);

                if (usuarioId.HasValue && !managedUserIds.Contains(usuarioId.Value))
                {
                    return ErrorResponse<IEnumerable<TarefaDto>>(403,
                        "Você não tem permissão para acessar tarefas deste usuário");
                }

                var userIdsToQuery = usuarioId.HasValue
                    ? new List<int> { usuarioId.Value }
                    : managedUserIds;

                var tarefas = await _context.Tarefas
                    .Where(t => userIdsToQuery.Contains(t.CdUsuario) && t.DtConclusao == null)
                    .Include(t => t.CdTags)
                    .Include(t => t.CdPrioridadeNavigation)
                    .Include(t => t.CdUsuarioNavigation)
                    .Include(t => t.CdUsuarioProprietarioNavigation)
                    .OrderBy(t => t.DtPrazo)
                    .Select(t => new TarefaDto
                    {
                        Cd = t.CdTarefa,
                        Nome = t.NmTarefa,
                        Descricao = t.DsTarefa,
                        DataPrazo = t.DtPrazo,
                        DataConclusao = t.DtConclusao,
                        DataCriacao = t.DtCriacao,
                        Prioridade = t.CdPrioridadeNavigation != null
                            ? new PrioridadeDto
                            {
                                Cd = t.CdPrioridadeNavigation.CdPrioridade,
                                Nome = t.CdPrioridadeNavigation.NmPrioridade
                            }
                            : null,
                        Usuario = new UsuarioResumoDto
                        {
                            Cd = t.CdUsuarioNavigation.CdUsuario,
                            Nome = t.CdUsuarioNavigation.NmUsuario
                        },
                        UsuarioProprietario = t.CdUsuarioProprietarioNavigation != null
                            ? new UsuarioResumoDto
                            {
                                Cd = t.CdUsuarioProprietarioNavigation.CdUsuario,
                                Nome = t.CdUsuarioProprietarioNavigation.NmUsuario
                            }
                            : null,
                        Tags = t.CdTags.Select(tag => new TagDto
                        {
                            Cd = tag.CdTag,
                            Nome = tag.NmTag
                        }).ToList()
                    })
                    .ToListAsync();

                return SuccessResponse<IEnumerable<TarefaDto>>(
                    tarefas,
                    $"Encontradas {tarefas.Count} tarefas pendentes");
            }
            catch (Exception ex)
            {
                return HandleException<IEnumerable<TarefaDto>>(ex, nameof(GetTarefasPendentes));
            }
        }

        /// <summary>
        /// tarefa por codigo
        /// </summary>
        [HttpGet("{Codigo:int}")]
        public async Task<ActionResult<ApiResponse<TarefaDto>>> GetTarefa(int pCodigo)
        {
            try
            {
                var currentUserId = _authService.GetCurrentUserId();
                var managedUserIds = await _authService.GetManagedUserIdsAsync(currentUserId);

                var tarefa = await _context.Tarefas
                    .Where(t => t.CdTarefa == pCodigo && managedUserIds.Contains(t.CdUsuario))
                    .Include(t => t.CdTags)
                    .Include(t => t.CdPrioridadeNavigation)
                    .Include(t => t.CdUsuarioNavigation)
                    .Include(t => t.CdUsuarioProprietarioNavigation)
                    .Select(t => new TarefaDto
                    {
                        Cd = t.CdTarefa,
                        Nome = t.NmTarefa,
                        Descricao = t.DsTarefa,
                        DataPrazo = t.DtPrazo,
                        DataCriacao = t.DtCriacao,
                        DataConclusao = t.DtConclusao,
                        Prioridade = t.CdPrioridadeNavigation != null
                            ? new PrioridadeDto
                            {
                                Cd = t.CdPrioridadeNavigation.CdPrioridade,
                                Nome = t.CdPrioridadeNavigation.NmPrioridade
                            }
                            : null,
                        Usuario = new UsuarioResumoDto
                        {
                            Cd = t.CdUsuarioNavigation.CdUsuario,
                            Nome = t.CdUsuarioNavigation.NmUsuario
                        },
                        UsuarioProprietario = t.CdUsuarioProprietarioNavigation != null
                            ? new UsuarioResumoDto
                            {
                                Cd = t.CdUsuarioProprietarioNavigation.CdUsuario,
                                Nome = t.CdUsuarioProprietarioNavigation.NmUsuario
                            }
                            : null,
                        Tags = t.CdTags.Select(tag => new TagDto
                        {
                            Cd = tag.CdTag,
                            Nome = tag.NmTag
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (tarefa == null)
                {
                    return ErrorResponse<TarefaDto>(404,
                        "Tarefa não encontrada ou você não tem permissão para acessá-la");
                }

                return SuccessResponse(tarefa, "Tarefa encontrada com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<TarefaDto>(ex, nameof(GetTarefa));
            }
        }

        /// <summary>
        /// cria uam nova tarefa
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TarefaCriadaDto>>> CriarTarefa(
            [FromBody] TarefaCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray();

                    return ErrorResponse<TarefaCriadaDto>(400, "Dados inválidos", errors);
                }

                var currentUserId = _authService.GetCurrentUserId();

                if (!await _authService.CanManageUserAsync(currentUserId, dto.CdUsuario))
                {
                    return ErrorResponse<TarefaCriadaDto>(403,
                        "Você não tem permissão para criar tarefas para este usuário");
                }

                await ValidarDependenciasTarefa(dto);

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var novaTarefa = new Tarefa
                    {
                        NmTarefa = dto.Nome.Trim(),
                        DsTarefa = !string.IsNullOrWhiteSpace(dto.Descricao)
                            ? dto.Descricao.Trim()
                            : null,
                        CdPrioridade = dto.CdPrioridade,
                        DtPrazo = dto.DataPrazo,
                        CdUsuario = dto.CdUsuario,
                        CdUsuarioProprietario = currentUserId,
                        DtCriacao = DateTime.Now
                    };

                    _context.Tarefas.Add(novaTarefa);
                    await _context.SaveChangesAsync();

                    if (dto.TagCds?.Any() == true)
                    {
                        await AssociarTagsATarefa(novaTarefa.CdTarefa, dto.TagCds);
                    }

                    await transaction.CommitAsync();

                    _logger.LogInformation(
                        "Tarefa {TarefaId} criada por {ProprietarioId} para usuário {UsuarioId}",
                        novaTarefa.CdTarefa, currentUserId, dto.CdUsuario);

                    var resultado = new TarefaCriadaDto
                    {
                        Cd = novaTarefa.CdTarefa,
                        Nome = novaTarefa.NmTarefa,
                        DataCriacao = novaTarefa.DtCriacao
                    };

                    return CreatedAtAction(
                        nameof(GetTarefa),
                        new { id = novaTarefa.CdTarefa },
                        ApiResponse<TarefaCriadaDto>.CreateSuccess(resultado, "Tarefa criada com sucesso"));
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return HandleException<TarefaCriadaDto>(ex, nameof(CriarTarefa));
            }
        }

        /// <summary>
        /// atualiza a tarefa
        /// </summary>
        [HttpPut("{Codigo:int}")]
        public async Task<ActionResult<ApiResponse<object>>> AtualizarTarefa(
            int pCodigo,
            [FromBody] TarefaUpdateDto dto)
        {
            try
            {
                var currentUserId = _authService.GetCurrentUserId();
                var managedUserIds = await _authService.GetManagedUserIdsAsync(currentUserId);

                var tarefa = await _context.Tarefas
                    .FirstOrDefaultAsync(t => t.CdTarefa == pCodigo && managedUserIds.Contains(t.CdUsuario));

                if (tarefa == null)
                {
                    return ErrorResponse<object>(404,
                        "Tarefa não encontrada ou você não tem permissão para atualizá-la");
                }

                if (!string.IsNullOrWhiteSpace(dto.Nome))
                    tarefa.NmTarefa = dto.Nome.Trim();

                if (dto.Descricao != null)
                    tarefa.DsTarefa = dto.Descricao.Trim();

                if (dto.CdPrioridade.HasValue)
                {
                    var prioridadeExiste = await _context.Prioridades
                        .AnyAsync(p => p.CdPrioridade == dto.CdPrioridade.Value);

                    if (!prioridadeExiste)
                    {
                        return ErrorResponse<object>(400, "Prioridade inválida");
                    }

                    tarefa.CdPrioridade = dto.CdPrioridade;
                }

                if (dto.DataPrazo.HasValue)
                    tarefa.DtPrazo = dto.DataPrazo;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Tarefa {TarefaId} atualizada pelo usuário {UsuarioId}",
                    pCodigo, currentUserId);

                return SuccessResponse("Tarefa atualizada com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<object>(ex, nameof(AtualizarTarefa));
            }
        }

        /// <summary>
        /// deleta a tarefa
        /// </summary>
        [HttpDelete("{Codigo:int}")]
        public async Task<ActionResult<ApiResponse<object>>> DeletarTarefa(int pCodigo)
        {
            try
            {
                var cdUsuarioAtual = _authService.GetCurrentUserId();
                var cdUsuariosGeridos = await _authService.GetManagedUserIdsAsync(cdUsuarioAtual);

                var tarefa = await _context.Tarefas
                    .FirstOrDefaultAsync(t => t.CdTarefa == pCodigo && cdUsuariosGeridos.Contains(t.CdUsuario));

                if (tarefa == null)
                {
                    return ErrorResponse<object>(404,
                        "Tarefa não encontrada ou você não tem permissão para deletá-la");
                }

                _context.Tarefas.Remove(tarefa);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Tarefa {CdTarefa} deletada pelo usuário {CdUsuario}",
                    pCodigo, cdUsuarioAtual);

                return SuccessResponse("Tarefa deletada com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<object>(ex, nameof(DeletarTarefa));
            }
        }

        /// <summary>
        /// marca como completo
        /// </summary>
        [HttpPatch("{Codigo:int}/concluir")]
        public async Task<ActionResult<ApiResponse<object>>> ConcluirTarefa(int pCodigo)
        {
            try
            {
                var cdUsuarioAtual = _authService.GetCurrentUserId();
                var cdUsuariosGeridos = await _authService.GetManagedUserIdsAsync(cdUsuarioAtual);

                var tarefa = await _context.Tarefas
                    .FirstOrDefaultAsync(t => t.CdTarefa == pCodigo && cdUsuariosGeridos.Contains(t.CdUsuario));

                if (tarefa == null)
                {
                    return ErrorResponse<object>(404,
                        "Tarefa não encontrada ou você não tem permissão para concluí-la");
                }

                if (tarefa.DtConclusao.HasValue)
                {
                    return ErrorResponse<object>(409, "Tarefa já está concluída");
                }

                tarefa.DtConclusao = DateTime.Now;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Tarefa {TarefaId} marcada como concluída pelo usuário {UsuarioId}",
                    pCodigo, cdUsuarioAtual);

                return SuccessResponse("Tarefa marcada como concluída");
            }
            catch (Exception ex)
            {
                return HandleException<object>(ex, nameof(ConcluirTarefa));
            }
        }

        /// <summary>
        /// reabre a tarefa
        /// </summary>
        [HttpPatch("{Codigo:int}/reabrir")]
        public async Task<ActionResult<ApiResponse<object>>> ReabrirTarefa(int pCodigo)
        {
            try
            {
                var currentUserId = _authService.GetCurrentUserId();
                var managedUserIds = await _authService.GetManagedUserIdsAsync(currentUserId);

                var tarefa = await _context.Tarefas
                    .FirstOrDefaultAsync(t => t.CdTarefa == pCodigo && managedUserIds.Contains(t.CdUsuario));

                if (tarefa == null)
                {
                    return ErrorResponse<object>(404,
                        "Tarefa não encontrada ou você não tem permissão para reabri-la");
                }

                if (!tarefa.DtConclusao.HasValue)
                {
                    return ErrorResponse<object>(409, "Tarefa já está em aberto");
                }

                tarefa.DtConclusao = null;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Tarefa {TarefaId} reaberta pelo usuário {UsuarioId}",
                    pCodigo, currentUserId);

                return SuccessResponse("Tarefa reaberta com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<object>(ex, nameof(ReabrirTarefa));
            }
        }

        #region Métodos privados de abstração >:D

        /// <summary>
        /// valida as dependencias da tarefa
        /// </summary>
        private async Task ValidarDependenciasTarefa(TarefaCreateDto dto)
        {
            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.CdUsuario == dto.CdUsuario);

            if (!usuarioExiste)
            {
                throw new ArgumentException("O usuário selecionado não existe");
            }

            if (dto.CdPrioridade.HasValue)
            {
                var prioridadeExiste = await _context.Prioridades
                    .AnyAsync(p => p.CdPrioridade == dto.CdPrioridade.Value);

                if (!prioridadeExiste)
                {
                    throw new ArgumentException("A prioridade selecionada não é válida");
                }
            }

            if (dto.TagCds?.Any() == true)
            {
                var tagsExistentes = await _context.Tags
                    .Where(t => dto.TagCds.Contains(t.CdTag))
                    .CountAsync();

                if (tagsExistentes != dto.TagCds.Count())
                {
                    throw new ArgumentException("Uma ou mais tags selecionadas não existem");
                }
            }
        }

        /// <summary>
        /// nome meio auto explicativo...
        /// </summary>
        private async Task AssociarTagsATarefa(int tarefaId, IEnumerable<int> tagCds)
        {
            var tags = await _context.Tags
                .Where(t => tagCds.Contains(t.CdTag))
                .ToListAsync();

            var tarefa = await _context.Tarefas
                .Include(t => t.CdTags)
                .FirstAsync(t => t.CdTarefa == tarefaId);

            foreach (var tag in tags)
            {
                if (!tarefa.CdTags.Contains(tag))
                {
                    tarefa.CdTags.Add(tag);
                }
            }

            await _context.SaveChangesAsync();
        }

        #endregion
    }
}