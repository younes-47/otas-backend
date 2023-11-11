namespace OTAS.Services
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
