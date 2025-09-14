namespace RabbitaskWebAPI.DTOs.Common
{
    public class HealthStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool DatabaseConnection { get; set; }
        public string Version { get; set; } = string.Empty;
    }
}
