using Microsoft.AspNetCore.Mvc;
using RabbitaskWebAPI.Data;
using RabbitaskWebAPI.DTOs.Common;

namespace RabbitaskWebAPI.Controllers
{
    /// <summary>
    /// Health check endpoint for container orchestration and monitoring
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly RabbitaskContext _context;
        private readonly ILogger<HealthController> _logger;

        public HealthController(RabbitaskContext context, ILogger<HealthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Returns health status of the API and database
        /// </summary>
        /// <returns>200 OK if healthy, 503 Service Unavailable if unhealthy</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<ActionResult<ApiResponse<object>>> Get()
        {
            try
            {
                // Check database connectivity
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (!canConnect)
                {
                    _logger.LogWarning("Health check failed: Cannot connect to database");
                    return StatusCode(StatusCodes.Status503ServiceUnavailable,
                        new ApiResponse<object>
                        {
                            Success = false,
                            Message = "Service unavailable: Database connection failed"
                        });
                }

                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Service is healthy"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Health check failed with exception");
                return StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Service unavailable"
                    });
            }
        }
    }
}
