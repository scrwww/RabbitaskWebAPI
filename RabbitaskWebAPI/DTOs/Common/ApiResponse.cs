using System.Text.Json.Serialization;

namespace RabbitaskWebAPI.DTOs.Common
{
    /// <summary>
    /// Classe base para padronizar todas as respostas da API
    /// Garante consistência na estrutura de retorno de todos os endpoints
    /// </summary>
    /// <typeparam name="T">Tipo dos dados retornados no campo Data</typeparam>
    public class ApiResponse<T>
    {
        /// <summary>
        /// Indica se a operação foi bem-sucedida
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensagem descritiva sobre o resultado da operação
        /// Deve ser legível para usuários finais
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Dados retornados pela operação quando bem-sucedida
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; set; }

        /// <summary>
        /// Lista de erros detalhados quando a operação falha
        /// Útil para validações e debugging
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string[]? Errors { get; set; }

        /// <summary>
        /// Timestamp de quando a resposta foi gerada
        /// Útil para debugging e auditoria
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // Métodos de conveniência para criar respostas padronizadas

        /// <summary>
        /// Cria uma resposta de sucesso com dados
        /// </summary>
        public static ApiResponse<T> CreateSuccess(T data, string message = "Operação realizada com sucesso")
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        /// <summary>
        /// Cria uma resposta de sucesso sem dados
        /// </summary>
        public static ApiResponse<object> CreateSuccess(string message = "Operação realizada com sucesso")
        {
            return new ApiResponse<object>
            {
                Success = true,
                Message = message
            };
        }

        /// <summary>
        /// Cria uma resposta de erro com mensagens detalhadas
        /// </summary>
        public static ApiResponse<T> CreateError(string message, params string[] errors)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
                Errors = errors.Length > 0 ? errors : null
            };
        }
    }
}