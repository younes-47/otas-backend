using OTAS.Models;

namespace OTAS.DTO.Get
{
    public class StatusHistoryDTO
    {
        public string Status { get; set; } = null!;

        public string? DeciderFirstName { get; set; }

        public string? DeciderLastName { get; set; }

        public string? DeciderComment { get; set; }

        public DateTime CreateDate { get; set; }

    }
}
