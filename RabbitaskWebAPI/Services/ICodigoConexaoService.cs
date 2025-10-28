using RabbitaskWebAPI.Models;

namespace RabbitaskWebAPI.Services
{
    /// <summary>
    /// Define operações relacionadas à criação e validação de códigos de conexão entre usuários.
    /// </summary>
    public interface ICodigoConexaoService
    {
        /// <summary>
        /// Cria um novo código de conexão para um usuário.
        /// </summary>
        /// <param name="cdUsuario">Código do usuário que vai gerar o código.</param>
        /// <returns>Instância do código de conexão criado.</returns>
        CodigoConexao CriarCodigoConexao(int cdUsuario);

        /// <summary>
        /// Valida e retorna o usuário associado ao código, se válido.
        /// </summary>
        /// <param name="codigo">Código de conexão informado.</param>
        /// <returns>Usuário correspondente, ou null se inválido ou expirado.</returns>
        Usuario? ValidarCodigo(string codigo);

        /// <summary>
        /// Remove do banco os códigos expirados há mais de 24 horas.
        /// </summary>
        void LimparCodigosExpirados();

        /// <summary>
        /// Obtém informações sobre um código (sem marcá-lo como usado).
        /// </summary>
        /// <param name="codigo">O código a ser buscado.</param>
        /// <returns>O código de conexão, se válido e não usado; caso contrário, null.</returns>
        CodigoConexao? ObterCodigoPorCodigo(string codigo);
    }
}
