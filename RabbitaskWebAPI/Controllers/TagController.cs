using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitaskWebAPI.Data;
using RabbitaskWebAPI.DTOs.Common;
using RabbitaskWebAPI.DTOs.Tag;
using RabbitaskWebAPI.Models;

namespace RabbitaskWebAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class TagController : BaseController
    {
        private readonly RabbitaskContext _context;

        public TagController(RabbitaskContext context, ILogger<TagController> logger)
            : base(logger)
        {
            _context = context;
        }

        /// <summary>
        /// Retorna todas as tags ou filtra por nome
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<TagDto>>>> GetTags([FromQuery] string? nome = null)
        {
            try
            {
                var query = _context.Tags.AsQueryable();

                if (!string.IsNullOrWhiteSpace(nome))
                    query = query.Where(t => EF.Functions.Like(t.NmTag, $"%{nome}%"));

                var tags = await query
                    .Select(t => new TagDto
                    {
                        Cd = t.CdTag,
                        Nome = t.NmTag
                    })
                    .ToListAsync();

                if(tags.Count == 0)
                {
                    return ErrorResponse<IEnumerable<TagDto>>(404, "Nenhuma tag encontrada");
                }

                return SuccessResponse<IEnumerable<TagDto>>(tags, "Encontradas as tags");
            }
            catch (Exception ex)
            {
                return HandleException<IEnumerable<TagDto>>(ex, nameof(GetTags));
            }
        }

        /// <summary>
        /// Retorna uma tag por código
        /// </summary>
        [HttpGet("{codigo:int}")]
        public async Task<ActionResult<ApiResponse<TagDto>>> GetTag(int codigo)
        {
            try
            {
                var tag = await _context.Tags
                    .Where(t => t.CdTag == codigo)
                    .Select(t => new TagDto
                    {
                        Cd = t.CdTag,
                        Nome = t.NmTag
                    })
                    .FirstOrDefaultAsync();

                if (tag == null)
                    return ErrorResponse<TagDto>(404, "Tag não encontrada");

                return SuccessResponse(tag);
            }
            catch (Exception ex)
            {
                return HandleException<TagDto>(ex, nameof(GetTag));
            }
        }

        /// <summary>
        /// Cria uma nova tag (se não existir)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<TagDto>>> CriarTag([FromBody] TagCreateDto dto)
        {
            try
            {
                var existente = await _context.Tags
                    .FirstOrDefaultAsync(t => t.NmTag.ToLower() == dto.Nome.ToLower());

                if (existente != null)
                    return SuccessResponse(new TagDto { Cd = existente.CdTag, Nome = existente.NmTag },
                        "Tag já existente");

                var proximoCodigo = await _context.Tags.MaxAsync(t => (int?)t.CdTag) ?? 0;

                var novaTag = new Tag
                {
                    CdTag = proximoCodigo + 1,
                    NmTag = dto.Nome.Trim()
                };

                _context.Tags.Add(novaTag);
                await _context.SaveChangesAsync();

                return SuccessResponse(new TagDto
                {
                    Cd = novaTag.CdTag,
                    Nome = novaTag.NmTag
                }, "Tag criada com sucesso");
            }
            catch (Exception ex)
            {
                return HandleException<TagDto>(ex, nameof(CriarTag));
            }
        }
    }
}
