using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitaskWebAPI.Data;
using RabbitaskWebAPI.Models;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using static RabbitaskWebAPI.Controllers.UsuarioController;

namespace RabbitaskWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class TarefaController : ControllerBase
    {
        private readonly RabbitaskContext _context;
        private readonly IConfiguration _configuration;

        public TarefaController(RabbitaskContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("Tarefas")]
        [Authorize]
        public IActionResult GetTarefasUsuario()
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

                if (!int.TryParse(userIdClaim.Value, out int cdUsuario))
                {
                    return Unauthorized("Formato do ID incorreto");
                }

                var tarefas = _context.Tarefas
                    .Where(t => t.CdUsuario == cdUsuario)
                    .Select(t => new
                    {
                        t.NmTarefa,
                        t.DtPrazo,
                        t.DtConclusao,
                        t.CdTags
                    });

                if (tarefas == null)
                    return NotFound("tarefas não encontradas");

                return Ok(tarefas);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("Criar")]
        [Authorize]
        public IActionResult Criar([FromBody] CriarRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.Nome))
                {
                    return BadRequest("Nome da tarefa é obrigatório");
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ??
                                 User.FindFirst(JwtRegisteredClaimNames.Sub) ??
                                 User.FindFirst("sub");

                if (userIdClaim == null)
                {
                    return Unauthorized("Claim do ID do usuário não encontrado no token");
                }

                if (!int.TryParse(userIdClaim.Value, out int cdUsuario))
                {
                    return Unauthorized("Formato do ID incorreto");
                }

                DateTime? dtPrazo = null;
                DateTime? dtCriacao = DateTime.Now;
                DateTime? dtConclusao = null;

                if (!string.IsNullOrEmpty(request.Prazo))
                {
                    if (DateTime.TryParse(request.Prazo, out DateTime parsedPrazo))
                    {
                        dtPrazo = parsedPrazo;
                    }
                    else
                    {
                        return BadRequest("Formato de data inválido para Prazo");
                    }
                }

                if (!string.IsNullOrEmpty(request.DtCriacao))
                {
                    if (DateTime.TryParse(request.DtCriacao, out DateTime parsedCriacao))
                    {
                        dtCriacao = parsedCriacao;
                    }
                }

                if (!string.IsNullOrEmpty(request.DtConclusao))
                {
                    if (DateTime.TryParse(request.DtConclusao, out DateTime parsedConclusao))
                    {
                        dtConclusao = parsedConclusao;
                    }
                }

                var newTarefa = new Tarefa
                {
                    NmTarefa = request.Nome,
                    DsTarefa = request.Descricao,
                    CdPrioridade = request.Prioridade > 0 ? request.Prioridade : null,
                    DtPrazo = dtPrazo,
                    CdUsuario = cdUsuario,
                    CdUsuarioProprietario = request.CdUsuarioProprietario > 0 ? request.CdUsuarioProprietario : cdUsuario,
                    DtCriacao = dtCriacao,
                    DtConclusao = dtConclusao
                };

                _context.Tarefas.Add(newTarefa);
                _context.SaveChanges();

                return Ok(new
                {
                    Message = "Tarefa criada com sucesso",
                    TarefaId = newTarefa.CdTarefa,
                    Nome = newTarefa.NmTarefa,
                    DataCriacao = newTarefa.DtCriacao
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }
    }

    public class CriarRequest
    {
        [Required(ErrorMessage = "Nome da tarefa é obrigatório")]
        [StringLength(250, ErrorMessage = "Nome deve ter no máximo 250 caracteres")]
        public string Nome { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Prioridade deve ser um valor positivo")]
        public int? Prioridade { get; set; }

        [StringLength(2000, ErrorMessage = "Descrição deve ter no máximo 2000 caracteres")]
        public string? Descricao { get; set; }

        public string? Prazo { get; set; } //"yyyy-MM-dd HH:mm:ss" or "yyyy-MM-dd"

        public int? CdUsuarioProprietario { get; set; }

        public string? DtCriacao { get; set; }
        public string? DtConclusao { get; set; }
    }
}
