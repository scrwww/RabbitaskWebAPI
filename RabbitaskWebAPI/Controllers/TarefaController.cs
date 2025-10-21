using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitaskWebAPI.Data;
using RabbitaskWebAPI.DTOs.Common;
using RabbitaskWebAPI.DTOs.Prioridade;
using RabbitaskWebAPI.DTOs.Tag;
using RabbitaskWebAPI.DTOs.Tarefa;
using RabbitaskWebAPI.DTOs.Usuario;
using RabbitaskWebAPI.Models;

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

        #region GET - Listar / Obter tarefas

        /// <summary>
        /// Retorna uma lista paginada de tarefas do usuário especificado ou do usuário atual.
        /// Permite filtro por código, prioridade e status de conclusão.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<TarefaDto>>>> GetTarefas(
            [FromQuery] int? codigo = null,
            [FromQuery] int? cdUsuario = null,
            [FromQuery] int? cdPrioridade = null,
            [FromQuery] bool? concluidas = null,
            [FromQuery] int pagina = 1,
            [FromQuery] int paginaTamanho = 10)
        {
            try
            {
                var cdUsuarioAtual = _authService.GetCurrentUserId();
                var cdsUsuariosGerenciados = await _authService.GetManagedUserIdsAsync(cdUsuarioAtual);
                var cdUsuarioAlvo = cdUsuario ?? cdUsuarioAtual;

                if (!cdsUsuariosGerenciados.Contains(cdUsuarioAlvo))
                    return ErrorResponse<IEnumerable<TarefaDto>>(403, "Você não tem permissão para acessar tarefas deste usuário");

                var query = BuildTarefaQuery()
                    .Where(t => t.CdUsuario == cdUsuarioAlvo);

                query = ApplyFilters(query, cdPrioridade, concluidas, codigo);

                var totalItems = await query.CountAsync();

                var tarefas = await query
                    .OrderByDescending(t => t.DtCriacao)
                    .Skip((pagina - 1) * paginaTamanho)
                    .Take(paginaTamanho)
                    .Select(t => MapToTarefaDto(t))
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

        #endregion

        #region POST - Criar tarefa

        /// <summary>
        /// Cria uma nova tarefa para o usuário especificado.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TarefaCriadaDto>>> CriarTarefa([FromBody] TarefaCreateDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToArray();
                    return ErrorResponse<TarefaCriadaDto>(400, "Dados inválidos", errors);
                }

                var cdUsuarioAtual = _authService.GetCurrentUserId();

                if (!await _authService.CanManageUserAsync(cdUsuarioAtual, dto.CdUsuario))
                    return ErrorResponse<TarefaCriadaDto>(403, "Você não tem permissão para criar tarefas para este usuário");

                await ValidarDependenciasTarefa(dto);

                using var transaction = await _context.Database.BeginTransactionAsync();

                var proximoCodigo = await ObterProximoCodigoTarefa(dto.CdUsuario);

                var novaTarefa = new Tarefa
                {
                    CdTarefa = proximoCodigo,
                    NmTarefa = dto.Nome.Trim(),
                    DsTarefa = dto.Descricao?.Trim(),
                    CdPrioridade = dto.CdPrioridade,
                    DtPrazo = dto.DataPrazo,
                    CdUsuario = dto.CdUsuario,
                    CdUsuarioProprietario = cdUsuarioAtual,
                    DtCriacao = DateTime.Now
                };

                _context.Tarefas.Add(novaTarefa);
                await _context.SaveChangesAsync();

                // Cria ou associa tags
                if (dto.TagNomes?.Any() == true)
                    await AssociarTagsPorNomeATarefa(novaTarefa, dto.TagNomes);

                await transaction.CommitAsync();

                return SuccessResponse(new TarefaCriadaDto
                {
                    Cd = novaTarefa.CdTarefa,
                    Nome = novaTarefa.NmTarefa,
                    DataCriacao = novaTarefa.DtCriacao
                }, "Tarefa criada com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<TarefaCriadaDto>(ex, nameof(CriarTarefa));
            }
        }

        #endregion

        #region PUT / DELETE / PATCH

        /// <summary>
        /// Atualiza os dados de uma tarefa existente pelo código e usuário.
        /// </summary>
        [HttpPut("{codigo:int}")]
        public async Task<ActionResult<ApiResponse<object>>> AtualizarTarefa(int codigo, [FromBody] TarefaUpdateDto dto, [FromQuery] int? cdUsuario = null)
        {
            try
            {
                var tarefa = await ObterTarefaComPermissao(codigo, cdUsuario);
                if (tarefa == null)
                    return ErrorResponse<object>(404, "Tarefa não encontrada ou sem permissão");

                if (!string.IsNullOrWhiteSpace(dto.Nome))
                    tarefa.NmTarefa = dto.Nome.Trim();

                if (dto.Descricao != null)
                    tarefa.DsTarefa = dto.Descricao.Trim();

                if (dto.CdPrioridade.HasValue)
                {
                    if (!await ValidarPrioridade(dto.CdPrioridade.Value))
                        return ErrorResponse<object>(400, "Prioridade inválida");
                    tarefa.CdPrioridade = dto.CdPrioridade;
                }

                if (dto.DataPrazo.HasValue)
                    tarefa.DtPrazo = dto.DataPrazo;

                if (dto.TagNomes?.Any() == true)
                    await AssociarTagsPorNomeATarefa(tarefa, dto.TagNomes);

                await _context.SaveChangesAsync();

                return SuccessResponse("Tarefa atualizada com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<object>(ex, nameof(AtualizarTarefa));
            }
        }

        /// <summary>
        /// Exclui uma tarefa existente pelo código e usuário.
        /// </summary>
        [HttpDelete("{codigo:int}")]
        public async Task<ActionResult<ApiResponse<object>>> DeletarTarefa(int codigo, [FromQuery] int? cdUsuario = null)
        {
            try
            {
                var tarefa = await ObterTarefaComPermissao(codigo, cdUsuario);
                if (tarefa == null)
                    return ErrorResponse<object>(404, "Tarefa não encontrada ou sem permissão");

                _context.Tarefas.Remove(tarefa);
                await _context.SaveChangesAsync();

                return SuccessResponse("Tarefa deletada com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<object>(ex, nameof(DeletarTarefa));
            }
        }

        /// <summary>
        /// Marca uma tarefa como concluída.
        /// </summary>
        [HttpPatch("{codigo:int}/concluir")]
        public async Task<ActionResult<ApiResponse<object>>> ConcluirTarefa(int codigo, [FromQuery] int? cdUsuario = null)
        {
            try
            {
                var tarefa = await ObterTarefaComPermissao(codigo, cdUsuario);
                if (tarefa == null)
                    return ErrorResponse<object>(404, "Tarefa não encontrada ou sem permissão");

                if (tarefa.DtConclusao.HasValue)
                    return ErrorResponse<object>(409, "Tarefa já está concluída");

                tarefa.DtConclusao = DateTime.Now;
                await _context.SaveChangesAsync();

                return SuccessResponse("Tarefa marcada como concluída");
            }
            catch (Exception ex)
            {
                return HandleException<object>(ex, nameof(ConcluirTarefa));
            }
        }

        /// <summary>
        /// Reabre uma tarefa previamente concluída.
        /// </summary>
        [HttpPatch("{codigo:int}/reabrir")]
        public async Task<ActionResult<ApiResponse<object>>> ReabrirTarefa(int codigo, [FromQuery] int? cdUsuario = null)
        {
            try
            {
                var tarefa = await ObterTarefaComPermissao(codigo, cdUsuario);
                if (tarefa == null)
                    return ErrorResponse<object>(404, "Tarefa não encontrada ou sem permissão");

                if (!tarefa.DtConclusao.HasValue)
                    return ErrorResponse<object>(409, "Tarefa já está em aberto");

                tarefa.DtConclusao = null;
                await _context.SaveChangesAsync();

                return SuccessResponse("Tarefa reaberta com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<object>(ex, nameof(ReabrirTarefa));
            }
        }

        #endregion

        #region Métodos Auxiliares

        private IQueryable<Tarefa> BuildTarefaQuery()
        {
            return _context.Tarefas
                .Include(t => t.CdTags)
                .Include(t => t.CdPrioridadeNavigation)
                .Include(t => t.CdUsuarioNavigation)
                .Include(t => t.CdUsuarioProprietarioNavigation);
        }

        private IQueryable<Tarefa> ApplyFilters(IQueryable<Tarefa> query, int? cdPrioridade, bool? concluidas, int? Codigo)
        {
            if (cdPrioridade.HasValue)
                query = query.Where(t => t.CdPrioridade == cdPrioridade.Value);


            if (concluidas.HasValue)
                query = concluidas.Value
                    ? query.Where(t => t.DtConclusao != null)
                    : query.Where(t => t.DtConclusao == null);

            if(Codigo.HasValue)
                query = query.Where(t => t.CdTarefa == Codigo.Value);

            return query;
        }

        private static TarefaDto MapToTarefaDto(Tarefa t)
        {
            return new TarefaDto
            {
                Cd = t.CdTarefa,
                Nome = t.NmTarefa,
                Descricao = t.DsTarefa,
                DataPrazo = t.DtPrazo,
                DataCriacao = t.DtCriacao,
                DataConclusao = t.DtConclusao,
                Prioridade = t.CdPrioridadeNavigation != null
                    ? new PrioridadeDto { Cd = t.CdPrioridadeNavigation.CdPrioridade, Nome = t.CdPrioridadeNavigation.NmPrioridade }
                    : null,
                Usuario = new UsuarioResumoDto { Cd = t.CdUsuarioNavigation.CdUsuario, Nome = t.CdUsuarioNavigation.NmUsuario },
                UsuarioProprietario = t.CdUsuarioProprietarioNavigation != null
                    ? new UsuarioResumoDto { Cd = t.CdUsuarioProprietarioNavigation.CdUsuario, Nome = t.CdUsuarioProprietarioNavigation.NmUsuario }
                    : null,
                Tags = t.CdTags.Select(tag => new TagDto { Cd = tag.CdTag, Nome = tag.NmTag }).ToList()
            };
        }

        private async Task<Tarefa?> ObterTarefaComPermissao(int codigoTarefa, int? cdUsuario = null)
        {
            var cdUsuarioAtual = _authService.GetCurrentUserId();
            var cdsUsuariosGerenciados = await _authService.GetManagedUserIdsAsync(cdUsuarioAtual);
            var cdUsuarioAlvo = cdUsuario ?? cdUsuarioAtual;

            if (!cdsUsuariosGerenciados.Contains(cdUsuarioAlvo))
                return null;

            return await _context.Tarefas
                .Include(t => t.CdTags)
                .FirstOrDefaultAsync(t => t.CdTarefa == codigoTarefa && t.CdUsuario == cdUsuarioAlvo);
        }

        private async Task<int> ObterProximoCodigoTarefa(int cdUsuario)
        {
            var ultimoCodigo = await _context.Tarefas
                .Where(t => t.CdUsuario == cdUsuario)
                .MaxAsync(t => (int?)t.CdTarefa) ?? 0;

            return ultimoCodigo + 1;
        }

        private async Task<bool> ValidarPrioridade(int cdPrioridade)
        {
            return await _context.Prioridades.AnyAsync(p => p.CdPrioridade == cdPrioridade);
        }

        private async Task ValidarDependenciasTarefa(TarefaCreateDto dto)
        {
            if (!await _context.Usuarios.AnyAsync(u => u.CdUsuario == dto.CdUsuario))
                throw new ArgumentException("O usuário selecionado não existe");

            if (dto.CdPrioridade.HasValue && !await ValidarPrioridade(dto.CdPrioridade.Value))
                throw new ArgumentException("A prioridade selecionada não existe");
        }

        private async Task AssociarTagsPorNomeATarefa(Tarefa tarefa, IEnumerable<string> tagNomes)
        {
            foreach (var nome in tagNomes)
            {
                var nomeTrim = nome.Trim().ToLower();
                var tag = await _context.Tags.FirstOrDefaultAsync(t => t.NmTag.ToLower() == nomeTrim);

                if (tag == null)
                {
                    var novoCd = await _context.Tags.MaxAsync(t => (int?)t.CdTag) ?? 0;
                    tag = new Tag { CdTag = novoCd + 1, NmTag = nome.Trim() };
                    _context.Tags.Add(tag);
                    await _context.SaveChangesAsync();
                }

                if (!tarefa.CdTags.Any(t => t.CdTag == tag.CdTag))
                    tarefa.CdTags.Add(tag);
            }

            await _context.SaveChangesAsync();
        }

        private async Task AtualizarTagsDaTarefa(Tarefa tarefa, IEnumerable<int> novasTagCds)
        {
            var tagsExistentes = await _context.Tags
                .Where(t => novasTagCds.Contains(t.CdTag))
                .ToListAsync();

            tarefa.CdTags.Clear();
            foreach (var tag in tagsExistentes)
                tarefa.CdTags.Add(tag);
        }

        #endregion
    }
}
