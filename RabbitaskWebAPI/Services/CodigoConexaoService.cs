using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RabbitaskWebAPI.Controllers;
using RabbitaskWebAPI.Data;
using RabbitaskWebAPI.Models;

namespace RabbitaskWebAPI.Services
{
    public class CodigoConexaoService : ICodigoConexaoService
    {
        private readonly RabbitaskContext _context;
        private readonly IUserAuthorizationService _authService;
        private readonly ILogger<CodigoConexaoService> _logger;
        private const int TAMANHO_CODIGO = 8;
        private const int MINUTOS_EXPIRACAO = 5;

        public CodigoConexaoService(
            RabbitaskContext context,
            IUserAuthorizationService authService,
            ILogger<CodigoConexaoService> logger)
        {
            _context = context;
            _authService = authService;
            _logger = logger;
        }


        /// <summary>
        /// Gera um código aleatório único de 8 caracteres
        /// </summary>
        private string GerarCodigoAleatorio()
        {
            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var bytes = new byte[TAMANHO_CODIGO];
            
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            var resultado = new StringBuilder(TAMANHO_CODIGO);
            foreach (var b in bytes)
            {
                resultado.Append(caracteres[b % caracteres.Length]);
            }

            return resultado.ToString();
        }

        /// <summary>
        /// Cria um novo código de conexão para um usuário
        /// </summary>
        public CodigoConexao CriarCodigoConexao(int cdUsuario)
        {
            // Invalida códigos anteriores não usados deste usuário
            var codigosAntigos = _context.CodigosConexao
                .Where(c => c.CdUsuario == cdUsuario && !c.Usado)
                .ToList();

            foreach (var codigo in codigosAntigos)
            {
                codigo.Usado = true;
            }

            // Gera um código único
            string novoCodigo;
            do
            {
                novoCodigo = GerarCodigoAleatorio();
            } while (_context.CodigosConexao.Any(c => c.Codigo == novoCodigo && !c.Usado));

            var codigoConexao = new CodigoConexao
            {
                Codigo = novoCodigo,
                CdUsuario = cdUsuario,
                DataCriacao = DateTime.Now,
                DataExpiracao = DateTime.Now.AddMinutes(MINUTOS_EXPIRACAO),
                Usado = false
            };

            _context.CodigosConexao.Add(codigoConexao);
            _context.SaveChanges();

            return codigoConexao;
        }

        /// <summary>
        /// Valida e retorna o usuário associado ao código
        /// </summary>
        public Usuario? ValidarCodigo(string codigo)
        {
            var codigoConexao = _context.CodigosConexao
                .Include(c => c.Usuario)
                .FirstOrDefault(c => c.Codigo == codigo && !c.Usado);

            if (codigoConexao == null)
            {
                return null; // Código inválido ou já usado
            }

            if (codigoConexao.DataExpiracao < DateTime.Now)
            {
                return null; // Código expirado
            }

            // Marca o código como usado
            codigoConexao.Usado = true;
            _context.SaveChanges();

            return codigoConexao.Usuario;
        }

        /// <summary>
        /// Remove códigos expirados do banco (limpeza)
        /// </summary>
        public void LimparCodigosExpirados()
        {
            var dataLimite = DateTime.Now.AddHours(-24);
            var codigosExpirados = _context.CodigosConexao
                .Where(c => c.DataCriacao < dataLimite)
                .ToList();

            _context.CodigosConexao.RemoveRange(codigosExpirados);
            _context.SaveChanges();
        }

        /// <summary>
        /// Obtém informações sobre um código (sem marcá-lo como usado)
        /// </summary>
        public CodigoConexao? ObterCodigoPorCodigo(string codigo)
        {
            return _context.CodigosConexao
                .Include(c => c.Usuario)
                .FirstOrDefault(c => c.Codigo == codigo && !c.Usado && c.DataExpiracao > DateTime.Now);
        }
    }
}
