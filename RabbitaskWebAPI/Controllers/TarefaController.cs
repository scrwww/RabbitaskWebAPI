using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitaskWebAPI.Data;
using RabbitaskWebAPI.Models;
using RabbitaskWebAPI.DTOs.Common;
using RabbitaskWebAPI.DTOs.Tarefa;
using RabbitaskWebAPI.RequestModels.Tarefa;
using System.ComponentModel.DataAnnotations;
using RabbitaskWebAPI.DTOs.Usuario;
using RabbitaskWebAPI.DTOs.Tag;
using RabbitaskWebAPI.DTOs.Prioridade;

namespace RabbitaskWebAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TarefaController : BaseController
    {
        private readonly RabbitaskContext _context;

        public TarefaController(RabbitaskContext context, ILogger<TarefaController> logger)
            : base(logger)
        {
            _context = context;
        }

        /// <summary>
        /// Obter todas as tarefas do usuário autenticado
        /// Note como o código ficou mais limpo e focado na lógica de negócio
        /// </summary>
        [HttpGet("minhas-tarefas")]
        public async Task<ActionResult<ApiResponse<IEnumerable<TarefaResumoDto>>>> GetMinhasTarefas(
            [FromQuery] int? prioridadeId = null,
            [FromQuery] bool? concluidas = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var cdUsuario = ObterIdUsuarioAutenticado();

                // Query base
                var query = _context.Tarefas
                    .Where(t => t.CdUsuario == cdUsuario)
                    .Include(t => t.CdTags)
                    .Include(t => t.CdPrioridadeNavigation)
                    .AsQueryable();

                // Aplicar filtros opcionais
                if (prioridadeId.HasValue)
                {
                    query = query.Where(t => t.CdPrioridade == prioridadeId.Value);
                }

                if (concluidas.HasValue)
                {
                    if (concluidas.Value)
                        query = query.Where(t => t.DtConclusao != null);
                    else
                        query = query.Where(t => t.DtConclusao == null);
                }

                // Paginação
                var totalItems = await query.CountAsync();
                var tarefas = await query
                    .OrderByDescending(t => t.DtCriacao)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new TarefaResumoDto
                    {
                        Cd = t.CdTarefa,
                        Nome = t.NmTarefa,
                        Descricao = t.DsTarefa,
                        DataPrazo = t.DtPrazo,
                        DataConclusao = t.DtConclusao,
                        DataCriacao = t.DtCriacao,
                        Prioridade = t.CdPrioridadeNavigation != null ? t.CdPrioridadeNavigation.NmPrioridade : null,
                        Tags = t.CdTags.Select(tag => new TagResumoDto
                        {
                            Cd = tag.CdTag,
                            Nome = tag.NmTag
                        }).ToList(),

                    })
                    .ToListAsync();

                var lista = tarefas as IEnumerable<TarefaResumoDto>;
                return SuccessResponse<IEnumerable<TarefaResumoDto>>(lista,
                    $"Encontradas {tarefas.Count} tarefas");
            }
            catch (Exception ex)
            {
                return HandleException<IEnumerable<TarefaResumoDto>>(ex, nameof(GetMinhasTarefas));
            }
        }

        /// <summary>
        /// Criar uma nova tarefa
        /// Demonstra como a lógica de negócio fica mais clara sem código repetitivo
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TarefaCriadaDto>>> CriarTarefa([FromQuery] CriarTarefaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToArray();

                    return ErrorResponse<TarefaCriadaDto>(400, "Dados inválidos fornecidos", errors);
                }

                var cdUsuario = ObterIdUsuarioAutenticado();

                // Validar dependências antes de iniciar transação
                await ValidarDependenciasTarefa(request);

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var novaTarefa = new Tarefa
                    {
                        NmTarefa = request.Nome.Trim(),
                        DsTarefa = !string.IsNullOrWhiteSpace(request.Descricao) ? request.Descricao.Trim() : null,
                        CdPrioridade = request.CdPrioridade,
                        DtPrazo = request.DataPrazo,
                        CdUsuario = cdUsuario,
                        CdUsuarioProprietario = request.CdUsuarioProprietario ?? cdUsuario,
                        DtCriacao = DateTime.Now
                    };

                    _context.Tarefas.Add(novaTarefa);
                    await _context.SaveChangesAsync();

                    // Associar tags se fornecidas
                    if (request.TagCds?.Any() == true)
                    {
                        await AssociarTagsATarefa(novaTarefa.CdTarefa, request.TagCds);
                    }

                    await transaction.CommitAsync();

                    _logger.LogInformation("Tarefa {TarefaId} criada pelo usuário {UsuarioId}",
                        novaTarefa.CdTarefa, cdUsuario);

                    var resultado = new TarefaCriadaDto
                    {
                        Cd = novaTarefa.CdTarefa,
                        Nome = novaTarefa.NmTarefa,
                        DataCriacao = novaTarefa.DtCriacao
                    };

                    return CreatedAtAction(nameof(GetTarefaPorId),
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
        /// Buscar tarefa por ID
        /// </summary>
        [HttpGet("{cd:int}")]
        public async Task<ActionResult<ApiResponse<TarefaDetalheDto>>> GetTarefaPorId(int cd)
        {
            try
            {
                var cdUsuario = ObterIdUsuarioAutenticado();

                var tarefa = await _context.Tarefas
                    .Include(t => t.CdTags)
                    .Include(t => t.CdPrioridadeNavigation)
                    .Include(t => t.CdUsuarioProprietarioNavigation)
                    .Where(t => t.CdTarefa == cd && t.CdUsuario == cdUsuario)
                    .Select(t => new TarefaDetalheDto
                    {
                        Cd = t.CdTarefa,
                        Nome = t.NmTarefa,
                        Descricao = t.DsTarefa,
                        DataPrazo = t.DtPrazo,
                        DataCriacao = t.DtCriacao,
                        DataConclusao = t.DtConclusao,
                        Prioridade = t.CdPrioridadeNavigation != null ?
                            new PrioridadeDto
                            {
                                Cd = t.CdPrioridadeNavigation.CdPrioridade,
                                Nome = t.CdPrioridadeNavigation.NmPrioridade
                            } : null,
                        UsuarioProprietario = t.CdUsuarioProprietarioNavigation != null ?
                            new UsuarioResumoDto
                            {
                                Cd = t.CdUsuarioProprietarioNavigation.CdUsuario,
                                Nome = t.CdUsuarioProprietarioNavigation.NmUsuario
                            } : null,
                        Tags = t.CdTags.Select(tag => new TagResumoDto
                        {
                            Cd = tag.CdTag,
                            Nome = tag.NmTag
                        }).ToList(),
                        //Conclusao é feita automaticamente com base na data de conclusão
                    })
                    .FirstOrDefaultAsync();

                if (tarefa == null)
                {
                    return ErrorResponse<TarefaDetalheDto>(404,
                        "Tarefa não encontrada",
                        $"Não foi possível encontrar uma tarefa com ID {cd}");
                }

                return SuccessResponse(tarefa, "Tarefa encontrada com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<TarefaDetalheDto>(ex, nameof(GetTarefaPorId));
            }
        }

        /// <summary>
        /// Marcar tarefa como concluída
        /// </summary>
        [HttpPatch("{cd:int}/concluir")]
        public async Task<ActionResult<ApiResponse<object>>> ConcluirTarefa(int cd)
        {
            try
            {
                var cdUsuario = ObterIdUsuarioAutenticado();

                var tarefa = await _context.Tarefas
                    .FirstOrDefaultAsync(t => t.CdTarefa == cd && t.CdUsuario == cdUsuario);

                if (tarefa == null)
                {
                    return ErrorResponse<object>(404, "Tarefa não encontrada");
                }

                if (tarefa.DtConclusao.HasValue)
                {
                    return ErrorResponse<object>(409, "Tarefa já está concluída");
                }

                tarefa.DtConclusao = DateTime.Now;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Tarefa {TarefaId} marcada como concluída pelo usuário {UsuarioId}",
                    cd, cdUsuario);

                return SuccessResponse("Tarefa marcada como concluída");
            }
            catch (Exception ex)
            {
                return HandleException<object>(ex, nameof(ConcluirTarefa));
            }
        }

        /// <summary>
        /// Reabrir tarefa concluída
        /// </summary>
        [HttpPatch("{cd:int}/reabrir")]
        public async Task<ActionResult<ApiResponse<object>>> ReabrirTarefa(int cd)
        {
            try
            {
                var cdUsuario = ObterIdUsuarioAutenticado();

                var tarefa = await _context.Tarefas
                    .FirstOrDefaultAsync(t => t.CdTarefa == cd && t.CdUsuario == cdUsuario);

                if (tarefa == null)
                {
                    return ErrorResponse<object>(404, "Tarefa não encontrada");
                }

                if (!tarefa.DtConclusao.HasValue)
                {
                    return ErrorResponse<object>(409, "Tarefa já está em aberto");
                }

                tarefa.DtConclusao = null;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Tarefa {TarefaId} reaberta pelo usuário {UsuarioId}",
                    cd, cdUsuario);

                return SuccessResponse("Tarefa reaberta com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<object>(ex, nameof(ReabrirTarefa));
            }
        }

        #region Métodos Privados de Apoio

        /// <summary>
        /// Valida se as dependências da tarefa existem
        /// </summary>
        private async Task ValidarDependenciasTarefa(CriarTarefaRequest request)
        {
            if (request.CdPrioridade.HasValue)
            {
                var prioridadeExiste = await _context.Prioridades
                    .AnyAsync(p => p.CdPrioridade == request.CdPrioridade.Value);

                if (!prioridadeExiste)
                {
                    throw new ArgumentException("A prioridade selecionada não é válida");
                }
            }

            if (request.CdUsuarioProprietario.HasValue)
            {
                var usuarioExiste = await _context.Usuarios
                    .AnyAsync(u => u.CdUsuario == request.CdUsuarioProprietario.Value);

                if (!usuarioExiste)
                {
                    throw new ArgumentException("O usuário proprietário selecionado não é válido");
                }
            }
        }

        /// <summary>
        /// Associa tags existentes a uma tarefa
        /// </summary>
        private async Task AssociarTagsATarefa(int tarefaCd, IEnumerable<int> tagCds)
        {
            var tags = await _context.Tags
                .Where(t => tagCds.Contains(t.CdTag))
                .ToListAsync();

            if (tags.Count != tagCds.Count())
            {
                throw new ArgumentException("Uma ou mais tags selecionadas não existem");
            }

            var tarefa = await _context.Tarefas
                .Include(t => t.CdTags)
                .FirstAsync(t => t.CdTarefa == tarefaCd);

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